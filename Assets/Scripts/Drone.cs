using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class Drone : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] int spawnRadius;
    [SerializeField] float maxStartDelay;
    [SerializeField] float updateInterval;
    [SerializeField] float minAttackForce;
    [SerializeField] float maxAttackForce;
    [SerializeField] float rotationSpeed;
    //[SerializeField] float maxOffset;
    [SerializeField] float overshoot;
    [SerializeField] float confusion;
    public float offset;

    [Header("Explosion")]
    [SerializeField] float explosionForce;
    [SerializeField] float explosionRadius;
    [SerializeField] GameObject explosion;
    Transform explosionTransform;
    [SerializeField] AudioClip[] explosionSounds;
    int explosionSoundsLength;

    float attackForce;
    //Vector3 offset;
    Quaternion targetRotation;
    public Transform target;
    Rigidbody targetRb;
    Transform myTransform;
    Rigidbody rb;
    bool collided;
    public static AudioSource audioSource;

    void Awake()
    {
        explosion.SetActive(false);
        myTransform = transform;
        rb = gameObject.GetComponent<Rigidbody>();
        explosionTransform = explosion.transform;
        explosionSoundsLength = explosionSounds.Length;
    }

    void Start()
    {
        targetRb = target.GetComponent<Rigidbody>();
        StartCoroutine(UpdateTarget());
    }

    void OnEnable()
    {
        ResetDrone();
    }

    void Update()
    {
        //Rotate to target
        myTransform.rotation = Quaternion.Slerp(myTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void FixedUpdate()
    {
        //Move to target
        rb.AddRelativeForce(Vector3.forward * attackForce);
    }

    void OnCollisionEnter(Collision other)
    {
        if(collided)
        {
            return;
        }
        collided = true;
        GameObject otherObject = other.gameObject;
        switch(otherObject.layer == 6)
        {
            case true:
                explosionTransform.parent = other.transform;
                break;

            case false:
                explosionTransform.parent = null;
                break;
        }
        explosionTransform.position = other.GetContact(0).point;
        explosion.SetActive(true);
        audioSource.PlayOneShot(explosionSounds[Random.Range(0, explosionSoundsLength)]);
        StartCoroutine(ResetEffect(explosion));
        Rigidbody otherRb = otherObject.GetComponent<Rigidbody>();
        if(otherRb != null)
        {
            otherRb.AddExplosionForce(explosionForce, myTransform.position, explosionRadius, 0, ForceMode.Impulse);
        }
        ResetDrone();
    }

    void OnCollisionExit(Collision other)
    {
        collided = false;
    }

    IEnumerator UpdateTarget()
    {
        //Update target position
        rb.isKinematic = true;
        yield return new WaitForSeconds(Random.Range(0, maxStartDelay));
        rb.isKinematic = false;
        Vector3 targetVelocity;
        while(true)
        {
            switch(targetRb.velocity.magnitude)
            {
                case > 1:
                    targetVelocity = targetRb.velocity.normalized;
                    break;
                    
                default:
                    targetVelocity = targetRb.velocity;
                    break;
            }
            targetRotation = Quaternion.LookRotation(target.position + new Vector3(0, 0, offset) + targetVelocity * (overshoot + targetRb.angularVelocity.magnitude * confusion) * updateInterval - myTransform.position);
            yield return new WaitForSeconds(updateInterval);
        }
    }

    public void ResetDrone()
    {
        //Set random position/rotation around player and random attack
        StopCoroutine(UpdateTarget());
        myTransform.localPosition = new Vector3(0, 0, spawnRadius);
        Vector3 targetPos = target.position;
        myTransform.RotateAround(targetPos, Vector3.up, Random.Range(0f, 360));
        myTransform.RotateAround(targetPos, Vector3.forward, Random.Range(0f, 360));
        myTransform.RotateAround(targetPos, Vector3.right, Random.Range(0f, 360));
        attackForce = Random.Range(minAttackForce, maxAttackForce);
        //offset = new Vector3(Random.Range(-maxOffset, maxOffset), Random.Range(-maxOffset, maxOffset), Random.Range(-maxOffset, maxOffset));
        StartCoroutine(UpdateTarget());
    }

    IEnumerator ResetEffect(GameObject effect, int waitTime = 2)
    {
        yield return new WaitForSeconds(waitTime);
        effect.SetActive(false);
    }
}
