namespace Empress.PocketDimensionCart;

[HarmonyPatch(typeof(ValueScreen), nameof(ValueScreen.UpdateValue))]
internal static class ValueScreenUpdateValuePatch
{
    private static void Prefix(ValueScreen __instance, ref int newValue)
    {
        if (!__instance || !PocketDimensionCartRuntime.IsRealLevel)
        {
            return;
        }

        PocketDimensionCartController? controller = PocketDimensionCartRuntime.GetControllerForValueScreen(__instance);
        if (controller != null)
        {
            newValue = controller.AddStoredValueToDisplay(newValue);
        }
    }
}
