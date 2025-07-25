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
        player = GetComponent<PlayerInput>();
    }

    public bool pressed(String name)
    {
        bool press = player.actions[name].WasPressedThisFrame();
        return press;
    }

    public bool released(String name)
    {
        return player.actions[name].WasReleasedThisFrame();
    }

    public Vector2 readVector(String name)
    {
        Vector2 read = player.actions[name].ReadValue<Vector2>();
        return read;
    }
}
