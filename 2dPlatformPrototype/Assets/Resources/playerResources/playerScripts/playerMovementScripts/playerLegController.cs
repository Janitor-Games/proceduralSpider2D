using UnityEngine;

public class playerLegController : MonoBehaviour
{
    [Header("General Values And References")]
    public playerBodyController pb;
    public playerLegController oppositeLeg;
    public playerLegData legData;
    public int stepPhase;
    private Vector3 finalTarget, currentTarget;
    [Header("Leg Cast Values")]
    public float castDirection;

    void Start()
    {
        currentTarget = transform.position;
    }

    void FixedUpdate()
    {
        //when the player lands on the ground, there's a slignt delay between casting the leg's rays, and updating the castPosition of them
        //causing the legs to not be in their correct positions
        //so, we check if the player is grounded, and if they didn't land on the ground just now
        if (pb.isGrounded && !pb.hasJustGrounded)
        {
            processCurvedRay();
            setPlatformLegPosition();
            calcWalk();
            moveFoot(currentTarget);
        }
        else
        {
            transform.position = finalTarget = currentTarget = pb.transform.position;
            //this makes the legs move to the new finalTarget position when landing on the ground
            stepPhase = 1;
        }
    }

    #region helperFuncs
    //get point to where to lift the leg
    private Vector2 findLiftPosition()
    {
        Vector2 halfway = (finalTarget + transform.position) / 2;
        //get position above the midpoint
        Vector2 liftTarget = halfway + (Vector2)pb.getTransformVec(Vector2.up) * legData.stepHeight;
        return liftTarget;
    }
    #endregion helperFuncs

    #region fixedUpdateFuncs
    //fire a curved ray to get the position the legs need to go to
    private void processCurvedRay()
    {
        var ray = generalStaticFuncs.fireCurvedRay(pb.getTransformVec(Vector2.up), transform.forward, castDirection, legData.legCast, legData.castRad, legData.castAngle, legData.castArc, legData.level, Color.green);
        var coll = ray.collider;
        if (coll != null) finalTarget = ray.point;
    }

    //calculate targets and stepPhases
    private void calcWalk()
    {
        //stepPhase of 0 means the leg is grounded
        //when the other leg is grounded and the distance between the finalTarget is bigger than the stepSize, the leg will begin to move to the liftPosition
        if (stepPhase == 0 && Vector2.Distance(transform.position, finalTarget) > legData.stepSize && oppositeLeg.stepPhase == 0)
        {
            stepPhase = 1;
            currentTarget = findLiftPosition();
        }
        //stepPhase of 1 means the leg is currently moving to the lift position
        //stepDistanceTolerate checks if the leg is close enough to the target
        else if (stepPhase == 1 && Vector2.Distance(transform.position, currentTarget) <= legData.stepDistanceTolerate)
        {
            stepPhase = 2;
            currentTarget = finalTarget;
        }
        //stepPhase of 2 means that the leg is moving to the final step position
        else if (stepPhase == 2 && Vector2.Distance(transform.position, currentTarget) <= legData.stepDistanceTolerate)
        {
            stepPhase = 0;
        }
    }

    private void setPlatformLegPosition()
    {
        //when the leg is grounded (i.e stepPhase is 0) the leg remains unmoving until its time to move it
        //this causes issues if the ground moves while the leg is grounded, because then the leg doesn't update to this new change
        //for example, when being on a moving platform
        if (pb.isOnPlat)
        {
            //if idling on a platform, teleport the leg position to be the curvedRay's point 
            // (the legs are always grounded when idling)
            if (!pb.isMoving)
            {
                transform.position = currentTarget = finalTarget;
                stepPhase = 0;
            }
            //else, fire a ray to receive the new grounded position
            else if (stepPhase == 0)
            {
                var hit = Physics2D.Raycast(currentTarget + pb.getTransformVec(Vector2.up) * legData.platformCastOffset, pb.getTransformVec(-Vector2.up), legData.platformCastLength, legData.level);
                Debug.DrawRay(currentTarget + pb.getTransformVec(Vector2.up) * legData.platformCastOffset, pb.getTransformVec(-Vector2.up) * legData.platformCastLength, Color.red);
                if (hit) transform.position = hit.point;
            }
        }
    }

    //smoothly move the leg position's towards the target using moveTowards
    private void moveFoot(Vector3 target)
    {
        //no need to move when the leg is grounded
        if (stepPhase != 0) transform.position = Vector3.MoveTowards(transform.position, target, Time.fixedDeltaTime * legData.stepSpeed);
    }
    #endregion fixedUpdateFuncs

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.5f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(finalTarget, 0.5f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(currentTarget, 0.5f);
    }
}