using System.Collections.Generic;
using UnityEngine;

public class Wormhole : MonoBehaviour
{
    List<GameObject> triggeredObjects = new List<GameObject>(0);
    void OnTriggerEnter(Collider other)
    {
        GameObject otherObject = other.gameObject;
        foreach(GameObject go in triggeredObjects)
        {
            if(otherObject == go)
            {
                return;
            }
        }
        switch(otherObject.tag)
        {
            case "Player":
                triggeredObjects.Add(otherObject);
                other.GetComponent<PlayerController>().Teleport();
                break;

            case "Drone":
                other.GetComponent<Drone>().ResetDrone();
                break;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject.CompareTag("Player")) triggeredObjects.Remove(other.gameObject);
    }
}
