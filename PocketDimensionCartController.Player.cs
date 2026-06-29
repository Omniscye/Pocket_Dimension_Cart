namespace Empress.PocketDimensionCart;

internal sealed partial class PocketDimensionCartController
{
    public void RequestLeave(PlayerAvatar player)
    {
        if (!player || Time.time < _nextLocalExitTime)
        {
            return;
        }

        _nextLocalExitTime = Time.time + 1f;
        if (SemiFunc.IsMultiplayer() && _cartPhotonView && _cartPhotonView.ViewID != 0)
        {
            int playerViewId = GetPlayerViewId(player);
            if (playerViewId == 0)
            {
                return;
            }

            _cartPhotonView.RPC(RequestLeaveRpc, RpcTarget.MasterClient, playerViewId);
        }
        else
        {
            LeaveRoom(player);
        }
    }

    private void UpdateLocalInput()
    {
        PlayerAvatar localPlayer = SemiFunc.PlayerGetLocal();
        if (!localPlayer || localPlayer.isDisabled || !SemiFunc.NoTextInputsActive())
        {
            return;
        }

        bool focused = LocalPlayerCanUseCart(localPlayer, out bool insideCart);
        if (focused)
        {
            ShowCartText();
            HandleActionKey(localPlayer);
            HandleInteractEnter(localPlayer);
        }

        if (PocketDimensionCartPlugin.EnterByHoppingInCart.Value && insideCart && Time.time >= _nextLocalEnterTime && !SemiFunc.InputHold(InputKey.Grab))
        {
            _insideCartTimer += Time.deltaTime;
            if (_insideCartTimer >= 0.45f)
            {
                _nextLocalEnterTime = Time.time + 2f;
                _insideCartTimer = 0f;
                RequestEnter(localPlayer);
            }
        }
        else
        {
            _insideCartTimer = 0f;
        }

        if (LocalPlayerInsideRoom(localPlayer))
        {
            PocketDimensionCartAccess.SuppressPocketRoomMovement(PlayerController.instance, localPlayer);
            HandleRoomExit(localPlayer);
        }
    }

    private void HandleActionKey(PlayerAvatar player)
    {
        KeyCode key = PocketDimensionCartPlugin.ActionKey.Value;
        if (Input.GetKeyDown(key))
        {
            _keyDownTime = Time.time;
            _holdRequestSent = false;
        }

        if (Input.GetKey(key) && !_holdRequestSent && Time.time - _keyDownTime >= PocketDimensionCartPlugin.HoldSeconds.Value)
        {
            _holdRequestSent = true;
            RequestEjectAll(player);
        }

        if (Input.GetKeyUp(key))
        {
            if (!_holdRequestSent)
            {
                RequestEjectOne(player);
            }

            _holdRequestSent = false;
        }
    }

    private void HandleInteractEnter(PlayerAvatar player)
    {
        if (Time.time < _nextLocalEnterTime)
        {
            return;
        }

        if (SemiFunc.InputDown(InputKey.Interact))
        {
            _nextLocalEnterTime = Time.time + 2f;
            RequestEnter(player);
        }
    }

    private void HandleRoomExit(PlayerAvatar player)
    {
        if (!PlayerTouchingExit(player))
        {
            _exitTouchTimer = 0f;
            return;
        }

        _exitTouchTimer += Time.deltaTime;
        ApplyExitPortalBrake(player);
        SemiFunc.UIFocusText("Leaving pocket dimension", Color.white, GetFocusFlashColor(), 0.1f);
        if (_exitTouchTimer >= 0.18f)
        {
            _exitTouchTimer = 0f;
            RequestLeave(player);
        }
    }

    private void ShowCartText()
    {
        string keyName = PocketDimensionCartPlugin.ActionKey.Value.ToString();
        string text = $"Press {keyName} to pop out one valuable\nHold {keyName} to pop out all valuables";
        SemiFunc.UIFocusText(text, Color.white, GetFocusFlashColor(), 0.1f);
    }

