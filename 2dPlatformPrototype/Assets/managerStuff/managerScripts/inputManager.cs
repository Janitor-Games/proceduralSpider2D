using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class inputManager : genericSingleton<inputManager>
{
    [Header("General Values And References")]
    public static PlayerInput player;

    protected override void Awake()
    {
        base.Awake();
        player=GetComponent<PlayerInput>();
    }

    public bool pressed(String name){
        return player.actions[name].WasPressedThisFrame();
    }

    public bool released(String name){
        return player.actions[name].WasReleasedThisFrame();
    }

    public Vector2 readVector(String name){
        return player.actions[name].ReadValue<Vector2>();
    }
}
