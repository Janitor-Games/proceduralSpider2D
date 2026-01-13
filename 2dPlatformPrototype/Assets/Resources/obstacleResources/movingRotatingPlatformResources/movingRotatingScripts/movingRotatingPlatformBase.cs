using UnityEngine;
using System;
using System.Collections.Generic;

public class movingRotatingPlatformBase : controlableObject
{
    [Header("General Values And References")]
    public Rigidbody2D rb;
    public bool isControlable;

    [Header("Sprites")]
    public SpriteRenderer currentSprite;
    protected List<Sprite> sprites=new List<Sprite>();

    [Header("Line Renderer")]
    public LineRenderer lineSolid;
    public LineRenderer lineDashed;
    public float dashedAnimateSpeed;

    protected void loadSprites(bool isInteract, String link, String spr)
    {
        sprites.AddRange(Resources.LoadAll<Sprite>("obstacleResources/movingRotatingPlatformResources/movingRotatingSprites/" + link));
        if (isInteract)
        {
            currentSprite.sprite = getSpriteFromSprites(spr);
        }
        else currentSprite.sprite = null;
    }

    protected Sprite getSpriteFromSprites(String str)
    {
        foreach (Sprite spr in sprites)
            if (spr.name.Contains(str)) return spr;
        return null;
    }
}
