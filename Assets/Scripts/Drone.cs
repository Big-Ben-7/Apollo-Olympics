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

    [Header("Explosion")]
    [SerializeField] GameObject explosion;
    Transform explosionTransform;
    [SerializeField] AudioClip[] explosionSounds;
    [SerializeField] string[] crashCaptions;
    int captionNumber;
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
    Vector3 confusionCalc;
    public static bool showCaptions;

    void Awake()
    {
        explosion.SetActive(false);
        rb = GetComponent<Rigidbody>();
        myTransform = transform;
        explosionTransform = explosion.transform;
        explosionSoundsLength = explosionSounds.Length;
        attackWait = new WaitForSeconds(updateInterval);
        randomWait = new WaitForSeconds(Random.Range(0f, updateInterval));
        captionNumber = crashCaptions.Length;
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
                float targetDistance = (target.position - myTransform.position).sqrMagnitude;
                if(showCaptions && ((confusionCalc.sqrMagnitude >= 300000 && target.InverseTransformPoint(myTransform.position).z > 0 && targetDistance < 2300000) || (targetDistance < 500000)))
                {
                    StartCoroutine(playerController.SetCaption(crashCaptions[Random.Range(0, captionNumber)], twoSeconds, true));
                    Explode(other.GetContact(0).point);
                }
                else Explode(other.GetContact(0).point, null, false);
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

    public void Explode(Vector3 pos, Transform parent = null, bool playSound = true)
    {
        explosionTransform.parent = parent;
        explosionTransform.position = pos;
        explosion.SetActive(true);
        if(playSound) audioSource.PlayOneShot(explosionSounds[Random.Range(0, explosionSoundsLength)]);
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
