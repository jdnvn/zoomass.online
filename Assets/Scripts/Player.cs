using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using PlayFab;
using PlayFab.DataModels;
using PlayFab.ClientModels;
using System;
using Random = UnityEngine.Random;

public class Player : NetworkBehaviour
{
    public PlayFab.DataModels.EntityKey playerEntity;

    public TextMesh playerUsernameText;
    public GameObject floatingInfo;

    [SyncVar(hook = nameof(OnRightsChanged))]
    public int rights = 1;

    [SyncVar(hook = nameof(OnNameChanged))]
    public string username;

    [SyncVar(hook = nameof(OnNameColorChanged))]
    public Color playerNameColor = Color.white;

    [SyncVar(hook = nameof(OnPlayerColorChanged))]
    public Color playerColor = Color.white;

    public bool isGuest;
    public uint guestNum;

    public static event Action<string> OnMessage;


    void OnRightsChanged(int _old, int _new)
    {
        if (_new > 1) playerUsernameText.color = new Color(255f, 174f, 0f);
    }

    void OnNameChanged(string _old, string _new)
    {
        playerUsernameText.text = username;
    }

    void OnNameColorChanged(Color _old, Color _new)
    {
        playerUsernameText.color = _new;
    }

    void OnPlayerColorChanged(Color _old, Color _new)
    {
        //if (isLocalPlayer)
        //{
        transform.Find("Head").GetComponent<SkinnedMeshRenderer>().sharedMaterial.color = _new;
        transform.Find("Torso").GetComponent<SkinnedMeshRenderer>().sharedMaterial.color = _new;
        transform.Find("Legs").GetComponent<SkinnedMeshRenderer>().sharedMaterial.color = _new;
        //}
    }

    public override void OnStartLocalPlayer()
    {
        if (LoginManager.instance.isGuest)
        {
            isGuest = true;

            CmdSetupPlayer("guest " + NetworkClient.connection.identity.netId, new Color(1f, 1f, 1f), Random.ColorHSV());
            CmdSendLoginMessage();
            return;
        }

        var userDataRequest = new GetUserDataRequest() { Keys = new List<string> { "rights" } };

        PlayFabClientAPI.GetUserData(userDataRequest, result =>
        {
            if (result.Data == null) return;

            if (result.Data.ContainsKey("rights")) rights = Int16.Parse(result.Data["rights"].Value);
        }, (error) =>
        {
            Debug.Log("Got error retrieving user data:");
            Debug.Log(error.GenerateErrorReport());
        });


        Color nameColor;
        Color matColor = Random.ColorHSV();
        string playerUserName;

        string email = String.IsNullOrEmpty(LoginManager.instance.emailField.text) ? PlayerPrefs.GetString("email") : LoginManager.instance.emailField.text;

        var accInfoRequest = new PlayFab.ClientModels.GetAccountInfoRequest { Email = email };

        PlayFabClientAPI.GetAccountInfo(accInfoRequest, (getResult) =>
        {
            playerUserName = getResult.AccountInfo.Username;

            if (rights > 1) nameColor = new Color(255f, 174f, 0f);
            else nameColor = new Color(1f, 1f, 1f);

            CmdSetupPlayer(playerUserName, nameColor, matColor);
            CmdSendLoginMessage();
        }, (error) =>
        {
            Debug.LogError(error);
            NetworkManager.singleton.StopClient();
        });

        playerEntity = LoginManager.instance.playerEntity;
    }


    [Command]
    public void CmdSetupPlayer(string _username, Color _nameColor, Color _matColor)
    {
        username = _username;
        playerNameColor = _nameColor;
        playerColor = _matColor;

        NewNetworkManager networkManager = GameObject.Find("NewNetworkManager").GetComponent<NewNetworkManager>();

        networkManager.players.Add(this);
        //RpcAddPlayer();
    }

    [Command]
    public void CmdLogOutPlayer()
    {
        NewNetworkManager networkManager = GameObject.Find("NewNetworkManager").GetComponent<NewNetworkManager>();
        networkManager.players.Remove(this);
        //RpcAddPlayer();
    }

    [Command]
    public void CmdSendMessage(string message)
    {
        string color = ColorUtility.ToHtmlStringRGB(playerNameColor);

        RpcHandleMessage($"<color=#{color}>{username}</color>: {message}");
    }

    [Command]
    public void CmdSendLoginMessage()
    {
        string color = ColorUtility.ToHtmlStringRGB(playerNameColor);

        RpcHandleMessage($"<sprite index=142> <color=#{color}>{username}</color> joined!");
    }


    [ClientRpc]
    private void RpcHandleMessage(string message)
    {
        OnMessage?.Invoke($"\n{message}");
    }

    [ClientRpc]
    public void RpcAddPlayer()
    {
        List<Player> players = GameObject.Find("NewNetworkManager").GetComponent<NewNetworkManager>().players;
        PlayerMenu.instance.LoadPlayers(players);
    }

    public void AttemptEnterBuilding(int buildingId)
    {
        if (!isLocalPlayer || playerEntity == null || isGuest) return;

        var request = new GetObjectsRequest { Entity = playerEntity };

        PlayFabDataAPI.GetObjects(request, (getResult) =>
        {
            var courses = getResult.Objects;
            foreach (KeyValuePair<string, PlayFab.DataModels.ObjectResult> entry in courses)
            {
                Meeting obj = JsonUtility.FromJson<Meeting>(entry.Value.DataObject.ToString());

                if (obj.building_id == buildingId)
                {
                    Application.ExternalEval("window.open(\"http://umass-amherst.zoom.us/j/" + obj.meeting_id + "\")");

                    if (obj.password != "")
                    {
                        // show password copied to clipboard dialog
                        GUIUtility.systemCopyBuffer = obj.password;
                    }
                    
                    break;
                }
            }
        }, GetObjectsFailure
        );
    }

    public void OpenLink(string link)
    {
        if (!isLocalPlayer)
        {
            return;
        }

        Application.ExternalEval("window.open(\" " + link + "\")");
    }

    private void GetObjectsFailure(PlayFabError error)
    {
        // show failure dialog
        Debug.Log("FAILED TO JOIN MEETING");
        Debug.LogError(error.GenerateErrorReport());
    }
}
