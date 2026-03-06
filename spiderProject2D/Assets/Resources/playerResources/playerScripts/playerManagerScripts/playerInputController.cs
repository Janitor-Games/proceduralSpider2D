using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class playerInputController : MonoBehaviour
{
    [Header("Player Actions")]
    public Vector2 directionVector;
    public bool pressedJump, releasedJump;

    [Header("Mouse")]
    public Vector2 mouseGamePos;
    public bool pressedMouse, releasedMouse;
    private Vector2 prevMouse;
    private bool hasMovedMouse;

    [Header("Idle Values")]
    public float idleTime;
    public bool isIdle;
    private float idleCount;

    void Update()
    {
        directionVector = globalInputManager.readVector("PlayerMove");
        setPressedReleased(ref pressedJump,ref releasedJump,"PlayerJump");
        setPressedReleased(ref pressedMouse,ref releasedMouse,"PlayerShoot");
        handleMousePos();
        idleTimer();
    }

    public void resetIdle()
    {
        idleCount=0f;
    }

    private void setPressedReleased(ref bool press, ref bool release, String name)
    {
        press = globalInputManager.pressed(name);
        release = globalInputManager.released(name);
    }

    private void handleMousePos()
    {
        var mouseScreenPos = globalInputManager.readVector("PlayerMousePos");
        //converts the screen position of the mouse to its position in the game world
        mouseGamePos = cameraController.instance.getMainCam().ScreenToWorldPoint(mouseScreenPos);
        hasMovedMouse = prevMouse != mouseScreenPos;
        prevMouse = mouseScreenPos;
    }

    //check if any action was done
    private bool didAction()
    {
        foreach(InputAction act in InputSystem.actions.actionMaps[0])
        {
            //mouse movement is already checked
            if (act != InputSystem.actions.FindAction("PlayerMousePos") && act.IsPressed()) 
                return true; 
        }
        return false;
    }

    private void idleTimer()
    {
        idleCount = (!hasMovedMouse && !didAction()) ? Mathf.Min(idleCount + Time.deltaTime, idleTime) : 0;
        isIdle = idleCount >= idleTime;
    }


}
