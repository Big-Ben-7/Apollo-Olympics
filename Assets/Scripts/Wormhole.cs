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
        if(!otherObject.CompareTag("Drone"))
        {
            switch(otherObject.layer)
            {
                case 6:
                    other.GetComponent<PlayerController>().Teleport();
                    triggeredObjects.Add(otherObject);
                    return;
                
                case not 13:
                    otherObject.SetActive(false);
                    return;
            }
            return;
        }
        other.GetComponent<Drone>().ResetDrone();
    }

    void OnTriggerExit(Collider other)
    {
        triggeredObjects.Remove(other.gameObject);
    }
}
