using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class cameraController : genericSingleton<cameraController>
{
    [Header("General Values And References")]
    private Camera mainCam;
    private CinemachineCamera cineCam;
    private CinemachineBasicMultiChannelPerlin currentShake;

    protected override void Awake()
    {
        base.Awake();
        mainCam = transform.GetComponentInChildren<Camera>();
        cineCam = transform.GetComponentInChildren<CinemachineCamera>();
        currentShake = transform.GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();
    }

    void Start()
    {
        setCamBounds(GameObject.Find("camBounds").GetComponent<Collider2D>());
    }

    public Camera getMainCam()
    {
        return mainCam;
    }

    public CinemachineCamera getCinemachine()
    {
        return cineCam;
    }

    public void setCamTarget(Transform newTarget)
    {
        cineCam.Target.TrackingTarget = newTarget;
    }

    public void setCamBounds(Collider2D coll)
    {
        cineCam.GetComponent<CinemachineConfiner2D>().BoundingShape2D = coll;
    }

    public void doCameraShake(CinemachineBasicMultiChannelPerlin perlin, float time)
    {
        StartCoroutine(cameraShakeCoroutine(perlin, time));
    }

    private IEnumerator cameraShakeCoroutine(CinemachineBasicMultiChannelPerlin perlin, float time)
    {
        setCameraShake(perlin);
        yield return new WaitForSeconds(time);
        clearCameraShake();
    }

    private void setCameraShake(CinemachineBasicMultiChannelPerlin perlin)
    {
        currentShake.NoiseProfile=perlin.NoiseProfile;
        currentShake.AmplitudeGain=perlin.AmplitudeGain;
        currentShake.FrequencyGain=perlin.FrequencyGain;
    }

    private void clearCameraShake()
    {
        currentShake.NoiseProfile=null;
        currentShake.AmplitudeGain=0;
        currentShake.FrequencyGain=0;
    }
}

