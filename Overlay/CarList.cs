using System;
using System.Collections.Generic;
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
        private static Vector2 scrollPosition;
        private static List<CarDetails> details = new List<CarDetails>();

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
                    stressPercent = car.stress.derailBuildUp / threshold * 100f;
                    stress = $"{stressPercent:F0}%";
                }

                if ((settings.showCarJobs || settings.showCarDestinations)
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
