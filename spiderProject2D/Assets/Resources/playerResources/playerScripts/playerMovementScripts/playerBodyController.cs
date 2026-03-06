using UnityEngine;
using UnityEngine.U2D.IK;

public class playerBodyController : MonoBehaviour
{
    [Header("General Values And References")]
    public Animator anim;
    public Rigidbody2D rb;
    public LayerMask level;
    public gameEvent playerCrushEvent;
    public Vector2 playerUpVector => getTransformVec(Vector2.up);
    public Vector2 playerRightVector => getTransformVec(Vector2.right);
    public Vector3 playerForwardVector => getTransformVec(Vector3.forward);
    private playerInputController input;

    [Header("Movement Values")]
    public Vector2 velocity;
    public float maxMoveSpeed, groundAccel, groundDecel, groundFriction, airAccel;
    public float wallAngleMin, wallAngleMax;
    public bool hasJustGrounded;
    public float groundSpeed;
    private float groundAngle;
    private Vector2 positionVelocity, prevVelocity;

    [Header("Jump Values")]
    public float jumpHeight;
    public float jumpCut, gravityJump, gravityFall, jumpBufferTime, maxFallSpeed;
    public bool isGrounded, isAirborne, isFalling;
    private bool groundedDisabled, doJump, doCancelNormalJump, doCancelBufferedJump, hasBuffered, hasJumped, hasCancelledJump;
    private float jumpBufferCount;

    [Header("Physics Values")]
    public Transform bodyCast;
    public float castRadGround, castRadAir, castArcGround, castArcAir, castAngle;
    public float verticalOverlapOffset, verticalOverlapRad, horizontalOverlapOffset, horizontalOverlapRad;
    public float collideRayLength, rayCrushDistance, rayCornerDistance, rayCornerLength, rayCornerAngle;
    private Vector2 bodyPoint1, bodyPoint2, bodyCastPosGround, cornerVector;
    private bool isCrushed, isCornered;

    [Header("Position And Rotation Values")]
    public float normalBodyOffset;
    public float posSpeed, rotSpeed;

    [Header("Knockback Values")]
    public Vector2 knockbackForce;
    public float knockbackDirection, knockbackTime;
    public float knockbackCount;
    private Vector2 knockbackVelocity;

    [Header("Animation Values")]
    public IKManager2D ik;
    public float idleBobSpeed;
    private bool setJumpTrigger;

     void Awake()
    {
        input=GetComponentInParent<playerInputController>();
    }

    void Start()
    {
        bodyCastPosGround = bodyCast.localPosition;
        ik.enabled = false;
        processGround();
    }

    void Update()
    {
        jumpBufferCount = generalStaticFuncs.timer(Time.deltaTime, jumpBufferCount, jumpBufferTime, input.pressedJump);
        handleJump();
        handleAnimations();
    }

    void FixedUpdate()
    {
        processGround();
        if (isGrounded) applyPositionRotation();
        applyVelocity();
        applyJump();
        prevVelocity = rb.linearVelocity;
    }

    #region generalHelperFuncs
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

    //function for getting the player angle on the ground
    private float getPlayerAngle()
    {
        return Mathf.Round(Vector2.SignedAngle(playerUpVector, Vector2.up));
    }
    #endregion generalHelperFuncs

    #region groundAndPlatformFuncs
    private void processGroundRays(Color dbgColor)
    {
        var ray1 = generalStaticFuncs.fireCurvedRay(playerUpVector, playerForwardVector, 1, bodyCast, castRadGround, castAngle, castArcGround, level, dbgColor, dbgColor);
        var ray2 = generalStaticFuncs.fireCurvedRay(playerUpVector, playerForwardVector, -1, bodyCast, castRadGround, castAngle, castArcGround, level, dbgColor, dbgColor);
        var coll1 = ray1.collider;
        var coll2 = ray2.collider;
        isGrounded = coll1 != null && coll2 != null && !groundedDisabled;
        //detach from ground
        if (!isGrounded)
        {
            groundedDisabled = false;
            isAirborne = true;
            return;
        }
        //land on ground
        if (isAirborne)
        {
            //velocity is calculated using playerRightVector (transform.right), and there are some peculiarities when calculating it this way
            //when the player is positioned in certain ways, the movement on the ground gets inverted:
            //for example when on the ceiling, transform.right is  (-1,0), the opposite of what it is on the ground (1,0)
            //This makes it so that any normal input on the ground, becomes inverted on the ceiling (as in, pressing right makes the player to left, and vice versa)
            //to counter this, we check if the player is on the ceiling, and invert the groundSpeed (multiply by -1)
            //(similar code is also used in "applyVelocity" for the same reasons)
            float ang = Mathf.Round(Vector2.Angle(playerUpVector, Vector2.up)); float abs = Mathf.Abs(ang);
            float dir = 1f;
            //when the player is on the ceiling
            if (abs > wallAngleMax)
            {
                dir = -1f;
            }
            //when the player was on the ceiling, but is now on a wall
            else if (abs >= wallAngleMin && Mathf.Abs(groundAngle) > wallAngleMax)
            {
                if (Mathf.Abs(prevVelocity.y) > Mathf.Abs(prevVelocity.x)) dir = Mathf.Sign(prevVelocity.y);
                else dir = Mathf.Sign(prevVelocity.x);
            }
            groundSpeed = prevVelocity.x * dir;
            hasJustGrounded = true;
        }
        //set variables 
        bodyPoint1 = ray1.point; bodyPoint2 = ray2.point;
        isAirborne = hasJumped = isFalling = hasCancelledJump = false;
        groundAngle = getPlayerAngle();
    }

