using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Player))] 
public class PlayerInput : MonoBehaviour
{

    Player player;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Called Start for PlayerInput.cs" ,gameObject);
        player = GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));
        player.SetDirectionalInput(directionalInput);
        if(Input.GetKeyDown(KeyCode.Space)) {
            player.OnJumpInputDown();
        }
        if(Input.GetKeyUp(KeyCode.Space)) {
            player.OnJumpInputUp();
        }
    }
}
