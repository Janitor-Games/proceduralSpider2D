using UnityEngine;
public class playerLegData : MonoBehaviour
{
    [Header("Leg Controller Values")]
    public float stepSize;
    public float stepSpeed;
    public float stepDistanceTolerate;
    public float stepHeight;
    [Header("Leg Cast Values")]
    public Transform legCast;
    public float castRad;
    public float castAngle;
    public float castArc;
    public float platformCastOffset;
    public float platformCastLength;
    public LayerMask level;

}
