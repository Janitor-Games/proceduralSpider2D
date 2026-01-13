using Unity.VisualScripting;
using UnityEngine;

public class uiManager : MonoBehaviour
{
    private static Canvas canv;

    void Awake()
    {
        canv=GetComponent<Canvas>();
    }

    void Start()
    {
        canv.worldCamera=cameraController.getMainCam();
    }

    public static Canvas getCanv()
    {
        return canv;
    }
    
    void Update()
    {
        
    }
}
