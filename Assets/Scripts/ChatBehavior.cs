using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.UI;

public class ChatBehavior : NetworkBehaviour
{
    [SerializeField] private string prefix = string.Empty;
    [SerializeField] private ConsoleCommand[] commands = new ConsoleCommand[0];

    [SerializeField] public TMP_Text chatBox = null;
    [SerializeField] private TMP_InputField chatInput = null;

    public Text chatHistory;
    public Scrollbar scrollbar;

    private Player player;

    private static DeveloperConsole developerConsole;

    private DeveloperConsole DeveloperConsole
    {
        get
        {
            if (developerConsole != null) { return developerConsole;  }
            return developerConsole = new DeveloperConsole(prefix, commands);
        }
    }

    public void Awake()
    {
        Player.OnMessage += HandleNewMessage;
    }


    private void OnDestroy()
    {
        Player.OnMessage -= HandleNewMessage;
    }

    private void HandleNewMessage(string message)
    {
        AppendMessage(message);
    }

    [Client]
    public void Send(CallbackContext context)
    {
        if (!context.action.triggered) { return; }
        string message = chatInput.text;

        chatInput.text = string.Empty;

        GameObject.Find("Local").GetComponent<PlayerMovementController>().EnableControls();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        chatInput.gameObject.SetActive(false);

        if (string.IsNullOrWhiteSpace(message)) { return; }

        if (message.StartsWith("/"))
        {
            ProcessCommand(message);
            return;
        }

        Player player = NetworkClient.connection.identity.GetComponent<Player>();

        player.CmdSendMessage(message);
    }

    public void UpdateField()
    {
        string text = chatInput.text;
        text = text.Replace("`", "");
        chatInput.text = text;
    }

    public void Toggle(CallbackContext context)
    {
        if (!context.action.triggered || GetComponent<UIManager>().meetingMenu.activeInHierarchy) { return; }

        if (chatInput.gameObject.activeSelf)
        {
            GameObject.Find("Local").GetComponent<PlayerMovementController>().EnableControls();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            chatInput.gameObject.SetActive(false);
        }
        else
        {
            GameObject.Find("Local").GetComponent<PlayerMovementController>().DisableControls();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            chatInput.gameObject.SetActive(true);
            chatInput.ActivateInputField();
        }
    }

    [Client]
    public void ProcessCommand(string inputValue)
    {
        DeveloperConsole.ProcessCommand(inputValue);

        chatInput.text = string.Empty;
        chatInput.ActivateInputField();
    }

    internal void AppendMessage(string message)
    {
        StartCoroutine(AppendAndScroll(message));
    }

    IEnumerator AppendAndScroll(string message)
    {
        chatBox.text += message;

        // it takes 2 frames for the UI to update ?!?!
        yield return null;
        yield return null;

        // slam the scrollbar down
        scrollbar.value = 0;
    }
}
