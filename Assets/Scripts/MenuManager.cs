using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;
using TMPro;
using Unity.VisualScripting;

public class MenuManager : MonoBehaviour
{
    [Header("Windows")]
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject gameSettings;
    [SerializeField] GameObject customization;
    [SerializeField] GameObject loading;

    [Header("Graphics")]
    [SerializeField] Slider difficultySlider;
    [SerializeField] Slider playersSlider;

    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip majorClick;
    [SerializeField] AudioClip minorClick;
    [SerializeField] AudioClip backClip;
    
    public static int playerNumber;
    public static int difficulty;
    public static int[] playerModels;

    void Awake()
    {
        mainMenu.SetActive(true);
        gameSettings.SetActive(false);
        customization.SetActive(false);
    }

    public void ShowMain()
    {
        audioSource.PlayOneShot(majorClick);
        mainMenu.SetActive(true);
        gameSettings.SetActive(false);
        customization.SetActive(false);
        loading.SetActive(false);
    }

    public void GameSettings()
    {
        audioSource.PlayOneShot(majorClick);
        gameSettings.SetActive(true);
        mainMenu.SetActive(false);
        customization.SetActive(false);
    }

    public void Customization()
    {
        audioSource.PlayOneShot(majorClick);
        customization.SetActive(true);
        mainMenu.SetActive(false);
        gameSettings.SetActive(false);
    }

    public void Play()
    {
        audioSource.PlayOneShot(majorClick);
        playerNumber = (int) playersSlider.value + 1;
        difficulty = (int) difficultySlider.value + 1;
        //playerModels = new int[] {ship1Dropdown.value, ship2Dropdown.value, ship3Dropdown.value, ship4Dropdown.value};
        gameSettings.SetActive(false);
        loading.SetActive(true);
        StartCoroutine(LoadGame());
    }

    public void MinorClick()
    {
        audioSource.PlayOneShot(minorClick);
    }

    public void MajorClick()
    {
        audioSource.PlayOneShot(majorClick);
    }

    IEnumerator LoadGame()
    {
        AsyncOperation load = SceneManager.LoadSceneAsync(1);
        while(!load.isDone)
        {
            yield return null;
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Play"));
        SceneManager.UnloadSceneAsync(0);
    }
}
