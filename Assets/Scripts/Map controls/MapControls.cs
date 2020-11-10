using UnityEngine;

public class MapControls : MonoBehaviour {
    private LoadJsonData jsonLoader;
    private LoadJsonData.RootNodeData nodeData;
    private GenerateMap map;

    Node curNode;
    Vector3 mousePos;

    int pickedType = 1;
    public int brushSize = 3;

    private void Awake() {
        map = GetComponent<GenerateMap>();
        jsonLoader = GetComponent<LoadJsonData>();
        nodeData = jsonLoader.ReadNodeData();
    }

    // Update is called once per frame
    void Update() {
        HandleMouseInput();
        PickPlacedType();
    }

    public void HandleMouseInput() {
        //clicked with mouse
        if (Input.GetMouseButton(0)) {
            for (int x = -brushSize; x < brushSize; x++) {
                for (int y = -brushSize; y < brushSize; y++) {
                    GetMousePos();
                    int targetX = x + curNode.x;
                    int targetY = y + curNode.y;

                    Node node = GetNode(targetX, targetY);

                    if (node == null) {
                        continue;
                    }

                    // change cell type
                    node.cellType = pickedType;
                    node.cellTypeDelta = node.cellType;
                    node.stateOfMatter = nodeData.particleDataRoot[node.cellType].state;
                    node.unchangedAge = 0;
                    map.chunkAgeCounter[Mod(targetX, map.chunkSize), Mod(targetY, map.chunkSize)] = 0;
                }
            }
        }
    }


    public void PickPlacedType() {
        if (Input.GetKeyDown(KeyCode.Alpha0)) { pickedType = 0; }
        if (Input.GetKeyDown(KeyCode.Alpha1)) { pickedType = 1; }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { pickedType = 2; }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { pickedType = 3; }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { pickedType = 4; }
        if (Input.GetKeyDown(KeyCode.Alpha5)) { pickedType = 5; }
        if (Input.GetKeyDown(KeyCode.Alpha6)) { pickedType = 6; }
    }

    public void GetMousePos() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        mousePos = ray.GetPoint(10);

        curNode = GetNodeFromWorldPos(mousePos);
    }

    public Node GetNodeFromWorldPos(Vector3 worldPos) {
        int targetX = Mathf.RoundToInt(worldPos.x);
        int targetY = Mathf.RoundToInt(worldPos.y);

        return GetNode(targetX, targetY);
    }

    public Node GetNode(int x, int y) {
        if (x < 0 || y < 0 || x > map.maxX - 1 || y > map.maxY - 1) {
            return null;
        }
        else {
            return map.grid[x, y];
        }
    }

    int Mod(int a, int b) {
        if (a < b) {
            return 0;
        }

        return (a - (a % b)) / b;
    }
}
