using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour
{
    public float maxJumpHeight = 4;
    public float minJumpHeight = 1;
    public float timeToJumpApex = 0.4f;
    float accelerationTimeAirborne = 0.2f;
    float accelerationTimeGrounded = 0.1f;
    float moveSpeed = 6;

    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;

    public float wallSlideSpeedMax = 2.5f;
    public float wallStickTime = 0.25f;
    float timeToWallUnStick;

    float gravity;
    float maxJumpVelocity;
    float minJumpVelocity;
    Vector3 velocity;
    float velocityXSmoothing;

    Controller2D controller;

    // Start is called before the first frame update
    void Start() {
        controller = GetComponent<Controller2D>();
        //print("Jump Height: " + jumpHeight + " and Time to Jump: " + timeToJumpApex);
        gravity = -(2*maxJumpHeight)/Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
        //print("Gravity: " + gravity + " and Jump Velocity: " + jumpVelocity);
    }

    // Update is called once per frame
    void Update() {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));
        int wallDirX = (controller.collisions.left)?-1:1;

        float targetVelocityx = input.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityx, ref velocityXSmoothing, controller.collisions.below?accelerationTimeGrounded:accelerationTimeAirborne);
        
        bool wallSliding = false;

        if((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0){
            wallSliding = true;
            if (velocity.y < -wallSlideSpeedMax) {
                velocity.y = -wallSlideSpeedMax;
            }
            if(timeToWallUnStick > 0) {
                velocityXSmoothing = 0;
                velocity.x = 0;

                if(input.x != wallDirX && input.x != 0) {
                    timeToWallUnStick -= Time.deltaTime;
                }
                else {
                    timeToWallUnStick = wallStickTime;
                }
            }
            else {
                timeToWallUnStick = wallStickTime;
            }
        }
        
        if(Input.GetKeyDown(KeyCode.Space)) {
            if(wallSliding) {
                if(wallDirX==input.x) {
                    velocity.x = -wallDirX * wallJumpClimb.x;
                    velocity.y = wallJumpClimb.y;
                }
                else if (input.x ==0 ) {
                    velocity.x = -wallDirX * wallJumpOff.x;
                    velocity.y = wallJumpOff.y;
                }
                else {
                    velocity.x = -wallDirX * wallLeap.x;
                    velocity.y = wallLeap.y;
                }
            }
            if(controller.collisions.below) {
                velocity.y = maxJumpVelocity;
            }
        }
        if(Input.GetKeyUp(KeyCode.Space)) {
            if (velocity.y > minJumpVelocity) {
                velocity.y = minJumpVelocity;
            }
        }

        velocity.y +=gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime, input);

        if (controller.collisions.above || controller.collisions.below) {
            velocity.y = 0;
        }
    }
}
