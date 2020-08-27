using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameStatus
{
    Initializing, PlayingAnimation, WaitingForInput
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static GameStatus GameStatus = GameStatus.Initializing;
    public static bool GravityInverted = false;
    [SerializeField] private GameObject endMenu;


    private void Awake()
    {
        Instance = this;
    }

    public void EndGame()
    {
        endMenu.SetActive(true);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Exit()
    {
        Application.Quit(0);
    }
}
