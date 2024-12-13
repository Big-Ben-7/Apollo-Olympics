using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class SectionSpawner : MonoBehaviour
{

    [Header("Asteroids")]
    [SerializeField] float maxMovementForce;
    [SerializeField] float maxRotationForce;
    [SerializeField] Vector3 asteroidProportions;
    [SerializeField] float minAsteroidScale;
    [SerializeField] float maxAsteroidScale;

    [Header("Collectables")]
    [SerializeField] float minRotationSpeed;
    [SerializeField] float maxRotationSpeed;
    [SerializeField] Vector3 collectableProportions;
    [SerializeField] float minCollectableScale;
    [SerializeField] float maxCollectableScale;

    [Header("Wormholes")]
    [SerializeField] Vector3 wormholeProportions;
    [SerializeField] float minWormholeScale;
    [SerializeField] float maxWormholeScale;

    public static int side;
    public int initialDisableNumber = -1;
    public bool avoidCenter;
    //public bool initialEnable;
    Transform myTransform;
    int disableNumber;

    void Awake()
    {
        myTransform = transform;
        asteroidProportions = asteroidProportions.normalized;
        collectableProportions = collectableProportions.normalized;
        wormholeProportions = wormholeProportions.normalized;
    }

    void OnEnable()
    {
        if(initialDisableNumber == -1)
        {
            return;
        }
        disableNumber = initialDisableNumber;

        //Regular enable - spread randomization across frames, don't avoid center
        /*if(!initialEnable)
        {
            StartCoroutine(Randomize());
            return;
        }*/

        //initialEnable = false;

        //Initial enable - don't spread randomization, don't avoid center
        if(!avoidCenter)
        {
            //Initial enable - don't spread randomization across frames, avoid center
            for(int i = 3; i < myTransform.childCount; i++)
            {
                Transform childTransform = myTransform.GetChild(i);
                GameObject child = childTransform.gameObject;

                //Variable interactable amount
                if(disableNumber > 0)
                {
                    if(child.activeInHierarchy)
                    {
                        child.SetActive(false);
                    }
                    disableNumber --;
                    continue;
                }
                else if(!child.activeInHierarchy)
                {
                    child.SetActive(true);
                }

                //Random position and rotation
                childTransform.localPosition = new Vector3(Random.Range((float) -side, side), Random.Range((float) -side, side), Random.Range((float) -side, side));
                childTransform.rotation = Quaternion.Euler(Random.Range(0f, 360), Random.Range(0f, 360), Random.Range(0f, 360));

                switch(child.tag)
                {
                    case "Asteroid":
                        //Random scale
                        float newScale = Random.Range(minAsteroidScale, maxAsteroidScale);
                        childTransform.localScale = asteroidProportions * newScale;

                        //Random force and torque
                        Rigidbody rb = child.GetComponent<Rigidbody>();
                        rb.velocity = rb.angularVelocity = Vector3.zero;
                        rb.mass = newScale * 2;
                        float movementForce = Random.Range(0f, maxMovementForce);
                        float rotationForce = Random.Range(0f, maxRotationForce);
                        rb.AddRelativeForce(new Vector3(Random.Range(-1f, 1), Random.Range(-1f, 1), Random.Range(-1f, 1)).normalized * movementForce, ForceMode.VelocityChange);
                        rb.AddRelativeTorque(new Vector3(Random.Range(-1f, 1), Random.Range(-1f, 1), Random.Range(-1f, 1)).normalized * rotationForce, ForceMode.VelocityChange);
                        break;

                    case "Collectable":
                        //Random scale
                        childTransform.localScale = collectableProportions * Random.Range(minCollectableScale, maxCollectableScale);

                        //Random rotation speed
                        int randomAxis = Random.Range(0, 3);
                        float rotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
                        switch(randomAxis)
                        {
                            case 0:
                                child.GetComponent<Collectable>().randomRot = new Vector3(rotationSpeed, 0, 0);
                                break;
                            case 1:
                                child.GetComponent<Collectable>().randomRot = new Vector3(0, rotationSpeed, 0);
                                break;
                            default:
                                child.GetComponent<Collectable>().randomRot = new Vector3(0, 0, rotationSpeed);
                                break;

                        }
                        break;

                    case "Wormhole":
                        //Random scale
                        childTransform.localScale = wormholeProportions * Random.Range(minWormholeScale, maxWormholeScale);
                        break;
                }
            }
            return;
        }
        
        //Initial center section - don't spread randomization across frames, avoid center
        for(int i = 3; i < myTransform.childCount; i++)
        {
            Transform childTransform = myTransform.GetChild(i);
            GameObject child = childTransform.gameObject;

            //Variable interactable amount
            if(disableNumber > 0)
            {
                child.SetActive(false);
                disableNumber --;
                continue;
            }
            else if(!child.activeInHierarchy)
            {
                child.SetActive(true);
            }

            //Random position and rotation, avoid center
            float randomX = Random.Range(50f, side);
            float randomY = Random.Range(50f, side);
            float randomZ = Random.Range(50f, side);
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
            childTransform.localPosition = new Vector3(randomX, randomY, randomZ);
            childTransform.rotation = Quaternion.Euler(Random.Range(0f, 360), Random.Range(0f, 360), Random.Range(0f, 360));

            switch(child.tag)
            {
                case "Asteroid":
                    //Random scale
                    float newScale = Random.Range(minAsteroidScale, maxAsteroidScale);
                    childTransform.localScale = asteroidProportions * newScale;

                    //Random force and torque
                    Rigidbody rb = child.GetComponent<Rigidbody>();
                    rb.velocity = rb.angularVelocity = Vector3.zero;
                    rb.mass = newScale * 2;
                    float movementForce = Random.Range(0f, maxMovementForce);
                    float rotationForce = Random.Range(0f, maxRotationForce);
                    rb.AddRelativeForce(new Vector3(Random.Range(-1f, 1), Random.Range(-1f, 1), Random.Range(-1f, 1)).normalized * movementForce, ForceMode.VelocityChange);
                    rb.AddRelativeTorque(new Vector3(Random.Range(-1f, 1), Random.Range(-1f, 1), Random.Range(-1f, 1)).normalized * rotationForce, ForceMode.VelocityChange);
                    break;

                case "Collectable":
                    //Random scale
                    childTransform.localScale = collectableProportions * Random.Range(minCollectableScale, maxCollectableScale);

                    //Random rotation speed
                    int randomAxis = Random.Range(0, 3);
                    float rotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
                    switch(randomAxis)
                    {
                        case 0:
                            child.GetComponent<Collectable>().randomRot = new Vector3(rotationSpeed, 0, 0);
                            break;
                        case 1:
                            child.GetComponent<Collectable>().randomRot = new Vector3(0, rotationSpeed, 0);
                            break;
                        default:
                            child.GetComponent<Collectable>().randomRot = new Vector3(0, 0, rotationSpeed);
                            break;

                    }
                    break;

                case "Wormhole":
                    //Random scale
                    childTransform.localScale = wormholeProportions * Random.Range(minWormholeScale, maxWormholeScale);
                    break;
            }
        }
    }
}
