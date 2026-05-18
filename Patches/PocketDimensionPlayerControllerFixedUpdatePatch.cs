namespace Empress.PocketDimensionCart;

[HarmonyPatch(typeof(PlayerController), nameof(PlayerController.FixedUpdate))]
internal static class PocketDimensionPlayerControllerFixedUpdatePatch
{
    private static void Postfix(PlayerController __instance)
    {
        PlayerAvatar? player = PocketDimensionCartAccess.GetControllerPlayer(__instance);
        if (PocketDimensionCartRuntime.PlayerInsideAnyPocketRoom(player))
        {
            PocketDimensionCartAccess.SuppressPocketRoomMovement(__instance, player!);
        }
    }
}
