using System.Collections;
using UnityEngine;

public class Drone : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] int spawnRadius;
    [SerializeField] float updateInterval;
    [SerializeField] float minAttackForce;
    [SerializeField] float maxAttackForce;
    [SerializeField] float rotationSpeed;
    [SerializeField] float confusion;
    public float offset;
    WaitForSeconds attackWait;

    [Header("Explosion")]
    [SerializeField] GameObject explosion;
    Transform explosionTransform;
    [SerializeField] AudioClip[] explosionSounds;
    int explosionSoundsLength;
    public static WaitForSeconds twoSeconds;

    float attackForce;
    Quaternion targetRotation;
    public Transform target;
    Rigidbody rb;
    Rigidbody targetRb;
    Transform myTransform;
    bool collided;
    public AudioSource audioSource;

    void Awake()
    {
        explosion.SetActive(false);
        rb = GetComponent<Rigidbody>();
        myTransform = transform;
        explosionTransform = explosion.transform;
        explosionSoundsLength = explosionSounds.Length;
        attackWait = new WaitForSeconds(updateInterval);
    }

    void OnEnable()
    {
        if(targetRb == null) targetRb = target.GetComponent<Rigidbody>();
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
        rb.AddRelativeForce(new Vector3(0, 0, attackForce), ForceMode.Acceleration);
    }

    void OnCollisionEnter(Collision other)
    {
        if(collided)
        {
            return;
        }
        collided = true;
        switch(other.gameObject.layer)
        {
            case 6:
                Explode(other.GetContact(0).point, other.transform);
                break;

            default:
                Explode(other.GetContact(0).point);
                break;
        }
    }

    void OnCollisionExit(Collision other)
    {
        collided = false;
    }

    IEnumerator UpdateTarget()
    {
        //Update target position
        while(true)
        {
            Vector3 targetVelocity = targetRb.velocity;
            if(targetVelocity.magnitude > 1)
            {
                targetVelocity = targetVelocity.normalized;
            }
            targetRotation = Quaternion.LookRotation(target.position + target.forward * offset + targetVelocity * (targetRb.angularVelocity.magnitude * confusion) * updateInterval - myTransform.position);
            yield return attackWait;
        }
    }

    public void Explode(Vector3 pos, Transform parent = null)
    {
        explosionTransform.parent = parent;
        explosionTransform.position = pos;
        explosion.SetActive(true);
        audioSource.PlayOneShot(explosionSounds[Random.Range(0, explosionSoundsLength)]);
        StartCoroutine(ResetEffect(explosion, twoSeconds));
        ResetDrone();
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
        StartCoroutine(UpdateTarget());
    }

    IEnumerator ResetEffect(GameObject effect, WaitForSeconds wait)
    {
        yield return wait;
        effect.SetActive(false);
    }
}
