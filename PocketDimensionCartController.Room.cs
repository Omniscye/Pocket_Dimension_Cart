namespace Empress.PocketDimensionCart;

internal sealed partial class PocketDimensionCartController
{
    private Color GetFocusFlashColor()
    {
        try
        {
            if (AssetManager.instance)
            {
                return AssetManager.instance.colorYellow;
            }
        }
        catch
        {
        }

        return new Color(0.95f, 0.68f, 1f, 1f);
    }

    private void BuildRoom()
    {
        int viewSeed = _cartPhotonView && _cartPhotonView.ViewID != 0 ? _cartPhotonView.ViewID : _roomIndex;
        _roomOrigin = new Vector3(10000f + viewSeed * 35f, 4000f, 10000f);
        _playerSpawnPosition = _roomOrigin + new Vector3(0f, 1.5f, 6f);
        _playerExitPosition = _roomOrigin + new Vector3(0f, 1.5f, 8.4f);
        _valuableExitPosition = _playerExitPosition + new Vector3(0f, 0.5f, 0f);
        _roomRoot = new GameObject($"Pocket Dimension Cart Room {viewSeed}");
        CreateRoomBlock("Floor", _roomOrigin + new Vector3(0f, -0.1f, 0f), new Vector3(18f, 0.2f, 18f), new Color(0.12f, 0.08f, 0.18f, 1f), trigger: false);
        CreateRoomBlock("North Wall", _roomOrigin + new Vector3(0f, 3f, 9f), new Vector3(18f, 6f, 0.3f), new Color(0.16f, 0.1f, 0.24f, 1f), trigger: false);
        CreateRoomBlock("South Wall", _roomOrigin + new Vector3(0f, 3f, -9f), new Vector3(18f, 6f, 0.3f), new Color(0.16f, 0.1f, 0.24f, 1f), trigger: false);
        CreateRoomBlock("East Wall", _roomOrigin + new Vector3(9f, 3f, 0f), new Vector3(0.3f, 6f, 18f), new Color(0.16f, 0.1f, 0.24f, 1f), trigger: false);
        CreateRoomBlock("West Wall", _roomOrigin + new Vector3(-9f, 3f, 0f), new Vector3(0.3f, 6f, 18f), new Color(0.16f, 0.1f, 0.24f, 1f), trigger: false);

        GameObject valuableExit = CreateRoomBlock("Pocket Exit", _valuableExitPosition, new Vector3(4.5f, 3.2f, 2.2f), new Color(0.3f, 0.8f, 1f, 0.35f), trigger: true);
        _exitCollider = valuableExit.GetComponent<Collider>();
        valuableExit.AddComponent<PocketDimensionValuableExitZone>().SetController(this);
    }

    private GameObject CreateRoomBlock(string name, Vector3 position, Vector3 scale, Color color, bool trigger)
    {
        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = name;
        block.transform.SetParent(_roomRoot.transform);
        block.transform.position = position;
        block.transform.localScale = scale;

        Collider collider = block.GetComponent<Collider>();
        collider.isTrigger = trigger;

        Renderer renderer = block.GetComponent<Renderer>();
        if (renderer)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (!shader)
            {
                shader = Shader.Find("Standard");
            }

            if (shader)
            {
                Material material = new(shader);
                material.color = color;
                renderer.material = material;
            }
        }

        int defaultLayer = LayerMask.NameToLayer("Default");
        if (defaultLayer >= 0)
        {
            block.layer = defaultLayer;
        }

        return block;
    }
}
