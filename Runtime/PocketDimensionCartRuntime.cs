namespace Empress.PocketDimensionCart;

internal static class PocketDimensionCartRuntime
{
    private static readonly List<PocketDimensionCartController> Controllers = new();
    private static readonly Dictionary<int, PocketDimensionCartController> StoredOwners = new();
    private static readonly Dictionary<int, float> AbsorbCooldowns = new();
    private static readonly Dictionary<int, PhysGrabObject> LocalPhysGrabObjects = new();
    private static int _nextRoomIndex;

    internal static bool IsRealLevel
    {
        get
        {
            try
            {
                if (!RunManager.instance || !LevelGenerator.Instance || !LevelGenerator.Instance.Generated)
                {
                    return false;
                }

                if (SemiFunc.MenuLevel() || SemiFunc.RunIsShop() || SemiFunc.RunIsTutorial())
                {
                    return false;
                }

                return SemiFunc.RunIsLevel();
            }
            catch
            {
                return false;
            }
        }
    }

    internal static bool CanAttachToCart
    {
        get
        {
            try
            {
                if (!RunManager.instance || SemiFunc.MenuLevel() || SemiFunc.RunIsShop() || SemiFunc.RunIsTutorial())
                {
                    return false;
                }

                return SemiFunc.RunIsLevel();
            }
            catch
            {
                return false;
            }
        }
    }

    internal static int ReserveRoomIndex()
    {
        _nextRoomIndex++;
        return _nextRoomIndex;
    }

    internal static void Register(PocketDimensionCartController controller)
    {
        if (!Controllers.Contains(controller))
        {
            Controllers.Add(controller);
        }
    }

    internal static void Unregister(PocketDimensionCartController controller)
    {
        Controllers.Remove(controller);
        foreach (int viewId in StoredOwners.Where(pair => pair.Value == controller).Select(pair => pair.Key).ToArray())
        {
            StoredOwners.Remove(viewId);
        }
    }

    internal static void ResetLevelState()
    {
        Controllers.Clear();
        StoredOwners.Clear();
        AbsorbCooldowns.Clear();
        LocalPhysGrabObjects.Clear();
        _nextRoomIndex = 0;
    }

    internal static bool IsStored(PhysGrabObject physGrabObject)
    {
        int viewId = GetViewId(physGrabObject);
        return viewId != 0 && StoredOwners.ContainsKey(viewId);
    }

    internal static bool CanAbsorb(PhysGrabObject physGrabObject)
    {
        int viewId = GetViewId(physGrabObject);
        if (viewId == 0)
        {
            return true;
        }

        if (StoredOwners.ContainsKey(viewId))
        {
            return false;
        }

        if (AbsorbCooldowns.TryGetValue(viewId, out float until) && Time.time < until)
        {
            return false;
        }

        if (AbsorbCooldowns.ContainsKey(viewId))
        {
            AbsorbCooldowns.Remove(viewId);
        }

        return true;
    }

    internal static void MarkStored(PhysGrabObject physGrabObject, PocketDimensionCartController owner)
    {
        int viewId = GetViewId(physGrabObject);
        if (viewId != 0)
        {
            StoredOwners[viewId] = owner;
        }
    }

    internal static void MarkReleased(PhysGrabObject physGrabObject, float cooldown)
    {
        int viewId = GetViewId(physGrabObject);
        if (viewId == 0)
        {
            return;
        }

        StoredOwners.Remove(viewId);
        AbsorbCooldowns[viewId] = Time.time + Mathf.Max(0.1f, cooldown);
    }

    internal static PocketDimensionCartController? GetOwner(PhysGrabObject physGrabObject)
    {
        int viewId = GetViewId(physGrabObject);
        if (viewId == 0)
        {
            return null;
        }

        StoredOwners.TryGetValue(viewId, out PocketDimensionCartController owner);
        return owner;
    }

    internal static PocketDimensionCartController? GetControllerForValueScreen(ValueScreen valueScreen)
    {
        if (!valueScreen)
        {
            return null;
        }

        foreach (PocketDimensionCartController controller in Controllers)
        {
            if (controller && controller.UsesValueScreen(valueScreen))
            {
                return controller;
            }
        }

        return null;
    }

