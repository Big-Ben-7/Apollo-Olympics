using UnityEngine;

public class Atmosphere : MonoBehaviour
{
    public float hitForce;
    public Rigidbody target;
    Rigidbody rb;
    public AudioSource audioSource;
    [SerializeField] AudioClip hitClip;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.CompareTag("Drone") || other.gameObject.CompareTag("Aura")) return;
        Rigidbody otherRb = other.rigidbody;
        otherRb.AddForce((otherRb.position - rb.position).normalized * hitForce, ForceMode.VelocityChange);
        audioSource.PlayOneShot(hitClip);
    }

    void LateUpdate()
    {
        rb.position = target.position;
        rb.rotation = target.rotation;
    }
}
