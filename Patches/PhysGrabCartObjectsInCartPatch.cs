namespace Empress.PocketDimensionCart;

[HarmonyPatch(typeof(PhysGrabCart), nameof(PhysGrabCart.ObjectsInCart))]
internal static class PhysGrabCartObjectsInCartPatch
{
    private static void Prefix(PhysGrabCart __instance, out bool __state)
    {
        __state = false;
        if (!__instance || !PocketDimensionCartRuntime.IsRealLevel)
        {
            return;
        }

        if (SemiFunc.PlayerNearestDistance(__instance.transform.position) > 12f || __instance.objectInCartCheckTimer > 0f)
        {
            return;
        }

        __state = true;
    }

    private static void Postfix(PhysGrabCart __instance, bool __state)
    {
        if (!__state || !__instance)
        {
            return;
        }

        PocketDimensionCartController controller = __instance.GetComponent<PocketDimensionCartController>();
        if (controller)
        {
            controller.AddStoredContentsToCartReadout();
        }
    }
}
