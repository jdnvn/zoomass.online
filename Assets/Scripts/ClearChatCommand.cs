using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

[CreateAssetMenu(fileName = "New Clear Chat Command", menuName = "Commands/Clear Chat Command")]
public class ClearChatCommand : ConsoleCommand
{
    public override bool Process(string[] args)
    {
        GameObject.Find("UI").GetComponentInChildren<ChatBehavior>().chatBox.text = "";

        return true;
    }
}
