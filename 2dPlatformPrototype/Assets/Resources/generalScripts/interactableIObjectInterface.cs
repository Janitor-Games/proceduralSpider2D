
using UnityEngine;

public interface interactableIObjectInterface
{
    void interact();
}

public class controlableObject : MonoBehaviour, interactableIObjectInterface
{
    [Header("General Values And References")]
    public bool isControlling;
    public int interactToControl;
    private int interactCount;
    
    public void interact()
    {
        interactCount++;
        isControlling = interactCount == interactToControl;
        if (interactCount > interactToControl) interactCount = 0;
    }

    public void resetInteract()
    {
        isControlling = false;
        interactCount = 0;
    }

    public void setCount(int num)
    {
        interactCount = num;
    }
}
