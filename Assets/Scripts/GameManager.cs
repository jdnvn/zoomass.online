using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Mirror;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    public List<GameState> gameStates = new List<GameState>();
    public GameState ActiveState = null;
    public string PreviousState = null;

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

        GameObject.DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        RegisterGameState<LoginState>();
        RegisterGameState<WorldState>();

        SwitchGameState<LoginState>();
    }

    public void RegisterGameState<T>() where T : GameState
    {
        gameStates.Add(Activator.CreateInstance<T>());
    }

    public void SwitchGameState<T>() where T : GameState
    {
        if (ActiveState != null)
        {
            PreviousState = ActiveState.SceneName;
        }

        foreach (GameState state in gameStates)
        {
            if (state is T)
            {
                ActiveState = state;
                ActiveState.OnEnabled();
            }
        }
    }
}

public abstract class GameState
{
    public string SceneName;

    public virtual void OnEnabled()
    {
        SceneManager.LoadScene(SceneName, LoadSceneMode.Single);
    }

    public virtual void OnDisabled()
    {
        SceneManager.UnloadSceneAsync(SceneName);
    }
}

public class LoginState : GameState
{
    public LoginState()
    {
        SceneName = "Login";
    }
}

public class WorldState : GameState
{
    public WorldState()
    {
        SceneName = "Main";
    }
}