using UnityEngine;

public class snapperBodyController : MonoBehaviour
{
    [Header("General Values And References")]
    public snapperHealthAndDeathManager snap;
    public Rigidbody2D rb;
    public fieldOfView fov;
    public LayerMask level;
    public Animator anim;
    public GameObject hurtbox,hitboxNormal,hitboxRetract;
    public Collider2D normalColl;
    public Collider2D[] compositeColl;
    private snapperStates currentState => snap.currentState;
    private bool isEngaged;

    [Header("Movement Values")]
    public float distanceCheck;
    public float lungeSpeed, retractSpeed, rotateSpeed;
    private Vector3 originalPos, targetPos, attackPos;
    private float originalRot;

    [Header("Attacking Values")]
    public float attackDelayTime;
    public float attackTelegraphTime;
    private bool canAttack,hasTelegraphed;
    private float attackDelayCount;

    void Start()
    {
        originalPos = rb.position;
        originalRot = rb.rotation;
    }

    void OnDisable()
    {
        if (currentState != snapperStates.dead)
        {
            snap.setState(snapperStates.idle);
            doStates();
            transform.position = originalPos;
            transform.rotation = Quaternion.Euler(0, 0, originalRot);
        }
    }

    void FixedUpdate()
    {
        insideFOV();
        setStates();
        doStates();
        anim.SetBool("hasTelegraphed",hasTelegraphed);
    }

    #region movementFuncs

    public Vector3 getAttackPos()
    {
        return attackPos;
    }

    private void move(float speed, Vector2 direction)
    {
        rb.MovePosition(rb.position + speed * direction * Time.deltaTime);
    }

    private void rotate(Quaternion goal)
    {
        var rot = Quaternion.RotateTowards(transform.rotation, goal, rotateSpeed * Time.deltaTime);
        rb.MoveRotation(rot);
    }
    #endregion movementFuncs

    #region stateFuncs

    private void insideFOV()
    {
        var detected = fov.getDetectedObject();
        if (detected)
        {
            isEngaged = true;
            if (currentState == snapperStates.idle) snap.setState(snapperStates.targeting);
            if (currentState == snapperStates.targeting)
            {
                if (!hasTelegraphed) targetPos = detected.transform.position;
                else attackPos = targetPos;
                if (canAttack) snap.setState(snapperStates.lunging);
            }
        }
        else isEngaged = false;
    }

    private void setStates()
    {
        //if the player is no longer in range, return to idle
        if (currentState == snapperStates.targeting && !isEngaged)
        {
            snap.setState(snapperStates.idle);
        }
        if (currentState == snapperStates.lunging && (Vector3.Distance(attackPos, rb.position) <= distanceCheck || Vector3.Distance(originalPos, rb.position) >= fov.radius))
        {
            snap.setState(snapperStates.retracting);
        }
        if (currentState == snapperStates.retracting && Vector3.Distance(originalPos, rb.position) <= distanceCheck)
        {
            //if the player is no longer in range, return to idle
            snap.setState(isEngaged ? snapperStates.targeting : snapperStates.idle);
        }
    }

    private void setStateValues(bool retracted, bool lunged, bool hurt, bool normal, bool composite, float delay)
    {
        anim.SetBool("hasRetracted", retracted);
        anim.SetBool("hasLunged", lunged);
        hurtbox.SetActive(hurt);
        normalColl.enabled = normal;
        //disable composite collider
        foreach (Collider2D coll in compositeColl)
            coll.enabled = composite;
        attackDelayCount = delay;
        //different hitboxes, because the normal one looks strange when the snapper is retracting
        hitboxNormal.SetActive(currentState!=snapperStates.retracting);
        hitboxRetract.SetActive(currentState==snapperStates.retracting);
    }

    private void doStates()
    {
        switch (currentState)
        {
            case snapperStates.idle:
                rb.position = originalPos;
                rotate(Quaternion.Euler(0, 0, originalRot));
                setStateValues(false, false, false, true, false, attackDelayTime);
                canAttack = hasTelegraphed = false;
                break;
            case snapperStates.targeting:
                var rot = Quaternion.LookRotation(Vector3.forward, (targetPos - originalPos).normalized);
                rotate(rot);
                setStateValues(false, false, false, true, false, generalStaticFuncs.timer(Time.deltaTime, attackDelayCount, attackDelayTime, false));
                if (attackDelayCount <= 0) canAttack = true;
                hasTelegraphed=attackDelayCount <= attackTelegraphTime;
                break;
            case snapperStates.lunging:
                move(lungeSpeed, (attackPos - originalPos).normalized);
                anim.SetBool("hasLunged", true);
                hasTelegraphed = canAttack = false;
                break;
            case snapperStates.retracting:
                move(retractSpeed, (originalPos - attackPos).normalized);
                setStateValues(true, true, true, false, true, attackDelayTime);
                break;
        }
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        //if the snapper hits a wall when lunging, retract
        if (coll.gameObject.layer == Mathf.Log(level.value, 2) && currentState == snapperStates.lunging)
        {
            snap.setState(snapperStates.retracting);
        }
    }
    #endregion stateFuncs

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(attackPos, 0.5f);
    }
}
