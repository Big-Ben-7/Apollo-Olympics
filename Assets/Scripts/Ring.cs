using UnityEngine;

public class Ring : MonoBehaviour
{
    Rigidbody rb;
    public Rigidbody target;
    public float hitForce;
    public Quaternion rotationOffset;
    public AudioSource audioSource;
    [SerializeField] AudioClip hitClip;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        rotationOffset = Quaternion.identity;
    }

    void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.CompareTag("Drone") || other.gameObject.CompareTag("Aura")) return;
        Rigidbody otherRb = other.rigidbody;
        if(!otherRb.isKinematic)
        {
            otherRb.AddForce((otherRb.position - rb.position).normalized * hitForce, ForceMode.VelocityChange);
            otherRb.AddTorque(new Vector3(Random.Range(0, 36000), Random.Range(0, 36000), Random.Range(0, 36000)) * hitForce, ForceMode.Impulse);
        }
        audioSource.PlayOneShot(hitClip);
    }

    void LateUpdate()
    {
        rb.position = target.position;
        rb.rotation = target.rotation * rotationOffset;
    }
}
