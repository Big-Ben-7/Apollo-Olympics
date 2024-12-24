using UnityEngine;

public class Collectable : MonoBehaviour
{
    public Vector3 rotation;
    Transform myTransform;
    
    void Awake()
    {
        myTransform = transform;
    }

    // Update is called once per frame
    void Update()
    {
        myTransform.Rotate(rotation * Time.deltaTime);
    }
}
