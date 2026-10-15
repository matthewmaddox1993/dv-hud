using System;
using System.Collections.Generic;
using System.Linq;
using DV.Logic.Job;
using UnityEngine;

namespace DvMod.HUDRevised
{
    public static class CarList
    {
        private const float MaxListHeight = 208f;
        private const int TargetRowsPerColumn = 12;
        private const int MaxColumns = 4;
        private const float ColumnWidth = 140f;
        private const float ColumnGap = 6f;
        private const float IndexWidth = 24f;
        private const float CardPadding = 6f;
        private const float CardGap = 6f;
        private const float DetailLineHeight = 14f;
        private const float ScrollbarReserve = 26f;
        private const float CompactListMaxWidth = 420f;
        private const float CompactRowHeight = 18f;
        private static Vector2 scrollPosition;
        private static List<CarDetails> details = new List<CarDetails>();
        private static GUIStyle? compactJobStyle;
        private static GUIStyle? compactCarsStyle;

        private readonly struct CarDetails
        {
            public readonly string Id;
            public readonly string Stress;
            public readonly string Job;
            public readonly string Destination;
            public readonly string Brake;
            public readonly float StressPercent;
            public readonly bool HandbrakeOn;

            public CarDetails(string id, string stress, string job, string destination,
                string brake, float stressPercent, bool handbrakeOn)
            {
                Id = id;
                Stress = stress;
                Job = job;
                Destination = destination;
                Brake = brake;
                StressPercent = stressPercent;
                HandbrakeOn = handbrakeOn;
            }
        }

        public static void RefreshDetails(Trainset trainset)
        {
            var settings = Main.settings.trainInfoSettings;
            var updated = new List<CarDetails>(trainset.cars.Count);
            var taskCache = new Dictionary<Job, List<TaskData>>();
            var jobsManager = JobsManager.Instance;
            // Display the accumulated derail stress as a bounded percentage of
            // the buildup threshold that actually triggers a derailment check.
            var threshold = DV.Globals.G?.GameParams?.DerailBuildUpThreshold ?? 0f;

            foreach (var car in trainset.cars)
            {
                var stress = "—";
                var jobId = "—";
                var destination = "—";
                var brakeStatus = "—";
                var stressPercent = float.NaN;
                var handbrakeOn = false;

                if (settings.showCarStress && car.stress != null && threshold > 0f)
                {
                    stressPercent = Mathf.Clamp(
                        car.stress.derailBuildUp / threshold * 100f, 0f, 100f);
                    stress = $"{stressPercent:F0}%";
                }

                if ((settings.showCarJobs || settings.showCarDestinations
                    || (settings.compactCarList && settings.groupCarsByJob))
                    && jobsManager != null && car.logicCar != null)
                {
                    var job = jobsManager.GetJobOfCar(car.logicCar, false);
                    if (job != null)
                    {
                        jobId = job.ID;
                        if (settings.showCarDestinations)
                            destination = GetDestination(job, car.logicCar, taskCache);
                    }
                }

                if (settings.showCarBrakeStatus && car.brakeSystem != null)
                {
                    var brakes = car.brakeSystem;
                    brakeStatus = $"{brakes.brakePipePressure:F1} bar  F {brakes.brakingFactor:P0}";
                    handbrakeOn = brakes.handbrakePosition > 0.1f;
                    if (handbrakeOn)
                        brakeStatus += "  HB";
                }

                updated.Add(new CarDetails(car.ID, stress, jobId, destination,
                    brakeStatus, stressPercent, handbrakeOn));
            }

            details = updated;
        }

        private static string GetDestination(Job job, Car car, Dictionary<Job, List<TaskData>> taskCache)
        {
            if (!taskCache.TryGetValue(job, out var tasks))
            {
                tasks = new List<TaskData>();
                foreach (var task in job.tasks)
                    CollectTaskData(task, tasks);
                taskCache[job] = tasks;
            }

            foreach (var task in tasks)
            {
                if (task.state == TaskState.InProgress
                    && task.cars != null && task.cars.Contains(car)
                    && task.destinationTrack?.ID != null)
                {
                    return task.destinationTrack.ID.FullDisplayID;
                }
            }

            return job.chainData?.chainDestinationYardId ?? "—";
        }

