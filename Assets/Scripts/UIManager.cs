using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.DataModels;
using PlayFab.ClientModels;
using Mirror;
using Cinemachine;

public class UIManager : MonoBehaviour
{
    public GameObject homeMenu;
    public GameObject meetingMenu;
    public GameObject bugMenu;
    public GameObject movementMenu;
    public GameObject aboutMenu;
    public GameObject playerMenu;
    public GameObject meetingsMenuButton;

    public GameObject chatInputField;

    public CinemachineFreeLook camera;

    private void Start()
    {
        if (LoginManager.instance.isGuest) meetingsMenuButton.SetActive(false);
    }


    public void ToggleHomeMenu()
    {
        if (!homeMenu.activeInHierarchy)
        {
            if (meetingMenu.activeInHierarchy) ToggleMeetingMenu();
            if (bugMenu.activeInHierarchy) ToggleBugMenu();
            if (movementMenu.activeInHierarchy) ToggleMovementMenu();
            if (aboutMenu.activeInHierarchy) ToggleAboutMenu();
            if (playerMenu.activeInHierarchy) TogglePlayerMenu();

            GameObject.Find("Local").GetComponentInChildren<PlayerMovementController>().DisableControls();

            camera.m_YAxis.m_InputAxisName = "";
            camera.m_XAxis.m_InputAxisName = "";

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            OnCloseUI();
        }

        homeMenu.SetActive(!homeMenu.activeInHierarchy);
    }

    public void ToggleMeetingMenu()
    {

        homeMenu.SetActive(false);

        if (meetingMenu.activeInHierarchy)
        {
            FindObjectOfType<MeetingMenu>().feedbackText.text = "";
            OnCloseUI();
        } else
        {
            GameObject.Find("Local").GetComponent<PlayerMovementController>().DisableControls();
        }

        meetingMenu.SetActive(!meetingMenu.activeInHierarchy);
    }

    public void ToggleBugMenu()
    {
        homeMenu.SetActive(false);

        if (bugMenu.activeInHierarchy)
        {
            OnCloseUI();
        }

        bugMenu.SetActive(!bugMenu.activeInHierarchy);
    }

    public void ToggleAboutMenu()
    {
        homeMenu.SetActive(false);

        if (aboutMenu.activeInHierarchy)
        {
            OnCloseUI();
        }

        aboutMenu.SetActive(!aboutMenu.activeInHierarchy);
    }

    public void TogglePlayerMenu()
    {
        if (chatInputField.activeInHierarchy || homeMenu.activeInHierarchy || meetingMenu.activeInHierarchy) return;

        if (!playerMenu.activeInHierarchy)
        {
            camera.m_YAxis.m_InputAxisName = "";
            camera.m_XAxis.m_InputAxisName = "";

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            OnCloseUI();
        }

        playerMenu.SetActive(!playerMenu.activeInHierarchy);
    }

    public void ToggleMovementMenu()
    {
        if (chatInputField.activeInHierarchy || homeMenu.activeInHierarchy || meetingMenu.activeInHierarchy) return;

        if (!movementMenu.activeInHierarchy)
        {
            camera.m_YAxis.m_InputAxisName = "";
            camera.m_XAxis.m_InputAxisName = "";

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            OnCloseUI();
        }

        movementMenu.SetActive(!movementMenu.activeInHierarchy);
    }

    private void OnCloseUI()
    {
        GameObject.Find("Local").GetComponentInChildren<PlayerMovementController>().EnableControls();

        camera.m_YAxis.m_InputAxisName = "Mouse Y";
        camera.m_XAxis.m_InputAxisName = "Mouse X";

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Logout()
    {
        NetworkClient.connection.identity.GetComponent<Player>().CmdLogOutPlayer();

        // CHANGE ON BUILD
        NetworkManager.singleton.StopClient();
        //NetworkManager.singleton.StopHost();
    }
}
