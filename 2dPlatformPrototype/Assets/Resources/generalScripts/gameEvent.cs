using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "gameEvent", menuName = "Scriptable Objects/gameEvent")]
public class gameEvent : ScriptableObject
{
    List<gameEventListener> listens=new List<gameEventListener>();
    public void triggerEvent()
    {
        for(int x=0; x<listens.Count; x++)
        {
            listens[x].OnEventTriggered();
        }
    }

    public void addListener(gameEventListener listen)
    {
        listens.Add(listen);
    }

    public void removeListener(gameEventListener listen)
    {
        listens.Remove(listen);
    }
}
