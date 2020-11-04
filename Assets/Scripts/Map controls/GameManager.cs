﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    private GenerateMap map;
    private LoadJsonData jsonLoader;
    private LoadJsonData.RootNodeData nodeData;  // loaded json data for all particles is here

    // controls variables
    int brushSize = 1;
    int pickedType = 1;
    Vector3 mousePos;

    // nodes stats
    Node curNode;
    enum StateName { solid, liquid, gas, }
    public int gravity = 2;

    private void Awake() {
        map = GetComponent<GenerateMap>();
        jsonLoader = GetComponent<LoadJsonData>();
    }

    private void Start() {
        nodeData = jsonLoader.ReadNodeData();
        map.CreateLevel();
    }

    private void Update() {
        GetMousePos();
        HandleMouseInput();
        PickPlacedType();

        UpdateGrid();
    }

    void HandleMouseInput() {
        //clicked with mouse
        if (Input.GetMouseButton(0)) {
            for(int x = -brushSize; x < brushSize; x++) {
                for(int y = -brushSize; y < brushSize; y++) {
                    int targetX = x + curNode.x;
                    int targetY = y + curNode.y;
                    
                    Node node = GetNode(targetX, targetY);

                    if (node == null) {
                        continue;
                    }

                    // change cell type
                    node.cellType = pickedType;
                    node.cellTypeDelta = node.cellType;
                    node.unchangedAge = 0;
                    map.chunkAgeCounter[Mod(targetX, map.chunkSize), Mod(targetY, map.chunkSize)] = 0;
                }
            }
        }
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
                Node checkedNode = GetNode(x, y);

                Gravity(checkedNode);
                VelocityChange(checkedNode);
                VelocityEffect(checkedNode);

                // TODO here will be the logic for types and nearby cells age reset
                // logic should take two arguments, current node, and node to change
                // node to change will be determined by taking speed of the current node
                // or maybe other variables, everything should take place alongside collision checks
                // maybe those should be node functions

                //if (checkedNode.cellType == 1) {
                //    if (GetNode(x, y - 1).cellType == 0 && y > 5) {
                //        checkedNode.cellTypeDelta = 0;
                //        GetNode(x, y - 1).cellTypeDelta = 1;
                            
                //        // check if change occurs on the edge to activate other chunks
                //        if (y <= (chunkY * map.chunkSize + map.chunkSize) - 1) {
                //            if (chunkY > 0) {
                //                map.chunkAgeCounter[chunkX, chunkY - 1] = 0;
                //            }
                //        }
                //    }
                //    else {
                //        int sideRoll = Random.Range(0, 2);
                //        if (sideRoll == 0) { sideRoll = -1; }

                //        if (GetNode(x + sideRoll, y - 1).cellType == 0 && y > 5) {
                //            checkedNode.cellTypeDelta = 0;
                //            GetNode(x + sideRoll, y - 1).cellTypeDelta = 1;

                //            // check if change occurs on the edge to activate other chunks
                //            if (y <= (chunkY * map.chunkSize + map.chunkSize) - 1) {
                //                if (chunkY > 0) {
                //                    map.chunkAgeCounter[chunkX, chunkY - 1] = 0;
                //                }
                //            }

                //            // left
                //            if (x <= (chunkX * map.chunkSize)) {
                //                if (chunkX > 0) {
                //                    map.chunkAgeCounter[chunkX - 1, chunkY] = 0;
                //                }
                //            }

                //            // right
                //            if (x >= (chunkX * map.chunkSize + map.chunkSize)) {
                //                if (chunkX < map.chunkMaxX) {
                //                    map.chunkAgeCounter[chunkX + 1, chunkY] = 0;
                //                }
                //            }
                //        }

                //        if (GetNode(x - 1, y - 1).cellType == 0 && y > 5) {
                //            checkedNode.cellTypeDelta = 0;
                //            GetNode(x - 1, y - 1).cellTypeDelta = 1;

                //            // check if change occurs on the edge to activate other chunks
                //            if (y == (chunkY * map.chunkSize + map.chunkSize) && x == (chunkX * map.chunkSize + map.chunkSize)) {
                //                if (chunkX > 0) {
                //                    map.chunkAgeCounter[chunkX - 1, chunkY - 1] = 0;
                //                }
                //            }
                //        }
                //        else {
                //            if (GetNode(x + 1, y - 1).cellType == 0 && y > 5) {
                //                checkedNode.cellTypeDelta = 0;
                //                GetNode(x + 1, y - 1).cellTypeDelta = 1;

                //                // check if change occurs on the edge to activate other chunks
                //                if (y == (chunkY * map.chunkSize + map.chunkSize) && x == (chunkX * map.chunkSize + map.chunkSize)) {
                //                    if (chunkY < map.chunkMaxY && chunkY > 0) {
                //                        if (chunkX < map.chunkMaxX) {
                //                            map.chunkAgeCounter[chunkX + 1, chunkY - 1] = 0;
                //                        }
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}
                // until then, above part is scuffed and should be done faster and in more optimal way
            }
        }
    }

    void Gravity(Node node) {
        if (node.y > 0) {
            if (GetNode(node.x, node.y - 1).cellType == 0) {
                //node.accY -= gravity;
            }
        }
    }

    void VelocityChange(Node node) {
        //node.velX += node.accX;
        //node.velY += node.accY;

        if (node.y > 0) {
            if (GetNode(node.x, node.y - 1).cellType == 0) {
                node.velY = -1;
            }
            else {
                node.velY = 0;
            }
        }
    }

    void VelocityEffect(Node node) {
        // all stats should be swapped, leaving only position unchanged
        if (node.y > 2) {
            Node exchangeNode = GetNode(node.x, node.y - 1);

            map.grid[node.x, node.y].cellTypeDelta = exchangeNode.cellType;
            map.grid[exchangeNode.x, exchangeNode.y].cellTypeDelta = node.cellType;
        }

        node.x = Mathf.Clamp(node.x, 0, map.maxX);
        node.y = Mathf.Clamp(node.y, 0, map.maxY);
    }

    void UpdateGridTypes(int chunkX, int chunkY) {
        for (int x = chunkX * map.chunkSize; x < chunkX * map.chunkSize + map.chunkSize; x++) {
            for (int y = chunkY * map.chunkSize; y < chunkY * map.chunkSize + map.chunkSize; y++) {
                Node checkedNode = GetNode(x, y);

                // No change to cell type occurs
                if (checkedNode.cellType == checkedNode.cellTypeDelta) {
                    checkedNode.unchangedAge += 1;

                    // increase the counter for chunk check CHANGED HEREEEEE TO 0 FROM 1
                    if (checkedNode.unchangedAge > 2) {
                        map.chunkAgeCounter[chunkX, chunkY] += 0;
                    }
                }
                // there was a cell type change
                else {
                    checkedNode.cellType = checkedNode.cellTypeDelta;
                    checkedNode.unchangedAge = 0;
                    map.chunkAgeCounter[chunkX, chunkY] = 0;
                }

                // reset deltas and update visuals here for optimal usage of loops
                UpdateGridVisuals(checkedNode);
                ResetGridDelta(checkedNode);  
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
    }

    void GetMousePos() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        mousePos = ray.GetPoint(10);

        curNode = GetNodeFromWorldPos(mousePos);
    }

    void PickPlacedType() {
        if (Input.GetKeyDown(KeyCode.Alpha0)) { pickedType = 0; }
        if (Input.GetKeyDown(KeyCode.Alpha1)) { pickedType = 1; }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { pickedType = 2; }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { pickedType = 3; }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { pickedType = 4; }
        if (Input.GetKeyDown(KeyCode.Alpha5)) { pickedType = 5; }
        if (Input.GetKeyDown(KeyCode.Alpha6)) { pickedType = 6; }
    }

    Node GetNodeFromWorldPos(Vector3 worldPos) {
        int targetX = Mathf.RoundToInt(worldPos.x);
        int targetY = Mathf.RoundToInt(worldPos.y);

        return GetNode(targetX, targetY);
    }

    Node GetNode(int x, int y) {
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

public class Node {
    public int x;
    public int y;

    public int velX;
    public int velY;
    public int accX;
    public int accY;

    public int unchangedAge = 0;
        
    public int cellType;
    public int cellTypeDelta;
}
