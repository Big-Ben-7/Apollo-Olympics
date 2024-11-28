using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        switch(otherObject.CompareTag("Drone"))
        {
            case false:
                switch(otherObject.layer)
                {
                    case 6:
                        other.GetComponent<PlayerController>().Teleport();
                        triggeredObjects.Add(otherObject);
                        break;
                    
                    case 13:
                        break;

                    default:
                        otherObject.SetActive(false);
                        break;
                }
                break;
                
            case true:
                other.GetComponent<Drone>().ResetDrone();
                break;
        }
    }

    void OnTriggerExit(Collider other)
    {
        triggeredObjects.Remove(other.gameObject);
    }
}
