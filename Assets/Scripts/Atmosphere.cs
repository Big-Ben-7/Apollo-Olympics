using UnityEngine;

public class Atmosphere : MonoBehaviour
{
    public Rigidbody target;
    Rigidbody rb;
    PlayerController player;
    public AudioSource audioSource;
    [SerializeField] AudioClip hitClip;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision other)
    {
        audioSource.PlayOneShot(hitClip);
    }

    void LateUpdate()
    {
        rb.position = target.position;
        rb.rotation = target.rotation;
    }
}
