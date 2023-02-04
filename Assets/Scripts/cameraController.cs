using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraController : MonoBehaviour
{
    public Transform[] povs;
    public float speed;

    private int index = 1;
    private Vector3 target;

    private void Update(){

        if (Input.GetKeyDown(KeyCode.Alpha1)) index = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha1)) index = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha1)) index = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha1)) index = 3;

        target = povs[index].position;

    }
    
    private void FixedUpdate(){

        transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * speed);
        transform.forward = povs[index].forward;

    }
}