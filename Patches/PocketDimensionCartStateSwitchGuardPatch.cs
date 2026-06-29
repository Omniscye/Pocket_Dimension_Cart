namespace Empress.PocketDimensionCart;

[HarmonyPatch(typeof(PhysGrabCart), nameof(PhysGrabCart.StateSwitchRPC))]
internal static class PocketDimensionCartStateSwitchGuardPatch
{
    private static bool Prefix(PhysGrabCart __instance, PhysGrabCart.State _state)
    {
        if (!__instance)
        {
            return false;
        }

        if (__instance.cartMesh && __instance.grabMaterial != null && __instance.capsuleColliders != null && __instance.cartInside != null && __instance.soundLocked != null && __instance.soundDragged != null && __instance.soundHandled != null)
        {
            return true;
        }

        __instance.currentState = _state;
        __instance.previousState = _state;
        return false;
    }
}
