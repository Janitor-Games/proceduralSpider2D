using UnityEngine;
public class playerLegData : MonoBehaviour
{
    [Header("General Values And References")]
    public playerBodyController pb;
    public Transform legCast;
    private Vector2 legCastPos;

    [Header("Leg Controller Values")]
    public float stepSize;
    public float stepSpeed;
    public float stepDistanceTolerate;
    public float stepHeight;

    [Header("Leg Cast Values")]
    public float legCastAdvance;
    public float castRad;
    public float castAngle;
    public float castArc;
    public LayerMask level;

    void Start()
    {
        legCastPos = legCast.localPosition;
    }

    void FixedUpdate()
    {
        setLegCorrection();
    }

    //shifts the castPosition of the leg's rays, creating a more natural walk cycle
    private void setLegCorrection()
    {
        if (pb.isGrounded)
        {
            float legOffset = pb.groundSpeed * legCastAdvance / pb.maxMoveSpeed;
            legCast.localPosition = legCastPos + (Vector2.right * legOffset);
        }
    }
}
