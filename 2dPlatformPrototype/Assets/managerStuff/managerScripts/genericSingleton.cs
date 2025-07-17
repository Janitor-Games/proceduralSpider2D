using UnityEngine;

public class genericSingleton<T> : MonoBehaviour where T : Component
{
    public static T instance {get; private set;}
    protected virtual void Awake(){
        if(instance==null){
            instance=this as T;
            DontDestroyOnLoad(this);
        } else Destroy(this);
    }
}
