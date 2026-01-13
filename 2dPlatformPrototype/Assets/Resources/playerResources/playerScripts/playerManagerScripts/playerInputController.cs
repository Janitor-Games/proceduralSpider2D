using UnityEngine;

public class playerInputController : MonoBehaviour
{
    [Header("Player Actions")]
    public Vector2 directionVector;
    public bool interacted, pressedJump,releasedJump;

    [Header("Mouse Position")]
    public Vector2 mouseGamePos;
    private Vector2 prevMouse;
    private bool hasMovedMouse;

    [Header("Idle Values")]
    public float idleTime;
    public bool isIdle;
    private float idleCount;

    void Update()
    {
        directionVector = globalInputManager.readVector("PlayerMove");
        interacted = globalInputManager.pressed("PlayerInteract");
        pressedJump=globalInputManager.pressed("PlayerJump");
        releasedJump = globalInputManager.released("PlayerJump");
        handleMouse();
        idleTimer();
    }

    public void handleDeath()
    {
        idleCount=0f;
    }

    private void handleMouse()
    {
        var mouseScreenPos = globalInputManager.readVector("PlayerMousePos");
        //converts the screen position of the mouse to its position in the game world
        mouseGamePos =cameraController.getMainCam().ScreenToWorldPoint(mouseScreenPos);
        hasMovedMouse = prevMouse != mouseScreenPos;
        prevMouse = mouseScreenPos;
    }

    private void idleTimer()
    {
        idleCount = (!hasMovedMouse && directionVector==Vector2.zero && !pressedJump) ? Mathf.Min(idleCount + Time.deltaTime, idleTime) : 0;
        isIdle = idleCount >= idleTime;
    }
}
