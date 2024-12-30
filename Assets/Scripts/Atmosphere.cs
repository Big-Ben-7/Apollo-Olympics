using UnityEngine;

public class Atmosphere : MonoBehaviour
{
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
        if(other.gameObject.CompareTag("Drone") || other.gameObject.layer == 13) return;
        Rigidbody otherRb = other.rigidbody;
        Vector3 direction = otherRb.position - rb.position;
        otherRb.AddForce(direction * 2, ForceMode.VelocityChange);
        target.AddForce(-direction * 2, ForceMode.VelocityChange);
        audioSource.PlayOneShot(hitClip);
    }

    void LateUpdate()
    {
        rb.position = target.position;
        rb.rotation = target.rotation;
    }
}
