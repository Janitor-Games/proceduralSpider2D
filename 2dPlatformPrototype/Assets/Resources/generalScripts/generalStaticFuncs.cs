using UnityEngine;
public static class generalStaticFuncs
{
    //acceleration, deceleration and friction function
    public static float accelDecel(float speed, float maxSpeed, float accel, float decel, float friction, float direction, float delta,bool grounded)
    {
        var currentSpeed = speed;
        if (direction != 0)
            currentSpeed += (speed * direction < 0 ? decel : accel) * direction * delta;
        else if (grounded)
            currentSpeed -= Mathf.Min(Mathf.Abs(currentSpeed), friction * delta) * Mathf.Sign(currentSpeed);
        if (grounded) currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);
        //if air speed is above max and player is moving in the same direction, dont increase it
        else if (Mathf.Abs(currentSpeed) >= maxSpeed && currentSpeed * direction > 0) return speed;
        return currentSpeed;
    }

    //timer function 
    public static float timer(float delta, float count, float time, bool condition)
    {
        if (condition) return time;
        else return Mathf.Max(0, count - delta);
    }

    //fires a curved linecast 
    public static RaycastHit2D fireCurvedRay(Vector3 up,Vector3 forward, float direction, Transform castPosition, float castRad, float castAngle, float castArc, int level, Color dbgColor)
    {
        //beginning of the line
        var lineS = up * castRad;
        //angle axis creates a rotation with an angle of direction*castAngle around the transform.forward vector
        //the sign of the angle (aka direction) changes how the linecast is fired: a positive angle creates a linecast that goes counter-clockwise, a negative angle goes clockwise
        var rot = Quaternion.AngleAxis(direction * castAngle, forward);
        //castArc determines the overall arc of the linecast (a value of 360 creates a fully circular cast, a value of 180 creates a half-circle cast)
        var iterations = Mathf.CeilToInt(castArc / Mathf.Abs(castAngle));
        for (int x = 0; x < iterations; x++)
        {
            //end of the line
            var lineE = rot * lineS;
            //the lines are fired around the castPosition
            var hit = Physics2D.Linecast(castPosition.position + lineS, castPosition.position + lineE, level);
            Debug.DrawLine(castPosition.position + lineS, castPosition.position + lineE, dbgColor);
            if (hit.collider != null) return hit;
            //new line begins where the previous has ended
            lineS = lineE;
        }
        return new RaycastHit2D();
    }

    //return a rotated vector by an angle
    public static Vector3 getRotatedVector(float angle,Vector3 vec)
    {
        return Quaternion.Euler(0,0,angle)*vec;
    }
}
