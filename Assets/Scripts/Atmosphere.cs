using UnityEngine;

public class Atmosphere : MonoBehaviour
{
    public float hitForce;
    public Transform target;
    Transform myTransform;
    public AudioSource audioSource;
    [SerializeField] AudioClip hitClip;

    void Awake()
    {
        myTransform = transform;
    }

    void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.CompareTag("Drone") || other.gameObject.CompareTag("Aura")) return;
        Rigidbody otherRb = other.rigidbody;
        otherRb.AddForce((otherRb.position - myTransform.position).normalized * hitForce, ForceMode.VelocityChange);
        audioSource.PlayOneShot(hitClip);
    }

    void LateUpdate()
    {
        myTransform.position = target.position;
        myTransform.rotation = target.rotation;
    }
}
