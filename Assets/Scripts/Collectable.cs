using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Transactions;
using Unity.VisualScripting;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    public Vector3 randomRot;
    Transform myTransform;

    void Awake()
    {
        myTransform = transform;
    }

    // Update is called once per frame
    void Update()
    {
        myTransform.Rotate(randomRot * Time.deltaTime);
    }
}
