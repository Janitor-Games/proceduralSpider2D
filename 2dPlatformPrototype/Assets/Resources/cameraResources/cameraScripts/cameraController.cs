using Unity.Cinemachine;
using UnityEngine;

public class cameraController : MonoBehaviour
{
    [Header("General Values And References")]
    private static Camera mainCam;
    private static CinemachineCamera cineCam;
    private Collider2D coll;

    void Awake()
    {
        mainCam = transform.GetChild(0).GetComponent<Camera>();
        cineCam = transform.GetChild(1).GetComponent<CinemachineCamera>();
    }

    void Start()
    {
        setCamBounds(GameObject.Find("camBounds").GetComponent<Collider2D>());
    }

    public static Camera getMainCam()
    {
        return mainCam;
    }

    public static CinemachineCamera getCinemachine()
    {
        return cineCam;
    }

    public static void setCamTarget(Transform newTarget)
    {
        cineCam.Target.TrackingTarget = newTarget;
    }

    public static void setCamBounds(Collider2D coll)
    {
        cineCam.GetComponent<CinemachineConfiner2D>().BoundingShape2D=coll;
    }
}

