using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pausa : MonoBehaviour
{
    public GameObject pausa;
    public static bool pausado;

    void Start()
    {
        pausa.SetActive(false);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pausado)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
    public void PauseGame()
    {
        pausa.SetActive(true);
        Time.timeScale = 0f;
        pausado = true;
    }

    public void ResumeGame()
    {
        pausa.SetActive(false);
        Time.timeScale = 1f;
        pausado = false;
    }

    //public void BMainMenu()
    //{
    //    Time.timeScale = 1f;
    //}
}