    internal static int SyncExtractionHaul(RoundDirector roundDirector)
    {
        if (!roundDirector || !IsRealLevel || SemiFunc.RunIsShop())
        {
            return 0;
        }

        int addedValue = 0;

        if (SemiFunc.IsMasterClientOrSingleplayer())
        {
            for (int i = roundDirector.dollarHaulList.Count - 1; i >= 0; i--)
            {
                GameObject haulObject = roundDirector.dollarHaulList[i];
                if (!haulObject)
                {
                    continue;
                }

                PhysGrabObject physGrabObject = haulObject.GetComponent<PhysGrabObject>();
                PocketDimensionCartController? owner = physGrabObject ? GetOwner(physGrabObject) : null;
                if (owner != null && !owner.CartInExtractionPoint())
                {
                    ValuableObject valuableObject = haulObject.GetComponent<ValuableObject>();
                    if (valuableObject)
                    {
                        roundDirector.dollarHaulList.RemoveAt(i);
                        valuableObject.RemoveFromDollarHaulList();
                    }
                }
            }

            for (int i = roundDirector.valuableBoxHaulList.Count - 1; i >= 0; i--)
            {
                ItemValuableBox valuableBox = roundDirector.valuableBoxHaulList[i];
                if (!valuableBox)
                {
                    continue;
                }

                PhysGrabObject physGrabObject = valuableBox.GetComponent<PhysGrabObject>();
                PocketDimensionCartController? owner = physGrabObject ? GetOwner(physGrabObject) : null;
                if (owner != null && !owner.CartInExtractionPoint())
                {
                    roundDirector.valuableBoxHaulList.RemoveAt(i);
                    valuableBox.RemoveFromExtractionHaul();
                }
            }

            foreach (PocketDimensionCartController controller in Controllers.ToArray())
            {
                if (!controller)
                {
                    Controllers.Remove(controller);
                    continue;
                }

                addedValue += controller.SyncStoredValuablesForExtraction(roundDirector);
            }
        }

        return addedValue;
    }

    internal static bool PlayerInsideAnyPocketRoom(PlayerAvatar? player)
    {
        if (player == null)
        {
            return false;
        }

        for (int i = Controllers.Count - 1; i >= 0; i--)
        {
            PocketDimensionCartController controller = Controllers[i];
            if (!controller)
            {
                Controllers.RemoveAt(i);
                continue;
            }

            if (controller.PlayerInsidePocketRoom(player))
            {
                return true;
            }
        }

        return false;
    }

    internal static int GetViewId(PhysGrabObject physGrabObject)
    {
        if (!physGrabObject)
        {
            return 0;
        }

        if (SemiFunc.IsMultiplayer() && physGrabObject.photonView && physGrabObject.photonView.ViewID != 0)
        {
            return physGrabObject.photonView.ViewID;
        }

        int instanceId = physGrabObject.GetInstanceID();
        if (instanceId == 0)
        {
            return 0;
        }

        int localId = instanceId == int.MinValue ? instanceId : -Mathf.Abs(instanceId);
        LocalPhysGrabObjects[localId] = physGrabObject;
        return localId;
    }

    internal static PhysGrabObject? ResolvePhysGrabObject(int viewId)
    {
        if (viewId == 0)
        {
            return null;
        }

        if (viewId < 0)
        {
            if (LocalPhysGrabObjects.TryGetValue(viewId, out PhysGrabObject physGrabObject) && physGrabObject)
            {
                return physGrabObject;
            }

            LocalPhysGrabObjects.Remove(viewId);
            return null;
        }

        PhotonView view = PhotonView.Find(viewId);
        if (!view)
        {
            return null;
        }

        return view.GetComponent<PhysGrabObject>();
    }

    internal static void TeleportPhysGrabObject(PhysGrabObject physGrabObject, Vector3 position, Quaternion rotation)
    {
        if (!physGrabObject)
        {
            return;
        }

        if (SemiFunc.IsMultiplayer() && physGrabObject.photonView && physGrabObject.photonView.ViewID != 0)
        {
            physGrabObject.photonView.RPC("SetPositionRPC", RpcTarget.All, position, rotation);
        }
        else
        {
            physGrabObject.Teleport(position, rotation);
        }

        Rigidbody rb = physGrabObject.rb;
        if (rb)
        {
            rb.position = position;
            rb.rotation = rotation;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.WakeUp();
        }
    }

    internal static void ProtectValuable(PhysGrabObject physGrabObject, float seconds)
    {
        if (!physGrabObject)
        {
            return;
        }

        PocketDimensionStoredValuable marker = physGrabObject.GetComponent<PocketDimensionStoredValuable>();
        if (!marker)
        {
            marker = physGrabObject.gameObject.AddComponent<PocketDimensionStoredValuable>();
        }

        marker.Protect(seconds);
    }
}
