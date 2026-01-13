using UnityEngine;
using UnityEngine.U2D.IK;

public class playerBodyController : MonoBehaviour
{
    [Header("General Values And References")]
    public playerInputController input;
    public Animator anim;
    public Rigidbody2D rb;
    public LayerMask level;
    public gameEvent playerCrushEvent;

    [Header("Movement Values")]
    public Vector2 velocity;
    public float maxMoveSpeed, groundAccel, groundDecel, groundFriction, airAccel;
    public float wallAngleMin, wallAngleMax;
    public bool isMoving,hasJustGrounded;
    private float groundSpeed;
    private Vector2 positionVelocity;
    private bool canMove;

    [Header("Jump Values")]
    public float jumpHeight;
    public float jumpCut, gravityJump, gravityFall, jumpBufferTime, maxFallSpeed;
    public bool isGrounded, isAirborne, isFalling;
    private bool groundedDisabled, hasJumped, doJump, hasBuffered, hasCancelledJump, cancelJump, cancelJumpDuringBuffer;
    private float jumpBufferCount;

    [Header("Physics Values")]
    public Transform bodyCast;
    public float castRadGround, castRadAir, castArcGround, castArcAir, castAngle;
    public float verticalOverlapOffset, verticalOverlapRad, horizontalOverlapOffset, horizontalOverlapRad;
    public float rayCrushLength, rayCornerLength, rayCrushDistance, rayCornerDistance, rayCornerAngle;
    private Vector2 bodyPoint1, bodyPoint2, bodyCastPosGround, cornerVector;
    private bool isCrushed, isCornered;

    [Header("Position And Rotation Values")]
    public float normalBodyOffset;
    public float posSpeed, rotSpeed;

    [Header("Leg Animation")]
    public Transform legCast;
    public float legCastAdvance;
    private Vector2 legCastPos;

    [Header("Moving/Rotating Platforms")]
    public bool isOnControllable, isOnPlat;
    private movingRotatingPlatformBase currentPlatform;
    private Vector2 platformRBVelocity;

    [Header("Animation Values")]
    public IKManager2D ik;
    public float idleBobSpeed;
    private bool setJumpTrigger;

    void Start()
    {
        //Application.targetFrameRate=60;
        cameraController.setCamTarget(transform);
        legCastPos = legCast.localPosition;
        bodyCastPosGround = bodyCast.localPosition;
        canMove = setJumpTrigger = true;
        processGround();
    }

    void Update()
    {
        jumpBufferCount = generalStaticFuncs.timer(Time.deltaTime, jumpBufferCount, jumpBufferTime, input.pressedJump);
        handleJump();
        handleAnimations();
        handleInteract();
    }

    void FixedUpdate()
    {
        processGround();
        if (isGrounded) applyPositionRotation();
        applyVelocity();
        applyJump();
        if (isGrounded) setLegCorrection();
    }

    #region smallHelperFuncs
    //function for calculating rigidbody equivalents of transform.up/right/etc
    public Vector3 getTransformVec(Vector3 vec)
    {
        return generalStaticFuncs.getRotatedVector(rb.rotation, vec);
    }

    //function for getting rotation angle with a vector
    public float getLookRot(Vector3 vec)
    {
        return Quaternion.LookRotation(Vector3.forward, vec).eulerAngles.z;
    }

    //function for detaching from a platform
    private void detachPlat()
    {
        if (isOnPlat)
        {
            platformRBVelocity = Vector2.zero;
            currentPlatform = null;
            isOnControllable = isOnPlat = false;
        }
    }

    //function for setting the angular velocity of the player to rotate for a specific angle
    private void setAngularVelocity(Vector2 vec)
    {
        var angle = getLookRot(vec);
        rb.angularVelocity = (angle - rb.rotation) / Time.fixedDeltaTime * rotSpeed;
    }

    //function for crushing the player
    private void handleCrush()
    {
        if (!isCrushed)
        {
            detachPlat();
            isCrushed = true;
            playerCrushEvent.triggerEvent();
        }
    }

