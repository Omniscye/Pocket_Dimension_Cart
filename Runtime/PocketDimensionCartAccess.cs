namespace Empress.PocketDimensionCart;

internal static class PocketDimensionCartAccess
{
    private static readonly AccessTools.FieldRef<ItemEquippable, ItemEquippable.ItemState>? ItemCurrentState = CreateItemStateRef("currentState") ?? CreateItemStateRef("_currentState");
    private static readonly AccessTools.FieldRef<ItemEquippable, bool>? ItemIsEquipped = CreateBoolRef("isEquipped");
    private static readonly AccessTools.FieldRef<ItemEquippable, bool>? ItemIsEquipping = CreateBoolRef("isEquipping");
    private static readonly AccessTools.FieldRef<ItemEquippable, bool>? ItemIsUnequipping = CreateBoolRef("isUnequipping");
    private static readonly AccessTools.FieldRef<ItemEquippable, InventorySpot>? ItemEquippedSpot = CreateInventorySpotRef("equippedSpot");
    private static readonly AccessTools.FieldRef<PlayerAvatar, PlayerTumble>? PlayerTumbleRef = CreatePlayerTumbleRef("tumble");
    private static readonly AccessTools.FieldRef<PlayerAvatar, bool>? PlayerIsLocalRef = CreatePlayerBoolRef("isLocal");
    private static readonly AccessTools.FieldRef<PlayerAvatar, bool>? PlayerIsSprintingRef = CreatePlayerBoolRef("isSprinting");
    private static readonly AccessTools.FieldRef<PlayerTumble, PlayerAvatar>? TumblePlayerRef = CreateTumblePlayerRef("playerAvatar");
    private static readonly AccessTools.FieldRef<PlayerTumble, bool>? TumbleIsTumblingRef = CreateTumbleBoolRef("isTumbling");
    private static readonly AccessTools.FieldRef<PlayerController, PlayerAvatar>? ControllerPlayerRef = CreateControllerPlayerRef("playerAvatarScript");
    private static readonly AccessTools.FieldRef<PlayerController, bool>? ControllerSprintingRef = CreateControllerBoolRef("sprinting");
    private static readonly AccessTools.FieldRef<PlayerController, bool>? ControllerToggleSprintRef = CreateControllerBoolRef("toggleSprint");
    private static readonly AccessTools.FieldRef<PlayerController, float>? ControllerTumbleInputDisableRef = CreateControllerFloatRef("tumbleInputDisableTimer");
    private static readonly AccessTools.FieldRef<PlayerController, float>? ControllerSprintedTimerRef = CreateControllerFloatRef("SprintedTimer");
    private static readonly AccessTools.FieldRef<PlayerController, float>? ControllerSprintDrainTimerRef = CreateControllerFloatRef("SprintDrainTimer");
    private static readonly AccessTools.FieldRef<PlayerController, float>? ControllerSprintSpeedLerpRef = CreateControllerFloatRef("SprintSpeedLerp");

    internal static bool IsEquippedOrInInventory(ItemEquippable itemEquippable)
    {
        if (!itemEquippable)
        {
            return false;
        }

        try
        {
            if (itemEquippable.IsEquipped())
            {
                return true;
            }
        }
        catch
        {
        }

        AccessTools.FieldRef<ItemEquippable, ItemEquippable.ItemState>? stateRef = ItemCurrentState;
        if (stateRef != null)
        {
            try
            {
                if (stateRef(itemEquippable) != ItemEquippable.ItemState.Idle)
                {
                    return true;
                }
            }
            catch
            {
            }
        }

        return ReadBool(itemEquippable, ItemIsEquipped)
            || ReadBool(itemEquippable, ItemIsEquipping)
            || ReadBool(itemEquippable, ItemIsUnequipping)
            || ReadEquippedSpot(itemEquippable);
    }

    internal static PlayerTumble? GetTumble(PlayerAvatar player)
    {
        AccessTools.FieldRef<PlayerAvatar, PlayerTumble>? tumbleRef = PlayerTumbleRef;
        if (tumbleRef == null || !player)
        {
            return null;
        }

        try
        {
            return tumbleRef(player);
        }
        catch
        {
            return null;
        }
    }

    internal static bool IsLocal(PlayerAvatar player)
    {
        AccessTools.FieldRef<PlayerAvatar, bool>? isLocalRef = PlayerIsLocalRef;
        if (isLocalRef == null || !player)
        {
            return false;
        }

        try
        {
            return isLocalRef(player);
        }
        catch
        {
            return false;
        }
    }

    internal static PlayerAvatar? GetTumbleOwner(PlayerTumble tumble)
    {
        AccessTools.FieldRef<PlayerTumble, PlayerAvatar>? playerRef = TumblePlayerRef;
        if (playerRef == null || !tumble)
        {
            return null;
        }

        try
        {
            return playerRef(tumble);
        }
        catch
        {
            return null;
        }
    }

    internal static PlayerAvatar? GetControllerPlayer(PlayerController controller)
    {
        AccessTools.FieldRef<PlayerController, PlayerAvatar>? playerRef = ControllerPlayerRef;
        if (playerRef == null || !controller)
        {
            return null;
        }

        try
        {
            return playerRef(controller);
        }
        catch
        {
            return null;
        }
    }

