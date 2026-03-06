using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public class playerHealthDeathManager : MonoBehaviour, damageInterface
{

    [Header("General Values And References")]
    public GameObject playerPrefab;
    public GameObject playerUI;
    public playerInputController input;
    public gameEvent playerHurtEvent;
    public objectPoolers playerProjPoolers;
    private Transform spawnPoint;

    [Header("Health And Death Values")]
    public float maxHP;
    public gameEvent playerDeathEvent;
    public GameObject playerGibs;
    public CinemachineBasicMultiChannelPerlin screenShake;
    public float respawnTime, invincibleTime, screenShakeTime;
    private bool canHurt;
    private float currentHP;
    public float currentHealth => currentHP;
    public float maxHealth => maxHP;
    public void setCurrent(float val) => currentHP = val;
    private TextMeshProUGUI healthText,deathText;

    void Awake()
    {
        spawnPoint = GameObject.Find("playerSpawnPoint").transform;
    }

    void Start()
    {
        canHurt = true;
        var ui=Instantiate(playerUI, GameObject.Find("Canvas").transform);
        healthText=ui.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        deathText=ui.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        spawnPlayer();
    }

    public void spawnPlayer()
    {
        setCurrent(maxHealth);
        setHealthText();
        Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity, transform);
        deathText.enabled=false;
    }

    public void killThing()
    {
        playerDeathEvent.triggerEvent();
        StartCoroutine(deathSequence());
    }

    public void hurtThing()
    {
        if (canHurt)
        {
            currentHP--;
            setHealthText();
            if (currentHP <= 0) killThing();
            else
            {
                playerHurtEvent.triggerEvent();
                StartCoroutine(invincibleCooldown());
            }
            cameraController.instance.doCameraShake(screenShake, screenShakeTime);
        }
    }

    private void setHealthText()
    {
        healthText.SetText("Health: "+currentHealth+"/"+maxHealth);
    }

    private IEnumerator invincibleCooldown()
    {
        canHurt = false;
        yield return new WaitForSeconds(invincibleTime);
        canHurt = true;
    }

    private IEnumerator deathSequence()
    {
        while (Time.timeScale == 0)
            yield return null;
        deathText.enabled=true;
        var pb = GetComponentInChildren<playerBodyController>();
        var gibs = Instantiate(playerGibs, pb.transform.position, pb.transform.rotation);
        var gibVector = pb.isGrounded ? (Vector2)pb.transform.up : Vector2.up;
        gibVector=generalStaticFuncs.getRotatedVector(45,gibVector);
        for(int x=0; x<gibs.transform.childCount; x++)
            gibs.transform.GetChild(x).GetComponent<gibScript>().gibVector=generalStaticFuncs.getRotatedVector(90*x,gibVector);
        //destroy player
        Destroy(transform.GetChild(0).gameObject);
        cameraController.instance.setCamTarget(gibs.transform);
        yield return new WaitForSeconds(respawnTime);
        spawnPlayer();
    }
}
