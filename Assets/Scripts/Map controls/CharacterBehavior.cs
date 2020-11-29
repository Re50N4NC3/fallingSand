using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBehavior : MonoBehaviour{
    // character is 3x5
    private MapControls mapCont;
    private GenerateMap map;

    bool alive = true;
    bool grounded = false;
    bool swimming = false;

    public GameObject[] solidCollider = new GameObject[4];  // maybe it should be only position
    enum Direction { left, right, bottom, top }

    float gravity = 1;  // probably should read it from somewhere
    float velX = 0;
    float velY = 0;
    float accX = 0;
    float accY = 0;

    private void Awake() {
        map = GetComponent<GenerateMap>();
        mapCont = GetComponent<MapControls>();
    }

    // Start is called before the first frame update
    void Start(){
        
    }

    // Update is called once per frame
    void Update(){
        ControlColliders();
        Gravity();
        MovementControls();
    }

    void ControlColliders() {
        // snap colliders to the grid
    }

    void Gravity() {
        // pull downwards, change the acc
    }

    void MovementControls() {
        // movement logic here
    }
}
