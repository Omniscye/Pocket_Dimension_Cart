namespace Empress.PocketDimensionCart;

[HarmonyPatch(typeof(ExtractionPoint), nameof(ExtractionPoint.StateExtracting))]
internal static class ExtractionPointStateExtractingPatch
{
    private static void Prefix()
    {
        PocketDimensionCartRuntime.SyncExtractionHaul(RoundDirector.instance);
    }
}
