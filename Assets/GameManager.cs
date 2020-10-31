using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SA {
    public class GameManager : MonoBehaviour {
        public Texture2D levelTex;
        Texture2D textureInstance;
        public SpriteRenderer levelRenderer;

        int maxX;
        int maxY;

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

            grid = new Node[maxX, maxY];

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
                // draw with color
                // Color c = Color.white;
                // c.a = 1;

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

                        // textureInstance.SetPixel(targetX, targetY, c);

                        // change cell type
                        node.cellType = 1;
                        node.cellTypeDelta = 1;
                    }
                }
            }
        }

        void UpdateGrid() {
            UpdateGridChanges();
            UpdateGridTypes();
        }

        void UpdateGridChanges() {
            for (int x = 1; x < maxX - 1; x++) {
                for (int y = 1; y < maxY - 1; y++) {
                    Node checkedNode = GetNode(x, y);

                    // TODO here will be the logic for types
                    if (checkedNode.cellType == 1) {
                        if (GetNode(x, y - 1).cellType == 0 && y > 20) {
                            checkedNode.cellTypeDelta = 0;
                            GetNode(x, y - 1).cellTypeDelta = 1;
                        }
                        else {
                            if (GetNode(x - 1, y - 1).cellType == 0 && y > 20) {
                                checkedNode.cellTypeDelta = 0;
                                GetNode(x - 1, y - 1).cellTypeDelta = 1;
                            }
                            else {
                                if (GetNode(x + 1, y - 1).cellType == 0 && y > 20) {
                                    checkedNode.cellTypeDelta = 0;
                                    GetNode(x + 1, y - 1).cellTypeDelta = 1;
                                }
                            }
                        }
                    }
                }
            }
        }

        void UpdateGridTypes() {
            for (int x = 1; x < maxX - 1; x++) {
                for (int y = 1; y < maxY - 1; y++) {
                    Node checkedNode = GetNode(x, y);

                    checkedNode.cellType = checkedNode.cellTypeDelta;

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
            if (x < 0 || y < 0 || x > maxX - 1 || y >maxY - 1) {
                return null;
            }
            else {
                return grid[x, y];
            }
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
