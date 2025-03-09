using System.Collections.Generic;
using UnityEngine;

public class Wormhole : MonoBehaviour
{
    List<GameObject> triggeredObjects = new List<GameObject>(0);
    void OnTriggerEnter(Collider other)
    {
        GameObject otherObject = other.gameObject;
        foreach(GameObject go in triggeredObjects) if(otherObject == go) return;
        if(otherObject.CompareTag("Player")) 
        {
            triggeredObjects.Add(otherObject);
            other.GetComponent<PlayerController>().Teleport();
        }
    }

    void OnTriggerExit(Collider other)
    {
        GameObject otherObject = other.gameObject;
        if(otherObject.CompareTag("Player")) triggeredObjects.Remove(otherObject);
    }
}
