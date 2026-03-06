using UnityEngine;

public class playerProjectile1Script : MonoBehaviour
{
    [Header("General Values And References")]
    public Rigidbody2D rb;
    public GameObject hitParticle;
    public float fireForce;
    public float fireCooldown;

    void FixedUpdate()
    {
        rb.linearVelocity = transform.right * fireForce;
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        var contact = coll.GetContact(0);
        var particleObj = Instantiate(hitParticle, contact.point, Quaternion.identity);
        var particleShape = particleObj.GetComponent<ParticleSystem>().shape;
        var particleRot = particleShape.rotation;
        particleRot.x = -Vector2.SignedAngle(contact.normal, Vector2.up) - 90f;
        particleShape.rotation = particleRot;
        Destroy(particleObj, 1f);
        gameObject.SetActive(false);
    }
}
