using System;
using UnityEngine;
using UnityEngine.InputSystem;

public static class globalInputManager
{
    public static bool pressed(String name){
        return InputSystem.actions.FindAction(name).WasPressedThisFrame();
    }

    public static bool released(String name)
    {
        return InputSystem.actions.FindAction(name).WasReleasedThisFrame();
    }

    public static Vector2 readVector(String name)
    {
        return InputSystem.actions.FindAction(name).ReadValue<Vector2>();
    }
}
