using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class playerEyeController : MonoBehaviour
{
    [Header("General Values And References")]
    public playerInputController input;
    public Transform eyeball;
    public GameObject snooze;
    public Animator anim;

    [Header("Eye Sprite")]
    public SpriteRenderer eyeSprite;
    public Sprite eyeAlive, eyeDead;

    [Header("Blinking")]
    public float blinkWaitMin;
    public float blinkWaitMax, idleBlinkSpeed;
    private bool startedBlink, isEyeIdle;
    private Coroutine blinkCor;

    [Header("Shooting")]
    public Transform lookAhead;
    public float rotateAimSpeed;
    public float maxAheadDistance;
    private Image aimCursor;

    void Start()
    {
        aimCursor=GameObject.Find("aimCursor").GetComponent<Image>();
        Cursor.visible = false;
        aimCursor.enabled = true;
        blinkCor = null;
        startedBlink = true;
        eyeSprite.sprite = eyeAlive;
    }

    void Update()
    {
        followMouse();
        handleBlink();
    }

    //animation event function that plays at the middle of the blinking animation
    public void closeEyeIdle()
    {
        isEyeIdle = input.isIdle;
        if (isEyeIdle) anim.speed = 0f;
    }

    public void setCursorToFollow(Image cursor)
    {
        aimCursor = cursor;
    }

    //eye death sequence when the player dies
    public void handleDeath()
    {
        aimCursor.enabled = false;
        anim.Play("playerEyeBlinkState", -1, 0);
        eyeSprite.transform.localPosition = new Vector3(0, 0, 0);
        eyeSprite.transform.rotation = Quaternion.Euler(0, 0, 0);
        eyeSprite.sprite = eyeDead;
        snooze.SetActive(false);
        enabled = false;
    }

    private void followMouse()
    {
        Vector2 mouse = input.mouseGamePos;
        aimCursor.rectTransform.position = mouse;
        aimCursor.rectTransform.Rotate(0, 0, Time.deltaTime * rotateAimSpeed);
        eyeball.up = mouse - (Vector2)transform.position;
        Vector2 bodyPos=transform.parent.parent.position;
        if(Vector2.Distance(mouse,bodyPos)/2<maxAheadDistance)
            lookAhead.position=(bodyPos+mouse)/2;
        else 
            lookAhead.position=bodyPos+(mouse-bodyPos).normalized*maxAheadDistance;
        cameraController.setCamTarget(lookAhead);
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
        //when the player is not idling, the blinking coroutine continues and the snooze particles are disabled
        if (!startedBlink) return;
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
}
