using System.Collections;
using UnityEngine;

public class SectionSpawner : MonoBehaviour
{
    [SerializeField] float maxMovementForce;
    [SerializeField] float maxRotationForce;
    public static int side;
    public int disableNumber;
    public bool avoidCenter;
    public GameObject[] children;
    public static int interactableNumber;

    void OnEnable()
    {
        if(children.Length == 0)
        {
            return;
        }

        if(!avoidCenter)
        {
            for(int i = 0; i < interactableNumber; i++)
            {
                GameObject child = children[i];

                //Variable interactable amount
                if(i < disableNumber)
                {
                    if(child.activeInHierarchy)
                    {
                        child.SetActive(false);
                    }
                    continue;
                }
                if(!child.activeInHierarchy)
                {
                    child.SetActive(true);
                }
                child.transform.localPosition = new Vector3(Random.Range((float) -side, side), Random.Range((float) -side, side), Random.Range((float) -side, side));

                //Random force
                if(child.CompareTag("Wormhole"))
                {
                    continue;
                }
                Rigidbody rb = child.GetComponent<Rigidbody>();
                rb.velocity = rb.angularVelocity = Vector3.zero;
                rb.AddRelativeForce(Random.Range(-maxMovementForce, maxMovementForce), Random.Range(-maxMovementForce, maxMovementForce), Random.Range(-maxMovementForce, maxMovementForce), ForceMode.VelocityChange);
                rb.AddRelativeTorque(Random.Range(-maxRotationForce, maxRotationForce), Random.Range(-maxRotationForce, maxRotationForce), Random.Range(-maxRotationForce, maxRotationForce), ForceMode.VelocityChange);
            }
        }

        else
        {
            for(int i = 0; i < interactableNumber; i++)
            {
                GameObject child = children[i];

                //Variable interactable amount
                if(i < disableNumber)
                {
                    child.SetActive(false);
                    continue;
                }

                //Random position and rotation, avoid center
                float randomX = Random.Range(200, side);
                float randomY = Random.Range(200, side);
                float randomZ = Random.Range(200, side);
                if(Random.Range(0, 2) == 0)
                {
                    randomX *= -1;
                }
                if(Random.Range(0, 2) == 0)
                {
                    randomY *= -1;
                }
                if(Random.Range(0, 2) == 0)
                {
                    randomZ *= -1;
                }
                child.transform.localPosition = new Vector3(randomX, randomY, randomZ);
                
                //Random force
                if(child.CompareTag("Wormhole"))
                {
                    continue;
                }
                Rigidbody rb = child.GetComponent<Rigidbody>();
                rb.AddRelativeForce(Random.Range(-maxMovementForce, maxMovementForce), Random.Range(-maxMovementForce, maxMovementForce), Random.Range(-maxMovementForce, maxMovementForce), ForceMode.VelocityChange);
                rb.AddRelativeTorque(Random.Range(-maxRotationForce, maxRotationForce), Random.Range(-maxRotationForce, maxRotationForce), Random.Range(-maxRotationForce, maxRotationForce), ForceMode.VelocityChange);
            }
        }
    }
}