    private void processGround()
    {
        bodyCast.localPosition = isGrounded ? bodyCastPosGround : Vector3.zero;
        //if not grounded
        if (!isGrounded)
        {
            //fire a curved ray all around the body
            var ray = generalStaticFuncs.fireCurvedRay(Vector2.up, Vector3.forward, 1, bodyCast, castRadAir, castAngle, castArcAir, level, Color.brown, Color.brown);
            if (ray.collider != null)
            {
                ik.enabled = true;
                //fire other ray to get correct normal
                ray = Physics2D.Raycast(transform.position, (ray.point - rb.position).normalized, Vector2.Distance(ray.point, rb.position) * 2, level);
                rb.rotation = getLookRot(ray.normal);
                //do ground calculations
                processGroundRays(Color.magenta);
            }
            else
            {
                isAirborne = true;
                isFalling = rb.linearVelocityY < 0;
                rb.angularVelocity = 0f;
            }
        }
        else
        {
            processGroundRays(Color.magenta);
            hasJustGrounded = false;
        }
    }
    #endregion groundAndPlatformFuncs

    #region positionAndRotationFuncs

    //function for setting the angular velocity of the player to rotate for a specific angle
    private void setAngularVelocity(Vector2 vec)
    {
        var angle = getLookRot(vec);
        rb.angularVelocity = (angle - rb.rotation) / Time.deltaTime * rotSpeed;
    }

