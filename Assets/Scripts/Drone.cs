using System.Collections;
using UnityEngine;

public class Drone : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] int spawnRadius;
    [SerializeField] float minAttackForce;
    [SerializeField] float maxAttackForce;
    [SerializeField] float confusion;
    [SerializeField] float maxWait;
    [SerializeField] float rotationSpeed;
    WaitForSeconds randomWait;
    Vector3 randomOffset;
    public float offset;
    Vector3 confusionCalc;
    bool waiting;
    public bool tutorialSpawn;

    [Header("Explosion")]
    [SerializeField] GameObject explosion;
    Transform explosionTransform;
    [SerializeField] AudioClip[] explosionSounds;
    int explosionSoundsLength;
    public static WaitForSeconds twoSeconds;

    float attackForce;
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
        randomWait = new WaitForSeconds(Random.Range(0f, maxWait));
        randomOffset = new Vector3(Random.Range(-2f, 2), Random.Range(-2f, 2), Random.Range(-2f, 2));
        render = gameObject.GetComponent<MeshRenderer>();
    }

    void OnEnable()
    {
        ResetDrone();
    }

    void Update()
    {
        if(waiting) return;

        //Update target position
        Vector3 targetVelocity = targetRb.velocity;
        if(targetVelocity.sqrMagnitude > 1)
        {
            targetVelocity = targetVelocity.normalized;
        }
        confusionCalc = targetVelocity * targetRb.angularVelocity.magnitude * confusion;
        myTransform.rotation = Quaternion.Slerp(myTransform.rotation, Quaternion.LookRotation(target.position + randomOffset + target.forward * offset + confusionCalc - myTransform.position), rotationSpeed * Time.deltaTime);
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
        switch(other.gameObject.tag)
        {
            case "Player":
                Explode(other.GetContact(0).point, other.transform);
                break;
            
            case "Drone" or "Asteroid":
                Vector3 direction = target.position - myTransform.position;
                float distance = direction.sqrMagnitude;

                //No explosion if drone is too far not in camera view
                if(!render.isVisible && distance > 10000)
                {
                    ResetDrone();
                    return;
                }

                //Explode if drone is inside camera view and not occluded
                float confusionMag = confusionCalc.sqrMagnitude;
                if(tutorialSpawn)
                {
                    if(confusionMag > 300000 || distance < 600000)
                    {
                        tutorialSpawn = false;
                        playerController.dronesConfused = true;
                    }
                    Explode(myTransform.position);
                    return;
                }
                Explode(myTransform.position);
                if(confusionMag < 1000 || offset > 0 || !showCaptions) return;

                //Show caption if drone is outmaneuvered
                if(confusionMag > 300000 || distance < 600000) StartCoroutine(playerController.SetDroneCaption(twoSeconds));
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

    public void Explode(Vector3 pos, Transform parent = null)
    {
        if(explosionTransform.parent != parent) explosionTransform.parent = parent;
        if(explosionTransform.localScale != new Vector3(50, 50, 50)) explosionTransform.localScale = new Vector3(50, 50, 50);
        explosionTransform.position = pos;
        explosion.SetActive(true);
        StartCoroutine(ResetEffect(explosion, twoSeconds));
        audioSource.PlayOneShot(explosionSounds[Random.Range(0, explosionSoundsLength)]);
        ResetDrone();
    }

    public void ResetDrone()
    {
        //Set random position/rotation around player and random attack
        waiting = true;
        Vector3 targetPos = target.position;
        if(!tutorialSpawn)
        {
            attackForce = Random.Range(minAttackForce, maxAttackForce);
            myTransform.localPosition = new Vector3(0, 0, spawnRadius);
            myTransform.RotateAround(targetPos, target.forward, Random.Range(0f, 360));
            myTransform.RotateAround(targetPos, target.right, Random.Range(0f, 360));
            myTransform.RotateAround(targetPos, target.up, Random.Range(0f, 360));
            StartCoroutine(Wait());
        }
        else
        {
            attackForce = minAttackForce;
            myTransform.localPosition = new Vector3(0, 0, 1000);
            myTransform.RotateAround(targetPos, target.forward, Random.Range(0f, 360));
            myTransform.RotateAround(targetPos, target.right, Random.Range(-45f, 45));
            myTransform.RotateAround(targetPos, target.up, Random.Range(-45f, 45));
            waiting = false;
        }
    }

    IEnumerator Wait()
    {
        yield return randomWait;
        waiting = false;
    }

    IEnumerator ResetEffect(GameObject effect, WaitForSeconds wait)
    {
        yield return wait;
        effect.SetActive(false);
    }
}
