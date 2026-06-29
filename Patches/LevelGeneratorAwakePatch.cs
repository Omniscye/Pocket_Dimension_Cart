namespace Empress.PocketDimensionCart;

[HarmonyPatch(typeof(LevelGenerator), nameof(LevelGenerator.Awake))]
internal static class LevelGeneratorAwakePatch
{
    private static void Prefix()
    {
        PocketDimensionCartRuntime.ResetLevelState();
    }
}
