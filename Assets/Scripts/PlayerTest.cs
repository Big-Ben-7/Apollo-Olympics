using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTest : MonoBehaviour
{
    [SerializeField] float speed;
    Rigidbody rb;
    bool sectionFront;
    bool sectionSide;
    bool sectionUp;
    [SerializeField] SpawnManager spawnManager;
    [SerializeField] Vector3 pos;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.AddRelativeForce(Vector3.forward * speed);
        pos = transform.position / 1000;
    }

    void OnTriggerEnter(Collider other)
    {
        //Passed section boundary
        if(other.gameObject.CompareTag("Section") && (sectionFront || sectionSide || sectionUp))
        {
            sectionFront = false;
            sectionSide = false;
            sectionUp = false;
        }
    }

    void OnTriggerExit(Collider other)
    {

        //Spawn new sections on trigger
        if(other.gameObject.CompareTag("Section"))
        {
            Vector3 sectionPos = other.transform.position;
            spawnManager.SpawnSectionsForward(sectionPos, 0);
        }
    }
}
