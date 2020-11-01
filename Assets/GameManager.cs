using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GRD {
    public class GameManager : MonoBehaviour {
        public Texture2D levelTex;
        Texture2D textureInstance;
        public SpriteRenderer levelRenderer;

        int maxX;
        int maxY;

        public int chunkMaxX;
        public int chunkMaxY;

        int brushSize = 2;

        readonly int chunkSize = 64;
        int chunkAmountX;
        int chunkAmountY;
        int[,] chunkAgeCounter;

        Node[,] grid;

        Vector3 mousePos;
        Node curNode;
        Node prevNode;

        private void Start() {
            CreateLevel();
        }

        private void CreateLevel() {
            maxX = levelTex.width;
            maxY = levelTex.height;

            chunkMaxX = maxX / chunkSize;
            chunkMaxY = maxY / chunkSize;

            grid = new Node[maxX, maxY];
            chunkAgeCounter = new int[chunkMaxX, chunkMaxY];

            textureInstance = new Texture2D(maxX, maxY) {
                filterMode = FilterMode.Point
            };

            for (int x = 0; x < maxX; x++) {
                for (int y = 0; y < maxY; y++) {
                    Node newNode = new Node {
                        x = x,
                        y = y
                    };

                    Color c = levelTex.GetPixel(x, y);
                    textureInstance.SetPixel(x, y, c);

                    if (c.r == 0 && c.g == 0 && c.b == 0) {
                        newNode.isEmpty = true;
                    }
                    else {
                        newNode.isEmpty = false;    
                    }

                    grid[x, y] = newNode;  // generate nodes
                    newNode.cellType = 0;  // reset grid types
                    newNode.cellTypeDelta = 0;  // reset grid types difference, should use function
                }
            }

            textureInstance.Apply();

            Rect rect = new Rect(0, 0, maxX, maxY);
            levelRenderer.sprite = Sprite.Create(textureInstance, rect, Vector2.zero, 1);
        }

        private void Update() {
            GetMousePos();
            HandleMouseInput();
            UpdateGrid();
        }

        void HandleMouseInput() {

            //clicked with mouse
            if (Input.GetMouseButton(0)) {
                for(int x = -brushSize; x < brushSize; x++) {
                    for(int y = -brushSize; y < brushSize; y++) {
                        int targetX = x + curNode.x;
                        int targetY = y + curNode.y;

                        // Check if not creating out of bounds
                        Node node = GetNode(targetX, targetY);

                        if (node == null) {
                            continue;
                        }

                        node.isEmpty = true; 

                        // change cell type
                        node.cellType = 1;
                        node.cellTypeDelta = 1;
                        node.unchangedAge = 0;
                        chunkAgeCounter[Mod(targetX, chunkSize), Mod(targetY, chunkSize)] = 0;
                    }
                }
            }
        }

        void UpdateGrid() {
            for (int x = 0; x < chunkMaxX; x++) {
                for (int y = 0; y < chunkMaxY; y++) {
                    if (chunkAgeCounter[x, y] < chunkSize * chunkSize) { UpdateGridChanges(x, y); }
                }
            }

            for (int x = 0; x < chunkMaxX; x++) {
                for (int y = 0; y < chunkMaxY; y++) {
                    if (chunkAgeCounter[x, y] < chunkSize * chunkSize) { UpdateGridTypes(x, y); }
                }
            }

            // debug chunk grids ////////////////////////////
            for (int x = 0; x <= chunkMaxX; x++) {
                for (int y = 0; y <= chunkMaxY; y++) {
                    Debug.DrawLine(new Vector3(0, y * chunkSize, 0), new Vector3(300, y * chunkSize, 0), Color.red);
                    Debug.DrawLine(new Vector3(x * chunkSize, 0, 0), new Vector3(x * chunkSize, 300, 0), Color.red);
                }
            }
        }

        void UpdateGridChanges(int chunkX, int chunkY) {
            for (int x = chunkX * chunkSize; x < chunkX * chunkSize + chunkSize; x++) {
                for (int y = chunkY * chunkSize; y < chunkY * chunkSize + chunkSize; y++) {
                    Node checkedNode = GetNode(x, y);

                    // TODO here will be the logic for types and nearby cells age reset
                    // logic should take two arguments, current node, and node to change
                    // node to change will be determined by taking speed of the current node
                    // or maybe other variables, everything should take place alongside collision checks
                    // maybe those should be node functions

                    if (checkedNode.cellType == 1) {
                        if (GetNode(x, y - 1).cellType == 0 && y > 5) {
                            checkedNode.cellTypeDelta = 0;
                            GetNode(x, y - 1).cellTypeDelta = 1;
                            
                            // check if change occurs on the edge to activate other chunks
                            if (y <= (chunkY * chunkSize + chunkSize) - 1) {
                                if (chunkY > 0) {
                                    chunkAgeCounter[chunkX, chunkY - 1] = 0;
                                }
                            }
                        }
                        else {
                            int sideRoll = Random.Range(0, 2);
                            if (sideRoll == 0) { sideRoll = -1; }

                            if (GetNode(x + sideRoll, y - 1).cellType == 0 && y > 5) {
                                checkedNode.cellTypeDelta = 0;
                                GetNode(x + sideRoll, y - 1).cellTypeDelta = 1;

                                // check if change occurs on the edge to activate other chunks
                                if (y <= (chunkY * chunkSize + chunkSize) - 1) {
                                    if (chunkY > 0) {
                                        chunkAgeCounter[chunkX, chunkY - 1] = 0;
                                    }
                                }

                                if (x >= (chunkX * chunkSize + chunkSize) - 1 || x <= (chunkX * chunkSize) + 1) {
                                    if (chunkX > 0 && chunkX < chunkMaxX) {
                                        chunkAgeCounter[chunkX + sideRoll, chunkY] = 0;
                                    }
                                }
                            }

                            if (GetNode(x - 1, y - 1).cellType == 0 && y > 5) {
                                checkedNode.cellTypeDelta = 0;
                                GetNode(x - 1, y - 1).cellTypeDelta = 1;

                                // check if change occurs on the edge to activate other chunks
                                if (y == (chunkY * chunkSize + chunkSize) && x == (chunkX * chunkSize + chunkSize)) {
                                    if (chunkX > 0) {
                                        chunkAgeCounter[chunkX - 1, chunkY - 1] = 0;
                                    }
                                }
                            }
                            else {
                                if (GetNode(x + 1, y - 1).cellType == 0 && y > 5) {
                                    checkedNode.cellTypeDelta = 0;
                                    GetNode(x + 1, y - 1).cellTypeDelta = 1;

                                    // check if change occurs on the edge to activate other chunks
                                    if (y == (chunkY * chunkSize + chunkSize) && x == (chunkX * chunkSize + chunkSize)) {
                                        if (chunkY < chunkMaxY && chunkY > 0) {
                                            if (chunkX < chunkMaxX) {
                                                chunkAgeCounter[chunkX + 1, chunkY - 1] = 0;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    // until then, above part is scuffed and should be done faster and in more optimal way
                }
            }
        }

        void UpdateGridTypes(int chunkX, int chunkY) {
            for (int x = chunkX * chunkSize; x < chunkX * chunkSize + chunkSize; x++) {
                for (int y = chunkY * chunkSize; y < chunkY * chunkSize + chunkSize; y++) {
                    Node checkedNode = GetNode(x, y);

                    // No change to cell type occurs
                    if (checkedNode.cellType == checkedNode.cellTypeDelta) {
                        checkedNode.unchangedAge += 1;

                        // increase counter for chunk check
                        if (checkedNode.unchangedAge > 2) {
                            chunkAgeCounter[chunkX, chunkY] += 1;
                        }
                    }
                    // there was a cell type change
                    else {
                        checkedNode.cellType = checkedNode.cellTypeDelta;
                        checkedNode.unchangedAge = 0;
                        chunkAgeCounter[chunkX, chunkY] = 0;
                    }
                    
                    UpdateGridVisuals(checkedNode, x, y);
                        
                    ResetGridDelta(checkedNode);  // reset deltas here for optimal usage of loops
                }
            }

            // update canvas
            textureInstance.Apply();
        }

        void UpdateGridVisuals(Node nodeToUpdate, int pixelX, int pixelY) {
            // TODO here will be the auto color assignement
            Color c = Color.black;

            if (nodeToUpdate.cellType == 0) {
                c = Color.black;
            }
            if (nodeToUpdate.cellType == 1) {
                c = Color.white;
            }
            
            textureInstance.SetPixel(pixelX, pixelY, c);
        }

        void ResetGridDelta(Node nodeToReset) {
            nodeToReset.cellTypeDelta = nodeToReset.cellType;
        }

        void GetMousePos() {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            mousePos = ray.GetPoint(10);

            curNode = GetNodeFromWorldPos(mousePos);
        }

        Node GetNodeFromWorldPos(Vector3 worldPos) {
            int targetX = Mathf.RoundToInt(worldPos.x);
            int targetY = Mathf.RoundToInt(worldPos.y);

            return GetNode(targetX, targetY);
        }

        Node GetNode(int x, int y) {
            if (x < 0 || y < 0 || x > maxX - 1 || y > maxY - 1) {
                return null;
            }
            else {
                return grid[x, y];
            }
        }

        int Mod(int a, int b) {
            if (a < b) {
                return 0;
            }

            return (a - (a % b)) / b;
        }
    }

    public class Node {
        public int x;
        public int y;

        public int unchangedAge = 0;

        public bool isEmpty;

        public bool isLiquid;
        public bool isSolid;
        public bool isGas;

        public int cellType;
        public int cellTypeDelta;
    }
}
