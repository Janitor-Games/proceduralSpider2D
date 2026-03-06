using UnityEngine;
using Unity.Cinemachine;
public enum snapperStates
{
    idle,
    targeting,
    lunging,
    retracting,
    dead
}

public class snapperHealthAndDeathManager : MonoBehaviour,damageInterface
{
    [Header("General Values And References")]
    public Rigidbody2D rb;
    public snapperBodyController controller;
    public LineRenderer neckLine;
    public Transform neckStart;
    public snapperStates currentState;
    public int neckPoints;

    [Header("Health Values")]
    public float maxHP;
    private float currentHP;
    public float currentHealth=>currentHP;
    public float maxHealth=>maxHP;
    public void setCurrent(float val)=>currentHP=val;

    [Header("Death Values")]
    public Vector2 scaleSpriteLine;
    public GameObject snapperDeadPrefab, snapperGibs;
    public Transform jaw1, jaw2;
    public CinemachineBasicMultiChannelPerlin shake;
    public float screenShakeTime;
    private Transform snapperDead;

    void Start()
    {
        setCurrent(maxHP);
        neckLine.enabled = true;
        neckLine.positionCount = neckPoints;
        neckLine.material.mainTextureScale = scaleSpriteLine;
    }

    void Update()
    {
        neckLine.SetPosition(0, neckStart.position);
        neckLine.SetPosition(1, currentState == snapperStates.dead ? snapperDead.position : controller.transform.position);
    }

    #region stateFuncs
    public void setState(snapperStates state)
    {
        currentState = state;
    }
    #endregion stateFuncs

    #region damageFuncs
    public void hurtThing()
    {
        currentHP--;
        if (currentHP <= 0) killThing();
    }

    public void killThing()
    {
        currentState = snapperStates.dead;
        snapperDead = Instantiate(snapperDeadPrefab, controller.transform.position, controller.transform.rotation, transform).transform;
        //snapperHeadDead consists of the 2 jaw bones, so we set them to be the same as the original bones
        var j1 = snapperDead.GetChild(2); j1.rotation = jaw1.rotation; var j2 = snapperDead.GetChild(3); j2.rotation = jaw2.rotation;
        controller.anim.enabled=false;
        //destroy children
        foreach (Transform child in transform)
            if (child != neckStart && child != snapperDead) Destroy(child.gameObject);
        var joint = snapperDead.GetComponent<DistanceJoint2D>();
        joint.connectedBody = rb;
        joint.distance = Vector3.Distance(snapperDead.position, neckStart.position);
        var gibs = Instantiate(snapperGibs, snapperDead.position, snapperDead.rotation);
        gibs.transform.GetChild(0).transform.SetPositionAndRotation(j1.position, j1.rotation);
        gibs.transform.GetChild(1).transform.SetPositionAndRotation(j2.position, j2.rotation);
        generalStaticFuncs.setGibsVector(gibs, snapperDead.transform.right);
        cameraController.instance.doCameraShake(shake, screenShakeTime);
    }
    #endregion damageFuncs
}
