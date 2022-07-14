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

    Vector2 directionalInput;
    bool wallSliding;
    int wallDirX;

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
        CalculateVelocity();
        HandleWallSliding();
        
        controller.Move(velocity * Time.deltaTime, directionalInput);

        if (controller.collisions.above || controller.collisions.below) {
            velocity.y = 0;
        }
    }

    public void SetDirectionalInput(Vector2 input) {
        directionalInput = input;
    }

    public void OnJumpInputDown() {
        if(wallSliding) {
            if(wallDirX==directionalInput.x) {
                velocity.x = -wallDirX * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
            }
            else if (directionalInput.x ==0 ) {
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

    public void OnJumpInputUp() {
        if (velocity.y > minJumpVelocity) {
            velocity.y = minJumpVelocity;
        }
    }

    void CalculateVelocity() {
        float targetVelocityx = directionalInput.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityx, ref velocityXSmoothing, controller.collisions.below?accelerationTimeGrounded:accelerationTimeAirborne);
        velocity.y +=gravity * Time.deltaTime;
    }

    void HandleWallSliding() {
        wallDirX = (controller.collisions.left)?-1:1;
        wallSliding = false;

        if((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0){
            wallSliding = true;
            if (velocity.y < -wallSlideSpeedMax) {
                velocity.y = -wallSlideSpeedMax;
            }
            if(timeToWallUnStick > 0) {
                velocityXSmoothing = 0;
                velocity.x = 0;

                if(directionalInput.x != wallDirX && directionalInput.x != 0) {
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
    }
}
