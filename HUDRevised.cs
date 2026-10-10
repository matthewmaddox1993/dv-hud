using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;

namespace DvMod.HUDRevised
{
    [EnableReloading]
    public static class Main
    {
        public static bool enabled = true;
        public static Settings settings = new Settings();
        public static UnityModManager.ModEntry? mod;
        public static GameObject? behaviourRoot;
        private static bool dvSignalsStatusLogged;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            mod = modEntry;
            enabled = true;
            dvSignalsStatusLogged = false;
            TrainSetUtils.ResetSignalCache();

            try
            {
                var loaded = Settings.Load<Settings>(modEntry);
                if (loaded.version == modEntry.Info.Version)
                    settings = loaded;
                else
                    settings = new Settings();
            }
            catch
            {
                settings = new Settings();
            }
            var harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll();

            modEntry.OnGUI = OnGui;
            modEntry.OnSaveGUI = OnSaveGui;
            modEntry.OnToggle = OnToggle;
            modEntry.OnUnload = OnUnload;

            Commands.Register();
            DataProviders.Register();

            // Create the host immediately. Waiting for the world-load callback
            // can miss save-game loading, depending on when UMM initializes.
            EnsureOverlay();

            WorldStreamingInit.LoadingFinished -= OnLoadingFinished;
            if (WorldStreamingInit.IsLoaded)
                OnLoadingFinished();
            else
                WorldStreamingInit.LoadingFinished += OnLoadingFinished;

            return true;
        }

        private static void OnGui(UnityModManager.ModEntry modEntry)
        {
            settings.Draw();
            if (GUILayout.Button("Reset position", GUILayout.ExpandWidth(false)))
            {
                settings.hudPosition = Settings.defaultPosition;
                Overlay.instance?.ResetPosition();
            }
        }

        private static void OnSaveGui(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (value != enabled)
            {
                enabled = value;
            }
            return true;
        }

        private static void OnLoadingFinished()
        {
            EnsureOverlay();
            Overlay.instance?.BeginSignalStatusCheck();
        }

        private static void EnsureOverlay()
        {
            if (behaviourRoot != null)
                return;

            behaviourRoot = new GameObject();
            Object.DontDestroyOnLoad(behaviourRoot);
            behaviourRoot.AddComponent<Overlay>();
        }

        public static bool TryLogDvSignalsStatus()
        {
            if (dvSignalsStatusLogged || mod == null)
                return true;

            if (!TrainSetUtils.IsDvSignalsInstalled)
            {
                mod.Logger.Log("DVSignals not detected; signal status will not be displayed.");
                dvSignalsStatusLogged = true;
                return true;
            }

            if (TrainSetUtils.TryGetSignalCount(out var signalCount) && signalCount > 0)
            {
                mod.Logger.Log($"DVSignals detected; {signalCount} signal(s) found.");
                dvSignalsStatusLogged = true;
                return true;
            }

            return false;
        }

        public static void LogNoDvSignalsFound()
        {
            if (dvSignalsStatusLogged || mod == null)
                return;

            dvSignalsStatusLogged = true;
            mod.Logger.Log("DVSignals detected, but no signals were found; signal status will not be displayed.");
        }

        private static bool OnUnload(UnityModManager.ModEntry modEntry)
        {
            WorldStreamingInit.LoadingFinished -= OnLoadingFinished;

            if (behaviourRoot != null)
                Object.Destroy(behaviourRoot);
            behaviourRoot = null;
            Overlay.instance = null;
            TrainSetUtils.ResetSignalCache();
            dvSignalsStatusLogged = false;
            var harmony = new Harmony(modEntry.Info.Id);
            harmony.UnpatchAll(modEntry.Info.Id);
            return true;
        }

        public static void DebugLog(string message)
        {
            if (settings.enableLogging && mod != null)
                mod.Logger.Log(message);
        }
    }
}
