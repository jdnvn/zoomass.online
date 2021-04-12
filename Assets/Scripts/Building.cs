using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public string name;
    public int id;

    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        player.AttemptEnterBuilding(id);
    }
}
