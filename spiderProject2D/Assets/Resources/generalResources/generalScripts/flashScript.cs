using System.Collections;
using UnityEngine;

public class flashScript : MonoBehaviour
{
    [Header("Flash Values")]
    public Color color;
    public Material flashMat;
    private Material[] materials;
    private SpriteRenderer[] sprites;
    public float flashTime;
    private float flashCount;

    void Awake()
    {
        sprites = GetComponentsInChildren<SpriteRenderer>();
        materials = new Material[sprites.Length];
        for (int x = 0; x < sprites.Length; x++)
        {
            sprites[x].material = flashMat;
            materials[x] = sprites[x].material;
        }
    }

    void Update()
    {
        flashCount = generalStaticFuncs.timer(Time.deltaTime, flashCount, flashTime, false);
        setFlashAmount(flashCount>0?Mathf.Lerp(0f, 1f, flashCount / flashTime):0);
    }

    public void doFlash()
    {
        setColor();
        flashCount = flashTime;
    }
    
    private void setFlashAmount(float amount)
    {
        for (int x = 0; x < materials.Length; x++)
        {
            materials[x].SetFloat("_FlashAmount", amount);
        }
    }

    private void setColor()
    {
        for (int x = 0; x < materials.Length; x++)
        {
            materials[x].SetColor("_Color", color);
        }
    }
}

