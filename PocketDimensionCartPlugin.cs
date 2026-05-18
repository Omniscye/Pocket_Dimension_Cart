namespace Empress.PocketDimensionCart;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class PocketDimensionCartPlugin : BaseUnityPlugin
{
    public const string PluginGuid = "empress.repo.pocketdimensioncart";
    public const string PluginName = "Pocket Dimension Cart";
    public const string PluginVersion = "1.3.0";

    internal static PocketDimensionCartPlugin Instance = null!;
    internal static ConfigEntry<KeyCode> ActionKey = null!;
    internal static ConfigEntry<float> HoldSeconds = null!;
    internal static ConfigEntry<float> EjectStaggerSeconds = null!;
    internal static ConfigEntry<float> EjectCooldownSeconds = null!;
    internal static ConfigEntry<float> EjectedProtectionSeconds = null!;
    internal static ConfigEntry<bool> EnterByHoppingInCart = null!;

    private Harmony _harmony = null!;

    private void Awake()
    {
        Instance = this;
        ActionKey = Config.Bind("Input", "ActionKey", KeyCode.X, "Press near a cart to eject one stored valuable. Hold to eject all stored valuables.");
        HoldSeconds = Config.Bind("Input", "HoldSeconds", 0.55f, "How long the action key must be held before eject-all starts.");
        EjectStaggerSeconds = Config.Bind("Cart", "EjectStaggerSeconds", 0.18f, "Delay between each valuable when ejecting all.");
        EjectCooldownSeconds = Config.Bind("Cart", "EjectCooldownSeconds", 2f, "How long an ejected valuable is blocked from being absorbed by a cart again.");
        EjectedProtectionSeconds = Config.Bind("Cart", "EjectedProtectionSeconds", 2f, "Temporary damage protection after a valuable exits the pocket dimension.");
        EnterByHoppingInCart = Config.Bind("Cart", "EnterByHoppingInCart", true, "Allows players to enter the pocket dimension by standing inside the cart.");

        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll();
        Logger.LogInfo($"{PluginName} v{PluginVersion} loaded.");
    }
}