    //because of the player's collider, the player cant climb on sharp corners 
    //to remedy this, this function occurs when the player is in such a corner (view "OnColliderEnter2D")
    //to rotate the player to the wall
    private void doCornerRotation()
    {
        if (groundSpeed != 0)
        {
            Debug.DrawRay(rb.position, generalStaticFuncs.getRotatedVector(rayCornerAngle * Mathf.Sign(groundSpeed), playerUpVector) * rayCornerLength, Color.aquamarine);
            //fire an angled ray thats pointing slightly up, to detect the wall of the corner
            var hit = Physics2D.Raycast(rb.position, generalStaticFuncs.getRotatedVector(rayCornerAngle * Mathf.Sign(groundSpeed), playerUpVector), rayCornerLength, level);
            if (hit)
            {
                cornerVector = hit.normal;
                isCornered = true;
            }
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
            //positionVelocity is added to the rb's velocity in "applyVelocity", and is used for adjusting the player to its new position
            positionVelocity = difference / Time.deltaTime * posSpeed;
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
    #endregion positionAndRotationFuncs

    #region movementFuncs
    public void calcKnockback()
    {
        knockbackCount = knockbackTime;
        knockbackVelocity = new Vector2(knockbackForce.x * knockbackDirection, knockbackForce.y);
        if (isGrounded)
        {
            var angle = Vector2.Angle(knockbackVelocity, Vector2.right);
            knockbackVelocity = generalStaticFuncs.getRotatedVector(angle, transform.right) * knockbackForce.magnitude;
        }
    }

    private void applyVelocity()
    {
        rb.gravityScale = isGrounded ? 0 : isFalling ? gravityFall : gravityJump;
        if (knockbackCount <= 0)
        {
            if (isGrounded)
            {
                //just like in "processGroundRays", we invert the player's direction when such cases appear
                float abs = Mathf.Abs(groundAngle);
                float dir = (abs >= wallAngleMin && abs <= wallAngleMax)
                //when the player is on a wall, and is positioned so groundAngle is negative
                ? input.directionVector.y * (groundAngle > 0 ? -1f : 1f)
                //when the player isnt on a wall, but is on the ceiling
                : input.directionVector.x * (abs < wallAngleMin ? 1f : -1f);
                groundSpeed = generalStaticFuncs.accelDecel(groundSpeed, maxMoveSpeed, groundAccel, groundDecel, groundFriction, dir, Time.deltaTime, isGrounded);
                //add groundSpeed, positionVelocity and the platform's velocity
                velocity = groundSpeed * playerRightVector + positionVelocity;
            }
            else
            {
                var airSpeed = generalStaticFuncs.accelDecel(rb.linearVelocityX, maxMoveSpeed, airAccel, airAccel, airAccel, input.directionVector.x, Time.deltaTime, false);
                velocity = new Vector2(airSpeed, Mathf.Max(rb.linearVelocityY, maxFallSpeed));
            }
        }
        else
        {
            velocity = knockbackVelocity;
            knockbackCount = generalStaticFuncs.timer(Time.deltaTime, knockbackCount, knockbackTime, false);
        }
        rb.linearVelocity = velocity;
    }

    //handle variables relating to jumping
    private void handleJump()
    {
        if (!hasJumped && isGrounded && jumpBufferCount > 0)
        {
            //if jumpBufferCount is equal to its original time, then the player has jumped normally
            hasBuffered = jumpBufferCount != jumpBufferTime;
            jumpBufferCount = 0;
            setJumpTrigger = true;
            //doCancelNormalJump allows the player to control their jump height (releasing the jump button during the jump will make it smaller)
            //we set it to false when the player is about to jump, so no mistaken small jumps will be performed
            //doCancelBufferedJump makes the player perform an automatic small jump if the player released the jump button during the buffer
            //so, of course we also set it to false if the player didnt perform a buffer
            if (hasBuffered) doJump = true;
            else doCancelBufferedJump = false;
            doCancelNormalJump = false;
        }
        //check if player cancelled jump
        if (input.releasedJump)
        {
            if (jumpBufferCount > 0) doCancelBufferedJump = true;
            else doCancelNormalJump = true;
        }
    }

    //animation event function when the player peforms a normal jump (not a buffered jump, because they have a different animation, hence why doJump is set in the above function when the player has buffered)
    public void initiateNormalJump()
    {
        ik.enabled = false;
        doJump = true;
    }

    //perform the actual physics caluclations of the jump
    private void applyJump()
    {
        if (doJump)
        {
            doJump = false;
            float jumpForce = Mathf.Sqrt(jumpHeight * (Physics2D.gravity.y * gravityJump) * -2f) * rb.mass;
            rb.linearVelocity += playerUpVector * jumpForce - positionVelocity;
            groundedDisabled = hasJumped = true;
        }
        if (hasJumped && !isFalling)
        {
            if ((doCancelNormalJump || doCancelBufferedJump) && !hasCancelledJump)
            {
                doCancelNormalJump = doCancelBufferedJump = false;
                hasCancelledJump = true;
                rb.linearVelocity = new Vector2(rb.linearVelocityX, rb.linearVelocityY * jumpCut);
            }
        }
    }
    #endregion movementFuncs

    #region crushFuncs
    //function for crushing the player
    private void handleCrush()
    {
        if (!isCrushed)
        {
            isCrushed = true;
            playerCrushEvent.triggerEvent();
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 3 && isGrounded)
        {
            var hitAbove = Physics2D.OverlapCircle(rb.position + playerUpVector * verticalOverlapOffset, verticalOverlapRad, level);
            var hitRight = Physics2D.OverlapCircle(rb.position + playerRightVector * horizontalOverlapOffset, horizontalOverlapRad, level);
            var hitLeft = Physics2D.OverlapCircle(rb.position - playerRightVector * horizontalOverlapOffset, horizontalOverlapRad, level);
            if (hitAbove)
            {
                Debug.DrawRay(rb.position, -playerUpVector * collideRayLength, Color.aquamarine);
                //if something is colliding with the player from above, fire a ray downwards
                var hit = Physics2D.Raycast(rb.position, -playerUpVector, collideRayLength, level);
                if (hit)
                {
                    //if the ray's distance is smaller than rayCrushDistance, then the player can be considered crushed
                    if (hit.distance <= rayCrushDistance) handleCrush();
                    //but if the distance is larger than crushDistance and shorter than rayCornerDistance, then the player is in a corner
                    else if (hit.distance <= rayCornerDistance) doCornerRotation();
                }
            }
            //if only one of the side overlaps detects a wall, then the player is in a corner
            else if (hitLeft ^ hitRight) doCornerRotation();
            //if both sides detect walls, then the player can be considered crushed
            else if (hitRight && hitLeft) handleCrush();
        }
    }
    #endregion crushFuncs

    #region visualFuncs
    private void handleAnimations()
    {
        anim.speed = input.isIdle ? idleBobSpeed : 1;
        anim.SetBool("isAirborne", isAirborne);
        anim.SetBool("hasBuffered", hasBuffered);
        if (setJumpTrigger)
        {
            anim.SetTrigger(hasBuffered ? "startAirBuffer" : "startAirNormal");
            hasBuffered = setJumpTrigger = false;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(bodyPoint1, 0.5f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(bodyPoint2, 0.5f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + (Vector3)playerUpVector * verticalOverlapOffset, verticalOverlapRad);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + (Vector3)playerRightVector * horizontalOverlapOffset, horizontalOverlapRad);
        Gizmos.DrawWireSphere(transform.position - (Vector3)playerRightVector * horizontalOverlapOffset, horizontalOverlapRad);
    }
    #endregion visualFuncs
}

