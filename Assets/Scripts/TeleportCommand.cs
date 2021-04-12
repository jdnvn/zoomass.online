using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

[CreateAssetMenu(fileName = "New Teleport Command", menuName = "Commands/Teleport Command")]
public class TeleportCommand : ConsoleCommand
{
    Dictionary<string, Vector3> locations = new Dictionary<string, Vector3>()
    {
        { "dubois", new Vector3(26.92f, 88.5f, -91f) },
        { "hotel", new Vector3(164.86f, 49f, -154.3f) },
        { "fac", new Vector3(-135f, 27f, -213f) },
        { "mullins", new Vector3(50.9f, 17f, 174.56f) },
        { "staff", new Vector3(-549.8f, 59f, -26.8f) },
        { "lib", new Vector3(33.16f, 27.6f, -85.4f) },
        { "spawn", new Vector3(40f, 24.55f, -132.63f) },
        { "hammock", new Vector3(36.52147f, 27.1925f, -45.45518f) },
    };
    public override bool Process(string[] args)
    {
        if (args.Length == 2 || args.Length > 3 || GameObject.Find("Local").GetComponent<Player>().rights < 2) { return false; }

        if (args.Length == 3)
        {
            if (!float.TryParse(args[0], out float x) || !float.TryParse(args[1], out float y) || !float.TryParse(args[2], out float z))
            {
                return false;
            }

            GameObject.Find("Local").transform.position = new Vector3(x, y, z);
        } else
        {
            if (!locations.ContainsKey(args[0])) return false;

            GameObject.Find("Local").transform.position = locations[args[0]];
        }

        return true;
    }
}
