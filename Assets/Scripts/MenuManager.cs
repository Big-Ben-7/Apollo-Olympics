using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using System.Security.Cryptography;
using System;
using Unity.Mathematics;
using Palmmedia.ReportGenerator.Core;

public class MenuManager : MonoBehaviour
{
    [Header("Windows")]
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject gameSettings;
    [SerializeField] GameObject customization;
    [SerializeField] GameObject loading;
    [SerializeField] GameObject settings;
    [SerializeField] GameObject vaSettings;
    [SerializeField] GameObject controlSettings;

    [Header("Main Menu")]
    [SerializeField] TMP_Text racesText;
    [SerializeField] TMP_Text collectsText;
    [SerializeField] TMP_Text begTime;
    [SerializeField] TMP_Text intTime;
    [SerializeField] TMP_Text advTime;
    [SerializeField] TMP_Text expTime;
    [SerializeField] bool resetRecords;

    [Header("Game Settings")]
    [SerializeField] Slider difficultySlider;
    [SerializeField] Slider playersSlider;

    [Header("Customization")]
    [SerializeField] GameObject[] ships;
    int shipCount;
    int activeShip;
    int activePlayer;
    [SerializeField] TMP_Text playerText;
    [SerializeField] TMP_InputField nameInput;
    GameObject nameObject;
    TMP_Text[] selectTexts;
    Transform[] models;
    GameObject[] abilities;
    [SerializeField] AudioClip[] abilitySounds;
    [SerializeField] AudioClip debuffSound;
    Transform activeModel;
    float rotationSpeed;
    Coroutine resetting;
    public static string[] playerNames = new string[4];

    [Header("Settings")]
    [SerializeField] Slider fovSlider;
    [SerializeField] Slider depthSlider;
    [SerializeField] Toggle cursorToggle;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider noiseSlider;
    [SerializeField] Slider effectsSlider;

    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip majorClick;
    [SerializeField] AudioClip minorClick;
    
    public static bool loaded;
    public static int playerNumber;
    public static int difficulty;
    public static int[] playerShips = new int[4];
    public static int[] times = new int[4];
    public static int collects;
    public static int races;
    public static float fov;
    public static float depth;
    public static bool cursorHide;
    public static float music;
    public static float noise;
    public static float effects;

    void Awake()
    {
        if(resetRecords)
        {
            resetRecords = false;
            for(int i = 0; i < 4; i++)
            {
                PlayerPrefs.SetInt("Time" + i, 0);
                PlayerPrefs.SetString("Description" + i, "");
                PlayerPrefs.SetInt("Ship" + i, 0);
                PlayerPrefs.SetString("Name" + i, "Player " + (i + 1));
            }
            PlayerPrefs.SetInt("PlayerNumber", 0);
            PlayerPrefs.SetInt("Difficulty", 0);
            PlayerPrefs.SetInt("Races", 0);
            PlayerPrefs.SetInt("Collects", 0);
            PlayerPrefs.SetFloat("Fov", 35);
            PlayerPrefs.SetFloat("Depth", 1600);
            PlayerPrefs.SetFloat("Music", 0.04f);
            PlayerPrefs.SetInt("CursorHide", 1);
            PlayerPrefs.SetFloat("Noise", 0.1f);
            PlayerPrefs.SetFloat("Effects", 0.2f);
        }
        for(int i = 0; i < 4; i++)
        {
            times[i] = PlayerPrefs.GetInt("Time" + i);
            playerShips[i] = PlayerPrefs.GetInt("Ship" + i);
            playerNames[i] = PlayerPrefs.GetString("Name" + i, "Player " + (i + 1));
        }

        races = PlayerPrefs.GetInt("Races");
        racesText.SetText("Races: " + races);
        collects = PlayerPrefs.GetInt("Collects");
        collectsText.SetText("Space Junk Collected: " + collects);
        TimeSpan begSpan = TimeSpan.FromMilliseconds(times[0]);
        begTime.SetText("Beginner: " + begSpan.Minutes + ":" + begSpan.Seconds.ToString("00") + "." + begSpan.Milliseconds.ToString("000") + PlayerPrefs.GetString("Description0"));
        TimeSpan intSpan = TimeSpan.FromMilliseconds(times[1]);
        intTime.SetText("Intermediate: " + intSpan.Minutes + ":" + intSpan.Seconds.ToString("00") + "." + intSpan.Milliseconds.ToString("000") + PlayerPrefs.GetString("Description1"));
        TimeSpan advSpan = TimeSpan.FromMilliseconds(times[2]);
        advTime.SetText("Advanced: " + advSpan.Minutes + ":" + advSpan.Seconds.ToString("00") + "." + advSpan.Milliseconds.ToString("000") + PlayerPrefs.GetString("Description2"));
        TimeSpan expSpan = TimeSpan.FromMilliseconds(times[3]);
        expTime.SetText("Expert: " + expSpan.Minutes + ":" + expSpan.Seconds.ToString("00") + "." + expSpan.Milliseconds.ToString("000") + PlayerPrefs.GetString("Description3"));
        mainMenu.SetActive(true);
        gameSettings.SetActive(false);
        customization.SetActive(false);
        settings.SetActive(false);
        vaSettings.SetActive(true);
        controlSettings.SetActive(false);

        shipCount = ships.Length;
        models = new Transform[shipCount];
        abilities = new GameObject[shipCount];
        selectTexts = new TMP_Text[shipCount];
        for(int i = 0; i < shipCount; i++)
        {
            GameObject ship = ships[i];
            Transform model = ship.transform.GetChild(0);
            models[i] = model;
            abilities[i] = model.GetChild(2).gameObject;
            selectTexts[i] = ship.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<TMP_Text>();
        }

        playerNumber = PlayerPrefs.GetInt("PlayerNumber");
        difficulty = PlayerPrefs.GetInt("Difficulty");
        playersSlider.value = playerNumber;
        difficultySlider.value = difficulty;
        nameObject = nameInput.gameObject;
        nameInput.text = "";

        fov = PlayerPrefs.GetFloat("Fov", 35);
        fovSlider.value = fov;
        depth = PlayerPrefs.GetFloat("Depth", 1600);
        depthSlider.value = depth;
        music = PlayerPrefs.GetFloat("Music", 0.04f);
        musicSlider.value = music;
        if(PlayerPrefs.GetInt("CursorHide", 1) == 0)
        {
            cursorHide = false;
            cursorToggle.isOn = false;
        }
        else
        {
            cursorHide = true;
            cursorToggle.isOn = true;
        }
        noise = PlayerPrefs.GetFloat("Noise", 0.1f);
        noiseSlider.value = noise;
        effects = PlayerPrefs.GetFloat("Effects", 0.2f);
        effectsSlider.value = effects;
        loaded = true;
    }

