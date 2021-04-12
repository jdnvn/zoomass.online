using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

[CreateAssetMenu(fileName = "New Jump Command", menuName = "Commands/Jump Command")]
public class JumpCommand : ConsoleCommand
{
    public override bool Process(string[] args)
    {
        if (args.Length != 1 || GameObject.Find("Local").GetComponent<Player>().rights < 2) { return false; }

        GameObject.Find("Local").GetComponent<PlayerMovementController>().jumpSpeed = float.Parse(args[0]);

        return true;
    }
}

