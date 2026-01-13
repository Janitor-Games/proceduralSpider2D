using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class playerHealthDeathManager : MonoBehaviour
{

    [Header("General Values And References")]
    public GameObject playerPrefab;
    public GameObject playerUI;
    public hitStopScript hitStop;
    public playerInputController input;
    private Transform spawnPoint;
    private bool startSpawn;
    private playerBodyController pb;

    [Header ("Death Values")]
    public gameEvent playerDeadEvent;
    public float timeForRespawn;

    [Header("Shatter Effect")]
    public GameObject playerShatter;
    public CinemachineImpulseSource screenShake;
    public float shatterScreenShake;
    public float shatterRBForce, shatterRBRotate;

    [Header("Hit Stop Values")]
    public float hitStopDuration;

    void Awake()
    {
        spawnPoint = GameObject.Find("playerSpawnPoint").transform;
    }

    void Start()
    {
        startSpawn=true;
        spawnPlayer();
    }

    public void spawnPlayer()
    {
        var player=Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
        pb=player.GetComponentInChildren<playerBodyController>();
        pb.input=input;
        player.GetComponentInChildren<playerEyeController>().input=input;
        if(startSpawn) Instantiate(playerUI, uiManager.getCanv().transform);
        startSpawn=false;
    }

    public void killPlayer()
    {
        hitStop.enableHitStop(hitStopDuration);
        playerDeadEvent.triggerEvent();
        StartCoroutine(deathSequence());
    }

    private void setShatter(GameObject shatter, int index, float directionForce)
    {
        var shatterScript = shatter.transform.GetChild(index).GetComponent<playerShatterScript>();
        shatterScript.death = this;
        shatterScript.shatterDirection = directionForce;
    }

    IEnumerator deathSequence()
    {
        while (Time.timeScale != 1.0f)
            yield return null;
        var shatter = Instantiate(playerShatter, pb.transform.position, pb.transform.rotation);
        //destroy player
        Destroy(pb.transform.parent.gameObject);
        setShatter(shatter,0, -1);
        setShatter(shatter,1, 1);
        cameraController.setCamTarget(shatter.transform);
        screenShake.GenerateImpulseWithVelocity(shatterScreenShake * shatter.transform.up);
        yield return new WaitForSeconds(timeForRespawn);
        Destroy(shatter);
        spawnPlayer();
    }

}
