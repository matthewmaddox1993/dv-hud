using QuantitiesNet;
using static QuantitiesNet.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DvMod.HUDRevised
{
    public static class Styles
    {
        public static readonly GUIStyle noChrome;
        public static readonly GUIStyle noWrap;
        public static readonly GUIStyle noWrapBold;
        public static readonly GUIStyle rightAlign;
        public static readonly GUIStyle richText;
        public static readonly GUIStyle panel;
        public static readonly GUIStyle windowTitle;
        public static readonly GUIStyle windowStatus;
        public static readonly GUIStyle panelHeader;
        public static readonly GUIStyle panelTag;
        public static readonly GUIStyle metricLabel;
        public static readonly GUIStyle metricValue;
        public static readonly GUIStyle signalStatus;
        public static readonly GUIStyle signalStop;
        public static readonly GUIStyle signalProceed;
        public static readonly GUIStyle signalReduce;
        public static readonly GUIStyle signalName;
        public static readonly GUIStyle statLabel;
        public static readonly GUIStyle statValue;
        public static readonly GUIStyle subHeader;
        public static readonly GUIStyle listIndex;
        public static readonly GUIStyle listValue;
        public static readonly GUIStyle carCard;
        public static readonly GUIStyle carCardTitle;
        public static readonly GUIStyle carDetail;
        public static readonly GUIStyle carDetailWarning;
        public static readonly GUIStyle carDetailDanger;
        public static readonly GUIStyle divider;

        /// <summary>Can only be called during OnGui()</summary>
        static Styles()
        {
            var steel = new Color(0.035f, 0.055f, 0.065f, 0.5f);
            var panelSteel = new Color(0.065f, 0.085f, 0.09f, 0.35f);
            var text = new Color(0.86f, 0.86f, 0.81f, 1f);
            var muted = new Color(0.56f, 0.61f, 0.60f, 1f);
            var amber = new Color(0.86f, 0.58f, 0.20f, 1f);
            var brightAmber = new Color(1f, 0.72f, 0.30f, 1f);
            var signalRed = new Color(1f, 0.34f, 0.28f, 1f);
            var signalGreen = new Color(0.38f, 0.88f, 0.48f, 1f);

            noChrome = new GUIStyle(GUI.skin.window);
            noChrome.normal.background = MakeTexture(steel);
            noChrome.onNormal.background = noChrome.normal.background;
            noChrome.border = new RectOffset(1, 1, 1, 1);
            noChrome.padding = new RectOffset(9, 9, 8, 9);

            noWrap = new GUIStyle(GUI.skin.label)
            {
                wordWrap = false,
                fontSize = 12,
            };
            noWrap.normal.textColor = text;

            noWrapBold = new GUIStyle(GUI.skin.label)
            {
                wordWrap = false,
                fontStyle = FontStyle.Bold,
                fontSize = 12,
            };
            noWrapBold.normal.textColor = text;

            rightAlign = new GUIStyle(noWrap)
            {
                alignment = TextAnchor.MiddleRight
            };

            richText = new GUIStyle(noWrap)
            {
                richText = true
            };

            panel = new GUIStyle(GUI.skin.box)
            {
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(8, 8, 7, 8),
                border = new RectOffset(1, 1, 1, 1),
            };
            panel.normal.background = MakeTexture(panelSteel);
            panel.onNormal.background = panel.normal.background;

            windowTitle = new GUIStyle(noWrapBold)
            {
                fontSize = 12,
                padding = new RectOffset(1, 0, 0, 1),
            };
            windowTitle.normal.textColor = brightAmber;

            windowStatus = new GUIStyle(noWrap)
            {
                fontSize = 9,
                alignment = TextAnchor.MiddleRight,
            };
            windowStatus.normal.textColor = amber;

            panelHeader = new GUIStyle(noWrapBold)
            {
                fontSize = 10,
                padding = new RectOffset(0, 0, 0, 2),
            };
            panelHeader.normal.textColor = brightAmber;

            panelTag = new GUIStyle(noWrap)
            {
                fontSize = 9,
                alignment = TextAnchor.MiddleRight,
            };
            panelTag.normal.textColor = muted;

            metricLabel = new GUIStyle(noWrap)
            {
                fontSize = 11,
            };
            metricLabel.normal.textColor = muted;

            metricValue = new GUIStyle(noWrap)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleRight,
            };
            metricValue.normal.textColor = text;

            signalStatus = new GUIStyle(noWrapBold)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
            };
            signalStatus.normal.textColor = brightAmber;

            signalStop = new GUIStyle(signalStatus);
            signalStop.normal.textColor = signalRed;

            signalProceed = new GUIStyle(signalStatus);
            signalProceed.normal.textColor = signalGreen;

            signalReduce = new GUIStyle(signalStatus);
            signalReduce.normal.textColor = brightAmber;

            signalName = new GUIStyle(metricLabel)
            {
                fontSize = 9,
            };

            statLabel = new GUIStyle(metricLabel)
            {
                fontSize = 9,
                alignment = TextAnchor.MiddleCenter,
            };

            statValue = new GUIStyle(metricValue)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
            };

            subHeader = new GUIStyle(noWrapBold)
            {
                fontSize = 9,
                padding = new RectOffset(0, 0, 4, 3),
            };
            subHeader.normal.textColor = amber;

            listIndex = new GUIStyle(metricLabel)
            {
                alignment = TextAnchor.MiddleRight,
            };
            listIndex.normal.textColor = amber;

            listValue = new GUIStyle(metricValue)
            {
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true,
            };

            carCard = new GUIStyle(GUI.skin.box)
            {
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0),
                border = new RectOffset(0, 0, 0, 0),
            };
            carCard.normal.background = MakeTexture(new Color(0.08f, 0.12f, 0.13f, 0.5f));
            carCard.hover.background = MakeTexture(new Color(0.12f, 0.17f, 0.18f, 0.65f));

            carCardTitle = new GUIStyle(noWrapBold)
            {
                fontSize = 10,
                wordWrap = true,
                clipping = TextClipping.Clip,
                padding = new RectOffset(0, 0, 0, 0),
            };

            carDetail = new GUIStyle(metricLabel)
            {
                fontSize = 9,
                wordWrap = false,
                clipping = TextClipping.Clip,
                padding = new RectOffset(0, 0, 0, 0),
            };

            carDetailWarning = new GUIStyle(carDetail);
            carDetailWarning.normal.textColor = brightAmber;

            carDetailDanger = new GUIStyle(carDetail);
            carDetailDanger.normal.textColor = signalRed;

            divider = new GUIStyle
            {
                fixedHeight = 1,
                margin = new RectOffset(0, 0, 3, 5),
            };
            divider.normal.background = MakeTexture(new Color(0.86f, 0.58f, 0.20f, 0.55f));
        }

        private static Texture2D MakeTexture(Color color)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Clamp,
            };
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }

    public class Overlay : MonoBehaviour
    {
        public const int ColumnSpacing = 10;

        public static Overlay? instance;

        private bool overlayEnabled = false;
        private bool signalStatusCheckStarted;
        private const float DisplayUpdatePeriod = 0.1f;
        private const float SignalUpdatePeriod = 0.5f;
        private float nextDisplayUpdate;
        private float nextTrainInfoUpdate;
        private float nextSignalUpdate;
        private bool hasSignalInfo;
        private bool playerInsideLocomotive;
        private bool playerActuallyInsideLocomotive;
        private TrainSetUtils.SignalInfo currentSignal;
        private TrainSetUtils.SignalInfo upcomingSignal;
        private readonly List<(string label, string value)> drivingInfo = new List<(string, string)>();
        private readonly List<string> carIds = new List<string>();
        private bool hasTrainInfo;
        private int trainCarCount;
        private string trainLength = "";
        private string trainMass = "";

        public void Start()
        {
            // Wait for a frame because for some reason RaycastAll doesn't detect colliders if called on the same frame.
            _ = StartCoroutine(DelayedEnable());
            instance = this;

            if (WorldStreamingInit.IsLoaded)
                BeginSignalStatusCheck();
        }

        public void BeginSignalStatusCheck()
        {
            if (signalStatusCheckStarted)
                return;

            signalStatusCheckStarted = true;
            _ = StartCoroutine(DelayedSignalStatusLog());
        }

        private IEnumerator DelayedSignalStatusLog()
        {
            // DVSignals may be loaded before its world signal controllers are populated.
            var deadline = Time.unscaledTime + 30f;
            while (Time.unscaledTime < deadline)
            {
                yield return new WaitForSeconds(0.5f);

                // WorldStreamingInit can be ready before the save's player
                // objects exist. Do not report an empty signal world yet.
                if (!WorldStreamingInit.IsLoaded || !TrainSetUtils.IsPlayerCarAvailable)
                    continue;

                if (Main.TryLogDvSignalsStatus())
                    yield break;
            }

            if (TrainSetUtils.IsPlayerCarAvailable)
                Main.LogNoDvSignalsFound();
        }

        private void Update()
        {
            if (!overlayEnabled || !Main.enabled)
                return;

            var now = Time.unscaledTime;

            if (now >= nextDisplayUpdate)
            {
                nextDisplayUpdate = now + DisplayUpdatePeriod;
                RefreshDisplayData(now);
            }

            if (now < nextSignalUpdate)
                return;

            nextSignalUpdate = now + SignalUpdatePeriod;

            if (!WorldStreamingInit.IsLoaded)
            {
                hasSignalInfo = false;
                return;
            }

            var signalSettings = Main.settings.signalInfoSettings;
            if (!signalSettings.enabled
                || (!signalSettings.showCurrent && !signalSettings.showUpcoming)
                || !TrainSetUtils.IsDvSignalsInstalled
                || !playerActuallyInsideLocomotive)
            {
                hasSignalInfo = false;
                return;
            }

            hasSignalInfo = TrainSetUtils.TryGetSignalInfo(out currentSignal, out upcomingSignal);
        }

        private void RefreshDisplayData(float now)
        {
            try
            {
                playerActuallyInsideLocomotive = TrainSetUtils.IsPlayerInsideLocomotive;
                playerInsideLocomotive = !Main.settings.onlyShowInsideLocomotive
                    || playerActuallyInsideLocomotive;

                if (!playerInsideLocomotive)
                {
                    drivingInfo.Clear();
                    carIds.Clear();
                    hasTrainInfo = false;
                    return;
                }

                RefreshDrivingInfo();
                if (now >= nextTrainInfoUpdate)
                {
                    var updatePeriod = Mathf.Max(0.05f, Main.settings.trainInfoSettings.updatePeriod);
                    nextTrainInfoUpdate = now + updatePeriod;
                    RefreshTrainInfo();
                }
            }
            catch
            {
                // Save loading can briefly expose incomplete player objects.
                // Retry on the next update instead of interrupting rendering.
                playerInsideLocomotive = false;
                playerActuallyInsideLocomotive = false;
                drivingInfo.Clear();
                carIds.Clear();
                hasTrainInfo = false;
            }
        }

        private void RefreshDrivingInfo()
        {
            drivingInfo.Clear();
            if (!Main.settings.drivingInfoSettings.enabled)
                return;

            var car = PlayerManager.Car;
            if (!car)
                return;

            foreach (var provider in Main.settings.drivingInfoSettings.OrderedProviders())
            {
                if (provider.TryGetFormatted(car, out var value))
                    drivingInfo.Add((provider.Label, value));
            }
        }

        private void RefreshTrainInfo()
        {
            carIds.Clear();
            hasTrainInfo = false;

            var settings = Main.settings.trainInfoSettings;
            if (!settings.enabled)
                return;

            var trainset = PlayerManager.Car?.trainset ?? PlayerManager.LastLoco?.trainset;
            if (trainset == null)
                return;

            hasTrainInfo = true;
            trainCarCount = trainset.cars.Count;
            var length = trainset.OverallLength();
            trainLength = settings.lengthUnits == Settings.TrainInfoSettings.LengthUnits.ft
                ? $"{new Quantities.Length(length).In(Foot):F0} ft"
                : $"{length:F0} m";
            var massKg = trainset.TotalMass();
            trainMass = float.IsNaN(massKg) || float.IsInfinity(massKg)
                ? "—"
                : $"{massKg / 1000f:F0} t";

            if (settings.showCarList)
            {
                foreach (var car in trainset.cars)
                    carIds.Add(car.ID);
                CarList.RefreshDetails(trainset);
            }
        }

        private IEnumerator DelayedEnable()
        {
            yield return null;
            overlayEnabled = true;
            Main.DebugLog($"Overlay enabled on frame {Time.frameCount}. Fixed update {Time.fixedTime / Time.fixedDeltaTime}");
        }

        private static Rect prevRect = new Rect();

        public void OnGUI()
        {
            if (!overlayEnabled)
                return;
            if (!Main.enabled)
                return;
            if (Main.settings.onlyShowInsideLocomotive && !playerInsideLocomotive)
                return;
            if (!HasVisibleContent())
                return;

            if (prevRect == new Rect())
                prevRect.position = Main.settings.hudPosition;
            var newRect = GUILayout.Window(
                GUIUtility.GetControlID(FocusType.Passive),
                prevRect,
                DrawDrivingInfoWindow,
                "",
                Styles.noChrome);
            newRect.x = Mathf.Clamp(newRect.x, 0f,
                Mathf.Max(0f, Screen.width - newRect.width));
            newRect.y = Mathf.Clamp(newRect.y, 0f,
                Mathf.Max(0f, Screen.height - newRect.height));
            prevRect = newRect;
            Main.settings.hudPosition = prevRect.position;
        }

        public void ResetPosition()
        {
            prevRect = new Rect();
        }

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        public static void DrawColumn<T>(IEnumerable<T> cells, string? label = null, Func<T, string>? renderer = null, GUIStyle? style = null)
        {
            renderer ??= x => x!.ToString();
            style ??= Styles.noWrap;
            GUILayout.Space(Overlay.ColumnSpacing);
            GUILayout.BeginVertical();
            if (label != null)
                GUILayout.Label(label, style);
            foreach (var cell in cells)
                GUILayout.Label(renderer(cell), style);
            GUILayout.EndVertical();
        }

        private void DrawDrivingInfoWindow(int windowID)
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label("CAB HUD", Styles.windowTitle);
            GUILayout.FlexibleSpace();
            GUILayout.Label("LIVE", Styles.windowStatus);
            GUILayout.EndHorizontal();
            GUILayout.Box(GUIContent.none, Styles.divider, GUILayout.ExpandWidth(true));

            var showDrivingInfo = Main.settings.drivingInfoSettings.enabled && drivingInfo.Count > 0;
            var signalSettings = Main.settings.signalInfoSettings;
            var showSignalInfo = signalSettings.enabled && hasSignalInfo
                && (signalSettings.showCurrent || signalSettings.showUpcoming);

            var availableWidth = Screen.width - Main.settings.hudPosition.x - 18f;
            var stackTopPanels = showDrivingInfo && showSignalInfo && availableWidth < 460f;
            if (stackTopPanels)
            {
                DrawCurrentCarInfo();
                GUILayout.Space(6f);
                DrawSignalInfo();
            }
            else if (showDrivingInfo || showSignalInfo)
            {
                GUILayout.BeginHorizontal();
                if (showDrivingInfo)
                    DrawCurrentCarInfo();
                if (showDrivingInfo && showSignalInfo)
                    GUILayout.Space(ColumnSpacing);
                if (showSignalInfo)
                    DrawSignalInfo();
                GUILayout.EndHorizontal();
            }

            var trainSettings = Main.settings.trainInfoSettings;
            var showTrainInfo = trainSettings.enabled && hasTrainInfo
                && (trainSettings.showTrainInfo || trainSettings.showCarList);
            if (showTrainInfo)
            {
                if (showDrivingInfo || showSignalInfo)
                    GUILayout.Space(8f);
                DrawTrainInfo();
            }

            GUILayout.EndVertical();

            if (!Main.settings.lockPosition)
                GUI.DragWindow(new Rect(0, 0, 10000, 28));
        }

        private void DrawCurrentCarInfo()
        {
            if (drivingInfo.Count == 0)
                return;

            GUILayout.BeginVertical(Styles.panel, GUILayout.ExpandHeight(false));
            DrawPanelHeader("LOCO DATA", "LIVE");
            for (var index = 0; index < drivingInfo.Count; index++)
            {
                if (index > 0)
                    GUILayout.Space(4f);

                var (label, value) = drivingInfo[index];
                GUILayout.BeginHorizontal();
                GUILayout.Label(label, Styles.metricLabel);
                GUILayout.FlexibleSpace();
                GUILayout.Label(value, Styles.metricValue);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private void DrawSignalInfo()
        {
            var settings = Main.settings.signalInfoSettings;
            if (!settings.showCurrent && !settings.showUpcoming)
                return;
            if (!hasSignalInfo)
                return;

            GUILayout.BeginVertical(Styles.panel, GUILayout.ExpandHeight(false));
            DrawPanelHeader("SIGNALS", "DV SIGNALS");
            if (settings.showCurrent)
                DrawSignalRow("CURRENT", currentSignal, settings.showDistance);
            if (settings.showUpcoming && upcomingSignal.name != null)
            {
                GUILayout.Space(7f);
                DrawSignalRow("NEXT", upcomingSignal, settings.showDistance);
            }
            GUILayout.EndVertical();
        }

        private static void DrawPanelHeader(string title, string tag)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(title, Styles.panelHeader);
            GUILayout.FlexibleSpace();
            GUILayout.Label(tag, Styles.panelTag);
            GUILayout.EndHorizontal();
        }

        private static void DrawSignalRow(string label, TrainSetUtils.SignalInfo signal, bool showDistance)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, Styles.metricLabel, GUILayout.Width(54));
            GUILayout.Label(signal.status, GetSignalStatusStyle(signal.status));
            GUILayout.FlexibleSpace();
            if (showDistance)
                GUILayout.Label($"{signal.distance:F0} m", Styles.metricValue);
            GUILayout.EndHorizontal();
            GUILayout.Label(signal.name, Styles.signalName);
            GUILayout.EndVertical();
        }

        private static GUIStyle GetSignalStatusStyle(string status)
        {
            switch (status)
            {
                case "STOP":
                    return Styles.signalStop;
                case "PROCEED":
                    return Styles.signalProceed;
                case "REDUCE SPEED":
                    return Styles.signalReduce;
                default:
                    return Styles.signalStatus;
            }
        }

        private void DrawTrainInfo()
        {
            if (!hasTrainInfo)
                return;

            GUILayout.BeginVertical(Styles.panel);
            DrawPanelHeader("TRAIN CONSIST", $"{trainCarCount:00} CARS");

            if (Main.settings.trainInfoSettings.showTrainInfo)
            {
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                DrawTrainStat(trainLength, "LENGTH");
                DrawTrainStat(trainMass, "MASS");
                GUILayout.EndHorizontal();
            }

            if (Main.settings.trainInfoSettings.showCarList)
            {
                if (Main.settings.trainInfoSettings.showTrainInfo)
                    GUILayout.Space(7f);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                CarList.DrawCarList(carIds);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        private static void DrawTrainStat(string value, string label)
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            GUILayout.Label(value, Styles.statValue, GUILayout.ExpandWidth(true));
            GUILayout.Label(label, Styles.statLabel, GUILayout.ExpandWidth(true));
            GUILayout.EndVertical();
        }

        private bool HasVisibleContent()
        {
            var settings = Main.settings;
            return (settings.drivingInfoSettings.enabled && drivingInfo.Count > 0)
                || (settings.signalInfoSettings.enabled && hasSignalInfo
                    && (settings.signalInfoSettings.showCurrent || settings.signalInfoSettings.showUpcoming))
                || (settings.trainInfoSettings.enabled && hasTrainInfo
                    && (settings.trainInfoSettings.showTrainInfo || settings.trainInfoSettings.showCarList));
        }
    }
}
