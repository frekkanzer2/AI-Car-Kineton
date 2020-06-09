using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarBehaviour : MonoBehaviour {

    public float staticSpeed = 5000f;
    private Rigidbody carBody;
    private WheelCollider wc_fl, wc_fr, wc_bl, wc_br; // f -> front, l -> left, r -> right

    // Start is called before the first frame update
    void Start() {
        carBody = GetComponent<Rigidbody>();
        wc_fl = GameObject.Find("Wheel_front_left").GetComponent<WheelCollider>();
        wc_fr = GameObject.Find("Wheel_front_right").GetComponent<WheelCollider>();
        wc_bl = GameObject.Find("Wheel_back_left").GetComponent<WheelCollider>();
        wc_br = GameObject.Find("Wheel_back_right").GetComponent<WheelCollider>();
    }

    // Update is called once per frame
    void Update() {

        float v = Input.GetAxis("Vertical") * staticSpeed;
        wc_bl.motorTorque = v;
        wc_br.motorTorque = v;
        if (Input.GetKey(KeyCode.Q)) {
            wc_bl.brakeTorque = staticSpeed * 100;
            wc_br.brakeTorque = staticSpeed * 100;
        } else {
            wc_bl.brakeTorque = 0;
            wc_br.brakeTorque = 0;
        }

    }

}
