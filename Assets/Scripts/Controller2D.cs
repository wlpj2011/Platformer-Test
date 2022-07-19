using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Controller2D : RaycastController
{
    public float maxSlopeAngle = 80; // Maximum angle at which a slope can be walked up. Any higher angle and the gameObject will just slide down

    public CollisionInfo collisions; // Stores all info about the collisions a gameObject is experiencing
    [HideInInspector]
    public Vector2 playerInput; //Stores player input for the current frame

    public override void Start() {
    /*
    * Start method called once at beginning of gameObject's lifetime
    */
        base.Start(); // Calculates ray spacing for the gameObject
        Debug.Log("Called Start for Controller2D.cs" ,gameObject);
        collisions.faceDir = 1; //Sets facing direction to right
    }

    public void Move(Vector2 moveAmount, bool standingOnPlatform = false) {
        Move(moveAmount, Vector2.zero, standingOnPlatform); //Calls move with no input
    }

    public void Move(Vector2 moveAmount, Vector2 input, bool standingOnPlatform = false) {
    /*
    * Moves the gameObject by moveAmount after correcting for upcoming collisions
    */
        collisions.Reset(); //Resets to the standard no-collisions state
        collisions.moveAmountOld = moveAmount; //Stores the distance moved previous frame
        playerInput = input; //renames input

        UpdateRaycastOrigins(); 

        if(moveAmount.y < 0 ) {
            DescendSlope(ref moveAmount); // If moving downwards, checks if will contact a slope and the appropriate way to move down that slope
        }

        if(moveAmount.x != 0) {
            collisions.faceDir = (int)Mathf.Sign(moveAmount.x); // If not standing still, sets the facing direction appropriately
        }
        
        HorizontalCollisions(ref moveAmount); // Checks for collisions in the horizontal direction and appropriately adjust the distance to move

        if(moveAmount.y != 0){
            VerticalCollisions(ref moveAmount); // If moving vertically checks for collisions in the vertical direction and appropriately adjust the distance to move
        }        

        transform.Translate(moveAmount); // Translate the gameObject by the adjusted moveAmount

        if(standingOnPlatform) {
            collisions.below = true; // If standing on a platform, mark that there is a collision below the gameObject
        }
    }

    void HorizontalCollisions(ref Vector2 moveAmount) {
    /*
    * Takes in a reference to the amount that the gameObject is planning to move and adjusts the moveAmount
    * based on the projected collisions
    */
        float directionX = collisions.faceDir; // Sets the direction along the x-axis to be the direction the gameObject is facing
        float rayLength = Mathf.Abs(moveAmount.x) + skinWidth; // Sets the length of the ray to be cast to be the distance planned to move + the distance from the ray origin to the gameObject surface

        if(Mathf.Abs(moveAmount.x) < skinWidth) {
            rayLength = 2* skinWidth; // Increases the ray length to 2*skinWidth if it is less that 2*skinWidth
        }

        for ( int i = 0; i < horizontalRayCount; i++){
            Vector2 rayOrigin = (directionX == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight; // Picks the bottom left to cast the first ray from if moving left and bottom right if moving right
            rayOrigin += Vector2.up * (horizontalRaySpacing * i); // Shifts up the raycast origin to space out rays
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask); // Cast ray of length rayLength and record if it hit anything in the collision mask

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red); // Draw rays if desired

            if (hit) {

                if(hit.distance == 0) {
                    continue; // If gameObject is intersecting with the colliding object, make no adjustments to moveAmount
                }

                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up); // Calculate the slope angle of the object that will be collided with

                if(i==0 && slopeAngle <= maxSlopeAngle) {
                    if(collisions.descendingSlope) {
                        collisions.descendingSlope = false;
                        moveAmount = collisions.moveAmountOld;
                    }
                    float distanceToSlopeStart = 0;
                    if(slopeAngle != collisions.slopeAngleOld){
                        distanceToSlopeStart = hit.distance - skinWidth;
                        moveAmount.x -=distanceToSlopeStart * directionX;
                    }
                    ClimbSlope(ref moveAmount, slopeAngle, hit.normal);
                    moveAmount.x += distanceToSlopeStart * directionX;
                }
                if(!collisions.climbingSlope || slopeAngle > maxSlopeAngle){
                    moveAmount.x = (hit.distance - skinWidth) * directionX;
                    rayLength = hit.distance;

                    if(collisions.climbingSlope) {
                        moveAmount.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x);
                    }

                    collisions.left = directionX == -1;
                    collisions.right = directionX == 1;
                }
                
            }
        }
    }

    void VerticalCollisions(ref Vector2 moveAmount) {
        float directionY = Mathf.Sign(moveAmount.y);
        float rayLength = Mathf.Abs(moveAmount.y) + skinWidth;

        for ( int i = 0; i < verticalRayCount; i++){
            Vector2 rayOrigin = (directionY == -1)?raycastOrigins.bottomLeft:raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

            if (hit) {

                if(hit.collider.tag == "Through") {
                    if (directionY == 1 || hit.distance == 0) {
                        continue;
                    }
                    if(collisions.fallingThroughPlatform) {
                        continue;
                    }
                    if (playerInput.y == -1) {
                        collisions.fallingThroughPlatform = true;
                        Invoke("ResetFallingThroughPlatform",0.5f);
                        continue;
                    }
                }
                moveAmount.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;

                if(collisions.climbingSlope) {
                    moveAmount.x =  moveAmount.y * Mathf.Sign(moveAmount.x)/Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad);
                }
                collisions.below = directionY == -1;
                collisions.above = directionY == -1;
            }
        }

        if(collisions.climbingSlope) {
            float directionX = Mathf.Sign(moveAmount.x);
            rayLength = Mathf.Abs(moveAmount.x) + skinWidth;
            Vector2 rayOrigin = ((directionX==-1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight) + Vector2.up * moveAmount.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (hit) {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if(slopeAngle != collisions.slopeAngle){
                    moveAmount.x = (hit.distance - skinWidth) * directionX;
                    collisions.slopeAngle = slopeAngle;
                    collisions.slopeNormal = hit.normal;
                }
            }
        }
    }

    void ClimbSlope(ref Vector2 moveAmount, float slopeAngle, Vector2 slopeNormal) {
        float moveDistance = Mathf.Abs(moveAmount.x);
        float climbmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad)*moveDistance;
        if(moveAmount.y <= climbmoveAmountY){
            moveAmount.y = climbmoveAmountY;
            moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad)*moveDistance * Mathf.Sign(moveAmount.x);
            collisions.below = true;
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
            collisions.slopeNormal = slopeNormal;
        }
    }

    void DescendSlope(ref Vector2 moveAmount) {

        RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(raycastOrigins.bottomLeft, Vector2.down, Mathf.Abs(moveAmount.y)+skinWidth, collisionMask);
        RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(raycastOrigins.bottomRight, Vector2.down, Mathf.Abs(moveAmount.y)+skinWidth, collisionMask);
        if(maxSlopeHitLeft ^ maxSlopeHitRight) {
            SlideDownMaxSlope(maxSlopeHitLeft,ref moveAmount);
            SlideDownMaxSlope(maxSlopeHitRight,ref moveAmount);
        }

        if(!collisions.slidingDownMaxSlope) {
            float directionX = Mathf.Sign(moveAmount.x);
            Vector2 rayOrigin = (directionX == -1)?raycastOrigins.bottomRight:raycastOrigins.bottomLeft;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up,Mathf.Infinity, collisionMask);

            if (hit) {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if(slopeAngle != 0 && slopeAngle <= maxSlopeAngle) {
                    if (Mathf.Sign(hit.normal.x) ==directionX ){
                        if( (hit.distance - skinWidth) <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad)*Mathf.Abs(moveAmount.x)) {
                            float moveDistance = Mathf.Abs(moveAmount.x);
                            float descendmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad)*moveDistance;
                            moveAmount.y -= descendmoveAmountY;
                            moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad)*moveDistance * Mathf.Sign(moveAmount.x);

                            collisions.slopeAngle = slopeAngle;
                            collisions.descendingSlope = true;
                            collisions.below = true;
                            collisions.slopeNormal = hit.normal;
                        }
                    }
                }
            }
        }
    }

    void SlideDownMaxSlope(RaycastHit2D hit, ref Vector2 moveAmount) {
        if(hit) {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if(slopeAngle > maxSlopeAngle) {
                moveAmount.x = hit.normal.x * (Mathf.Abs(moveAmount.y) - hit.distance)/Mathf.Tan(slopeAngle*Mathf.Deg2Rad);
                collisions.slopeAngle = slopeAngle;
                collisions.slidingDownMaxSlope = true;
                collisions.slopeNormal = hit.normal;
            }
        }
    }

    void ResetFallingThroughPlatform() {
        collisions.fallingThroughPlatform = false;
    }

    public struct CollisionInfo {
    /*
    * Structure storing info about the collisions the gameObject is experiencing. 
    * This includes bools telling which sides have collisions
    * How bools telling how the gameObject is interacting with slopes
    * Information about the current and previous slope and movement
    * Facing information
    * Comes with a function to reset everything appropriately each frame
    */
        public bool above, below;
        public bool left, right;
        
        public bool climbingSlope;
        public bool descendingSlope;
        public bool slidingDownMaxSlope;

        public float slopeAngle, slopeAngleOld;
        public Vector2 slopeNormal;
        public Vector2 moveAmountOld;
        public int faceDir;
        public bool fallingThroughPlatform;

        public void Reset() {
            above = below = false;
            left = right = false;
            climbingSlope = false;
            descendingSlope = false;
            slidingDownMaxSlope = false;
            slopeNormal = Vector2.zero;

            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }
}
