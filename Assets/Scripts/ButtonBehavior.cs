using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonBehavior : MonoBehaviour
{
    public void QuitGame() {
        #if UNITY_EDITOR 
        UnityEditor.EditorApplication.isPlaying = false; // If running in the Unity Editor, just stop playing
        #endif
        
        Application.Quit(); // Otherwise, quit the application
    }
}
