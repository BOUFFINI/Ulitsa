using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoxT_ImputHandler : MonoBehaviour
{
    [Header("Touches correspondantes")]
    public KeyCode jump = KeyCode.Space;

    public FoxT_PlayerControler controller;
    void Update()
    {
        controller.jumpKey = Input.GetKey(jump);
        controller.jumpKeyDown = Input.GetKeyDown(jump);
        controller.jumpKeyUp = Input.GetKeyUp(jump);
        Debug.Log(controller.jumpKeyDown);
    }
}
