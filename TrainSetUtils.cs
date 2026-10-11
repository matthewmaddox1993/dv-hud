using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace DvMod.HUDRevised
{
    public static class TrainSetUtils
    {
        private const string SignalManagerTypeName = "Signals.Game.SignalManager, Signals.Game";
        private const float SignalManagerRetryPeriod = 1f;
        private const float SignalDirectionSpeedThreshold = 0.1f;

        private sealed class ControllerProperties
        {
            public readonly PropertyInfo? Name;
            public readonly PropertyInfo? Position;
            public readonly PropertyInfo? Signals;
            public readonly PropertyInfo? ShuntingSignals;
            public readonly PropertyInfo? AllSignals;
            public readonly PropertyInfo? Definition;
            public readonly MethodInfo? GetControllerSignal;
            public readonly MethodInfo? GetControllerShuntingSignal;
            public readonly PropertyInfo? PlacementInfo;
            public readonly FieldInfo? PlacementTrack;
            public readonly PropertyInfo? StartingTrack;
            public readonly PropertyInfo? Junction;

            public ControllerProperties(Type type)
            {
                Name = type.GetProperty("Name", BindingFlags.Instance | BindingFlags.Public);
                Position = type.GetProperty("Position", BindingFlags.Instance | BindingFlags.Public);
                Signals = type.GetProperty("Signals", BindingFlags.Instance | BindingFlags.Public);
                ShuntingSignals = type.GetProperty("ShuntingSignals", BindingFlags.Instance | BindingFlags.Public);
                AllSignals = type.GetProperty("AllSignals", BindingFlags.Instance | BindingFlags.Public);
                Definition = type.GetProperty("Definition", BindingFlags.Instance | BindingFlags.Public);
                GetControllerSignal = type.GetMethod(
                    "GetControllerSignal",
                    BindingFlags.Instance | BindingFlags.Public,
                    binder: null,
                    types: Type.EmptyTypes,
                    modifiers: null);
                GetControllerShuntingSignal = type.GetMethod(
                    "GetControllerShuntingSignal",
                    BindingFlags.Instance | BindingFlags.Public,
                    binder: null,
                    types: Type.EmptyTypes,
                    modifiers: null);

                PlacementInfo = type.GetProperty("PlacementInfo", BindingFlags.Instance | BindingFlags.Public);
                if (PlacementInfo != null)
                {
                    var underlying = Nullable.GetUnderlyingType(PlacementInfo.PropertyType) ?? PlacementInfo.PropertyType;
                    PlacementTrack = underlying.GetField("Track", BindingFlags.Instance | BindingFlags.Public);
                }
                StartingTrack = type.GetProperty("StartingTrack", BindingFlags.Instance | BindingFlags.Public);
                Junction = type.GetProperty("Junction", BindingFlags.Instance | BindingFlags.Public);
            }
        }

        private sealed class SignalProperties
        {
            public readonly PropertyInfo? Name;
            public readonly PropertyInfo? IsOff;
            public readonly PropertyInfo? CurrentAspect;
            public readonly PropertyInfo? DisplayText;

            public SignalProperties(Type type)
            {
                Name = type.GetProperty("Name", BindingFlags.Instance | BindingFlags.Public);
                IsOff = type.GetProperty("IsOff", BindingFlags.Instance | BindingFlags.Public);
                CurrentAspect = type.GetProperty("CurrentAspect", BindingFlags.Instance | BindingFlags.Public);
                DisplayText = type.GetProperty("DisplayText", BindingFlags.Instance | BindingFlags.Public);
            }
        }

        private sealed class AspectProperties
        {
            public readonly PropertyInfo? DisplayText;
            public readonly PropertyInfo? Id;
            public readonly PropertyInfo? DisallowPassing;
            public readonly FieldInfo? IdField;
            public readonly MethodInfo? GetDefinition;
            public readonly FieldInfo? DefinitionIdField;

            public AspectProperties(Type type)
            {
                DisplayText = type.GetProperty("DisplayText", BindingFlags.Instance | BindingFlags.Public);
                var aspectInterface = Array.Find(
                    type.GetInterfaces(),
                    candidate => candidate.FullName == "Signals.Game.Aspects.IAspect");
                Id = type.GetProperty("Id", BindingFlags.Instance | BindingFlags.Public)
                    ?? aspectInterface?.GetProperty("Id", BindingFlags.Instance | BindingFlags.Public);
                DisallowPassing = type.GetProperty("DisallowPassing", BindingFlags.Instance | BindingFlags.Public)
                    ?? aspectInterface?.GetProperty("DisallowPassing", BindingFlags.Instance | BindingFlags.Public);
                IdField = type.GetField("Id", BindingFlags.Instance | BindingFlags.Public);
                GetDefinition = type.GetMethod("GetDefinition", BindingFlags.Instance | BindingFlags.Public)
                    ?? aspectInterface?.GetMethod("GetDefinition", BindingFlags.Instance | BindingFlags.Public);
                DefinitionIdField = GetDefinition?.ReturnType.GetField("Id", BindingFlags.Instance | BindingFlags.Public);
            }
        }

        private readonly struct SignalCandidate
        {
            public readonly object Signal;
            public readonly object? ControllerInstance;
            public readonly ControllerProperties Controller;
            public readonly float DistanceSquared;

            public SignalCandidate(object signal, object? controllerInstance, ControllerProperties controller, float distanceSquared)
            {
                Signal = signal;
                ControllerInstance = controllerInstance;
                Controller = controller;
                DistanceSquared = distanceSquared;
            }
        }

        private static readonly Dictionary<Type, ControllerProperties> controllerProperties = new Dictionary<Type, ControllerProperties>();
        private static readonly Dictionary<Type, SignalProperties> signalProperties = new Dictionary<Type, SignalProperties>();
        private static readonly Dictionary<Type, AspectProperties> aspectProperties = new Dictionary<Type, AspectProperties>();
        private static Type? signalManagerType;
        private static object? signalManager;
        private static PropertyInfo? allControllersProperty;
        private static bool? dvSignalsModInstalled;
        private static bool revisedMphInstalled;
        private static float nextRevisedMphCheck;
        private static float nextSignalManagerTypeLookup;
        private static float nextSignalManagerLookup;

        public readonly struct SignalInfo
        {
            public readonly string name;
            public readonly string status;
            public readonly float distance;

            public SignalInfo(string name, string status, float distance)
            {
                this.name = name;
                this.status = status;
                this.distance = distance;
            }
        }

        public static TrainCar CarAtEnd(TrainCar car, bool reversed)
        {
            var cars = car.trainset.cars;
            return reversed ? cars[0] : cars[cars.Count - 1];
        }

        public static float OverallLength(this Trainset trainset)
        {
            var total = 0f;
            foreach (var car in trainset.cars)
                total += car.logicCar.length;
            return total;
        }

        public static bool IsPlayerInsideLocomotive
        {
            get
            {
                try
                {
                    var car = PlayerManager.Car;
                    // PlayerManager.Car is the game's occupied vehicle link.
                    // IsLoco identifies the vehicle type directly and does not
                    // remain true merely because LastLoco remembers a loco.
                    return car != null && car.IsLoco;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static bool IsPlayerCarAvailable
        {
            get
            {
                try
                {
                    return PlayerManager.Car != null;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static float TotalMass(this Trainset trainset)
        {
            var total = 0f;
            foreach (var car in trainset.cars)
            {
                if (car == null || car.massController == null)
                    return float.NaN;

                // The game's total includes the car, bogies, cargo, and resources.
                total += car.massController.TotalMass;
            }

            return total;
        }

        public static bool IsRevisedMPHInstalled
        {
            get
            {
                try
                {
                    var now = Time.unscaledTime;
                    if (now >= nextRevisedMphCheck)
                    {
                        nextRevisedMphCheck = now + 1f;
                        revisedMphInstalled = UnityModManagerNet.UnityModManager.FindMod("Revised_Mph") != null
                            || UnityModManagerNet.UnityModManager.FindMod("RevisedMPH") != null
                            || IsRevisedMphInstalledOnDisk();
                    }
                }
                catch
                {
                    // Optional compatibility check; keep the last known state.
                }

                return revisedMphInstalled;
            }
        }

        public static string DefaultSpeedUnitSymbol => IsRevisedMPHInstalled ? "mph" : "km/h";

        private static bool IsRevisedMphInstalledOnDisk()
        {
            try
            {
                var gameDirectory = Directory.GetParent(Application.dataPath)?.FullName;
                if (string.IsNullOrEmpty(gameDirectory))
                    return false;

                var modsDirectory = Path.Combine(gameDirectory, "Mods");
                if (!Directory.Exists(modsDirectory))
                    return false;

                foreach (var modDirectory in Directory.GetDirectories(modsDirectory))
                {
                    var folderName = Path.GetFileName(modDirectory);
                    if (!string.Equals(folderName, "Revised_Mph", StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(folderName, "RevisedMPH", StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Revised_Mph may be loaded too early for FindMod(), but the
                    // installed assembly is still enough to select MPH units.
                    var revisedMphDll = Path.Combine(modDirectory, "Revised_Mph.dll");
                    var legacyMphDll = Path.Combine(modDirectory, "RevisedMPH.dll");
                    if (File.Exists(revisedMphDll) || File.Exists(legacyMphDll))
                        return true;
                }
            }
            catch
            {
                // Optional compatibility check; keep the last known state.
            }

            return false;
        }

        public static void ResetSignalCache()
        {
            signalManagerType = null;
            signalManager = null;
            allControllersProperty = null;
            dvSignalsModInstalled = null;
            revisedMphInstalled = false;
            nextRevisedMphCheck = 0f;
            nextSignalManagerTypeLookup = 0f;
            nextSignalManagerLookup = 0f;
            controllerProperties.Clear();
            signalProperties.Clear();
            aspectProperties.Clear();
        }

        private static RailTrack? GetControllerTrack(object controller, ControllerProperties info)
        {
            try
            {
                if (info.PlacementInfo != null && info.PlacementTrack != null)
                {
                    var placement = info.PlacementInfo.GetValue(controller);
                    if (placement != null)
                    {
                        var track = info.PlacementTrack.GetValue(placement) as RailTrack;
                        if (track != null)
                            return track;
                    }
                }

                if (info.StartingTrack != null)
                {
                    var track = info.StartingTrack.GetValue(controller) as RailTrack;
                    if (track != null)
                        return track;
                }

                if (info.Junction != null)
                {
                    var junction = info.Junction.GetValue(controller) as Junction;
                    if (junction != null)
                        return junction.inBranch?.track;
                }
            }
            catch
            {
            }

            return null;
        }

        private static object? GetActiveSignal(object controller, ControllerProperties info)
        {
            try
            {
                var sig = info.GetControllerSignal?.Invoke(controller, null);
                if (sig != null)
                    return sig;

                sig = info.GetControllerShuntingSignal?.Invoke(controller, null);
                if (sig != null)
                    return sig;

                if (info.Signals != null)
                {
                    var list = info.Signals.GetValue(controller) as System.Collections.IEnumerable;
                    if (list != null)
                    {
                        foreach (var item in list)
                        {
                            if (item != null)
                                return item;
                        }
                    }
                }

                if (info.ShuntingSignals != null)
                {
                    var list = info.ShuntingSignals.GetValue(controller) as System.Collections.IEnumerable;
                    if (list != null)
                    {
                        foreach (var item in list)
                        {
                            if (item != null)
                                return item;
                        }
                    }
                }

                if (info.AllSignals != null)
                {
                    var list = info.AllSignals.GetValue(controller) as System.Collections.IEnumerable;
                    if (list != null)
                    {
                        foreach (var item in list)
                        {
                            if (item != null)
                                return item;
                        }
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        private static List<RailTrack> GetRouteTracksAhead(TrainCar car, Vector3 travelDirection, int maxTracks = 25)
        {
            var tracks = new List<RailTrack>();
            try
            {
                var forwardSpeed = car.GetForwardSpeed();
                var reverser = car.GetComponent<DV.Simulation.Controllers.ReverserControl>();
                bool forward = Mathf.Abs(forwardSpeed) > SignalDirectionSpeedThreshold
                    ? forwardSpeed >= 0f
                    : (reverser == null || reverser.Value >= DV.Simulation.Controllers.ReverserControl.NEUTRAL_VALUE);

                var leadingBogie = forward ? car.FrontBogie : car.RearBogie;
                var trailingBogie = forward ? car.RearBogie : car.FrontBogie;

                var leadingTrack = leadingBogie?.track;
                var trailingTrack = trailingBogie?.track;

                if (leadingTrack != null)
                    tracks.Add(leadingTrack);
                if (trailingTrack != null && trailingTrack != leadingTrack)
                    tracks.Add(trailingTrack);

                if (tracks.Count == 0)
                    return tracks;

                var currentTrack = leadingTrack ?? trailingTrack!;
                var inNode = currentTrack.GetInNodeT();
                var outNode = currentTrack.GetOutNodeT();
                bool movingTowardsOut = inNode != null && outNode != null &&
                    Vector3.Dot(travelDirection, outNode.position - inNode.position) > 0f;

                for (int i = 0; i < maxTracks; i++)
                {
                    var branch = movingTowardsOut ? currentTrack.GetOutBranch() : currentTrack.GetInBranch();
                    if (branch == null || branch.track == null || tracks.Contains(branch.track))
                        break;

                    currentTrack = branch.track;
                    tracks.Add(currentTrack);
                    movingTowardsOut = branch.first;
                }
            }
            catch (Exception ex)
            {
                Main.DebugLog($"Failed to trace route tracks: {ex.Message}");
            }

            return tracks;
        }

        public static bool TryGetSignalInfo(out SignalInfo current, out SignalInfo upcoming)
        {
            current = default;
            upcoming = default;

            if (!IsDvSignalsInstalled)
                return false;

            try
            {
                var car = PlayerManager.Car;
                if (car == null)
                    return false;

                var manager = GetSignalManager();
                var controllers = allControllersProperty?.GetValue(manager) as System.Collections.IEnumerable;
                if (controllers == null)
                    return false;

                var origin = car.transform.position;
                var travelDirection = GetSignalTravelDirection(car);
                var routeTracks = GetRouteTracksAhead(car, travelDirection);

                var foundRouteCandidate = false;
                var foundSecondRouteCandidate = false;
                var bestRouteIndex = int.MaxValue;
                var bestSecondRouteIndex = int.MaxValue;
                var bestRouteForwardDist = float.MaxValue;
                var bestSecondRouteForwardDist = float.MaxValue;
                SignalCandidate routeNearest = default;
                SignalCandidate routeSecondNearest = default;

                var foundSpatialCandidate = false;
                var foundSecondSpatialCandidate = false;
                var bestSpatialForwardDist = float.MaxValue;
                var bestSecondSpatialForwardDist = float.MaxValue;
                SignalCandidate spatialNearest = default;
                SignalCandidate spatialSecondNearest = default;

                foreach (var controller in controllers)
                {
                    if (controller == null)
                        continue;

                    var controllerInfo = GetControllerProperties(controller.GetType());
                    var positionValue = controllerInfo.Position?.GetValue(controller);
                    var definition = controllerInfo.Definition?.GetValue(controller) as Component;

                    Vector3 position;
                    if (definition != null)
                        position = definition.transform.position;
                    else if (positionValue is Vector3 p)
                        position = p + WorldMover.currentMove;
                    else
                        continue;

                    var offset = position - origin;
                    var forwardDist = Vector3.Dot(travelDirection, offset);
                    var distanceSquared = offset.sqrMagnitude;

                    // Opposing signal heads are often colocated, so proximity
                    // cannot distinguish them. DVSignals defines the controlled
                    // travel direction as the negative of the controller
                    // definition's forward axis (the head faces the train).
                    if (definition != null
                        && Vector3.Dot(travelDirection, -definition.transform.forward) <= 0f)
                        continue;

                    var signal = GetActiveSignal(controller, controllerInfo);
                    if (signal == null)
                        continue;

                    var signalCandidate = new SignalCandidate(signal, controller, controllerInfo, distanceSquared);

                    // 1. Route-aware matching:
                    var track = GetControllerTrack(controller, controllerInfo);
                    int routeIndex = track != null ? routeTracks.IndexOf(track) : -1;
                    if (routeIndex >= 0)
                    {
                        int currentTrackCount = (car.FrontBogie?.track != null && car.RearBogie?.track != null && car.FrontBogie.track != car.RearBogie.track) ? 2 : 1;
                        bool isCurrentTrack = routeIndex < currentTrackCount;

                        // On current track(s), signal must be ahead of train (forwardDist > 0.5f).
                        // On downstream tracks, it is ahead along the route.
                        if (!isCurrentTrack || forwardDist > 0.5f)
                        {
                            if (!foundRouteCandidate
                                || routeIndex < bestRouteIndex
                                || (routeIndex == bestRouteIndex && forwardDist < bestRouteForwardDist))
                            {
                                routeSecondNearest = routeNearest;
                                bestSecondRouteIndex = bestRouteIndex;
                                bestSecondRouteForwardDist = bestRouteForwardDist;
                                foundSecondRouteCandidate = foundRouteCandidate;

                                foundRouteCandidate = true;
                                bestRouteIndex = routeIndex;
                                bestRouteForwardDist = forwardDist;
                                routeNearest = signalCandidate;
                            }
                            else if (!foundSecondRouteCandidate
                                || routeIndex < bestSecondRouteIndex
                                || (routeIndex == bestSecondRouteIndex && forwardDist < bestSecondRouteForwardDist))
                            {
                                foundSecondRouteCandidate = true;
                                bestSecondRouteIndex = routeIndex;
                                bestSecondRouteForwardDist = forwardDist;
                                routeSecondNearest = signalCandidate;
                            }
                        }
                    }

                    // 2. Spatial corridor fallback:
                    // In yards, tracks are ~4.5m apart. Filter out signals on adjacent tracks
                    // and any signals behind the train.
                    if (forwardDist >= 0.5f)
                    {
                        var lateralVec = offset - travelDirection * forwardDist;
                        var maxLateral = Mathf.Max(4.0f, forwardDist * 0.15f);
                        if (lateralVec.sqrMagnitude <= maxLateral * maxLateral)
                        {
                            if (!foundSpatialCandidate || forwardDist < bestSpatialForwardDist)
                            {
                                spatialSecondNearest = spatialNearest;
                                bestSecondSpatialForwardDist = bestSpatialForwardDist;
                                foundSecondSpatialCandidate = foundSpatialCandidate;

                                foundSpatialCandidate = true;
                                bestSpatialForwardDist = forwardDist;
                                spatialNearest = signalCandidate;
                            }
                            else if (!foundSecondSpatialCandidate || forwardDist < bestSecondSpatialForwardDist)
                            {
                                foundSecondSpatialCandidate = true;
                                bestSecondSpatialForwardDist = forwardDist;
                                spatialSecondNearest = signalCandidate;
                            }
                        }
                    }
                }

                if (foundRouteCandidate)
                {
                    current = BuildSignalInfo(routeNearest);
                    if (foundSecondRouteCandidate)
                        upcoming = BuildSignalInfo(routeSecondNearest);
                    return true;
                }

                if (foundSpatialCandidate)
                {
                    current = BuildSignalInfo(spatialNearest);
                    if (foundSecondSpatialCandidate)
                        upcoming = BuildSignalInfo(spatialSecondNearest);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Main.DebugLog($"DVSignals integration unavailable: {ex.Message}");
                return false;
            }
        }

        private static Vector3 GetSignalTravelDirection(TrainCar car)
        {
            var forward = car.transform.forward;
            var forwardSpeed = car.GetForwardSpeed();
            if (Mathf.Abs(forwardSpeed) > SignalDirectionSpeedThreshold)
                return forwardSpeed >= 0f ? forward : -forward;

            var reverser = car.GetComponent<DV.Simulation.Controllers.ReverserControl>();
            return reverser != null
                && reverser.Value < DV.Simulation.Controllers.ReverserControl.NEUTRAL_VALUE
                ? -forward
                : forward;
        }

        /// <summary>
        /// Gets the number of DV Signals signal objects currently available.
        /// This does not require the player to be in a train, so it can be used
        /// when reporting the integration status during world loading.
        /// </summary>
        public static bool TryGetSignalCount(out int signalCount)
        {
            signalCount = 0;

            if (!IsDvSignalsInstalled)
                return false;

            try
            {
                var manager = GetSignalManager();
                var controllers = allControllersProperty?.GetValue(manager) as System.Collections.IEnumerable;
                if (controllers == null)
                    return false;

                foreach (var controller in controllers)
                {
                    if (controller == null)
                        continue;

                    var signals = GetControllerProperties(controller.GetType()).Signals?.GetValue(controller) as System.Collections.IEnumerable;
                    if (signals == null)
                        continue;

                    foreach (var signal in signals)
                    {
                        if (signal != null)
                            signalCount++;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Main.DebugLog($"DVSignals signal detection failed: {ex.Message}");
                return false;
            }
        }

        public static bool IsDvSignalsInstalled
        {
            get
            {
                try
                {
                    if (!dvSignalsModInstalled.HasValue)
                        dvSignalsModInstalled = UnityModManagerNet.UnityModManager.FindMod("DVSignals") != null;

                    return dvSignalsModInstalled.Value
                        || GetSignalManagerType() != null;
                }
                catch
                {
                    return false;
                }
            }
        }

        private static Type? GetSignalManagerType()
        {
            if (signalManagerType != null)
                return signalManagerType;

            var now = Time.unscaledTime;
            if (now < nextSignalManagerTypeLookup)
                return null;

            nextSignalManagerTypeLookup = now + SignalManagerRetryPeriod;
            signalManagerType = Type.GetType(SignalManagerTypeName);
            return signalManagerType;
        }

        private static object? GetSignalManager()
        {
            var managerType = GetSignalManagerType();
            if (managerType == null)
                return null;

            if (signalManager != null
                && (!(signalManager is UnityEngine.Object unityObject) || unityObject != null))
            {
                return signalManager;
            }

            signalManager = null;
            allControllersProperty = null;

            var now = Time.unscaledTime;
            if (now < nextSignalManagerLookup)
                return null;

            nextSignalManagerLookup = now + SignalManagerRetryPeriod;
            var managers = Resources.FindObjectsOfTypeAll(managerType);
            if (managers.Length == 0)
                return null;

            signalManager = managers[0];
            allControllersProperty = signalManager.GetType().GetProperty("AllControllers", BindingFlags.Instance | BindingFlags.Public);
            return signalManager;
        }

        private static ControllerProperties GetControllerProperties(Type type)
        {
            if (!controllerProperties.TryGetValue(type, out var properties))
            {
                properties = new ControllerProperties(type);
                controllerProperties[type] = properties;
            }

            return properties;
        }

        private static SignalProperties GetSignalProperties(Type type)
        {
            if (!signalProperties.TryGetValue(type, out var properties))
            {
                properties = new SignalProperties(type);
                signalProperties[type] = properties;
            }

            return properties;
        }

        private static AspectProperties GetAspectProperties(Type type)
        {
            if (!aspectProperties.TryGetValue(type, out var properties))
            {
                properties = new AspectProperties(type);
                aspectProperties[type] = properties;
            }

            return properties;
        }

        private static SignalInfo BuildSignalInfo(SignalCandidate candidate)
        {
            var signalInfo = GetSignalProperties(candidate.Signal.GetType());
            var name = ReadString(candidate.ControllerInstance, candidate.Controller.Name)
                ?? ReadString(candidate.Signal, signalInfo.Name)
                ?? "Signal";
            var aspect = signalInfo.CurrentAspect?.GetValue(candidate.Signal);
            var aspectInfo = aspect == null ? null : GetAspectProperties(aspect.GetType());
            // Signal.DisplayText is empty in current DVSignals builds. Read
            // the active aspect through its interface, then fall back to its
            // definition ID; these IDs carry the actionable aspect (such as
            // Proceed, Reduce, or Stop).
            var status = ReadString(aspect, aspectInfo?.DisplayText);
            if (string.IsNullOrWhiteSpace(status))
                status = ReadString(aspect, aspectInfo?.Id);
            if (string.IsNullOrWhiteSpace(status))
                status = ReadString(aspect, aspectInfo?.IdField);
            if (string.IsNullOrWhiteSpace(status))
                status = ReadAspectDefinitionId(aspect, aspectInfo);
            if (string.IsNullOrWhiteSpace(status))
                status = ReadString(candidate.Signal, signalInfo.DisplayText);
            if (string.IsNullOrWhiteSpace(status))
            {
                status = aspect == null
                    ? (ReadString(candidate.Signal, signalInfo.IsOff) == "True" ? "Off" : "Unknown")
                    : aspect.ToString();
            }
            if (string.IsNullOrWhiteSpace(status))
                status = "Unknown";
            status = status ?? "Unknown";
            var disallowPassing = ReadBool(aspect, aspectInfo?.DisallowPassing);
            status = FormatSignalStatus(status, disallowPassing);

            return new SignalInfo(name, status, Mathf.Sqrt(candidate.DistanceSquared));
        }

        private static string FormatSignalStatus(string status, bool? disallowPassing)
        {
            var normalized = status.Replace('_', ' ').Replace('-', ' ').Trim().ToUpperInvariant();

            // DVSignals aspect IDs describe both the current and following
            // signal. For example, NEXT_STOP still permits passing this signal.
            // Prefer the aspect's authoritative movement restriction instead
            // of treating every ID containing the word "stop" as a stop aspect.
            if (disallowPassing == true)
                return "STOP";
            if (normalized.IndexOf("REDUC", StringComparison.Ordinal) >= 0
                || normalized.StartsWith("RESTRICTED", StringComparison.Ordinal)
                || normalized.IndexOf("CAUTION", StringComparison.Ordinal) >= 0
                || normalized.IndexOf("APPROACH", StringComparison.Ordinal) >= 0)
                return "REDUCE SPEED";
            if (disallowPassing == false)
                return "PROCEED";

            // Compatibility fallback for aspect implementations that do not
            // expose IAspect.DisallowPassing.
            if (normalized == "STOP"
                || normalized.StartsWith("STOP ", StringComparison.Ordinal))
                return "STOP";
            if (normalized.IndexOf("PROCEED", StringComparison.Ordinal) >= 0
                || normalized.IndexOf("CLEAR", StringComparison.Ordinal) >= 0)
                return "PROCEED";

            return status.Replace('_', ' ').Replace('-', ' ').Trim();
        }

        private static string? ReadAspectDefinitionId(object? aspect, AspectProperties? properties)
        {
            if (aspect == null || properties == null)
                return null;

            var definition = properties.GetDefinition?.Invoke(aspect, null);
            if (definition == null || properties.DefinitionIdField == null)
                return null;

            return properties.DefinitionIdField.GetValue(definition)?.ToString();
        }

        private static string? ReadString(object? instance, PropertyInfo? property)
        {
            if (instance == null)
                return null;

            var value = property?.GetValue(instance);
            return value?.ToString();
        }

        private static bool? ReadBool(object? instance, PropertyInfo? property)
        {
            if (instance == null)
                return null;

            var value = property?.GetValue(instance);
            return value is bool boolValue ? boolValue : (bool?)null;
        }

        private static string? ReadString(object? instance, FieldInfo? field)
        {
            if (instance == null)
                return null;

            var value = field?.GetValue(instance);
            return value?.ToString();
        }
    }
}