    private void RequestEjectOne(PlayerAvatar player)
    {
        if (SemiFunc.IsMultiplayer() && _cartPhotonView && _cartPhotonView.ViewID != 0)
        {
            int playerViewId = GetPlayerViewId(player);
            if (playerViewId == 0)
            {
                return;
            }

            _cartPhotonView.RPC(RequestEjectOneRpc, RpcTarget.MasterClient, playerViewId);
        }
        else
        {
            EjectOne(player);
        }
    }

    private void RequestEjectAll(PlayerAvatar player)
    {
        if (SemiFunc.IsMultiplayer() && _cartPhotonView && _cartPhotonView.ViewID != 0)
        {
            int playerViewId = GetPlayerViewId(player);
            if (playerViewId == 0)
            {
                return;
            }

            _cartPhotonView.RPC(RequestEjectAllRpc, RpcTarget.MasterClient, playerViewId);
        }
        else
        {
            EjectAll(player);
        }
    }

    private void RequestEnter(PlayerAvatar player)
    {
        if (SemiFunc.IsMultiplayer() && _cartPhotonView && _cartPhotonView.ViewID != 0)
        {
            int playerViewId = GetPlayerViewId(player);
            if (playerViewId == 0)
            {
                return;
            }

            _cartPhotonView.RPC(RequestEnterRpc, RpcTarget.MasterClient, playerViewId);
        }
        else
        {
            EnterRoom(player);
        }
    }

    [PunRPC]
    private void PocketDimensionRequestEjectOne(int playerViewId, PhotonMessageInfo info = default)
    {
        if (!SemiFunc.IsMasterClientOrSingleplayer() || !RequestOwnerIsValid(playerViewId, info))
        {
            return;
        }

        PlayerAvatar? player = GetPlayer(playerViewId);
        EjectOne(player);
    }

    [PunRPC]
    private void PocketDimensionRequestEjectAll(int playerViewId, PhotonMessageInfo info = default)
    {
        if (!SemiFunc.IsMasterClientOrSingleplayer() || !RequestOwnerIsValid(playerViewId, info))
        {
            return;
        }

        PlayerAvatar? player = GetPlayer(playerViewId);
        EjectAll(player);
    }

    [PunRPC]
    private void PocketDimensionRequestEnter(int playerViewId, PhotonMessageInfo info = default)
    {
        if (!SemiFunc.IsMasterClientOrSingleplayer() || !RequestOwnerIsValid(playerViewId, info))
        {
            return;
        }

        PlayerAvatar? player = GetPlayer(playerViewId);
        EnterRoom(player);
    }

    [PunRPC]
    private void PocketDimensionRequestLeave(int playerViewId, PhotonMessageInfo info = default)
    {
        if (!SemiFunc.IsMasterClientOrSingleplayer() || !RequestOwnerIsValid(playerViewId, info))
        {
            return;
        }

        PlayerAvatar? player = GetPlayer(playerViewId);
        LeaveRoom(player);
    }

    [PunRPC]
    private void PocketDimensionStabilizeExit(int playerViewId, Vector3 exitPosition, Quaternion exitRotation, PhotonMessageInfo info = default)
    {
        if (SemiFunc.IsMultiplayer() && info.Sender != PhotonNetwork.MasterClient)
        {
            return;
        }

        PlayerAvatar? player = GetPlayer(playerViewId);
        if (player == null)
        {
            return;
        }

        StabilizePlayerAfterPocketExit(player, exitPosition, exitRotation);
        player.RoomVolumeCheck?.CheckSet();
    }

    private void EnterRoom(PlayerAvatar? player)
    {
        if (player == null || !CanPlayerUseCartOnHost(player))
        {
            return;
        }

        RefreshSafeCartExitPose(force: true);
        player.Spawn(_playerSpawnPosition, Quaternion.LookRotation(Vector3.forward, Vector3.up));
        player.RoomVolumeCheck?.CheckSet();
    }

    private void LeaveRoom(PlayerAvatar? player)
    {
        if (player == null)
        {
            return;
        }

        GetCartExitPose(out Vector3 exitPosition, out Quaternion exitRotation);
        player.Spawn(exitPosition, exitRotation);
        BroadcastExitStabilize(player, exitPosition, exitRotation);
        player.RoomVolumeCheck?.CheckSet();
    }

