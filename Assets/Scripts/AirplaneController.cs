using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirplaneController : MonoBehaviour
{

    public float throttleIncrement = 0.1f;
    public float maxThrust = 200f;
    public float responsiveness = 5f;
    public double fuel = 100;
    public GameObject acornPrefab;
    public double CurrentFuel;
    public double MaxFuel = 100;

    public HealthBar hb;

    public float lift = 135f;
    private float throttle;
    private float roll;
    private float pitch;
    private float yaw;
    
    Rigidbody rb; 

    public Transform propella;

    private float responseModifier{
        get {
            return (rb.mass / 10f)*responsiveness;
        }
    }
    

    private void Start(){
        rb = GetComponent<Rigidbody>();
        CurrentFuel = MaxFuel;
    }

    private void HandleInputs(){
        roll = Input.GetAxis("Roll");
        pitch = Input.GetAxis("Pitch");
        yaw = Input.GetAxis("Yaw");

        if (Input.GetKey(KeyCode.LeftShift)) throttle += throttleIncrement;
        else if (Input.GetKey(KeyCode.LeftControl)) throttle -= throttleIncrement;
        throttle = Mathf.Clamp(throttle, 0f, 100f);

        if (Input.GetKey(KeyCode.Space))
            Instantiate(acornPrefab, transform.position + Vector3.down * 2, Quaternion.identity);

    }

    private void Update(){
        HandleInputs();
        propella.Rotate(Vector3.right*throttle);
        hb.setFuel(CurrentFuel);
        hb.setMax(MaxFuel);
    }

    private void FixedUpdate(){
        rb.AddForce(transform.right * maxThrust * throttle);
        rb.AddTorque(transform.up * yaw * responseModifier);
        rb.AddTorque(transform.right * pitch * responseModifier);
        rb.AddTorque(-transform.forward * roll * responseModifier);

        rb.AddForce(Vector3.up * rb.velocity.magnitude * lift);
        updateFuel();
    }

    private void updateFuel(){
        CurrentFuel = CurrentFuel - (throttle/maxThrust)*0.1; 
    }
}
