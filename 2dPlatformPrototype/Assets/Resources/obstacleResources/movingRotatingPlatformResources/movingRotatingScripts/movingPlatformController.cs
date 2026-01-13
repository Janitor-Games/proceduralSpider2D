using UnityEngine;
using System;

public class movingPlatformController : movingRotatingPlatformBase
{
    [Header("Target Values")]
    public Transform targetsHolder;
    public float targetDistanceTolerance;
    private int currentTargetIndex,movementDirection;
    private Vector2 currentTargetPos;

    [Header("Movement Values")]
    public float maxMoveSpeed;

    void Start()
    {
        loadSprites(isControlable, "movingSprites", "movePlatformOffSprite");
        lineSolid.positionCount = targetsHolder.childCount;
        lineDashed.positionCount = targetsHolder.childCount;
        for (int x = 0; x < targetsHolder.childCount; x++)
        {
            var thisTarget = targetsHolder.GetChild(x);
            Vector3 dir1, dir2;
            //set rotation of rectangles of the start and end of platform route 
            if (x == 0 || x == targetsHolder.childCount - 1)
            {
                var otherTarget = targetsHolder.GetChild(x + (x == 0 ? 1 : -1)).position;
                dir1 = dir2 = getTargetRotation(otherTarget, thisTarget.position);
            }
            //set rotation of rectangles for every other target, and set the arrow sprite
            else
            {
                dir1 = getTargetRotation(targetsHolder.GetChild(x - 1).position, thisTarget.position);
                dir2 = getTargetRotation(targetsHolder.GetChild(x + 1).position, thisTarget.position);
                setTargetSign(x, "Arrow");
            }
            thisTarget.GetChild(1).right = dir1;
            thisTarget.GetChild(2).right = dir2;
            lineSolid.SetPosition(x, thisTarget.position);
            lineDashed.SetPosition(x, thisTarget.position);
        }
        currentTargetPos = targetsHolder.GetChild(currentTargetIndex).position;
        transform.position = currentTargetPos;
        calcNextTarget();
    }

    void FixedUpdate()
    {
        handleMove();
    }

    private void handleMove()
    {
        if (isControlable)
        {
            if (isControlling) setCount(0);
            currentSprite.sprite = getSpriteFromSprites(isControlling ? "movePlatformOffSprite" : "movePlatformOnSprite");
        }
        if (!isControlable || isControlling)
            rb.linearVelocity=(currentTargetPos-rb.position).normalized * maxMoveSpeed;
        if (Vector2.Distance(currentTargetPos, rb.position) <= targetDistanceTolerance)
            calcNextTarget();
    }

    private void calcNextTarget()
    {
        if (currentTargetIndex >= targetsHolder.childCount - 1 || currentTargetIndex <= 0)
        {
            movementDirection = currentTargetIndex <= 0 ? 1 : -1;
            if (isControlable) resetInteract();
            //set the stop sign for end of route, and set the arrow sign at the start of the route
            //and flip between them
            int startIndex = movementDirection > 0 ? 0 : targetsHolder.childCount - 1;
            int endIndex = movementDirection > 0 ? targetsHolder.childCount - 1 : 0;
            setTargetSign(startIndex, "Arrow");
            setTargetSign(endIndex, "Stop");
            targetsHolder.GetChild(endIndex).GetChild(0).rotation = Quaternion.Euler(0, 0, 0);
            int step = movementDirection > 0 ? 1 : -1;
            //rotate the arrows to point to the next target in line
            for (int x = startIndex; x != endIndex; x += step)
            {
                var sign = targetsHolder.GetChild(x).GetChild(0);
                var nextPos = targetsHolder.GetChild(x + step).position;
                sign.right = getTargetRotation(nextPos, sign.position);
            }
            lineDashed.material.SetFloat("_animateSpeed", movementDirection * dashedAnimateSpeed);
        }
        currentTargetIndex += movementDirection;
        currentTargetPos = targetsHolder.GetChild(currentTargetIndex).position;
    }

    private void setTargetSign(int target, String sign)
    {
        targetsHolder.GetChild(target).GetChild(0).GetComponent<SpriteRenderer>().sprite = getSpriteFromSprites(sign);
    }

    private Vector3 getTargetRotation(Vector3 pos1, Vector3 pos2)
    {
        return (pos1 - pos2).normalized;
    }
}