    //because of the player's collider, the player cant climb on sharp corners 
    //to remedy this, this function occurs when the player is in such a corner (view "OnColliderEnter2D")
    //to rotate the player to the wall
    private void doCornerRotation()
    {
        if (canMove && groundSpeed != 0)
        {
            Debug.DrawRay(rb.position, generalStaticFuncs.getRotatedVector(rayCornerAngle * Mathf.Sign(groundSpeed), (Vector2)getTransformVec(Vector2.up)) * rayCornerLength, Color.aquamarine);
            //fire an angled ray thats pointing slightly up, to detect the wall of the corner
            var hit = Physics2D.Raycast(rb.position, generalStaticFuncs.getRotatedVector(rayCornerAngle * Mathf.Sign(groundSpeed), (Vector2)getTransformVec(Vector2.up)), rayCornerLength, level);
            if (hit)
            {
                cornerVector = hit.normal;
                isCornered = true;
            }
        }
    }
    #endregion smallHelperFuncs

    #region majorHelperFuncs
    private void processGroundRays(ref Vector2 point1, ref Vector2 point2, Color dbgColor)
    {
        var ray1 = generalStaticFuncs.fireCurvedRay(getTransformVec(Vector2.up), getTransformVec(Vector3.forward), 1, bodyCast, castRadGround, castAngle, castArcGround, level, dbgColor);
        var ray2 = generalStaticFuncs.fireCurvedRay(getTransformVec(Vector2.up), getTransformVec(Vector3.forward), -1, bodyCast, castRadGround, castAngle, castArcGround, level, dbgColor);
        var coll1 = ray1.collider;
        var coll2 = ray2.collider;
        isGrounded = coll1 != null && coll2 != null && !groundedDisabled;
        //detach from ground
        if (!isGrounded)
        {
            groundedDisabled = false;
            setJumpTrigger = isAirborne = true;
            detachPlat();
            return;
        }
        //land on ground
        if (isAirborne)
        {
            //velocity is calculated using transform.right (more specifcally its rigidbody equivalent)
            // because of the way transform.right behaves, inputs on a wall whose angle is below 0, or inputs on the ceiling, will be reversed
            //(as in pressing left on the ceiling will make the player go right, and vice versa)
            //to counter this, we check if the player is on the ceiling, and invert the groundSpeed
            //this also happens in "applyVelocity"
            float ang = Mathf.Round(Vector2.Angle(getTransformVec(Vector2.up), Vector2.up));
            groundSpeed = rb.linearVelocity.x * (ang > wallAngleMax ? -1 : 1);
            hasJustGrounded=true;
        }
        //set variables 
        point1 = ray1.point; point2 = ray2.point;
        isAirborne = hasJumped = isFalling = false;
        //moving platforms logic
        //first check if both tags are of a moving/rotating platform
        if (coll1.tag == "MovingRotating" && coll2.tag == "MovingRotating")
        {
            //get platform
            var newPlatform1 = coll1.transform.parent.GetComponent<movingRotatingPlatformBase>();
            var newPlatform2 = coll2.transform.parent.GetComponent<movingRotatingPlatformBase>();
            var testPlatform = newPlatform1 != null ? newPlatform1 : newPlatform2;
            //check if we're on a new platform
            if (testPlatform != currentPlatform)
            {
                canMove = isOnPlat = true;
                currentPlatform = testPlatform;
                //isOnControllable = currentPlatform.canMoveAndControl() || currentPlatform.canRotateAndControl();
            }
        }
        //if both legs are not touching a platform
        else if (coll1.tag != "MovingRotating" && coll2.tag != "MovingRotating") detachPlat();
        if (isOnPlat) platformRBVelocity = currentPlatform.rb.linearVelocity;
    }
    #endregion majorHelperFuncs

    #region updateFuncs
    //intitate jump, and handle variables relating to jump buffering
    private void handleJump()
    {
        if (!hasJumped && isGrounded && jumpBufferCount > 0)
        {
            //if jumpBufferCount is equal to its original time, then the player has jumped normally
            hasBuffered = jumpBufferCount != jumpBufferTime;
            //cancelJumpDuringBuffer allows the player to buffer a small jump if the player pressed jump and released jump during the buffer
            //of course, this means that this variable must always be false if the player didnt buffer their jump
            if (!hasBuffered) cancelJumpDuringBuffer = false;
            //hasCancelledJump only lets the player cancel their jump once
            hasCancelledJump = false;
            doJump = true;
            jumpBufferCount = 0;
        }
        //check if player cancelled jump
        if (input.releasedJump)
        {
            if (!isFalling && !hasCancelledJump) cancelJump = true;
            else if (isFalling && jumpBufferCount > 0) cancelJumpDuringBuffer = true;
        }
    }

