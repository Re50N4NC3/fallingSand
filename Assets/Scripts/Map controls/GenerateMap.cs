using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateMap : MonoBehaviour {
    public Node[,] grid;

    public int maxX;
    public int maxY;

    public Texture2D levelTex;
    public SpriteRenderer levelRenderer;
    public Texture2D textureInstance;

    public int chunkMaxX;
    public int chunkMaxY;
    public readonly int chunkSize = 64;
    public int[,] chunkAgeCounter;

    public void CreateLevel() {
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

                grid[x, y] = newNode;  // assign generated nodes
                newNode.cellType = 0;  // reset grid types
                newNode.cellTypeDelta = 0;  // reset grid types difference
            }
        }

        textureInstance.Apply();

        Rect rect = new Rect(0, 0, maxX, maxY);
        levelRenderer.sprite = Sprite.Create(textureInstance, rect, Vector2.zero, 1);
    }
}
