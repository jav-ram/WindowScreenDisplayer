using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float speed = 4;
    public float accelerometerUpdateInterval = 1.0f / 60.0f;
    public float lowPassKernelWidthInSeconds = 1.0f;
    public float lowPassFilterFactor;
    private Vector3 movement;
    private Rigidbody rb;
    private Vector3 lowPassValue;
    private float pastAccelerationMagnitude;
    private float currentAccelerationMagnitude;
    private float accelerationDifference; 

    void Start () {
        rb = gameObject.GetComponent<Rigidbody>();
        lowPassValue = Vector3.zero;
        pastAccelerationMagnitude = 9.9f;
        lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;
    }

    Vector3 LowPassFilterAccelerometer() {
        // Smooths out noise from accelerometer data
        lowPassValue = Vector3.Lerp(lowPassValue, Input.acceleration, lowPassFilterFactor);
        return lowPassValue;
    }

    void FixedUpdate () {
        movement = Vector3.zero;
        currentAccelerationMagnitude = LowPassFilterAccelerometer().magnitude;
        accelerationDifference = Mathf.Abs(pastAccelerationMagnitude - currentAccelerationMagnitude);
        
        // Moves player in the direction of the camera
        if ( accelerationDifference > .0015 && .004 > accelerationDifference ) {
            movement = Camera.main.transform.forward;
        } 

        pastAccelerationMagnitude = Mathf.Abs(currentAccelerationMagnitude);

        // Maps the player's real-world steps into in-game movement
        rb.AddForce(movement * speed);
    }
}