    void Update()
    {
        if(customization.activeInHierarchy)
        {
            activeModel.Rotate(0, rotationSpeed, 0);
        }
    }

    public void ShowMain()
    {
        mainMenu.SetActive(true);
        gameSettings.SetActive(false);
        customization.SetActive(false);
        settings.SetActive(false);
        loading.SetActive(false);
        audioSource.PlayOneShot(majorClick);
    }

    public void GameSettings()
    {
        gameSettings.SetActive(true);
        mainMenu.SetActive(false);
        audioSource.PlayOneShot(majorClick);
    }

    public void Customization()
    {
        for(int i = 0; i < shipCount; i++)
        {
            ships[i].SetActive(false);
            abilities[i].SetActive(false);
            selectTexts[i].SetText("Select");
        }
        playerText.SetText(playerNames[0]);
        activePlayer = 0;
        activeShip = playerShips[0];
        ships[activeShip].SetActive(true);
        selectTexts[activeShip].SetText("Selected");
        activeModel = models[activeShip];
        activeModel.rotation = Quaternion.Euler(30, 180, 0);
        if(activeShip > 3 && rotationSpeed != 0.05)
        {
            rotationSpeed = 0.05f;
        }
        else if(rotationSpeed != 0.1)
        {
            rotationSpeed = 0.1f;
        }
        nameObject.SetActive(false);
        playerText.enabled = true;
        customization.SetActive(true);
        mainMenu.SetActive(false);
        audioSource.PlayOneShot(majorClick);
    }

    public void Settings()
    {
        settings.SetActive(true);
        mainMenu.SetActive(false);
        audioSource.PlayOneShot(majorClick);
    }

    public void Play()
    {
        SavePrefs();
        gameSettings.SetActive(false);
        loading.SetActive(true);
        audioSource.PlayOneShot(majorClick);
        StartCoroutine(LoadGame());
    }

    public void NextShip(int increment)
    {
        activeShip += increment;
        if(activeShip == shipCount)
        {
            activeShip = 0;
        }
        else if(activeShip == -1)
        {
            activeShip = shipCount - 1;
        }
        ChangeShip();
        audioSource.PlayOneShot(minorClick);
    }

    public void NextPlayer(int increment)
    {
        activePlayer += increment;
        switch(activePlayer)
        {
            case 4:
                activePlayer = 0;
                break;
            case -1:
                activePlayer = 3;
                break;
        }
        playerText.SetText(playerNames[activePlayer]);
        activeShip = playerShips[activePlayer];
        ChangeShip();
        for(int i = 0; i < shipCount; i++)
        {
            selectTexts[i].SetText("Select");
        }
        selectTexts[activeShip].SetText("Selected");
        audioSource.PlayOneShot(minorClick);
    }

