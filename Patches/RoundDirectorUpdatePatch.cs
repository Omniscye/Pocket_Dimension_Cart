namespace Empress.PocketDimensionCart;

[HarmonyPatch(typeof(RoundDirector), nameof(RoundDirector.Update))]
internal static class RoundDirectorUpdatePatch
{
    private static void Prefix(RoundDirector __instance)
    {
        PocketDimensionCartRuntime.SyncExtractionHaul(__instance);
    }
}
