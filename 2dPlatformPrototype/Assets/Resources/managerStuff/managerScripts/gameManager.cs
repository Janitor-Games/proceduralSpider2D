using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
public class gameManager : genericSingleton<gameManager>
{
    [Header ("Initialize Objects")]
    public GameObject playerSpawner;
    public GameObject cameras;
    public GameObject uiCanvas;

    protected override void Awake()
    {
        base.Awake();
    }

    public void changeScene(string name)
    {
        StartCoroutine(loadScene(name));
    }

    private IEnumerator loadScene(string name)
    {
        yield return SceneManager.LoadSceneAsync(name);
        if (name == "testLevel")
        {
            initObjects();           
        }
    }

    public void initObjects()
    {
        Instantiate(cameras,new Vector3(0,0,0),Quaternion.identity);
        Instantiate(playerSpawner,new Vector3(0,0,0),Quaternion.identity);
        Instantiate(uiCanvas,new Vector3(0,0,0),Quaternion.identity);
    }
}