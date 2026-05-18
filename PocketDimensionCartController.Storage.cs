namespace Empress.PocketDimensionCart;

internal sealed partial class PocketDimensionCartController
{
    public bool OwnsStoredValuable(PhysGrabObject physGrabObject)
    {
        int viewId = PocketDimensionCartRuntime.GetViewId(physGrabObject);
        return viewId != 0 && _storedViewIds.Contains(viewId);
    }

    public void StoreValuable(PhysGrabObject physGrabObject)
    {
        if (!PocketDimensionCartRuntime.IsRealLevel || !SemiFunc.IsMasterClientOrSingleplayer() || !CartCanOpenPocket() || !physGrabObject || !physGrabObject.GetComponent<ValuableObject>())
        {
            return;
        }

        if (physGrabObject.grabbed || physGrabObject.playerGrabbing.Count > 0 || !PocketDimensionCartRuntime.CanAbsorb(physGrabObject))
        {
            return;
        }

        int viewId = PocketDimensionCartRuntime.GetViewId(physGrabObject);
        if (viewId == 0 || _storedViewIds.Contains(viewId))
        {
            return;
        }

        _storedViewIds.Add(viewId);
        PocketDimensionCartRuntime.MarkStored(physGrabObject, this);

        PocketDimensionStoredValuable marker = physGrabObject.GetComponent<PocketDimensionStoredValuable>();
        if (!marker)
        {
            marker = physGrabObject.gameObject.AddComponent<PocketDimensionStoredValuable>();
        }

        marker.SetStored(this);
        ResetCartState(physGrabObject);
        PocketDimensionCartRuntime.ProtectValuable(physGrabObject, 0.75f);
        PocketDimensionCartRuntime.TeleportPhysGrabObject(physGrabObject, GetStoragePosition(_storedViewIds.Count - 1), Quaternion.identity);
        SyncStoredTotals();
    }

    public void EjectStoredFromRoom(PhysGrabObject physGrabObject)
    {
        if (!SemiFunc.IsMasterClientOrSingleplayer() || !OwnsStoredValuable(physGrabObject))
        {
            return;
        }

        EjectValuable(physGrabObject);
    }

    private void EjectOne(PlayerAvatar? player)
    {
        if (!CanPlayerUseCartOnHost(player))
        {
            return;
        }

        PhysGrabObject? valuable = FindStoredValuable(preferReleased: true);
        if (valuable != null)
        {
            EjectValuable(valuable);
        }
    }

    private void EjectAll(PlayerAvatar? player)
    {
        if (!CanPlayerUseCartOnHost(player) || _ejectAllRunning)
        {
            return;
        }

        StartCoroutine(EjectAllRoutine());
    }

    private IEnumerator EjectAllRoutine()
    {
        _ejectAllRunning = true;
        while (_storedViewIds.Count > 0)
        {
            PhysGrabObject? valuable = FindStoredValuable(preferReleased: true);
            if (valuable == null)
            {
                CleanupStoredList();
                break;
            }

            EjectValuable(valuable);
            yield return new WaitForSeconds(Mathf.Max(0.05f, PocketDimensionCartPlugin.EjectStaggerSeconds.Value));
        }

        _ejectAllRunning = false;
    }

    private void EjectValuable(PhysGrabObject physGrabObject)
    {
        if (!physGrabObject)
        {
            return;
        }

        int viewId = PocketDimensionCartRuntime.GetViewId(physGrabObject);
        _storedViewIds.Remove(viewId);
        PocketDimensionCartRuntime.MarkReleased(physGrabObject, PocketDimensionCartPlugin.EjectCooldownSeconds.Value);
        SyncStoredTotals();

        int releaseViewId = viewId > 0 ? viewId : -1;
        foreach (PhysGrabber grabber in physGrabObject.playerGrabbing.ToList())
        {
            if (grabber)
            {
                grabber.ReleaseObject(releaseViewId, 0.35f);
            }
        }

        physGrabObject.OverrideGrabDisable(0.35f);

        PocketDimensionStoredValuable marker = physGrabObject.GetComponent<PocketDimensionStoredValuable>();
        if (marker)
        {
            marker.SetReleased(PocketDimensionCartPlugin.EjectedProtectionSeconds.Value);
        }

        ResetCartState(physGrabObject);
        GetCartExitPose(out Vector3 exitPosition, out Quaternion exitRotation);
        PocketDimensionCartRuntime.ProtectValuable(physGrabObject, PocketDimensionCartPlugin.EjectedProtectionSeconds.Value);
        PocketDimensionCartRuntime.TeleportPhysGrabObject(physGrabObject, exitPosition, exitRotation);

        if (physGrabObject.rb)
        {
            physGrabObject.rb.velocity = _cart.transform.up * 1.4f + _cart.transform.forward * 0.7f;
            physGrabObject.rb.angularVelocity = Vector3.zero;
            physGrabObject.rb.WakeUp();
        }
    }

