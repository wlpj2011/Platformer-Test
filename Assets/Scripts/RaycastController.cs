using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent (typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour
{
    public LayerMask collisionMask; //The set of layers that the current gameObject will collide with

    public const float skinWidth = 0.015f; //The offset into the game object from which the first and last ray are fired
    const float distanceBetweenRays = 0.25f; //Upper bound on the distance between rays
    [HideInInspector]
    public int horizontalRayCount; //Number of rays fired horizontally spaced along the vertical axis
    [HideInInspector]
    public int verticalRayCount; //Number of rays fired vertically spaced along the horizontal axis

    [HideInInspector]
    public float horizontalRaySpacing; //Actual vertical distance between rays fired horizontally
    [HideInInspector]
    public float verticalRaySpacing; //Actual horizontal distance between rays fired vertically

    [HideInInspector]
    public BoxCollider2D collider; //Collider for the current gameObject
    public RaycastOrigins raycastOrigins; //Collection of useful locations to send raycasts from

    public virtual void Awake() { 
    /*
    * Awake method called before Start method even if script is disabled
    * Initializes the collider for the gameObject
    */
        collider = GetComponent<BoxCollider2D>();
    }

    public virtual void Start() {
    /*
    * Start method called once at beginning of gameObject's lifetime
    * Calculates ray spacing for the gameObject
    */
        Debug.Log("Called Start for RaycastController.cs", gameObject);
        CalculateRaySpacing();
    }

    public void UpdateRaycastOrigins() {
    /*
    * Recalculates the locations to start sending raycasts from in each direction
    */
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth*-2);
        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    } 

    public void CalculateRaySpacing() {
    /*
    * Calculates for the first time the appropriate spacings between rays based on the desired distance between rays
    */
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth*-2);

        float boundsWidth = bounds.size.x;
        float boundsHeight = bounds.size.y;

        horizontalRayCount = Mathf.RoundToInt(boundsHeight / distanceBetweenRays);
        verticalRayCount = Mathf.RoundToInt(boundsWidth / distanceBetweenRays);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    public struct RaycastOrigins {
    /*
    * Stores the ideal places to cast rays from for a box
    */
        public Vector2 topLeft,topRight;
        public Vector2 bottomLeft, bottomRight;
    }
}
