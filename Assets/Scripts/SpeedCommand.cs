using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

[CreateAssetMenu(fileName = "New Speed Command", menuName = "Commands/Speed Command")]
public class SpeedCommand : ConsoleCommand
{
    public override bool Process(string[] args)
    {
        if (args.Length != 1) { return false; }

        GameObject.Find("Local").GetComponent<PlayerMovementController>().movementSpeed = float.Parse(args[0]);

        return true;
    }
}
