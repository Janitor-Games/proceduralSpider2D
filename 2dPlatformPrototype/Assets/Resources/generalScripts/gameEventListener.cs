using UnityEngine;
using UnityEngine.Events;

public class gameEventListener : MonoBehaviour
{
    public gameEvent gameEv;
    public UnityEvent triggeredEvent;
    void OnEnable()
    {
        gameEv.addListener(this);
    }

    void OnDisable()
    {
        gameEv.removeListener(this);
    }

    public void OnEventTriggered()
    {
        triggeredEvent.Invoke();
    }
}