    private void StabilizePlayerAfterPocketExit(PlayerAvatar player, Vector3 position, Quaternion rotation)
    {
        player.FallDamageResetSet(2f);
        Rigidbody playerRigidbody = player.GetComponent<Rigidbody>();
        if (playerRigidbody)
        {
            playerRigidbody.position = position;
            playerRigidbody.rotation = rotation;
            playerRigidbody.velocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }

        PlayerTumble? tumble = PocketDimensionCartAccess.GetTumble(player);
        if (tumble != null)
        {
            tumble.TumbleRequest(_isTumbling: false, _playerInput: false);
            tumble.TumbleOverrideTime(0.25f);
            tumble.DisableCustomGravity(0.25f);
            tumble.OverrideDisableTumbleMoveSound(0.25f);

            Rigidbody tumbleRigidbody = tumble.GetComponent<Rigidbody>();
            if (tumbleRigidbody)
            {
                tumbleRigidbody.position = position + Vector3.up * 0.3f;
                tumbleRigidbody.rotation = rotation;
                tumbleRigidbody.velocity = Vector3.zero;
                tumbleRigidbody.angularVelocity = Vector3.zero;
            }
        }

        if (PocketDimensionCartAccess.IsLocal(player))
        {
            if (PlayerController.instance)
            {
                PlayerController.instance.transform.position = position;
                PlayerController.instance.transform.rotation = rotation;
            }
        }
    }

    private void BroadcastExitStabilize(PlayerAvatar player, Vector3 position, Quaternion rotation)
    {
        int playerViewId = GetPlayerViewId(player);
        if (playerViewId == 0)
        {
            StabilizePlayerAfterPocketExit(player, position, rotation);
            return;
        }

        if (SemiFunc.IsMultiplayer() && _cartPhotonView && _cartPhotonView.ViewID != 0)
        {
            _cartPhotonView.RPC(StabilizeExitRpc, RpcTarget.All, playerViewId, position, rotation);
            return;
        }

        PocketDimensionStabilizeExit(playerViewId, position, rotation);
    }

    private void ApplyExitPortalBrake(PlayerAvatar player)
    {
        DampenBody(player.GetComponent<Rigidbody>(), 0.15f);

        PlayerTumble? tumble = PocketDimensionCartAccess.GetTumble(player);
        if (tumble != null)
        {
            DampenBody(tumble.GetComponent<Rigidbody>(), 0.08f);
            tumble.DisableCustomGravity(0.12f);
            tumble.OverrideDisableTumbleMoveSound(0.12f);
        }

        if (PocketDimensionCartAccess.IsLocal(player) && PlayerController.instance)
        {
            DampenBody(PlayerController.instance.GetComponent<Rigidbody>(), 0.15f);
        }
    }

    private static void DampenBody(Rigidbody body, float upwardLimit)
    {
        if (!body || body.isKinematic)
        {
            return;
        }

        Vector3 velocity = body.velocity;
        velocity.x *= 0.25f;
        velocity.z *= 0.25f;
        velocity.y = Mathf.Clamp(velocity.y * 0.2f, -upwardLimit, upwardLimit);
        body.velocity = velocity;
        body.angularVelocity *= 0.15f;
    }

    private bool CartCanOpenPocket()
    {
        if (!_cart || !_cart.gameObject.activeInHierarchy || !_cart.GetComponent<PhysGrabObject>())
        {
            return false;
        }

        return !PocketDimensionCartAccess.IsEquippedOrInInventory(_cart.GetComponent<ItemEquippable>());
    }

