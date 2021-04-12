using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomBuilding : MonoBehaviour
{
    public string link;

    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        player.OpenLink(link);
    }
}
