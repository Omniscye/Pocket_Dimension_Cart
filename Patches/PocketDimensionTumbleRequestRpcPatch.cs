namespace Empress.PocketDimensionCart;

[HarmonyPatch(typeof(PlayerTumble), nameof(PlayerTumble.TumbleRequestRPC))]
internal static class PocketDimensionTumbleRequestRpcPatch
{
    private static bool Prefix(PlayerTumble __instance, bool _isTumbling)
    {
        return !_isTumbling || !PocketDimensionTumbleRequestPatch.BlockPocketRoomTumble(__instance);
    }
}
