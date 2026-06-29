namespace Empress.PocketDimensionCart;

[HarmonyPatch(typeof(PhysGrabObjectImpactDetector), nameof(PhysGrabObjectImpactDetector.OnTriggerStay))]
internal static class PhysGrabObjectImpactDetectorOnTriggerStayPatch
{
    private static void Postfix(PhysGrabObjectImpactDetector __instance, Collider other)
    {
        if (!__instance || !other || !PocketDimensionCartRuntime.IsRealLevel || !SemiFunc.IsMasterClientOrSingleplayer())
        {
            return;
        }

        if (!other.CompareTag("Cart") || !__instance.physGrabObject || !__instance.currentCart)
        {
            return;
        }

        if (!PocketDimensionCartController.CanStoreObject(__instance.physGrabObject))
        {
            return;
        }

        PocketDimensionCartController controller = __instance.currentCart.GetComponent<PocketDimensionCartController>();
        if (!controller)
        {
            return;
        }

        controller.StoreValuable(__instance.physGrabObject);
    }
}
