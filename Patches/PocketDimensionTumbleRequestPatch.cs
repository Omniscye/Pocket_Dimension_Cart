namespace Empress.PocketDimensionCart;

[HarmonyPatch(typeof(PlayerTumble), nameof(PlayerTumble.TumbleRequest))]
internal static class PocketDimensionTumbleRequestPatch
{
    private static bool Prefix(PlayerTumble __instance, bool _isTumbling)
    {
        return !_isTumbling || !BlockPocketRoomTumble(__instance);
    }

    internal static bool BlockPocketRoomTumble(PlayerTumble tumble)
    {
        PlayerAvatar? player = PocketDimensionCartAccess.GetTumbleOwner(tumble);
        if (!PocketDimensionCartRuntime.PlayerInsideAnyPocketRoom(player))
        {
            return false;
        }

        PocketDimensionCartAccess.SuppressPocketRoomMovement(PlayerController.instance, player!);
        return true;
    }
}
