using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class CarAgent : Agent {

    public float staticSpeed = 5000f;
    public float brakeForce = 100000f;
    private float speed = 0;
    private Rigidbody carBody;
    private WheelCollider wc_fl, wc_fr, wc_bl, wc_br; // f -> front, l -> left, r -> right
    private GameObject wheel_front_left, wheel_front_right, wheel_back_left, wheel_back_right;
    private GameObject steering_wheel;
    private GameObject brakeLight1R, brakeLight1L, brakeLight2R, brakeLight2L;
    private Direction lastDirection, actualDirection;
    private bool isBraking = false;
    Vector3 myPosition; Quaternion myRotation;
    //variables for agent
    private Vector3 spawnPosition;

    // Start code in initialization method

    // Update is called once per frame
    void Update() {

        

    }

    //Enumarator
    private enum Direction {
        Acceleration,
        Backward,
        Stop
    }

    /*
                AGENT FUNCTIONS
         */

    //Initialization
    public override void Initialize() {
        lastDirection = Direction.Stop;
        myPosition = new Vector3(0, 0, 0);
        myRotation = new Quaternion(0, 0, 0, 0);
        carBody = GetComponent<Rigidbody>();
        wheel_front_left = GameObject.Find("Wheel_front_left");
        wheel_front_right = GameObject.Find("Wheel_front_right");
        wheel_back_left = GameObject.Find("Wheel_back_left");
        wheel_back_right = GameObject.Find("Wheel_back_right");
        brakeLight1R = GameObject.Find("BrakeLight1_dx");
        brakeLight1L = GameObject.Find("BrakeLight1_sx");
        brakeLight2R = GameObject.Find("BrakeLight2_dx");
        brakeLight2L = GameObject.Find("BrakeLight2_sx");
        wc_fl = wheel_front_left.GetComponentInParent<WheelCollider>();
        wc_fr = wheel_front_right.GetComponentInParent<WheelCollider>();
        wc_bl = wheel_back_left.GetComponentInParent<WheelCollider>();
        wc_br = wheel_back_right.GetComponentInParent<WheelCollider>();
        //agent initialization
        spawnPosition = transform.position;
        var statsRecorder = Academy.Instance.StatsRecorder;
        statsRecorder.Add("MyMetric", 1.0f);
    }

    public override void OnEpisodeBegin() {
        resetCar();
    }

    public override void CollectObservations(VectorSensor sensor) {
        sensor.AddObservation(transform.position);
        sensor.AddObservation(transform.rotation);
        //saving wheels rotations
        sensor.AddObservation(wheel_front_left.transform.rotation);
        sensor.AddObservation(wheel_front_right.transform.rotation);
        sensor.AddObservation(wheel_back_left.transform.rotation);
        sensor.AddObservation(wheel_back_right.transform.rotation);
    }

    public override void Heuristic(float[] actionsOut) {
        actionsOut[0] = Input.GetAxis("Horizontal");
        actionsOut[1] = Input.GetAxis("Vertical");
        if (Input.GetKey(KeyCode.Q))
            actionsOut[2] = 1f;
        else actionsOut[2] = 0f;
    }

    public override void OnActionReceived(float[] vectorAction) {

        //Acceleration and brake
        verticalMovement(vectorAction[1], vectorAction[2]);

        //Rotation
        rotation(vectorAction[0]);
        followWheelRotation();

    }

    /*
                CUSTOM FUNCTIONS
         */

    private void verticalMovement(float vertical, float brakeAgent) {

        /*
            Car uses speed to move.
            The v parameter is an acceleration gain on the speed.
         */

        // Getting acceleration
        float v = vertical * staticSpeed;
        if (v > 1000) v = 1000;
        if (v < -1000) v = -1000;
        speed += v;
        // Checking acceleration limit
        if (speed > staticSpeed) speed = staticSpeed;
        if (speed < staticSpeed * (-1)) speed = staticSpeed * (-1);
        // Getting direction
        float temp = 0;
        if (vertical <= 1f && vertical > 0f) temp = 1;
        else if (vertical >= -1 && vertical < 0f) temp = -1;
        else if (vertical == 0) temp = 0;
        switch (temp) {
            case 1:
                actualDirection = Direction.Acceleration;
                break;
            case -1:
                actualDirection = Direction.Backward;
                break;
            case 0:
                actualDirection = Direction.Stop;
                break;
        }
        // Getting localVelocity
        Vector3 localVelocity = transform.InverseTransformDirection(carBody.velocity);
        if (localVelocity.z > 0.1f && actualDirection == Direction.Backward) {
            brake(0.035f);
            isBraking = true;
            speed = 0;
            brakeLight1R.SetActive(true);
            brakeLight1L.SetActive(true);
            brakeLight2R.SetActive(true);
            brakeLight2L.SetActive(true);
        }
        else if (localVelocity.z < 0.1f * (-1) && actualDirection == Direction.Acceleration) {
            brake(0.035f * 2);
            isBraking = true;
            speed = 0;
            brakeLight1R.SetActive(true);
            brakeLight1L.SetActive(true);
            brakeLight2R.SetActive(true);
            brakeLight2L.SetActive(true);
        }
        else {
            isBraking = false;
            brakeLight1R.SetActive(false);
            brakeLight1L.SetActive(false);
            brakeLight2R.SetActive(false);
            brakeLight2L.SetActive(false);
        }
        // Manual brake
        if (brakeAgent == 1f) {
            brake(0.035f);
            brakeLight1R.SetActive(true);
            brakeLight1L.SetActive(true);
            brakeLight2R.SetActive(true);
            brakeLight2L.SetActive(true);
        }
        else if (!isBraking) {
            wc_bl.brakeTorque = 0;
            wc_br.brakeTorque = 0;
            wc_fl.brakeTorque = 0;
            wc_fr.brakeTorque = 0;
            wc_bl.motorTorque = speed;
            wc_br.motorTorque = speed;
        }
        // Idle
        if (actualDirection == Direction.Stop) brake(0.005f);
        if (localVelocity == Vector3.zero) {
            speed = 0;
            v = 0;
            actualDirection = Direction.Stop;
        }
    }

    private void rotation(float horizontal) {
        float h = horizontal * 35;
        wc_fl.steerAngle = h;
        wc_fr.steerAngle = h;
    }

    private void followWheelRotation() {
        //front right
        wc_fr.GetWorldPose(out myPosition, out myRotation);
        wheel_front_right.transform.position = myPosition;
        wheel_front_right.transform.rotation = myRotation;
        //front left
        wc_fl.GetWorldPose(out myPosition, out myRotation);
        wheel_front_left.transform.position = myPosition;
        wheel_front_left.transform.rotation = myRotation;
        //back right
        wc_br.GetWorldPose(out myPosition, out myRotation);
        wheel_back_right.transform.position = myPosition;
        wheel_back_right.transform.rotation = myRotation;
        //back left
        wc_bl.GetWorldPose(out myPosition, out myRotation);
        wheel_back_left.transform.position = myPosition;
        wheel_back_left.transform.rotation = myRotation;
    }

    private void brake(float decrement) {
        wc_bl.brakeTorque = brakeForce;
        wc_br.brakeTorque = brakeForce;
        wc_fl.brakeTorque = brakeForce;
        wc_fr.brakeTorque = brakeForce;
        decrementSpeed(1f, decrement);
    }

    private void resetCar() {
        transform.position = spawnPosition;
        transform.rotation = Quaternion.identity;
        speed = 0;
        carBody.velocity = Vector3.zero;
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
