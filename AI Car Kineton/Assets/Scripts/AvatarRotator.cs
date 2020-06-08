using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarRotator : MonoBehaviour
{
    private float yRot = -90;
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("prova");
        if(other.gameObject.CompareTag("HumanStop"))
        {
            
            transform.rotation = Quaternion.Euler(0, yRot, 0);

            yRot *= -1;
        }
    }
}
