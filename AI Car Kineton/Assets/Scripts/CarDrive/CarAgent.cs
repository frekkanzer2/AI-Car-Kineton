using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine.SceneManagement;

public class CarAgent : Agent {

    /*
     
        SPAWNER CLASS

         */
    public class Spawn {
        public Vector3 spawnPosition;
        public Quaternion spawnRotation;
        public Spawn(Vector3 position, Quaternion rotation) {
            spawnPosition = position;
            spawnRotation = rotation;
        }
    }

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

    //vars for curve steering fix
    [Tooltip("Default value: 14. You can insert 0 and the system will automatically load 14.")]
    public float SPD_LIMIT_STEERING = 14f;
    private float SPD_LIMIT_STEERING_MAX = 28.0f;
    private float friction = 0; //difference to set in rotation for high speed -> x variable

    //Automatic cam manager
    [Tooltip("Positive: automatic camera activated | Negative: manual camera" + "\r\n" + "If positive, attach the camera controller in the following field.")]
    public bool autoCam = true;
    public GameObject camControllerObj;
    private CameraController camController;
    private Camera frontcam, backcam;

    /*
     
        DO NOT DELETE HERE
         
         */
    //Agent spawn position
    private ArrayList spawner;
    [Tooltip("Add here a fixed custom position. Leave empty for the random generator.")]
    public Vector3 customSpawnPosition;
    [Tooltip("Add here a fixed custom rotation. Leave empty for the random generator.")]
    public Quaternion customSpawnRotation;

    // Start code in initialization method

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

        //Spawn Position
        spawner = new ArrayList(); //ArrayList of Spawn objects
        if (SceneManager.GetActiveScene().name.Equals("ParkingScene")) {
            spawner.Add(new Spawn(new Vector3(18.18465f, 0.4371328f, 13.53141f), Quaternion.Euler(0, 270, 1)));
            spawner.Add(new Spawn(new Vector3(-17.45f, 0.491f, 5.19f), Quaternion.identity));
            spawner.Add(new Spawn(new Vector3(-8.73f, 0.491f, 4.71f), Quaternion.Euler(0, 180, 0)));
        }
        else if (SceneManager.GetActiveScene().name.Equals("CrosswalkScene")) {
            spawner.Add(new Spawn(new Vector3(-2.32f, -0.07f, -26.8f), Quaternion.identity));
            spawner.Add(new Spawn(new Vector3(-2.32f, -0.07f, -12f), Quaternion.identity));
        }
        //End of spawn section

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

        if (SPD_LIMIT_STEERING == 0) SPD_LIMIT_STEERING = 14.0f;
        if (autoCam) {
            camController = camControllerObj.GetComponent<CameraController>();
            frontcam = GameObject.Find("AutoCamera_Front").GetComponent<Camera>();
            backcam = GameObject.Find("AutoCamera_Behind").GetComponent<Camera>();
        }
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

