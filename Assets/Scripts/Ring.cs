using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ring : MonoBehaviour
{
    public Transform target;
    public float hitForce;
    Transform myTransform;
    PlayerController player;
    public Quaternion rotationOffset;
    public static AudioSource audioSource;
    [SerializeField] AudioClip hitClip;
    void Awake()
    {
        myTransform = transform;
    }
    void Start()
    {
        player = target.GetComponent<PlayerController>();
    }

    void OnEnable()
    {
        rotationOffset = Quaternion.identity;
    }

    void OnCollisionEnter(Collision other)
    {
        if(other.transform == target)
        {
            return;
        }
        player.canCollide = false;
        Rigidbody otherRb = other.gameObject.GetComponent<Rigidbody>();
        if(!otherRb.isKinematic)
        {
            otherRb.AddForce((other.transform.position - myTransform.position) * hitForce, ForceMode.Impulse);
            otherRb.angularVelocity *= hitForce;
        }
        audioSource.PlayOneShot(hitClip);
    }

    void OnCollisionExit(Collision other)
    {
        if(other.transform == target)
        {
            return;
        }
        player.canCollide = true;
    }

    void LateUpdate()
    {
        myTransform.position = target.position;
        myTransform.rotation = target.rotation * rotationOffset;
    }
}
