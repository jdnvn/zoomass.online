using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

[CreateAssetMenu(fileName = "Dance Command", menuName = "Commands/Dance Command")]
public class DanceCommand : ConsoleCommand
{
    public override bool Process(string[] args)
    {
        //if (args.Length != 1 || GameObject.Find("Local").GetComponent<Player>().rights < 2) { return false; }

        GameObject.Find("Local").GetComponent<PlayerMovementController>().macarena();

        return true;
    }
}
