using UnityEngine;

public class snapperHurt : MonoBehaviour,hurtInterface
{
    [Header("General Values And References")]
    public snapperHealthAndDeathManager snap;

    public void doHurt()
    {
        snap.hurtThing();
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        MonoBehaviour[] scripts=coll.gameObject.GetComponents<MonoBehaviour>();
        //find hitInterface script
        foreach(MonoBehaviour script in scripts)
        {
            hitInterface hit=script as hitInterface;
            if(hit!=null) hit.doHit();
        }
        doHurt();
    }
}
