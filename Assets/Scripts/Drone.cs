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
    WaitForSeconds randomWait;
    Vector3 confusionCalc;

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
    public Rigidbody targetRb;
    public PlayerController playerController;
    Transform myTransform;
    bool collided;
    public AudioSource audioSource;
    public static bool showCaptions;
    MeshRenderer render;

    void Awake()
    {
        explosion.SetActive(false);
        rb = GetComponent<Rigidbody>();
        myTransform = transform;
        explosionTransform = explosion.transform;
        explosionSoundsLength = explosionSounds.Length;
        attackWait = new WaitForSeconds(updateInterval);
        randomWait = new WaitForSeconds(Random.Range(0f, updateInterval));
        render = gameObject.GetComponent<MeshRenderer>();
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
            
            case 14 or 15:
                Vector3 direction = target.position - myTransform.position;
                float distance = direction.sqrMagnitude;

                //No explosion if drone is too far not in camera view
                if(!render.isVisible && distance > 10000)
                {
                    ResetDrone();
                    return;
                }

                //Explode if drone is inside camera view and not occluded
                Explode(other.GetContact(0).point);
                if(offset > 0 || !showCaptions || distance > 2000000) return;

                //Show caption if drone is outmaneuvered
                if(confusionCalc.sqrMagnitude > 300000 || distance < 900000) StartCoroutine(playerController.SetDroneCaption(twoSeconds));
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
        yield return randomWait;

        //Update target position
        while(true)
        {
            Vector3 targetVelocity = targetRb.velocity;
            if(targetVelocity.sqrMagnitude > 1)
            {
                targetVelocity = targetVelocity.normalized;
            }
            confusionCalc = targetVelocity * targetRb.angularVelocity.magnitude * confusion;
            targetRotation = Quaternion.LookRotation(target.position + target.forward * offset + confusionCalc - myTransform.position);
            yield return attackWait;
        }
    }

    public void Explode(Vector3 pos, Transform parent = null)
    {
        explosionTransform.parent = parent;
        explosionTransform.position = pos;
        explosion.SetActive(true);
        StartCoroutine(ResetEffect(explosion, twoSeconds));
        audioSource.PlayOneShot(explosionSounds[Random.Range(0, explosionSoundsLength)]);
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
