using UnityEngine;

public class gibScript : MonoBehaviour
{
   [Header("General Values And References")]
    public Rigidbody2D rb;
    public Vector2 gibVector;
    public float gibDirection;
    public float gibForce;
    public float gibRotate;
    public float timeToDestroy;

    void Start()
    {       
        rb.AddForce(gibVector * gibDirection* gibForce, ForceMode2D.Impulse);
        Destroy(gameObject,timeToDestroy);
    }

    void Update()
    {
        transform.Rotate(-Vector3.forward, gibDirection * gibRotate * Time.deltaTime);
    }
}
