namespace Empress.PocketDimensionCart;

[HarmonyPatch(typeof(PhysGrabCart), nameof(PhysGrabCart.Start))]
internal static class PhysGrabCartStartPatch
{
    private static void Postfix(PhysGrabCart __instance)
    {
        if (!__instance || !PocketDimensionCartRuntime.CanAttachToCart)
        {
            return;
        }

        if (!__instance.GetComponent<PocketDimensionCartController>())
        {
            __instance.gameObject.AddComponent<PocketDimensionCartController>();
        }
    }
}
