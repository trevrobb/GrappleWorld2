using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndOfLevelMenu : MonoBehaviour
{
    [SerializeField] GameObject endMenu;
    [SerializeField] TextMeshProUGUI _text;
    Player player;
    void Start()
    {
        endMenu.SetActive(false);
    }

    private void Update()
    {
        if (Player.instance.gameEnd())
        {
            endMenu.SetActive(true);
            int minutes = Mathf.FloorToInt(Player.instance.getTime() / 60f);
            int seconds = Mathf.FloorToInt(Player.instance.getTime() % 60f);
            int milliseconds = Mathf.FloorToInt((Player.instance.getTime() * 100f) % 100f);
            _text.text = "Completion Time: " + minutes.ToString("00") + ":" + seconds.ToString("00") + ":" + milliseconds.ToString("00");
        }
    }

    public void Menu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
