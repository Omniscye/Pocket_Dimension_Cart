namespace Empress.PocketDimensionCart;

internal sealed class PocketDimensionValuableExitZone : MonoBehaviour
{
    private PocketDimensionCartController _controller = null!;

    public void SetController(PocketDimensionCartController controller)
    {
        _controller = controller;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryEject(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryEject(other);
    }

    private void TryEject(Collider other)
    {
        if (!_controller || !PocketDimensionCartRuntime.IsRealLevel || !SemiFunc.IsMasterClientOrSingleplayer())
        {
            return;
        }

        PhysGrabObject physGrabObject = other.GetComponentInParent<PhysGrabObject>();
        if (!physGrabObject || !_controller.OwnsStoredValuable(physGrabObject))
        {
            return;
        }

        _controller.EjectStoredFromRoom(physGrabObject);
    }
}
