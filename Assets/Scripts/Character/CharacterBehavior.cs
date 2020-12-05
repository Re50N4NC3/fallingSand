using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBehavior : MonoBehaviour{
    // character is 2x5
    private MapControls mapCont;
    private GenerateMap map;
    private GameManager gameManager;

    public bool alive = true;
    public bool grounded = true;
    public bool swimming = false;

    public Transform[] solidCollider = new Transform[4];
    enum Direction { left, right, bottom, top }

    public float gravity = 0.09f;  // probably should read it from somewhere
    public float velX = 0;
    public float velY = 0;
    public float accX = 0;
    public float accY = 0;

    private void Awake() {
        map = GameObject.Find("GameManager").GetComponent<GenerateMap>();
        mapCont = GameObject.Find("GameManager").GetComponent<MapControls>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Start is called before the first frame update
    void Start(){
        
    }

    // Update is called once per frame
    void Update(){
        Gravity();
        Friction();
        MovementControls();
        VelocityEffect();
        ControlColliders();
    }

    void ControlColliders() {
        float groundCheckPos = solidCollider[(int)Direction.bottom].position.y;

        Node botNode = mapCont.GetNode(Mathf.RoundToInt(transform.position.x), (int)Mathf.Ceil(groundCheckPos));

        if (botNode != null) {
            if (botNode.stateOfMatter == (int)GameManager.StateName.solid) {
                grounded = true;

                while (true) {
                    botNode = mapCont.GetNode(
                        Mathf.RoundToInt(transform.position.x), (int)Mathf.Ceil(solidCollider[(int)Direction.bottom].position.y + 1));

                    if (botNode.stateOfMatter == (int)GameManager.StateName.solid) {
                        transform.position = new Vector2(transform.position.x, (int)Mathf.Ceil(transform.position.y) + 1);
                        velY = 0;
                        accY = 0;
                    }
                    else{
                        break;
                    }
                }
            }
            else {
                grounded = false;
            }
        }
    }

    void Gravity() {
        // pull downwards, change the acc
        if (grounded == false) {
            accY = -gravity;
        }
    }

    void Friction() {
        // slow down movement
        //if (grounded == true) {
            velX += (0 - (velX)) / (8);
            accX += (0 - (accX)) / (4);
        //}
    }

    void MovementControls() {
        // movement logic here
        if (alive == true) {
            if (Input.GetKey("d")) {
                accX = 0.052f;
            }
            if (Input.GetKey("a")) {
                accX = -0.052f;

            }
            if (grounded == true) {
                if (Input.GetKey("w")) {
                    accY = 1.0f;
                }
            }
        }
    }

    void VelocityEffect() {
        velX += accX;
        velY += accY;
        velY = Mathf.Clamp(velY, -4, 4);

        Vector3 pos = transform.position;
        pos.x += velX;
        pos.y += velY;
        transform.position = pos;
    }
}
