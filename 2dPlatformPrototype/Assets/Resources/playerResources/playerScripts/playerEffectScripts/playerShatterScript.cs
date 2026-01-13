using UnityEngine;

public class playerShatterScript : MonoBehaviour
{
    [Header("General Values And References")]
    public Rigidbody2D rb;
    public playerHealthDeathManager death;
    public float shatterDirection;
    private float shatterForce, shatterRotate;

    void Start()
    {
        shatterForce = death.shatterRBForce;
        shatterRotate = death.shatterRBRotate;
        rb.AddForce(shatterDirection * transform.right * shatterForce, ForceMode2D.Impulse);
    }

    void Update()
    {
        transform.Rotate(-Vector3.forward, shatterDirection * shatterRotate * Time.deltaTime);
    }
}
