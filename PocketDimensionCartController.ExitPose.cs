namespace Empress.PocketDimensionCart;

internal sealed partial class PocketDimensionCartController
{
    private Vector3 GetCartExitPosition()
    {
        Vector3 forward = GetFlatCartForward();
        Vector3 basePosition = _cart.inCart ? _cart.inCart.position : _cart.transform.position;
        Vector3 position = basePosition + Vector3.up * 1.45f + forward * 1.25f;
        return SnapExitAboveFloor(position);
    }

    private Quaternion GetCartExitRotation()
    {
        return Quaternion.LookRotation(GetFlatCartForward(), Vector3.up);
    }

    private Vector3 GetFlatCartForward()
    {
        if (!_cart)
        {
            return Vector3.forward;
        }

        Vector3 forward = Vector3.ProjectOnPlane(_cart.transform.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.001f)
        {
            forward = Vector3.ProjectOnPlane(_cart.transform.right, Vector3.up);
        }

        return forward.sqrMagnitude < 0.001f ? Vector3.forward : forward.normalized;
    }

    private static Vector3 SnapExitAboveFloor(Vector3 position)
    {
        int layerMask = LayerMask.GetMask("Default", "PhysGrabObjectCart", "StaticGrabObject");
        if (layerMask == 0)
        {
            layerMask = ~LayerMask.GetMask("Ignore Raycast");
        }

        Vector3 rayOrigin = position + Vector3.up * 1.5f;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 4.5f, layerMask, QueryTriggerInteraction.Ignore))
        {
            position.y = Mathf.Max(position.y, hit.point.y + 1.15f);
        }

        return position;
    }

    private void GetCartExitPose(out Vector3 position, out Quaternion rotation)
    {
        if (CartCanOpenPocket())
        {
            RefreshSafeCartExitPose(force: true);
        }

        if (_lastSafeCartExitValid)
        {
            position = _lastSafeCartExitPosition;
            rotation = _lastSafeCartExitRotation;
            return;
        }

        if (TruckSafetySpawnPoint.instance)
        {
            position = TruckSafetySpawnPoint.instance.transform.position;
            rotation = TruckSafetySpawnPoint.instance.transform.rotation;
            return;
        }

        position = GetCartExitPosition();
        rotation = GetCartExitRotation();
    }

    private void RefreshSafeCartExitPose(bool force = false)
    {
        if (!force && Time.time < _nextSafePoseRefreshTime)
        {
            return;
        }

        _nextSafePoseRefreshTime = Time.time + 0.25f;
        if (!CartCanOpenPocket())
        {
            return;
        }

        _lastSafeCartExitPosition = GetCartExitPosition();
        _lastSafeCartExitRotation = GetCartExitRotation();
        _lastSafeCartExitValid = true;
    }
}
