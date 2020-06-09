using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarBehaviour : MonoBehaviour {

    public float staticSpeed = 5000f;
    public float brakeForce = 100000f;
    private float decrement = 0.05f;
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
            wc_bl.brakeTorque = brakeForce;
            wc_br.brakeTorque = brakeForce;
            wc_fl.brakeTorque = brakeForce;
            wc_fr.brakeTorque = brakeForce;
            if ((carBody.velocity.x > 1 || carBody.velocity.x < -1) || (carBody.velocity.y > 1 || carBody.velocity.y < -1) || (carBody.velocity.z > 1 || carBody.velocity.z < -1)) {
                Debug.Log("Breaking");
                if (carBody.velocity.x > 1)
                    carBody.velocity = new Vector3(carBody.velocity.x - decrement, carBody.velocity.y, carBody.velocity.z);
                if (carBody.velocity.x < -1)
                    carBody.velocity = new Vector3(carBody.velocity.x + decrement, carBody.velocity.y, carBody.velocity.z);
                if (carBody.velocity.y > 1)
                    carBody.velocity = new Vector3(carBody.velocity.x, carBody.velocity.y - decrement, carBody.velocity.z);
                if (carBody.velocity.y < -1)
                    carBody.velocity = new Vector3(carBody.velocity.x, carBody.velocity.y + decrement, carBody.velocity.z);
                if (carBody.velocity.z > 1)
                    carBody.velocity = new Vector3(carBody.velocity.x, carBody.velocity.y, carBody.velocity.z - decrement);
                if (carBody.velocity.z < -1)
                    carBody.velocity = new Vector3(carBody.velocity.x, carBody.velocity.y, carBody.velocity.z + decrement);
            }
        } else {
            wc_bl.brakeTorque = 0;
            wc_br.brakeTorque = 0;
            wc_fl.brakeTorque = 0;
            wc_fr.brakeTorque = 0;
        }

    }

}
