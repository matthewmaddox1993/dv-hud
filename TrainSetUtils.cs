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
            public readonly PropertyInfo? Definition;
            public readonly MethodInfo? GetControllerSignal;

            public ControllerProperties(Type type)
            {
                Name = type.GetProperty("Name", BindingFlags.Instance | BindingFlags.Public);
                Position = type.GetProperty("Position", BindingFlags.Instance | BindingFlags.Public);
                Signals = type.GetProperty("Signals", BindingFlags.Instance | BindingFlags.Public);
                Definition = type.GetProperty("Definition", BindingFlags.Instance | BindingFlags.Public);
                GetControllerSignal = type.GetMethod(
                    "GetControllerSignal",
                    BindingFlags.Instance | BindingFlags.Public,
                    binder: null,
                    types: Type.EmptyTypes,
                    modifiers: null);
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
            public readonly ControllerProperties Controller;
            public readonly float DistanceSquared;

            public SignalCandidate(object signal, ControllerProperties controller, float distanceSquared)
            {
                Signal = signal;
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
                var foundCurrent = false;
                var foundUpcoming = false;
                SignalCandidate nearest = default;
                SignalCandidate secondNearest = default;

                foreach (var controller in controllers)
                {
                    if (controller == null)
                        continue;

                    var controllerInfo = GetControllerProperties(controller.GetType());
                    var positionValue = controllerInfo.Position?.GetValue(controller);
                    if (!(positionValue is Vector3 position))
                        continue;

                    var offset = position - origin;
                    var distanceSquared = offset.sqrMagnitude;
                    if (distanceSquared < 1f || Vector3.Dot(travelDirection, offset) < -10f)
                        continue;

                    // Opposing signal heads are often colocated, so proximity
                    // cannot distinguish them. DVSignals defines the controlled
                    // travel direction as the negative of the controller
                    // definition's forward axis (the head faces the train).
                    var definition = controllerInfo.Definition?.GetValue(controller) as Component;
                    if (definition != null
                        && Vector3.Dot(travelDirection, -definition.transform.forward) <= 0f)
                        continue;

                    // Junction controllers can contain one signal per branch.
                    // Let DVSignals select the active head for the aligned route
                    // instead of assuming the first entry is the live signal.
                    var signal = controllerInfo.GetControllerSignal?.Invoke(controller, null);
                    if (signal == null)
                    {
                        // Compatibility fallback for DVSignals versions that do
                        // not expose GetControllerSignal().
                        var signals = controllerInfo.Signals?.GetValue(controller) as System.Collections.IEnumerable;
                        if (signals != null)
                        {
                            foreach (var signalObject in signals)
                            {
                                if (signalObject != null)
                                {
                                    signal = signalObject;
                                    break;
                                }
                            }
                        }
                    }
                    if (signal == null)
                        continue;

                    var signalCandidate = new SignalCandidate(signal, controllerInfo, distanceSquared);

                    if (!foundCurrent || distanceSquared < nearest.DistanceSquared)
                    {
                        secondNearest = nearest;
                        foundUpcoming = foundCurrent;
                        nearest = signalCandidate;
                        foundCurrent = true;
                    }
                    else if (!foundUpcoming || distanceSquared < secondNearest.DistanceSquared)
                    {
                        secondNearest = signalCandidate;
                        foundUpcoming = true;
                    }
                }

                if (!foundCurrent)
                    return false;

                current = BuildSignalInfo(nearest);
                if (foundUpcoming)
                    upcoming = BuildSignalInfo(secondNearest);
                return true;
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
            var name = ReadString(candidate.Signal, signalInfo.Name)
                ?? ReadString(candidate.Signal, candidate.Controller.Name)
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
