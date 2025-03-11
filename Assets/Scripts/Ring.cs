using UnityEngine;

public class Ring : MonoBehaviour
{
    Transform myTransform;
    public Transform target;
    public float hitForce;
    public Quaternion rotationOffset;
    public AudioSource audioSource;
    [SerializeField] AudioClip hitClip;

    void Awake()
    {
        myTransform = transform;
    }

    void OnEnable()
    {
        rotationOffset = Quaternion.identity;
    }

    void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.CompareTag("Drone")) return;
        Rigidbody otherRb = other.rigidbody;
        if(otherRb.isKinematic) return;
        otherRb.AddForce((otherRb.position - myTransform.position).normalized * hitForce, ForceMode.VelocityChange);
        otherRb.AddTorque(new Vector3(Random.Range(0, 36000), Random.Range(0, 36000), Random.Range(0, 36000)) * hitForce, ForceMode.Impulse);
        audioSource.PlayOneShot(hitClip);
    }

    void LateUpdate()
    {
        myTransform.position = target.position;
        myTransform.rotation = target.rotation * rotationOffset;
    }
}
