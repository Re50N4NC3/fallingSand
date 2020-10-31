using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratePixelMap : MonoBehaviour{
    
    static readonly int gridHeight = 64;
    static readonly int gridWidth = 256;
    static readonly int pixelsAmount = gridWidth * gridHeight;

    public static Texture2D texLevel;
    Texture2D texInstance = Instantiate(texLevel);

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update(){
        
    }
}
