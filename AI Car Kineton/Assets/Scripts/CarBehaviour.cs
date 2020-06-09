using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarBehaviour : MonoBehaviour {

    public float staticSpeed = 5000f;
    public float brakeForce = 100000f;
    private Rigidbody carBody;
    private WheelCollider wc_fl, wc_fr, wc_bl, wc_br; // f -> front, l -> left, r -> right
    private GameObject wheel_front_left, wheel_front_right, wheel_back_left, wheel_back_right;
    private GameObject steering_wheel;

    // Start is called before the first frame update
    void Start() {
        carBody = GetComponent<Rigidbody>();
        wheel_front_left = GameObject.Find("Wheel_front_left");
        wheel_front_right = GameObject.Find("Wheel_front_right");
        wheel_back_left = GameObject.Find("Wheel_back_left");
        wheel_back_right = GameObject.Find("Wheel_back_right");
        wc_fl = wheel_front_left.GetComponent<WheelCollider>();
        wc_fr = wheel_front_right.GetComponent<WheelCollider>();
        wc_bl = wheel_back_left.GetComponent<WheelCollider>();
        wc_br = wheel_back_right.GetComponent<WheelCollider>();
    }

    // Update is called once per frame
    void Update() {

        //Acceleration and brake
        verticalMovement();

        //Rotation
        rotation();

    }

    private void verticalMovement() {
        float v = Input.GetAxis("Vertical") * staticSpeed;
        wc_bl.motorTorque = v;
        wc_br.motorTorque = v;
        if (Input.GetKey(KeyCode.Q)) {
            wc_bl.brakeTorque = brakeForce;
            wc_br.brakeTorque = brakeForce;
            wc_fl.brakeTorque = brakeForce;
            wc_fr.brakeTorque = brakeForce;
            decrementSpeed(1f, 0.035f);
        } else {
            wc_bl.brakeTorque = 0;
            wc_br.brakeTorque = 0;
            wc_fl.brakeTorque = 0;
            wc_fr.brakeTorque = 0;
        }
        if (v == 0) {
            decrementSpeed(0.06f, 0.05f);
        }
    }

    private void rotation() {
        float h = Input.GetAxis("Horizontal") * 35;
        wc_fl.steerAngle = h;
        wc_fr.steerAngle = h;
        wheel_front_left.transform.localRotation = Quaternion.Euler(0, h, 0);
        wheel_front_right.transform.localRotation = Quaternion.Euler(0, h, 0);
    }

    //Other functions
    private void decrementSpeed(float toCheck, float decrement) {
        if ((carBody.velocity.x > toCheck || carBody.velocity.x < toCheck * (-1)) || (carBody.velocity.y > toCheck || carBody.velocity.y < toCheck * (-1))
            || (carBody.velocity.z > toCheck || carBody.velocity.z < toCheck * (-1))) {
            if (carBody.velocity.x > toCheck)
                carBody.velocity = new Vector3(carBody.velocity.x - decrement, carBody.velocity.y, carBody.velocity.z);
            if (carBody.velocity.x < toCheck * (-1))
                carBody.velocity = new Vector3(carBody.velocity.x + decrement, carBody.velocity.y, carBody.velocity.z);
            if (carBody.velocity.y > toCheck)
                carBody.velocity = new Vector3(carBody.velocity.x, carBody.velocity.y - decrement, carBody.velocity.z);
            if (carBody.velocity.y < toCheck * (-1))
                carBody.velocity = new Vector3(carBody.velocity.x, carBody.velocity.y + decrement, carBody.velocity.z);
            if (carBody.velocity.z > toCheck)
                carBody.velocity = new Vector3(carBody.velocity.x, carBody.velocity.y, carBody.velocity.z - decrement);
            if (carBody.velocity.z < toCheck * (-1))
                carBody.velocity = new Vector3(carBody.velocity.x, carBody.velocity.y, carBody.velocity.z + decrement);
        }
    }

}
