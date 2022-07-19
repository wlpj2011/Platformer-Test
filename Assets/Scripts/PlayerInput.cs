using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Player))] 
public class PlayerInput : MonoBehaviour
{

    Player player; // Store the player of that receives input

    void Start() {
    /*
    * Set the player that is on the same gameObject as playerInput
    */
        Debug.Log("Called Start for PlayerInput.cs" ,gameObject);
        player = GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical")); // Set the input equal to the raw axis input
        player.SetDirectionalInput(directionalInput); // Store the input in the player
        if(Input.GetKeyDown(KeyCode.Space)) {
            player.OnJumpInputDown(); // Tell the player the space key was preseed
        }
        if(Input.GetKeyUp(KeyCode.Space)) {
            player.OnJumpInputUp(); // Tell the player the space key was released
        }
    }
}
