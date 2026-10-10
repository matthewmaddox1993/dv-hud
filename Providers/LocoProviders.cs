namespace DvMod.HUDRevised
{
    // The 99.7 game build moved the locomotive simulation types out of
    // Assembly-CSharp and changed their public API. Keep the optional
    // locomotive providers disabled until those APIs are stable; the HUD's
    // car, brake, job, and track providers remain fully available.
    internal static class LocoProviders
    {
        public static void Register()
        {
            Main.DebugLog("Locomotive providers are disabled for Derail Valley 99.7 API compatibility.");
        }
    }
}
