using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    private GenerateMap map;
    private MapControls controls;
    private LoadJsonData jsonLoader;
    private LoadJsonData.RootNodeData nodeData;  // loaded json data for all particles is here
    
    // nodes stats
    enum StateName { solid, particle, gas, liquid, }

    private void Awake() {
        map = GetComponent<GenerateMap>();
        jsonLoader = GetComponent<LoadJsonData>();
        controls = GetComponent<MapControls>();
        nodeData = jsonLoader.ReadNodeData();
    }

    private void Start() {
        map.CreateLevel();
    }

    private void Update() {
        UpdateGrid();
    }

    void UpdateGrid() {
        for (int i = 0; i < 2; i++) {
            for (int x = 0; x < map.chunkMaxX; x++) {
                for (int y = 0; y < map.chunkMaxY; y++) {
                    if (i == 0) {
                        if (map.chunkAgeCounter[x, y] < map.chunkSize * map.chunkSize) { UpdateGridChanges(x, y); }
                    }
                    else {
                        if (map.chunkAgeCounter[x, y] < map.chunkSize * map.chunkSize) { UpdateGridTypes(x, y); }

                        // debug chunk sizes ///
                        Debug.DrawLine(new Vector3(0, y * map.chunkSize, 0), new Vector3(map.maxX , y * map.chunkSize, 0), Color.red);
                        Debug.DrawLine(new Vector3(x * map.chunkSize, 0, 0), new Vector3(x * map.chunkSize, map.maxY, 0), Color.red);
                    }
                }
            }
        }
    }

    void UpdateGridChanges(int chunkX, int chunkY) {
        for (int x = chunkX * map.chunkSize; x < chunkX * map.chunkSize + map.chunkSize; x++) {
            for (int y = chunkY * map.chunkSize; y < chunkY * map.chunkSize + map.chunkSize; y++) {
                Node checkedNode = controls.GetNode(x, y);

                StateEffect(checkedNode);
                VelocityEffect(checkedNode);
                MovementChunkAwake(checkedNode, chunkX, chunkY);
            }
        }
    }

    void StateEffect(Node node) {
        switch (node.stateOfMatter){
            case (int)StateName.solid:
                StateBehaviorSolid(node);
                break;

            case (int)StateName.particle:
                StateBehaviorParticle(node);
                break;

            case (int)StateName.gas:
                break;

            case (int)StateName.liquid:
                StateBehaviorLiquid(node);
                break;

        }
    }

    // behaviors need a lot of work on conditions
    private void StateBehaviorSolid(Node node) {
        node.accX = 0;
        node.accY = 0;

        node.velX = 0;
        node.velY = 0;
    }

    private void StateBehaviorParticle(Node node) {
        if (node.y > 0) {
            int randomSide = Random.Range(0, 2);
            if (randomSide == 0) { randomSide = -1; }

            // check under
            if (controls.GetNode(node.x, node.y - 1).stateOfMatter == (int)StateName.gas ||
                controls.GetNode(node.x, node.y - 1).stateOfMatter == (int)StateName.liquid) {
                node.accY += -1;
                node.accX = 0;
            }
            //check on the sides under
            else if (node.x + randomSide < map.maxX && node.x + randomSide > 0) {
                if (controls.GetNode(node.x + randomSide, node.y - 1).stateOfMatter == (int)StateName.gas ||
                    controls.GetNode(node.x + randomSide, node.y - 1).stateOfMatter == (int)StateName.liquid) {
                    node.accY += -1;
                    node.accX += randomSide;
                }
                else {
                    node.accX = 0;
                    node.accY = 0;

                    node.velX = 0;
                    node.velY = 0;
                }
            }
            else {
                node.accX = 0;
                node.accY = 0;

                node.velX = 0;
                node.velY = 0;
            }
        }
    }

    private void StateBehaviorLiquid(Node node) {
        if (node.y > 0) {
            int randomSide = Random.Range(0, 2);
            if (randomSide == 0) { randomSide = -1; }

            // check under
            if (controls.GetNode(node.x, node.y - 1).stateOfMatter == (int)StateName.gas) {
                node.accY += -1;
                node.accX = 0;
            }
            else if (controls.GetNode(node.x + randomSide, node.y - 1).stateOfMatter == (int)StateName.gas) { 
                node.accY += -1;
                node.accX += randomSide;
            }
            else if (controls.GetNode(node.x + randomSide, node.y).stateOfMatter == (int)StateName.gas) {
                node.accY = 0;
                node.accX += randomSide;
            }
            else {
                node.accX = 0;
                node.accY = 0;

                node.velX = 0;
                node.velY = 0;
            }
        }
    }

    void VelocityEffect(Node node) {
        node.velX = Mathf.Clamp(node.accX, -1, 1);
        node.velY = Mathf.Clamp(node.accY, -1, 1);

        // all stats are swapped between two cells, leaving only position unchanged to match the grid
        if (node.cellType != 0 && node.moved == false) {
            if (node.y > 1 && node.y < map.maxY - 1 && node.x > 1 && node.x < map.maxX - 1) {
                if (controls.GetNode(node.x + node.velX, node.y + node.velY).stateOfMatter != (int)StateName.solid){ 
                Node exchangeNode = controls.GetNode(node.x + node.velX, node.y + node.velY);

                    if (exchangeNode.cellType != node.cellType && exchangeNode.moved == false) {
                        node.moved = true;
                        exchangeNode.moved = true;

                        int exPosX = exchangeNode.x;
                        int exPosY = exchangeNode.y;
                        int exType = exchangeNode.cellType;

                        int curPosX = node.x;
                        int curPosY = node.y;
                        int curType = node.cellType;

                        // switch position of the nodes
                        map.grid[exPosX, exPosY] = node;
                        map.grid[exPosX, exPosY].x = exPosX;
                        map.grid[exPosX, exPosY].y = exPosY;

                        map.grid[curPosX, curPosY] = exchangeNode;
                        map.grid[curPosX, curPosY].x = curPosX;
                        map.grid[curPosX, curPosY].y = curPosY;

                        // bring back types and set deltas
                        // move to (other type)
                        map.grid[exPosX, exPosY].cellTypeDelta = curType;
                        map.grid[exPosX, exPosY].cellType = exType;

                        //// move from (0 here)
                        map.grid[curPosX, curPosY].cellTypeDelta = exType;
                        map.grid[curPosX, curPosY].cellType = curType;
                    }
                }
            }
        }
    }

    // i cant find faster method with loops (consider using quadtrees)
    void MovementChunkAwake(Node node, int chunkX, int chunkY) {
        // left
        if (node.x <= chunkX * map.chunkSize) {
            if (chunkX > 0) { map.chunkAgeCounter[chunkX - 1, chunkY] = 0; }
        }
        // right
        if (node.x >= chunkX * map.chunkSize + map.chunkSize - 2) {
            if (chunkX + 1 < map.chunkMaxX) { map.chunkAgeCounter[chunkX + 1, chunkY] = 0; }
        }

        // bot
        if (node.y <= chunkY * map.chunkSize) {
            if (chunkY > 0) { map.chunkAgeCounter[chunkX, chunkY - 1] = 0; }
        }
        // top
        if (node.y >= chunkY * map.chunkSize + map.chunkSize - 2) {
            if (chunkY + 1 < map.chunkMaxY) { map.chunkAgeCounter[chunkX, chunkY + 1] = 0; }
        }
    }

    void UpdateGridTypes(int chunkX, int chunkY) {
        for (int x = chunkX * map.chunkSize; x < chunkX * map.chunkSize + map.chunkSize; x++) {
            for (int y = chunkY * map.chunkSize; y < chunkY * map.chunkSize + map.chunkSize; y++) {
                Node checkedNode = controls.GetNode(x, y);

                // No change to cell type occurs
                if (checkedNode.cellType == checkedNode.cellTypeDelta && checkedNode.moved == false) {
                    checkedNode.unchangedAge += 1;

                    // increase the counter for chunk check
                    if (checkedNode.unchangedAge > 20) {
                        map.chunkAgeCounter[chunkX, chunkY] += 1;
                    }
                }
                // there was a cell type change
                else {
                    checkedNode.cellType = checkedNode.cellTypeDelta;
                    checkedNode.unchangedAge = 0;
                    map.chunkAgeCounter[chunkX, chunkY] = 0;
                }

                // reset deltas and update visuals here for optimal usage of loops
                checkedNode.stateOfMatter = nodeData.particleDataRoot[checkedNode.cellType].state;
                ResetGridDelta(checkedNode);
                UpdateGridVisuals(checkedNode);
            }
        }

        // update canvas
        map.textureInstance.Apply();
    }

    void UpdateGridVisuals(Node nodeToUpdate) {
        Color c = new Color(
            nodeData.particleDataRoot[nodeToUpdate.cellType].cellColorR,
            nodeData.particleDataRoot[nodeToUpdate.cellType].cellColorG,
            nodeData.particleDataRoot[nodeToUpdate.cellType].cellColorB,
            nodeData.particleDataRoot[nodeToUpdate.cellType].cellColorA);

        map.textureInstance.SetPixel(nodeToUpdate.x, nodeToUpdate.y, c);
    }

    void ResetGridDelta(Node nodeToReset) {
        nodeToReset.cellTypeDelta = nodeToReset.cellType;
        nodeToReset.moved = false;
    }
}

public class Node {
    public int x;
    public int y;

    public bool moved = false;

    public int velX;
    public int velY;
    public int accX;
    public int accY;

    public int unchangedAge = 0;

    public int stateOfMatter;
    public int cellType;
    public int cellTypeDelta;
}