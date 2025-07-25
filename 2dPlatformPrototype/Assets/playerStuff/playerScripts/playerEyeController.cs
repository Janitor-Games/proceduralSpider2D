using System.Collections;
using Oddworm.Framework;
using UnityEngine;

public class playerEyeController : MonoBehaviour
{
    [Header("General Values And References")]
    public playerBodyController pb;
    public Transform eyeball;
    public GameObject snooze;
    public Animator anim;
    public Camera mainCam;

    [Header("Mouse Position")]
    public Vector2 mouseGamePos;
    public bool hasMovedMouse;
    private Vector2 prevMouse;

    [Header("Blinking")]
    public float blinkWaitMin;
    public float blinkWaitMax, normalBlinkSpeed, idleBlinkSpeed;
    private bool startedBlink;
    private Coroutine blinkCor;
    public bool isEyeIdle;

    void Start()
    {
        blinkCor = null;
        startedBlink = true;
    }

    void Update()
    {
        followMouse();
        handleBlink();
        if (pb.debugMode) DbgDraw.Sphere(mouseGamePos, transform.rotation, new Vector2(0.5f, 0.5f), Color.red);
    }

    private void followMouse()
    {
        var mouseScreenPos = inputManager.instance.readVector("PlayerMousePos");
        //converts the screen position of the mouse to its position in the game world
        mouseGamePos = mainCam.ScreenToWorldPoint(mouseScreenPos);
        eyeball.transform.up = mouseGamePos - (Vector2)transform.position;
        hasMovedMouse = prevMouse != mouseScreenPos;
        prevMouse = mouseScreenPos;
    }

    private void handleBlink()
    {
        if (pb.isIdle)
        {
            if (blinkCor != null) StopCoroutine(blinkCor);
            startedBlink = true;
            if (!isEyeIdle)
            {
                anim.speed = idleBlinkSpeed;
                anim.Play("playerBlinkState");
                snooze.SetActive(true);
            }
        }
        else if (startedBlink)
        {
            anim.speed = normalBlinkSpeed;
            blinkCor = StartCoroutine(doPeriodicBlink());
            startedBlink = false;
            snooze.SetActive(false);
        }
    }

    private IEnumerator doPeriodicBlink()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(blinkWaitMin, blinkWaitMax));
            anim.Play("playerBlinkState");
            //waits for the animation to end
            yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        }
    }

    //animation event function that plays at the middle of the blinking animation
    public void closeEyeIdle()
    {
        isEyeIdle = pb.isIdle;
        if (isEyeIdle) anim.speed = 0f;
    }


}
