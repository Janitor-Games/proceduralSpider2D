
using Oddworm.Framework;
using UnityEngine;
[DefaultExecutionOrder(0)]

public class playerBodyController : MonoBehaviour
{
    [Header("General Values And References")]
    public Rigidbody2D rb;
    public playerLegController legL, legR;
    public playerEyeController eye;
    public Vector2 velocity;
    public bool debugMode;

    [Header("Movement Values")]
    public float moveSpeed;
    private float xInput;

    [Header("Linecast Values")]
    public LayerMask level;
    public float castRad, castAngle, castCircle;
    public Transform bodyCast;
    public Vector2 bodyPoint1, bodyPoint2;

    [Header("Position And Rotation Values")]
    public float bodyOffset;
    public float posTime, rotTime;

    [Header("Leg Animation Correction")]
    public Transform legCast;
    public float legCastAdvance;
    private Vector2 legCastPos;

    [Header("Idle State")]
    public float idleTime;
    public bool isIdle;
    private float idleCount;

    [Header("Bobbing Animation")]
    public float bobHeight;
    public float normalBobSpeed, idleBobSpeed;
    private float signBob, currentBobOffset;

    void Start()
    {
        getPoints(ref bodyPoint1, ref bodyPoint2, bodyCast, Color.red);
        legCastPos = legCast.localPosition;
        signBob = 1f;
    }

    void Update()
    {
        xInput = inputManager.instance.readVector("PlayerMove").x;
        idleTimer();
        if (inputManager.instance.pressed("PlayerDebug")) debugMode = !debugMode;
    }

    void FixedUpdate()
    {
        setBobbing();
        getPoints(ref bodyPoint1, ref bodyPoint2, bodyCast, Color.red);
        setLegCorrection();
        applyPositionRotation();
        applyVelocity();
    }
    
    private void idleTimer()
    {
        idleCount = (!eye.hasMovedMouse && xInput == 0) ? Mathf.Min(idleCount + Time.deltaTime, idleTime) : 0;
        isIdle = idleCount >= idleTime;
    }

    //set currentBobOffset for bobbing animation 
    private void setBobbing()
    {
        currentBobOffset = Mathf.MoveTowards(currentBobOffset, signBob * bobHeight, Time.deltaTime * (isIdle ? idleBobSpeed : normalBobSpeed));
        if (Mathf.Abs(currentBobOffset) >= bobHeight) signBob *= -1f;
    }

    //fires a curved linecast 
    private Vector2 fireCurvedRay(float dirWall, Transform castPosition, Vector2 currentPoint, Color dbgColor)
    {
        if (debugMode) DbgDraw.Sphere(castPosition.position, transform.rotation, new Vector2(0.5f, 0.5f), dbgColor, 0.02f);
        //beginning of the line
        var lineS = transform.up * castRad;
        //angle axis creates a rotation with an angle of dirWall*castAngle around the transform.forward vector
        //the sign of the angle (aka dirWall) changes how the linecast is fired: a positive angle creates a linecast that goes counter-clockwise, a negative angle goes clockwise
        var rot = Quaternion.AngleAxis(dirWall * castAngle, transform.forward);
        //raycastCircle determines the overall curve of the linecast (a value of 360 creates a fully circular cast, a value of 180 creates a half-circle cast)
        var iterations = Mathf.CeilToInt(castCircle / Mathf.Abs(castAngle));
        for (int x = 0; x < iterations; x++)
        {
            //end of the line
            var lineE = rot * lineS;
            if (debugMode) DbgDraw.Line(castPosition.position + lineS, castPosition.position + lineE, dbgColor, 0.02f);
            //the lines are fired around the castPosition
            var hit = Physics2D.Linecast(castPosition.position + lineS, castPosition.position + lineE, level);
            if (hit.collider != null)
            {
                return hit.point;
            }
            //new line begins where the previous has ended
            lineS = lineE;
        }
        return currentPoint;
    }

    private void getPoints(ref Vector2 firstPoint, ref Vector2 otherPoint, Transform castPos, Color dbgColor)
    {
        firstPoint = fireCurvedRay(1, castPos, firstPoint, dbgColor);
        otherPoint = fireCurvedRay(-1, castPos, otherPoint, dbgColor);
    }

    private void applyVelocity()
    {
        velocity = transform.right * xInput * moveSpeed;
        rb.linearVelocity = velocity;
    }

    //bodyPoint1 and bodyPoint2 are responsible for determing the player's position and rotation
    private void applyPositionRotation()
    {
        //position and rotation is applied using the perpendicular of the vector of the points;
        var perp = -Vector2.Perpendicular(bodyPoint1 - bodyPoint2).normalized;
        if (debugMode)
        {
            DbgDraw.Line(bodyPoint1, bodyPoint2, Color.magenta, 0.02f);
            DbgDraw.Ray((Vector3)(bodyPoint1 + bodyPoint2) / 2f, perp * bodyOffset, Color.black, 0.02f);
        }
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.forward, perp), Time.deltaTime / rotTime);
        //an offset is put to make the body move above the ground
        rb.position = Vector3.Slerp(rb.position, (bodyPoint1 + bodyPoint2) / 2 + perp * (bodyOffset + currentBobOffset), Time.deltaTime / posTime);
    }

    //legL.point and legR.point are the points where the legs' targets move to
    //this function is responsible for shifting the castPosition of these points
    //this makes for a more natural walk cycle (see video for more info)
    private void setLegCorrection()
    {
        legCast.localPosition = legCastPos + (Vector2.right * xInput * legCastAdvance);
        getPoints(ref legL.point, ref legR.point, legCast, Color.green);
    }
}