    private void handleAnimations()
    {
        ik.enabled = isGrounded;
        anim.speed = input.isIdle ? idleBobSpeed : 1;
        anim.SetBool("isAirborne", isAirborne);
        anim.SetBool("hasBuffered", hasBuffered);
        if (setJumpTrigger)
        {
            anim.SetTrigger(hasBuffered ? "startAirBuffer" : "startAirNormal");
            hasBuffered = setJumpTrigger = false;
        }
    }

    //handle interactions
    private void handleInteract()
    {
        //check platform interactablity
        if (isOnControllable)
        {
            if (input.interacted) currentPlatform.interact();
            //if interactable rotating platform, lock movement and transfer input
           // if (currentPlatform.canRotateAndControl())
            //{
                //canMove = !currentPlatform.isControlling;
               // if (!canMove) currentPlatform.rotateFunc.rotateDirection = input.directionVector.x;
            //}
        }
        else canMove = true;
    }
    #endregion updateFuncs

    #region fixedUpdateFuncs
    private void processGround()
    {
        bodyCast.localPosition = isGrounded ? bodyCastPosGround : Vector3.zero;
        //if not grounded
        if (!isGrounded)
        {
            //fire a curved ray all around the body
            var ray = generalStaticFuncs.fireCurvedRay(Vector2.up, Vector3.forward, 1, bodyCast, castRadAir, castAngle, castArcAir, level, Color.brown);
            if (ray.collider != null)
            {
                //fire other ray to get correct normal
                ray = Physics2D.Raycast(transform.position, (ray.point - rb.position).normalized, Vector2.Distance(ray.point, rb.position) * 2, level);
                rb.rotation = getLookRot(ray.normal);
                //do ground calculations
                processGroundRays(ref bodyPoint1, ref bodyPoint2, Color.magenta);
            }
            else
            {
                isAirborne = true;
                isFalling = rb.linearVelocityY < 0;
                rb.angularVelocity = 0f;
            }
        }
        else {
            processGroundRays(ref bodyPoint1, ref bodyPoint2, Color.magenta);
            hasJustGrounded=false;
        }
    }

    private void applyPositionRotation()
    {
        //bodyPoint1 and bodyPoint2 are responsible for determining the player's position and rotation
        if (!isCornered)
        {
            //position and rotation is applied using the perpendicular of the vector of the points;
            var perpendicular = -Vector2.Perpendicular(bodyPoint1 - bodyPoint2).normalized;
            setAngularVelocity(perpendicular);
            var target = (bodyPoint1 + bodyPoint2) / 2 + perpendicular * normalBodyOffset;
            //transform.position is used because when a rigidbody is interpolated, the transform.position is the interpolated position of the rigidbody
            //this is useful for us, as we want to get the most recent position
            var difference = target - (Vector2)transform.position;
            //positionVelocity is added to the rb's velocity in "applyVelocity"
            positionVelocity = difference / Time.fixedDeltaTime * posSpeed;
        }
        //however, when in a corner, the cornerVector is the one determining the rotation
        else
        {
            //result of "doCornerRotation"
            setAngularVelocity(cornerVector);
            cornerVector = Vector2.zero;
            isCornered = false;
        }
    }

    private void applyVelocity()
    {
        rb.gravityScale = isGrounded ? 0 : isFalling ? gravityFall : gravityJump;
        if (!canMove)
        {
            groundSpeed = 0f;
            rb.linearVelocity = Vector2.zero;
        }
        else if (isGrounded)
        {
            //just like in "processGroundRays", we invert the player's direction when such cases appear
            float ang = Mathf.Round(Vector2.SignedAngle(getTransformVec(Vector2.up), Vector2.up)); float abs = Mathf.Abs(ang);
            float dir = (abs >= wallAngleMin && abs <= wallAngleMax)
            ? input.directionVector.y * (ang > 0 ? -1f : 1f)
            : input.directionVector.x * (abs < wallAngleMin ? 1f : -1f);
            groundSpeed = generalStaticFuncs.accelDecel(groundSpeed, maxMoveSpeed, groundAccel, groundDecel, groundFriction, dir, Time.fixedDeltaTime, isGrounded);
            //add groundSpeed, positionVelocity and the platform's velocity if the player is on it
            rb.linearVelocity = groundSpeed * (Vector2)getTransformVec(Vector2.right) + positionVelocity + platformRBVelocity;
        }
        else
        {
            var airSpeed = generalStaticFuncs.accelDecel(rb.linearVelocityX, maxMoveSpeed, airAccel, airAccel, airAccel, input.directionVector.x, Time.deltaTime, false);
            rb.linearVelocity = new Vector2(airSpeed, Mathf.Max(rb.linearVelocityY, maxFallSpeed));
        }
        velocity = rb.linearVelocity;
        isMoving = groundSpeed != 0f;
    }

