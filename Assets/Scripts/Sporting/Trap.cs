using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    public Vector3 force;

    private GameObject clay;
    private Vector3 posClayDefault;
    private Quaternion rotClayDefault;

    private bool isReady = true;

    void Start ()
    {
        clay = transform.Find("Clay").gameObject;
        Rigidbody rb = clay.GetComponent<Rigidbody>();
        rb.isKinematic = true;

        posClayDefault = clay.transform.position;
        rotClayDefault = clay.transform.rotation;
    }

    //
    // Start target
    // 
    public void StartTarget()
    {
        if (!isReady)
        {
            Reset();
        }

        posClayDefault = clay.transform.position;
        rotClayDefault = clay.transform.rotation;

        Rigidbody rb = clay.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.AddForce(rotClayDefault * force);

        isReady = false;
        Debug.Log("-- Trap:StartTarget: " + force);
    }

    //
    // Reset
    // 
    public void Reset()
    {
        Rigidbody rb = clay.GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        clay.transform.position = posClayDefault;
        clay.transform.rotation = rotClayDefault;

        isReady = true;
        Debug.Log("-- Trap:Reset()");
    }

    //
    // Start target on mouse click
    //
    public void OnClick()
    {
        if (isReady)
        {
            StartTarget();
        }
        else
        {
            Reset();
        }
    }
}
