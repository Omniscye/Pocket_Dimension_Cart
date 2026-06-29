namespace Empress.PocketDimensionCart;

[HarmonyPatch(typeof(ExtractionPoint), nameof(ExtractionPoint.HaulInternalStatsUpdate))]
internal static class ExtractionPointHaulInternalStatsUpdatePatch
{
    private static void Postfix(ExtractionPoint __instance)
    {
        int addedValue = PocketDimensionCartRuntime.SyncExtractionHaul(RoundDirector.instance);
        if (addedValue > 0)
        {
            __instance.haulCurrent += addedValue;
        }
    }
}