    internal static void SuppressPocketRoomMovement(PlayerController? controller, PlayerAvatar player)
    {
        if (!player)
        {
            return;
        }

        SetPlayerBool(player, PlayerIsSprintingRef, false);

        if (controller)
        {
            SetControllerBool(controller!, ControllerSprintingRef, false);
            SetControllerBool(controller!, ControllerToggleSprintRef, false);
            SetControllerFloat(controller!, ControllerTumbleInputDisableRef, 0.2f, useMax: true);
            SetControllerFloat(controller!, ControllerSprintedTimerRef, 0f, useMax: false);
            SetControllerFloat(controller!, ControllerSprintDrainTimerRef, 0f, useMax: false);
            SetControllerFloat(controller!, ControllerSprintSpeedLerpRef, 0f, useMax: false);
        }

        PlayerTumble? tumble = GetTumble(player);
        if (tumble != null)
        {
            if (ReadTumbleBool(tumble, TumbleIsTumblingRef))
            {
                tumble.TumbleRequest(_isTumbling: false, _playerInput: false);
            }

            tumble.TumbleOverrideTime(0.2f);
            tumble.DisableCustomGravity(0.1f);
            tumble.OverrideDisableTumbleMoveSound(0.2f);
        }
    }

    private static bool ReadBool(ItemEquippable itemEquippable, AccessTools.FieldRef<ItemEquippable, bool>? accessor)
    {
        if (accessor == null)
        {
            return false;
        }

        try
        {
            return accessor(itemEquippable);
        }
        catch
        {
            return false;
        }
    }

    private static bool ReadTumbleBool(PlayerTumble tumble, AccessTools.FieldRef<PlayerTumble, bool>? accessor)
    {
        if (accessor == null)
        {
            return false;
        }

        try
        {
            return accessor(tumble);
        }
        catch
        {
            return false;
        }
    }

    private static void SetPlayerBool(PlayerAvatar player, AccessTools.FieldRef<PlayerAvatar, bool>? accessor, bool value)
    {
        if (accessor == null)
        {
            return;
        }

        try
        {
            accessor(player) = value;
        }
        catch
        {
        }
    }

    private static void SetControllerBool(PlayerController controller, AccessTools.FieldRef<PlayerController, bool>? accessor, bool value)
    {
        if (accessor == null)
        {
            return;
        }

        try
        {
            accessor(controller) = value;
        }
        catch
        {
        }
    }

    private static void SetControllerFloat(PlayerController controller, AccessTools.FieldRef<PlayerController, float>? accessor, float value, bool useMax)
    {
        if (accessor == null)
        {
            return;
        }

        try
        {
            accessor(controller) = useMax ? Mathf.Max(accessor(controller), value) : value;
        }
        catch
        {
        }
    }

    private static bool ReadEquippedSpot(ItemEquippable itemEquippable)
    {
        AccessTools.FieldRef<ItemEquippable, InventorySpot>? accessor = ItemEquippedSpot;
        if (accessor == null)
        {
            return false;
        }

        try
        {
            InventorySpot spot = accessor(itemEquippable);
            return spot;
        }
        catch
        {
            return false;
        }
    }

    private static AccessTools.FieldRef<ItemEquippable, ItemEquippable.ItemState>? CreateItemStateRef(string name)
    {
        try
        {
            return AccessTools.FieldRefAccess<ItemEquippable, ItemEquippable.ItemState>(name);
        }
        catch
        {
            return null;
        }
    }

    private static AccessTools.FieldRef<ItemEquippable, bool>? CreateBoolRef(string name)
    {
        try
        {
            return AccessTools.FieldRefAccess<ItemEquippable, bool>(name);
        }
        catch
        {
            return null;
        }
    }

    private static AccessTools.FieldRef<ItemEquippable, InventorySpot>? CreateInventorySpotRef(string name)
    {
        try
        {
            return AccessTools.FieldRefAccess<ItemEquippable, InventorySpot>(name);
        }
        catch
        {
            return null;
        }
    }

    private static AccessTools.FieldRef<PlayerAvatar, PlayerTumble>? CreatePlayerTumbleRef(string name)
    {
        try
        {
            return AccessTools.FieldRefAccess<PlayerAvatar, PlayerTumble>(name);
        }
        catch
        {
            return null;
        }
    }

    private static AccessTools.FieldRef<PlayerAvatar, bool>? CreatePlayerBoolRef(string name)
    {
        try
        {
            return AccessTools.FieldRefAccess<PlayerAvatar, bool>(name);
        }
        catch
        {
            return null;
        }
    }

    private static AccessTools.FieldRef<PlayerTumble, PlayerAvatar>? CreateTumblePlayerRef(string name)
    {
        try
        {
            return AccessTools.FieldRefAccess<PlayerTumble, PlayerAvatar>(name);
        }
        catch
        {
            return null;
        }
    }

    private static AccessTools.FieldRef<PlayerTumble, bool>? CreateTumbleBoolRef(string name)
    {
        try
        {
            return AccessTools.FieldRefAccess<PlayerTumble, bool>(name);
        }
        catch
        {
            return null;
        }
    }

    private static AccessTools.FieldRef<PlayerController, PlayerAvatar>? CreateControllerPlayerRef(string name)
    {
        try
        {
            return AccessTools.FieldRefAccess<PlayerController, PlayerAvatar>(name);
        }
        catch
        {
            return null;
        }
    }

    private static AccessTools.FieldRef<PlayerController, bool>? CreateControllerBoolRef(string name)
    {
        try
        {
            return AccessTools.FieldRefAccess<PlayerController, bool>(name);
        }
        catch
        {
            return null;
        }
    }

    private static AccessTools.FieldRef<PlayerController, float>? CreateControllerFloatRef(string name)
    {
        try
        {
            return AccessTools.FieldRefAccess<PlayerController, float>(name);
        }
        catch
        {
            return null;
        }
    }
}
