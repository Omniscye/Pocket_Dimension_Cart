namespace Empress.PocketDimensionCart;

internal sealed partial class PocketDimensionCartController : MonoBehaviourPun
{
    private const string RequestEjectOneRpc = "PocketDimensionRequestEjectOne";
    private const string RequestEjectAllRpc = "PocketDimensionRequestEjectAll";
    private const string RequestEnterRpc = "PocketDimensionRequestEnter";
    private const string RequestLeaveRpc = "PocketDimensionRequestLeave";
    private const string StabilizeExitRpc = "PocketDimensionStabilizeExit";
    private const string SyncStoredTotalsRpc = "PocketDimensionSyncStoredTotals";

    private readonly List<int> _storedViewIds = new();
    private readonly HashSet<int> _extractingCosmeticViewIds = new();
    private PhysGrabCart _cart = null!;
    private PhotonView _cartPhotonView = null!;
    private GameObject _roomRoot = null!;
    private Vector3 _roomOrigin;
    private Vector3 _playerSpawnPosition;
    private Vector3 _playerExitPosition;
    private Vector3 _valuableExitPosition;
    private Collider _exitCollider = null!;
    private float _keyDownTime;
    private float _nextLocalEnterTime;
    private float _nextLocalExitTime;
    private float _exitTouchTimer;
    private float _insideCartTimer;
    private float _nextSafePoseRefreshTime;
    private Vector3 _lastSafeCartExitPosition;
    private Quaternion _lastSafeCartExitRotation = Quaternion.identity;
    private bool _lastSafeCartExitValid;
    private bool _holdRequestSent;
    private bool _ejectAllRunning;
    private int _roomIndex;
    private int _syncedStoredCount;
    private int _syncedStoredValue;

    public int StoredCount => _storedViewIds.Count;

    private void Awake()
    {
        _cart = GetComponent<PhysGrabCart>();
        _cartPhotonView = GetComponent<PhotonView>();
        _roomIndex = PocketDimensionCartRuntime.ReserveRoomIndex();
        BuildRoom();
        PocketDimensionCartRuntime.Register(this);
    }

    private void OnDisable()
    {
        PocketDimensionCartRuntime.Unregister(this);
    }

    private void Update()
    {
        if (!PocketDimensionCartRuntime.IsRealLevel || !_cart)
        {
            return;
        }

        RefreshSafeCartExitPose();
        UpdateLocalInput();
    }
}