    public void AddStoredContentsToCartReadout()
    {
        if (!_cart)
        {
            return;
        }

        if (SemiFunc.IsMasterClientOrSingleplayer())
        {
            RefreshStoredTotals();
        }

        if (_syncedStoredCount <= 0 && _syncedStoredValue <= 0)
        {
            return;
        }

        _cart.itemsInCartCount += _syncedStoredCount;
        _cart.haulCurrent += _syncedStoredValue;
    }

    public int AddStoredValueToDisplay(int baseValue)
    {
        if (SemiFunc.IsMasterClientOrSingleplayer())
        {
            RefreshStoredTotals();
        }

        return Mathf.Max(0, baseValue + _syncedStoredValue);
    }

    public bool UsesValueScreen(ValueScreen valueScreen)
    {
        return _cart && _cart.valueScreen == valueScreen;
    }

    private void SyncStoredTotals()
    {
        RefreshStoredTotals();
        if (SemiFunc.IsMultiplayer() && _cartPhotonView && _cartPhotonView.ViewID != 0)
        {
            _cartPhotonView.RPC(SyncStoredTotalsRpc, RpcTarget.All, _syncedStoredCount, _syncedStoredValue);
            return;
        }

        PocketDimensionSyncStoredTotals(_syncedStoredCount, _syncedStoredValue);
    }

    private void RefreshStoredTotals()
    {
        CleanupStoredList();
        int count = 0;
        int value = 0;

        foreach (int viewId in _storedViewIds)
        {
            PhysGrabObject? physGrabObject = GetPhysGrabObject(viewId);
            if (physGrabObject == null)
            {
                continue;
            }

            count++;
            ValuableObject valuableObject = physGrabObject.GetComponent<ValuableObject>();
            if (valuableObject)
            {
                value += (int)valuableObject.dollarValueCurrent;
            }
        }

        _syncedStoredCount = count;
        _syncedStoredValue = value;
    }

    [PunRPC]
    private void PocketDimensionSyncStoredTotals(int count, int value, PhotonMessageInfo info = default)
    {
        if (SemiFunc.IsMultiplayer() && info.Sender != PhotonNetwork.MasterClient)
        {
            return;
        }

        _syncedStoredCount = Mathf.Max(0, count);
        _syncedStoredValue = Mathf.Max(0, value);
    }

    private PhysGrabObject? FindStoredValuable(bool preferReleased)
    {
        CleanupStoredList();

        foreach (int viewId in _storedViewIds)
        {
            PhysGrabObject? physGrabObject = GetPhysGrabObject(viewId);
            if (physGrabObject != null && (!preferReleased || (!physGrabObject.grabbed && physGrabObject.playerGrabbing.Count == 0)))
            {
                return physGrabObject;
            }
        }

        foreach (int viewId in _storedViewIds)
        {
            PhysGrabObject? physGrabObject = GetPhysGrabObject(viewId);
            if (physGrabObject != null)
            {
                return physGrabObject;
            }
        }

        return null;
    }

    private void CleanupStoredList()
    {
        for (int i = _storedViewIds.Count - 1; i >= 0; i--)
        {
            if (!GetPhysGrabObject(_storedViewIds[i]))
            {
                _storedViewIds.RemoveAt(i);
            }
        }
    }

    private Vector3 GetStoragePosition(int index)
    {
        int column = index % 5;
        int row = index / 5;
        return _roomOrigin + new Vector3((column - 2) * 2.4f, 1.2f, -4.8f + row * 2.4f);
    }

    private static void ResetCartState(PhysGrabObject physGrabObject)
    {
        if (!physGrabObject || !physGrabObject.impactDetector)
        {
            return;
        }

        PhysGrabObjectImpactDetector detector = physGrabObject.impactDetector;
        detector.timerInCart = 0f;
        detector.inCart = false;
        detector.currentCart = null;
        detector.currentCartPrev = null;
        detector.ImpactDisable(0.25f);
    }
}
