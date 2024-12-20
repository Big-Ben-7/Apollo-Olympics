using UnityEngine;

public class Ring : MonoBehaviour
{
    public Transform target;
    public float hitForce;
    Transform myTransform;
    PlayerController player;
    public Quaternion rotationOffset;
    public AudioSource audioSource;
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
            otherRb.AddForce((other.transform.position - myTransform.position).normalized * hitForce, ForceMode.VelocityChange);
            otherRb.AddTorque(new Vector3(Random.Range(0f, 360), Random.Range(0f, 360), Random.Range(0f, 360)) * hitForce, ForceMode.Impulse);
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
