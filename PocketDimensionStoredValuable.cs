namespace Empress.PocketDimensionCart;

internal sealed class PocketDimensionStoredValuable : MonoBehaviour
{
    private PhysGrabObject _physGrabObject = null!;
    private PocketDimensionCartController? _owner;
    private float _protectUntil;
    private bool _stored;

    private void Awake()
    {
        _physGrabObject = GetComponent<PhysGrabObject>();
    }

    private void Update()
    {
        if (!_physGrabObject)
        {
            return;
        }

        if (_stored || Time.time < _protectUntil)
        {
            _physGrabObject.OverrideIndestructible(0.35f);
            _physGrabObject.OverrideBreakEffects(0.35f);
            if (_physGrabObject.impactDetector)
            {
                _physGrabObject.impactDetector.ImpactDisable(0.35f);
            }
        }
    }

    public void SetStored(PocketDimensionCartController owner)
    {
        _owner = owner;
        _stored = true;
        Protect(1f);
    }

    public void SetReleased(float protectionSeconds)
    {
        _owner = null;
        _stored = false;
        Protect(protectionSeconds);
    }

    public void Protect(float seconds)
    {
        _protectUntil = Mathf.Max(_protectUntil, Time.time + Mathf.Max(0f, seconds));
    }

    public PocketDimensionCartController? GetOwner()
    {
        return _owner;
    }
}
