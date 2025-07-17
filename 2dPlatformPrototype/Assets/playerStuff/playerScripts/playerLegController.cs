using Oddworm.Framework;
using UnityEngine;
[DefaultExecutionOrder(1)]

public class playerLegController : MonoBehaviour
{

    [Header("General Values And References")]
    public playerBodyController pb;
    public playerLegController oppositeLeg;

    [Header("Walking Values")]
    public float stepPhase;
    public float stepSize, stepSpeed, stepDistanceTolerate, stepHeight;
    public Vector2 point;
    private Vector2 nextTarget;

    void Update()
    {
        setPhase();
        moveFoot();
        if(pb.debugMode) DbgDraw.Sphere(nextTarget, transform.rotation, new Vector2(0.5f, 0.5f), Color.blue ,0.02f);
    }

    private void setPhase()
    {
        float distTarget = Vector2.Distance(transform.position, nextTarget);
        float distPoint = Vector2.Distance(transform.position, point);
        //stepPhase of 0 means the leg is grounded
        //when the other leg is grounded and the distance between the point is bigger than the stepSize, the leg will begin to move to the liftPosition
        if (stepPhase == 0 && distPoint > stepSize && oppositeLeg.stepPhase == 0)
        {
            stepPhase = 1;
            nextTarget = findLiftPosition();
        }
        //stepPhase of 1 means the leg is currently moving
        //stepDistanceTolerate checks if the leg is close enough to nextTarget
        else if (stepPhase == 1 && distTarget <= stepDistanceTolerate)
        {
            stepPhase = 2;
            nextTarget = point;
        }
        //stepPhase of 2 means that the leg reached its destination
        else if (stepPhase == 2 && distTarget <= stepDistanceTolerate)
        {
            stepPhase = 0;
        }
    }

    private void moveFoot()
    {
        transform.position = Vector2.MoveTowards(transform.position, nextTarget, stepSpeed * Time.deltaTime);
    }

    //calculates the point to lift the leg to
    private Vector2 findLiftPosition()
    {
        Vector2 differenceVector = (Vector3)point - transform.position;
        Vector2 liftTarget = (differenceVector / 2) + ((Vector2)pb.transform.up * stepHeight);
        return (Vector2)transform.position + liftTarget;
    }
}
