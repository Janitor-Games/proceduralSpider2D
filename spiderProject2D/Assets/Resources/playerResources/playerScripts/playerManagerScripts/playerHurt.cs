using UnityEngine;

public class playerHurt : MonoBehaviour, hurtInterface
{
    [Header("General Values And References")]
    public gameEvent registerHurtEvent;
    public playerBodyController pb;

    public void doHurt()
    {
        registerHurtEvent.triggerEvent();
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        MonoBehaviour[] scripts = coll.gameObject.GetComponents<MonoBehaviour>();
        //find hitInterface script
        foreach (MonoBehaviour script in scripts)
        {
            hitInterface hit = script as hitInterface;
            if (hit != null) hit.doHit();
        }
        //if the player isnt on the ground, the player can only knocked back to the global left or the right
        //else, the player will be knocked back according to its relative left or right 
        pb.knockbackDirection = Mathf.Sign(pb.isGrounded ? Vector2.Dot(transform.right, (transform.position - coll.transform.position).normalized) : transform.position.x - coll.transform.position.x);
        doHurt();
    }
}
