using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class playerEyeController : MonoBehaviour
{
    [Header("General Values And References")]
    public flashScript flash;
    public hitStopScript hitStop;
    public Transform eyeball;
    public GameObject snooze;
    public Animator anim;
    private playerInputController input;

    [Header("Eye Sprite")]
    public SpriteRenderer eyeSprite;
    public Sprite eyeAlive, eyeDead, eyeHurt;
    private Vector3 originalEyePos;

    [Header("HIt Stop Values")]
    public float hurtStopTime;
    public float deathStopTime;

    [Header("Blinking")]
    public float blinkWaitMin;
    public float blinkWaitMax, idleBlinkSpeed;
    private bool startedBlink, isEyeIdle;
    private Coroutine blinkCor;

    [Header("Aiming And Shooting")]
    public Collider2D playerColl;
    public Transform camFollow;
    public float rotateAimSpeed;
    public float maxAheadDistance;
    private bool canShoot, canAim;
    private objectPoolers projPoolers;
    private Image aimCursor;


    void Awake()
    {
        input = GetComponentInParent<playerInputController>();
        projPoolers = GetComponentInParent<objectPoolers>();
    }

    void Start()
    {
        Cursor.visible = false;
        blinkCor = null;
        eyeSprite.sprite = eyeAlive;
        originalEyePos = eyeSprite.transform.localPosition;
        cameraController.instance.setCamTarget(camFollow);
        aimCursor = GameObject.Find("aimCursor").GetComponent<Image>();
        aimCursor.enabled = startedBlink = canShoot = canAim = true;
    }

    void Update()
    {
        if (canAim) followMouse();
        fireProjectile(0);
        handleBlink();
    }

    #region visualFuncs
    //animation event function that plays at the middle of the blinking animation
    public void closeEyeIdle()
    {
        isEyeIdle = input.isIdle;
        if (isEyeIdle) anim.speed = 0f;
    }

    //set cursor image
    public void setCursorToFollow(Image cursor)
    {
        aimCursor = cursor;
    }

    private void handleBlink()
    {
        if (input.isIdle)
        {
            //when the player is idling, the blinking coroutine stops
            //the blink plays and pauses when the eye is closed because of closeEyeIdle
            //and snooze particles are active
            if (blinkCor != null) StopCoroutine(blinkCor);
            startedBlink = true;
            if (!isEyeIdle)
            {
                anim.speed = idleBlinkSpeed;
                anim.Play("playerEyeBlinkState");
                snooze.SetActive(true);
            }
            return;
        }
        if (!startedBlink) return;
        //when the player is not idling, the blinking coroutine continues and the snooze particles are disabled
        anim.speed = 1;
        blinkCor = StartCoroutine(doPeriodicBlink());
        startedBlink = false;
        snooze.SetActive(false);
    }

    private IEnumerator doPeriodicBlink()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(blinkWaitMin, blinkWaitMax));
            anim.Play("playerEyeBlinkState");
            //waits for the animation to end
            yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        }
    }

    //sequence for when the player gets hit
    public void handleHurt()
    {
        setEye(eyeHurt, hurtStopTime);
        StartCoroutine(waitForStopEnd());
    }

    //sequence for when the player dies
    public void handleDeath()
    {
        setEye(eyeDead, deathStopTime);
        aimCursor.enabled=false;
    }

    private void setEye(Sprite eye, float stop)
    {
        canAim = false;
        anim.Play("playerEyeBlinkState", -1, 0);
        eyeSprite.transform.localPosition = Vector3.zero;
        eyeball.up = Vector2.up;
        eyeSprite.sprite = eye;
        snooze.SetActive(false);
        hitStop.stopDuration = stop;
        hitStop.enableHitStop();
    }

    private IEnumerator waitForStopEnd()
    {
        while (Time.timeScale == 0)
            yield return null;
        flash.doFlash();
        eyeSprite.transform.localPosition = originalEyePos;
        eyeSprite.sprite = eyeAlive;
        canAim = true;
    }

    #endregion visualFuncs

    #region shootingFuncs
    private void followMouse()
    {
        Vector2 mouse = input.mouseGamePos;
        aimCursor.rectTransform.position = mouse;
        aimCursor.rectTransform.Rotate(0, 0, Time.deltaTime * rotateAimSpeed);
        eyeball.up = (mouse - (Vector2)transform.position).normalized;
        Vector2 bodyPos = transform.parent.parent.position;
        if (Vector2.Distance(mouse, bodyPos) / 2 < maxAheadDistance) camFollow.position = (bodyPos + mouse) / 2;
        else camFollow.position = bodyPos + (mouse - bodyPos).normalized * maxAheadDistance;
    }

    private void fireProjectile(int typeProj)
    {
        if (input.pressedMouse)
        {
            //pull projectile from the projectile pool
            var p = projPoolers.getObjFromPool(typeProj);
            if (p != null && canShoot)
            {
                anim.SetTrigger("hasShot");
                Physics2D.IgnoreCollision(p.GetComponent<Collider2D>(), playerColl);
                p.transform.position = transform.position;
                p.transform.right = (input.mouseGamePos - (Vector2)transform.position).normalized;
                p.SetActive(true);
                StartCoroutine(doShootCooldown(p.GetComponent<playerProjectile1Script>().fireCooldown));
            }
        }
    }

    private IEnumerator doShootCooldown(float cool)
    {
        canShoot = false;
        yield return new WaitForSeconds(cool);
        canShoot = true;
    }
    #endregion shootingFuncs
}
