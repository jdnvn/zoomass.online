using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class PlayerMenu : MonoBehaviour
{
    public GameObject playerEntryPrefab;
    public static PlayerMenu instance;

    //// Start is called before the first frame update
    //void Start()
    //{
    //    LoadPlayers();
    //}

    //// Update is called once per frame
    //void Update()
    //{
    //    LoadPlayers();
    //}
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }


    public void LoadPlayers(List<Player> players)
    {
        //List<Player> players = GameObject.Find("NewNetworkManager").GetComponent<NewNetworkManager>().players;
        GameObject playerEntry;
        Debug.Log(players.Count);

        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        foreach (Player player in players)
        {
            playerEntry = Instantiate(playerEntryPrefab, transform);
            playerEntry.transform.GetChild(0).GetComponent<TMP_Text>().text = player.username;
        }
    }
}