        //Automatic camera
        if (autoCam) autocamManager();

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
            switchLight(true);
        }
        else if (localVelocity.z < 0.1f * (-1) && actualDirection == Direction.Acceleration) {
            brake(0.035f * 2);
            isBraking = true;
            speed = 0;
            switchLight(true);
        }
        else {
            isBraking = false;
            switchLight(false);
        }
        // Manual brake
        if (brakeAgent == 1f) {
            brake(0.035f);
            switchLight(true);
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
        float localRotation = horizontal * 35;
        float startingRotationSpeed = getVelocitySpeed(); //auto speed in velocity
        if (startingRotationSpeed > SPD_LIMIT_STEERING) {
            // auto speed in velocity > 14.0f 
            if (startingRotationSpeed > SPD_LIMIT_STEERING_MAX)
                startingRotationSpeed = SPD_LIMIT_STEERING_MAX;
            // auto speed in velocity will be set at max of 28.0f
            // startingRotationSpeed is a variable -> y
            // 14 < y <= 28
            // SPD_LIMIT_STEERING < y <= SPD_LIMIT_STEERING_MAX
            // at 14 spd correspond 35 rotation
            // at y spd correspond x rotation
            // 14 : 35 = y : x
            friction = 35 * startingRotationSpeed / SPD_LIMIT_STEERING;
            // friction is the rotation difference, so...
            friction -= 35;
        }

        //Reliability checks
        if (friction < 0) friction = 0;
        else if (friction > 0) {
            if (friction > 35) friction = 35;
            friction /= 100;
        }

        wc_fl.steerAngle = localRotation;
        wc_fr.steerAngle = localRotation;

        if (startingRotationSpeed > SPD_LIMIT_STEERING &&
            (localRotation == 35 || localRotation == 35 * (-1))) {
            brake(friction);
            switchLight(true);
        }
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

    private void autocamManager() {
        float localVelocity = getVelocitySpeed();
        if (localVelocity >= -1) camController.activateCamera(frontcam);
        else camController.activateCamera(backcam);
    }

    private void resetCar() {
        carBody.isKinematic = true;
        executeSpawn();
        speed = 0;
        // wc_fl, wc_fr, wc_bl, wc_br
        wc_fl.brakeTorque = Mathf.Infinity;
        wc_fr.brakeTorque = Mathf.Infinity;
        wc_bl.brakeTorque = Mathf.Infinity;
        wc_br.brakeTorque = Mathf.Infinity;
        carBody.velocity = Vector3.zero;
        carBody.angularVelocity = Vector3.zero;
        carBody.isKinematic = false;
    }

    private void executeSpawn() {
        Spawn mySpawn;
        if (Vector3.Distance(customSpawnPosition, Vector3.zero) == 0 &&
            (customSpawnRotation == new Quaternion(0, 0, 0, 0) || customSpawnRotation.Equals(new Quaternion(0, 0, 0, 0))))

            mySpawn = (Spawn)spawner[Random.Range(0, spawner.Count)];
        else
            mySpawn = new Spawn(customSpawnPosition, customSpawnRotation);
        transform.position = mySpawn.spawnPosition;
        transform.rotation = mySpawn.spawnRotation;
    }

    /*
     
        COLLISIONS CHECKS
         
         */

    private void OnCollisionEnter(Collision collision) {
        
        if (collision.gameObject.CompareTag("Environment Object")) {

            AddReward(-0.5f);
            EndEpisode();
        }
        else if (collision.gameObject.CompareTag("Environment Car")) {

            AddReward(-1f);
            EndEpisode();
        }
        else if (collision.gameObject.CompareTag("Human")) {

            AddReward(-1f);
            EndEpisode();
        }
        else if (collision.gameObject.CompareTag("Untagged")) {
            GameObject myParent;
            myParent = collision.gameObject.transform.parent.gameObject;
            while (true) {
                if (myParent == null) break;
                if (myParent.CompareTag("Untagged")) {
                    myParent = myParent.transform.parent.gameObject;
                    continue;
                }
                //CASES HERE
                if (myParent.CompareTag("Environment Object")){
                    AddReward(-0.5f);
                    EndEpisode();
                    break;
                }
                else if (myParent.CompareTag("Environment Car")) {

                    AddReward(-1f);
                    EndEpisode();
                    break;
                }
                else if (myParent.CompareTag("Human")) {

                    AddReward(-1f);
                    EndEpisode();
                    break;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (SceneManager.GetActiveScene().name.Equals("ParkingScene")) {
            //Collision code for Parking Scene
            if (other.gameObject.CompareTag("Checkpoint")) {
                AddReward(1f);
            } else if (other.gameObject.CompareTag("OutOfMap")) {
                AddReward(-3f);
                EndEpisode();
            }
        }
        else if (SceneManager.GetActiveScene().name.Equals("CrosswalkScene")) {
            //Collision code for Crosswalk Scene
            if (other.gameObject.CompareTag("EndGame")) {
                AddReward(3f);
                EndEpisode();
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (SceneManager.GetActiveScene().name.Equals("ParkingScene")) {
            //Collision code for Parking Scene

        }
        else if (SceneManager.GetActiveScene().name.Equals("CrosswalkScene")) {
            //Collision code for Crosswalk Scene

        }
    }

    private void OnTriggerStay(Collider other) {
        if (SceneManager.GetActiveScene().name.Equals("ParkingScene")) {
            //Collision code for Parking Scene
            if (other.bounds.Contains(GetComponent<BoxCollider>().bounds.min)
             && other.bounds.Contains(GetComponent<BoxCollider>().bounds.max)) {
                // Inside the box collider
                AddReward(3f);
                EndEpisode();
            }
        }
    }

    //Other functions
    private float getVelocitySpeed() {
        return transform.InverseTransformDirection(carBody.velocity).z;
    }

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

    private void switchLight(bool switchCommand) {
        brakeLight1R.SetActive(switchCommand);
        brakeLight1L.SetActive(switchCommand);
        brakeLight2R.SetActive(switchCommand);
        brakeLight2L.SetActive(switchCommand);
    }

    //Testing method
    private void debug_localVelocity() {
        Debug.Log(transform.InverseTransformDirection(carBody.velocity));
    }

}