    private void applyJump()
    {
        if (doJump)
        {
            doJump = false;
            float jumpForce = Mathf.Sqrt(jumpHeight * (Physics2D.gravity.y * gravityJump) * -2f) * rb.mass;
            Vector2 platformBoost = Vector2.zero;
            if (currentPlatform)
            {
                Vector2 platformVelocity = platformRBVelocity;
                Vector2 playerVector = rb.linearVelocity - positionVelocity - platformRBVelocity + (Vector2)getTransformVec(Vector2.up);
                //add the platform's velocity to the player
                //if the player is moving in the same direction as the platform
                //or if the player is about to jump in the same direction as the platform
                if (playerVector.x * platformVelocity.x > 0) platformBoost.x = platformVelocity.x;
                if (playerVector.y * platformVelocity.y > 0) platformBoost.y = platformVelocity.y;
            }
            rb.linearVelocity += (Vector2)getTransformVec(Vector2.up) * jumpForce + platformBoost - positionVelocity - platformRBVelocity;
            groundedDisabled = hasJumped = true;
        }
        else if (hasJumped && !isFalling && (cancelJump || cancelJumpDuringBuffer))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, rb.linearVelocityY * jumpCut);
            hasCancelledJump = true;
            cancelJump = cancelJumpDuringBuffer = false;
        }
    }
    
    //shifts the castPosition of the leg's rays, creating a more natural walk cycle
    private void setLegCorrection()
    {
        float legOffset;
        if (!canMove) legOffset = 0.5f * input.directionVector.x * legCastAdvance;
        else
            //if player is moving, change the offset according to the velocity of the player
            legOffset = groundSpeed * legCastAdvance / maxMoveSpeed;
        legCast.localPosition = legCastPos + (Vector2.right * legOffset);
    }
    #endregion fixedUpdateFuncs

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 3 && isGrounded)
        {
            var hitAbove = Physics2D.OverlapCircle(rb.position + (Vector2)getTransformVec(Vector2.up) * verticalOverlapOffset, verticalOverlapRad, level);
            var hitRight = Physics2D.OverlapCircle(rb.position + (Vector2)getTransformVec(Vector2.right) * horizontalOverlapOffset, horizontalOverlapRad, level);
            var hitLeft = Physics2D.OverlapCircle(rb.position - (Vector2)getTransformVec(Vector2.right) * horizontalOverlapOffset, horizontalOverlapRad, level);
            if (hitAbove)
            {
                //if something is colliding with the player from above, fire a ray downwards
                var hit = Physics2D.Raycast(rb.position, -getTransformVec(Vector2.up), rayCrushLength, level);
                //if the ray's distance is smaller than rayCrushDistance, then the player can be considered crushed
                if (hit.distance <= rayCrushDistance) handleCrush();
                //but if the distance is larger than crushDistance and shorter than rayCornerDistance, then the player is in a corner
                else if (hit.distance <= rayCornerDistance) doCornerRotation();
            }
            //if only one of the side overlaps detects a wall, then the player is in a corner
            if (hitLeft ^ hitRight) doCornerRotation();
            //if both sides detect walls, then the player can be considered crushed
            else if (hitRight && hitLeft) handleCrush();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(bodyPoint1, 0.5f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(bodyPoint2, 0.5f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Quaternion.Euler(0, 0, rb.rotation) * Vector2.up * verticalOverlapOffset, verticalOverlapRad);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + Quaternion.Euler(0, 0, rb.rotation) * Vector2.right * 1 * horizontalOverlapOffset, horizontalOverlapRad);
        Gizmos.DrawWireSphere(transform.position + Quaternion.Euler(0, 0, rb.rotation) * Vector2.right * -1 * horizontalOverlapOffset, horizontalOverlapRad);
    }
}

