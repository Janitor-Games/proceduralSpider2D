using UnityEngine;

public class snapperAttackHit : MonoBehaviour, hitInterface
{
    [Header("General Values And References")]
    public snapperHealthAndDeathManager snap;

    public void doHit()
    {
        if (snap.currentState == snapperStates.lunging) snap.setState(snapperStates.retracting);
    }
}