        private static void CollectTaskData(DV.Logic.Job.Task task, List<TaskData> result)
        {
            var data = task.GetTaskData();
            if (data.type == TaskType.Transport || data.type == TaskType.Warehouse)
                result.Add(data);

            if (data.nestedTasks == null)
                return;

            foreach (var nested in data.nestedTasks)
                CollectTaskData(nested, result);
        }

        public static void DrawCarList(IReadOnlyList<string> carIds)
        {
            var settings = Main.settings.trainInfoSettings;
            if (settings.compactCarList)
            {
                DrawCompactCarList(carIds, settings);
                return;
            }

            var availableWidth = Mathf.Max(ColumnWidth + ScrollbarReserve,
                Screen.width - Main.settings.hudPosition.x - 12f);
            var screenColumns = Mathf.Max(1, Mathf.FloorToInt(
                (availableWidth - ScrollbarReserve + ColumnGap) / (ColumnWidth + ColumnGap)));
            var maxColumns = Mathf.Min(MaxColumns, screenColumns);
            if (carIds.Count > TargetRowsPerColumn
                && Screen.width >= ColumnWidth * 2f + ColumnGap + ScrollbarReserve + 18f)
            {
                // Keep a multi-column list even when the HUD was parked too
                // close to the right edge; the window is clamped back onscreen.
                maxColumns = Mathf.Max(2, maxColumns);
            }
            var columns = Mathf.Clamp(Mathf.CeilToInt((float)carIds.Count / TargetRowsPerColumn), 1, maxColumns);
            var rows = Mathf.CeilToInt((float)carIds.Count / columns);
            var contentWidth = columns * ColumnWidth + (columns - 1) * ColumnGap;
            var viewWidth = contentWidth + ScrollbarReserve;
            var detailLines = (settings.showCarStress || settings.showCarJobs ? 1 : 0)
                + (settings.showCarDestinations ? 1 : 0)
                + (settings.showCarBrakeStatus ? 1 : 0);
            var titleHeight = 19f;
            foreach (var id in carIds)
            {
                if (!string.IsNullOrEmpty(id) && (id.Length > 16 || id.IndexOf('\n') >= 0
                    || id.IndexOf("\\n", StringComparison.Ordinal) >= 0))
                {
                    titleHeight = 32f;
                    break;
                }
            }
            var cardHeight = CardPadding * 2f + titleHeight + detailLines * DetailLineHeight;
            var contentHeight = rows * cardHeight + Mathf.Max(0, rows - 1) * CardGap + 2f;
            var viewHeight = Mathf.Min(MaxListHeight, Mathf.Max(36f, contentHeight));

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal(GUILayout.Width(viewWidth));
            GUILayout.Label("CARS", Styles.subHeader);
            GUILayout.FlexibleSpace();
            if (contentHeight > viewHeight)
                GUILayout.Label("SCROLL FOR MORE", Styles.panelTag);
            GUILayout.EndHorizontal();

            var viewport = GUILayoutUtility.GetRect(viewWidth, viewHeight,
                GUILayout.Width(viewWidth), GUILayout.Height(viewHeight));
            scrollPosition.x = 0f;
            scrollPosition.y = Mathf.Clamp(scrollPosition.y, 0f,
                Mathf.Max(0f, contentHeight - viewHeight));
            scrollPosition = GUI.BeginScrollView(viewport, scrollPosition,
                new Rect(0f, 0f, contentWidth, contentHeight), false, false);

            for (var column = 0; column < columns; column++)
            {
                for (var row = 0; row < rows; row++)
                {
                    var index = column * rows + row;
                    if (index >= carIds.Count)
                        break;

                    var x = column * (ColumnWidth + ColumnGap);
                    var y = row * (cardHeight + CardGap);
                    var cardRect = new Rect(x, y, ColumnWidth, cardHeight);
                    GUI.Box(cardRect, GUIContent.none, Styles.carCard);
                    var textX = x + CardPadding;
                    var textWidth = ColumnWidth - CardPadding * 2f;
                    GUI.Label(new Rect(textX, y + CardPadding, IndexWidth, titleHeight),
                        $"{index + 1:00}", Styles.listIndex);
                    GUI.Label(new Rect(textX + IndexWidth + 3f, y + CardPadding,
                        textWidth - IndexWidth - 3f, titleHeight),
                        FormatCarName(carIds[index]), Styles.carCardTitle);

                    if (index < details.Count && details[index].Id == carIds[index])
                    {
                        var car = details[index];
                        var detailY = y + CardPadding + titleHeight;
                        if (settings.showCarStress)
                        {
                            var style = float.IsNaN(car.StressPercent) ? Styles.carDetail
                                : car.StressPercent >= 70f ? Styles.carDetailDanger
                                : car.StressPercent >= 30f ? Styles.carDetailWarning
                                : Styles.carDetail;
                            var stressWidth = settings.showCarJobs ? 62f : textWidth;
                            GUI.Label(new Rect(textX, detailY, stressWidth, DetailLineHeight),
                                $"STRESS {car.Stress}", style);
                        }
                        if (settings.showCarJobs)
                        {
                            var jobX = settings.showCarStress ? textX + 64f : textX;
                            GUI.Label(new Rect(jobX, detailY, textWidth - (jobX - textX), DetailLineHeight),
                                $"JOB {car.Job}", Styles.carDetail);
                        }
                        if (settings.showCarStress || settings.showCarJobs)
                            detailY += DetailLineHeight;
                        if (settings.showCarDestinations)
                        {
                            GUI.Label(new Rect(textX, detailY, textWidth, DetailLineHeight),
                                $"TO {car.Destination}", Styles.carDetail);
                            detailY += DetailLineHeight;
                        }
                        if (settings.showCarBrakeStatus)
                            GUI.Label(new Rect(textX, detailY, textWidth, DetailLineHeight),
                                $"BRK {car.Brake}", car.HandbrakeOn
                                    ? Styles.carDetailWarning : Styles.carDetail);
                    }
                }
            }

            GUI.EndScrollView();
            GUILayout.EndVertical();
        }

