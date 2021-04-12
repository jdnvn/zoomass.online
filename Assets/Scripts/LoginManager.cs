using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.DataModels;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System;
using TMPro;

public class LoginManager : MonoBehaviour
{
    [Scene] [SerializeField] public string mainScene = string.Empty;

    public static LoginManager instance;

    public GameObject startMenu;
    public GameObject loadingHUD;

    public GameObject loginButton;
    public GameObject registerButton;
    public GameObject createAccountButton;
    public GameObject switchLoginButton;
    public Toggle rememberMeToggle;

    public GameObject createAccountText;
    public GameObject errorText;

    public InputField emailField;
    public InputField usernameField;
    public InputField passwordField;

    public bool isGuest = false;

    public PlayFab.DataModels.EntityKey playerEntity;
    public string playFabId;

    private void Awake()
    {
        PlayFabSettings.TitleId = "13A16";

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }

        if (String.IsNullOrEmpty(GameManager.instance.PreviousState) && PlayerPrefs.HasKey("email"))
        {
            loadingHUD.SetActive(true);

            string emailPref = PlayerPrefs.GetString("email");
            string passwordPref = PlayerPrefs.GetString("password");

            var loginRequest = new LoginWithEmailAddressRequest { Email = emailPref, Password = passwordPref };
            PlayFabClientAPI.LoginWithEmailAddress(loginRequest, OnLoginSuccess, OnLoginFailure);
        }
        else
        {
            emailField.ActivateInputField();
        }
    }

    private void Start()
    {
        isGuest = false;
        if (GameManager.instance.PreviousState == "Main") loadingHUD.SetActive(false);
        emailField.ActivateInputField();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            GameObject c = EventSystem.current.currentSelectedGameObject;
            if (c == null) { return; }

            Selectable s = c.GetComponent<Selectable>();
            if (s == null) { return; }

            Selectable jump = Input.GetKey(KeyCode.LeftShift)
                ? s.FindSelectableOnUp() : s.FindSelectableOnDown();
            if (jump != null) { jump.Select(); }
        }
    }

    public void ConnectToServer()
    {
        loadingHUD.SetActive(true);
        errorText.GetComponent<UnityEngine.UI.Text>().text = "";

        var loginRequest = new LoginWithEmailAddressRequest { Email = emailField.text, Password = passwordField.text };
        PlayFabClientAPI.LoginWithEmailAddress(loginRequest, OnLoginSuccess, OnLoginFailure);
    }

    public void Register()
    {
        loadingHUD.SetActive(true);
        errorText.GetComponent<UnityEngine.UI.Text>().text = "";

        var registerRequest = new RegisterPlayFabUserRequest { Email = emailField.text, Password = passwordField.text, Username = usernameField.text };
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegisterSuccess, OnRegisterFailure);
    }

    public void LoginAsGuest()
    {
        isGuest = true;

        SendIntoGame();
    }

    public void CreateAccountSwitch()
    {
        loginButton.SetActive(false);
        createAccountButton.SetActive(false);
        switchLoginButton.SetActive(true);
        rememberMeToggle.gameObject.SetActive(false);
        registerButton.SetActive(true);

        var emailFieldObject = emailField.GetComponent<RectTransform>();
        var pos = emailFieldObject.anchoredPosition;
        emailFieldObject.anchoredPosition = new Vector3(pos.x, 76f);

        usernameField.gameObject.SetActive(true);
        emailField.ActivateInputField();
    }

    public void LoginSwitch()
    {
        registerButton.SetActive(false);
        switchLoginButton.SetActive(false);
        createAccountButton.SetActive(true);
        rememberMeToggle.gameObject.SetActive(true);
        loginButton.SetActive(true);

        var emailFieldObject = emailField.GetComponent<RectTransform>();
        var pos = emailFieldObject.anchoredPosition;
        emailFieldObject.anchoredPosition = new Vector3(pos.x, -15.8f);

        usernameField.gameObject.SetActive(false);
        emailField.ActivateInputField();
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("You are now logged into PlayFab");

        if (rememberMeToggle.isOn && emailField.text.Length > 0 && passwordField.text.Length > 0)
        {
            PlayerPrefs.SetString("email", emailField.text);
            PlayerPrefs.SetString("password", passwordField.text);
        }

        playerEntity.Id = result.EntityToken.Entity.Id;
        playerEntity.Type = result.EntityToken.Entity.Type;

        loadingHUD.SetActive(true);
        SendIntoGame();
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("You are now registered with playfab. Logging you in...");

        playerEntity.Id = result.EntityToken.Entity.Id;
        playerEntity.Type = result.EntityToken.Entity.Type;

        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>() {
                {"rights", "1"},
            }
        },
        res => Debug.Log("Successfully added user rights"),
        error => {
            Debug.Log(error.GenerateErrorReport());
        });

        loadingHUD.SetActive(true);
        SendIntoGame();
    }

    private void SendIntoGame()
    {
        loadingHUD.SetActive(true);
        startMenu.SetActive(false);
        emailField.interactable = false;
        usernameField.interactable = false;
        passwordField.interactable = false;


        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        GameManager.instance.SwitchGameState<WorldState>();
    }

    private void OnLoginFailure(PlayFabError error)
    {
        loadingHUD.SetActive(false);
        ShowErrorText(error);
    }

    private void OnRegisterFailure(PlayFabError error)
    {
        loadingHUD.SetActive(false);
        ShowErrorText(error);
    }

    private void ShowErrorText(PlayFabError error)
    {
        Debug.Log(error.Error);

        // Recognize and handle the error
        switch (error.Error)
        {
            case PlayFabErrorCode.InvalidTitleId:
                Debug.LogError(error.GenerateErrorReport());
                break;
            case PlayFabErrorCode.AccountNotFound:
                errorText.GetComponent<UnityEngine.UI.Text>().text = "<color=red>*</color> There wasn't an account with those credentials, try creating a new account";
                break;
            case PlayFabErrorCode.InvalidEmailOrPassword:
                errorText.GetComponent<UnityEngine.UI.Text>().text = "<color=red>*</color> Invalid Email or Password";
                break;
            case PlayFabErrorCode.InvalidUsernameOrPassword:
                errorText.GetComponent<UnityEngine.UI.Text>().text = "<color=red>*</color> Invalid Username or Password";
                break;
            case PlayFabErrorCode.RequestViewConstraintParamsNotAllowed:
                Debug.LogError(error.GenerateErrorReport());
                break;
            case PlayFabErrorCode.UsernameNotAvailable:
                errorText.GetComponent<UnityEngine.UI.Text>().text = "<color=red>*</color> That username isn't available";
                break;
            case PlayFabErrorCode.InvalidPassword:
                errorText.GetComponent<UnityEngine.UI.Text>().text = "<color=red>*</color> Password must be between 6 and 100 characters";
                break;
            case PlayFabErrorCode.EmailAddressNotAvailable:
                errorText.GetComponent<UnityEngine.UI.Text>().text = "<color=red>*</color> That email isn't available";
                break;
            case PlayFabErrorCode.InvalidParams:
                errorText.GetComponent<UnityEngine.UI.Text>().text = "<color=red>*</color> Make sure all the fields are filled out correctly";
                break;
            default:
                errorText.GetComponent<UnityEngine.UI.Text>().text = "<color=red>*</color> " + error.GenerateErrorReport();
                break;
        }
    }
}