    private bool LocalPlayerCanUseCart(PlayerAvatar player, out bool insideCart)
    {
        if (!CartCanOpenPocket())
        {
            insideCart = false;
            return false;
        }

        insideCart = PlayerInsideCartVolume(player);
        if (insideCart)
        {
            return true;
        }

        if (PhysGrabber.instance && PhysGrabber.instance.grabbedPhysGrabObject == _cart.physGrabObject)
        {
            return true;
        }

        if (Vector3.Distance(player.transform.position, _cart.transform.position) > 5.5f)
        {
            return false;
        }

        Transform aim = GetAimTransform(player);
        if (!aim)
        {
            return false;
        }

        Ray ray = new(aim.position, aim.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, 6f, ~0, QueryTriggerInteraction.Collide))
        {
            return false;
        }

        PhysGrabCart hitCart = hit.collider.GetComponentInParent<PhysGrabCart>();
        return hitCart == _cart;
    }

    private bool PlayerInsideCartVolume(PlayerAvatar player)
    {
        if (!player || !_cart || !_cart.inCart)
        {
            return false;
        }

        BoxCollider box = _cart.inCart.GetComponent<BoxCollider>();
        Vector3 sample = player.transform.position + Vector3.up * 0.25f;
        if (box)
        {
            return Vector3.Distance(box.ClosestPoint(sample), sample) < 0.01f;
        }

        Vector3 local = _cart.inCart.InverseTransformPoint(sample);
        Vector3 half = _cart.inCart.localScale * 0.5f;
        return Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.y) <= half.y && Mathf.Abs(local.z) <= half.z;
    }

    private bool CanPlayerUseCartOnHost(PlayerAvatar? player)
    {
        if (!PocketDimensionCartRuntime.IsRealLevel || player == null || !_cart)
        {
            return false;
        }

        if (LocalOrHostPlayerInsideRoom(player))
        {
            return true;
        }

        if (!CartCanOpenPocket())
        {
            return false;
        }

        return Vector3.Distance(player.transform.position, _cart.transform.position) <= 8f || PlayerInsideCartVolume(player);
    }

    private bool LocalPlayerInsideRoom(PlayerAvatar? player)
    {
        return player != null && Vector3.Distance(player.transform.position, _roomOrigin) <= 14f && Mathf.Abs(player.transform.position.y - _roomOrigin.y) <= 8f;
    }

    internal bool PlayerInsidePocketRoom(PlayerAvatar? player)
    {
        return LocalPlayerInsideRoom(player);
    }

    private bool LocalOrHostPlayerInsideRoom(PlayerAvatar? player)
    {
        return LocalPlayerInsideRoom(player);
    }

    private bool PlayerTouchingExit(PlayerAvatar player)
    {
        if (!player || !_exitCollider)
        {
            return false;
        }

        Vector3 position = player.transform.position + Vector3.up * 0.35f;
        if (Vector3.Distance(_exitCollider.ClosestPoint(position), position) < 0.04f)
        {
            return true;
        }

        Vector3 upperPosition = player.transform.position + Vector3.up * 1.1f;
        return Vector3.Distance(_exitCollider.ClosestPoint(upperPosition), upperPosition) < 0.04f;
    }

    private Transform GetAimTransform(PlayerAvatar player)
    {
        if (player.localCamera)
        {
            return player.localCamera.GetOverrideTransform();
        }

        Camera camera = Camera.main;
        return camera ? camera.transform : player.transform;
    }

    private int GetPlayerViewId(PlayerAvatar player)
    {
        if (!player || !player.photonView)
        {
            return 0;
        }

        return player.photonView.ViewID;
    }

    private PlayerAvatar? GetPlayer(int playerViewId)
    {
        PhotonView view = PhotonView.Find(playerViewId);
        if (!view)
        {
            return null;
        }

        return view.GetComponent<PlayerAvatar>();
    }

    private PhysGrabObject? GetPhysGrabObject(int viewId)
    {
        return PocketDimensionCartRuntime.ResolvePhysGrabObject(viewId);
    }

    private bool RequestOwnerIsValid(int playerViewId, PhotonMessageInfo info)
    {
        if (!SemiFunc.IsMultiplayer())
        {
            return true;
        }

        PhotonView view = PhotonView.Find(playerViewId);
        if (!view)
        {
            return false;
        }

        return view.Owner == info.Sender || view.Controller == info.Sender || info.Sender == PhotonNetwork.MasterClient;
    }
}