    void ChangeShip()
    {
        if(resetting != null)
        {
            StopCoroutine(resetting);
        }
        for(int i = 0; i < shipCount; i++)
        {
            ships[i].SetActive(false);
            abilities[i].SetActive(false);
        }
        ships[activeShip].SetActive(true);
        Quaternion newRot = activeModel.rotation;
        activeModel = models[activeShip];
        activeModel.rotation = newRot;
        if(activeShip > 3 && rotationSpeed != 0.05)
        {
            rotationSpeed = 0.05f;
        }
        else if(rotationSpeed != 0.1)
        {
            rotationSpeed = 0.1f;
        }
    }

    void SelectShip()
    {
        playerShips[activePlayer] = activeShip;
        for(int i = 0; i < shipCount; i++)
        {
            selectTexts[i].SetText("Select");
        }
        selectTexts[activeShip].SetText("Selected");
        audioSource.PlayOneShot(majorClick);
    }

    public void EditName()
    {
        playerText.enabled = false;
        nameInput.text = "";
        nameObject.SetActive(true);
        audioSource.PlayOneShot(majorClick);
    }

    public void ChangeName()
    {
        nameObject.SetActive(false);
        string newName = nameInput.text;
        bool valid = true;
        for(int i = 0; i < 4; i++)
        {
            if(playerNames[i] == newName)
            {
                valid = false;
                break;
            }
        }
        if(newName != "" && valid)
        {
            playerText.SetText(newName);
            playerNames[activePlayer] = newName;
            PlayerPrefs.SetString("Name" + activePlayer, newName);
        }
        playerText.enabled = true;
    }

    public void Ability()
    {
        GameObject activeAbility = abilities[activeShip];
        if(!activeAbility.activeInHierarchy || activeShip == 7)
        {
            switch(activeShip)
            {
                case 0:
                    resetting = StartCoroutine(ResetEffect(activeAbility, 2));
                    break;
                case 1:
                    resetting = StartCoroutine(ResetEffect(activeAbility, 5, true));
                    break;
                case 7:
                    activeAbility.SetActive(false);
                    resetting = StartCoroutine(ResetEffect(activeAbility, 2));
                    break;
            }
            activeAbility.SetActive(true);
            audioSource.PlayOneShot(abilitySounds[activeShip]);
        }
        else
        {
            if(activeShip <= 1 && resetting != null)
            {
                StopCoroutine(resetting);
            }
            activeAbility.SetActive(false);
            audioSource.PlayOneShot(debuffSound);
        }
    }

    public void NextSetting()
    {
        vaSettings.SetActive(!vaSettings.activeInHierarchy);

    }

    public void ChangeText(Slider slider, TMP_Text text)
    {
        text.SetText("" + slider.value);
    }

    public void ChangeAudio(Slider slider, AudioSource source)
    {
        source.volume = slider.value;
    }

    public void Quit()
    {
        SavePrefs();
        audioSource.PlayOneShot(majorClick);
        StartCoroutine(QuitApplication());
    }

    public void MinorClick()
    {
        audioSource.PlayOneShot(minorClick);
    }

    public void MajorClick()
    {
        audioSource.PlayOneShot(majorClick);
    }

    void SavePrefs()
    {
        playerNumber = (int) playersSlider.value;
        difficulty = (int) difficultySlider.value;
        PlayerPrefs.SetInt("PlayerNumber", playerNumber);
        PlayerPrefs.SetInt("Difficulty", difficulty);
        for(int i = 0; i < 4; i++)
        {
            PlayerPrefs.SetInt("Ship" + i, playerShips[i]);
        }
        fov = fovSlider.value;
        PlayerPrefs.SetFloat("Fov", fov);
        depth = depthSlider.value;
        PlayerPrefs.SetFloat("Depth", depth);
        music = musicSlider.value;
        PlayerPrefs.SetFloat("Music", music);
        cursorHide = cursorToggle.isOn;
        if(cursorHide)
        {
            PlayerPrefs.SetInt("CursorHide", 1);
        }
        else
        {
            PlayerPrefs.SetInt("CursorHide", 0);
        }
        noise = noiseSlider.value;
        PlayerPrefs.SetFloat("Noise", noise);
        effects = effectsSlider.value;
        PlayerPrefs.SetFloat("Effects", effects);
    }

    IEnumerator ResetEffect(GameObject effect, int waitTime, bool sound = false)
    {
        yield return new WaitForSeconds(waitTime);
        effect.SetActive(false);
        if(sound)
        {
            audioSource.PlayOneShot(debuffSound);
        }
    }

    IEnumerator LoadGame()
    {
        AsyncOperation load = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        while(!load.isDone)
        {
            yield return null;
        }
        SceneManager.UnloadSceneAsync(0);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Play"));
    }

    IEnumerator QuitApplication()
    {
        yield return new WaitForSeconds(majorClick.length);
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Application.Quit();
    }
}
