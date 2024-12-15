using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Atmosphere : MonoBehaviour
{
    public Transform target;
    Transform myTransform;
    PlayerController player;
    public static AudioSource audioSource;
    [SerializeField] AudioClip hitClip;
    void Awake()
    {
        myTransform = transform;
    }
    void Start()
    {
        player = target.GetComponent<PlayerController>();
    }

    void OnCollisionEnter(Collision other)
    {
        if(other.transform == target)
        {
            return;
        }
        player.canCollide = false;
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
        myTransform.rotation = target.rotation;
    }
}
