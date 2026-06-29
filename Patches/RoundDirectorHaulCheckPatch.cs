namespace Empress.PocketDimensionCart;

[HarmonyPatch(typeof(RoundDirector), nameof(RoundDirector.HaulCheck))]
internal static class RoundDirectorHaulCheckPatch
{
    private static void Postfix(RoundDirector __instance)
    {
        int addedValue = PocketDimensionCartRuntime.SyncExtractionHaul(__instance);
        if (addedValue > 0)
        {
            __instance.currentHaul += addedValue;
        }
    }
}