        private static void DrawCompactCarList(IReadOnlyList<string> carIds,
            Settings.TrainInfoSettings settings)
        {
            var groups = new List<List<CarDetails>>();
            var groupIndexes = new Dictionary<string, int>();

            for (var index = 0; index < carIds.Count; index++)
            {
                var car = index < details.Count && details[index].Id == carIds[index]
                    ? details[index]
                    : new CarDetails(carIds[index], "—", "—", "—", "—", float.NaN, false);
                var key = settings.groupCarsByJob
                    ? car.Job == "—" ? "\0NO_JOB" : car.Job
                    : $"\0CAR_{index}";
                if (!groupIndexes.TryGetValue(key, out var groupIndex))
                {
                    groupIndex = groups.Count;
                    groupIndexes.Add(key, groupIndex);
                    groups.Add(new List<CarDetails>());
                }

                groups[groupIndex].Add(car);
            }

            var availableWidth = Mathf.Max(160f, Screen.width - Main.settings.hudPosition.x - 24f);
            var listWidth = Mathf.Min(CompactListMaxWidth, availableWidth);
            var jobStyle = compactJobStyle;
            if (jobStyle == null)
            {
                jobStyle = new GUIStyle(Styles.carDetail)
                {
                    alignment = TextAnchor.MiddleLeft,
                    clipping = TextClipping.Clip,
                    fontSize = 9,
                    margin = new RectOffset(0, 0, 0, 0),
                    padding = new RectOffset(0, 0, 0, 0),
                    wordWrap = false,
                };
                compactJobStyle = jobStyle;
            }
            var carsStyle = compactCarsStyle;
            if (carsStyle == null)
            {
                carsStyle = new GUIStyle(Styles.carDetail)
                {
                    alignment = TextAnchor.MiddleLeft,
                    clipping = TextClipping.Clip,
                    fontSize = 9,
                    margin = new RectOffset(0, 0, 0, 0),
                    padding = new RectOffset(0, 0, 0, 0),
                    wordWrap = true,
                };
                compactCarsStyle = carsStyle;
            }

            GUILayout.BeginVertical(GUILayout.Width(listWidth));
            GUILayout.BeginHorizontal(GUILayout.Width(listWidth));
            GUILayout.Label($"CARS  {carIds.Count}", Styles.subHeader);
            GUILayout.FlexibleSpace();
            GUILayout.Label("COMPACT", Styles.panelTag);
            GUILayout.EndHorizontal();

            foreach (var group in groups)
            {
                var job = group[0].Job == "—" ? "NO JOB" : $"JOB {group[0].Job}";
                var destinations = new List<string>();
                if (settings.showCarDestinations)
                {
                    foreach (var car in group)
                    {
                        if (car.Destination != "—" && !destinations.Contains(car.Destination))
                            destinations.Add(car.Destination);
                    }
                }

                var carText = string.Join("  ", group.Select(car => FormatCompactCar(car, settings)));
                var jobWidth = settings.showCarJobs ? 80f : 0f;
                var destinationWidth = settings.showCarDestinations ? 80f : 0f;
                var carTextWidth = Mathf.Max(30f, listWidth - jobWidth - destinationWidth - 16f);
                var destination = destinations.Count == 0 ? "—" : string.Join(", ", destinations);
                var rowHeight = Mathf.Max(CompactRowHeight,
                    carsStyle.CalcHeight(new GUIContent(carText), carTextWidth));
                if (settings.showCarJobs)
                    rowHeight = Mathf.Max(rowHeight, jobStyle.CalcHeight(new GUIContent(job), 74f));
                if (settings.showCarDestinations)
                    rowHeight = Mathf.Max(rowHeight,
                        jobStyle.CalcHeight(new GUIContent(destination), 74f));
                var rowRect = GUILayoutUtility.GetRect(listWidth, rowHeight,
                    GUILayout.Width(listWidth), GUILayout.Height(rowHeight));
                GUI.Box(rowRect, GUIContent.none, Styles.carCard);
                var textX = rowRect.x + 4f;
                var textWidth = rowRect.width - 8f;
                if (settings.showCarJobs)
                {
                    GUI.Label(new Rect(textX, rowRect.y, 74f, rowRect.height), job, jobStyle);
                    textX += jobWidth;
                    textWidth -= jobWidth;
                }

                if (settings.showCarDestinations)
                {
                    GUI.Label(new Rect(textX, rowRect.y, 74f, rowRect.height),
                        destination, jobStyle);
                    textX += destinationWidth;
                    textWidth -= destinationWidth;
                }

                GUI.Label(new Rect(textX, rowRect.y, Mathf.Max(0f, textWidth), rowRect.height),
                    carText, carsStyle);
            }

            GUILayout.EndVertical();
        }

        private static string FormatCompactCar(CarDetails car, Settings.TrainInfoSettings settings)
        {
            var name = FormatCarName(car.Id).Replace('\n', ' ');
            if (!settings.compactCarDetails)
                return name;

            var carDetails = new List<string>();
            if (settings.showCarStress)
                carDetails.Add($"S:{car.Stress}");
            if (settings.showCarBrakeStatus)
            {
                var brake = car.Brake
                    .Replace(" bar  F ", "bar/")
                    .Replace(" %", "%");
                carDetails.Add($"B:{brake}");
            }
            return carDetails.Count == 0 ? name : $"{name} ({string.Join(",", carDetails)})";
        }

        private static string FormatCarName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "UNNAMED";

            // Support both actual line breaks and the literal escaped form
            // commonly used in vehicle naming schemes: "L-001\\nBOXCAR".
            return name
                .Replace("\\r\\n", "\n")
                .Replace("\\n", "\n")
                .Replace("\r\n", "\n")
                .Replace('\r', '\n');
        }
    }
}