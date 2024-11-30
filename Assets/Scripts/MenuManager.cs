using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;

public class MenuManager : MonoBehaviour
{
    [Header("Windows")]
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject gameSettings;
    [SerializeField] GameObject customization;
    [SerializeField] GameObject loading;

    [Header("Game Settings")]
    [SerializeField] Slider difficultySlider;
    [SerializeField] Slider playersSlider;

    [Header("Customization")]
    [SerializeField] GameObject[] ships;
    int shipCount;
    int activeShip;
    int activePlayer;
    [SerializeField] TMP_Text playerText;
    TMP_Text[] selectTexts;
    Transform[] models;
    GameObject[] abilities;
    [SerializeField] AudioClip[] abilitySounds;
    [SerializeField] AudioClip debuffSound;
    Transform activeModel;
    float rotationSpeed;
    Coroutine resetting;

    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip majorClick;
    [SerializeField] AudioClip minorClick;
    
    public static int playerNumber;
    public static int difficulty;
    public static int[] playerShips;

    void Awake()
    {
        mainMenu.SetActive(true);
        gameSettings.SetActive(false);
        customization.SetActive(false);
        shipCount = ships.Length;
        models = new Transform[shipCount];
        abilities = new GameObject[shipCount];
        selectTexts = new TMP_Text[shipCount];
        for(int i = 0; i < shipCount; i++)
        {
            GameObject ship = ships[i];
            ship.SetActive(false);
            Transform model = ship.transform.GetChild(0);
            models[i] = model;
            abilities[i] = model.GetChild(2).gameObject;
            abilities[i].SetActive(false);
            TMP_Text selectText = ship.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<TMP_Text>();
            selectText.SetText("Select");
            selectTexts[i] = selectText;
        }
        ships[0].SetActive(true);
        selectTexts[0].SetText("Selected");
        activeShip = activePlayer = 0;
        activeModel = models[0];
        activeModel.rotation = Quaternion.Euler(30, 180, 0);
        rotationSpeed = 0.1f;
        playerNumber = 1;
        difficulty = 2;
        playerShips = new int[] {0, 0, 0, 0};
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
        gameSettings.SetActive(false);
        loading.SetActive(true);
        StartCoroutine(LoadGame());
    }

    public void NextShip(int increment)
    {
        audioSource.PlayOneShot(minorClick);
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
    }

    public void NextPlayer(int increment)
    {
        audioSource.PlayOneShot(minorClick);
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
        playerText.SetText("Player " + (activePlayer + 1));
        activeShip = playerShips[activePlayer];
        ChangeShip();
        for(int i = 0; i < shipCount; i++)
        {
            selectTexts[i].SetText("Select");
        }
        selectTexts[activeShip].SetText("Selected");
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
        audioSource.PlayOneShot(majorClick);
        playerShips[activePlayer] = activeShip;
        for(int i = 0; i < shipCount; i++)
        {
            selectTexts[i].SetText("Select");
        }
        selectTexts[activeShip].SetText("Selected");
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

    public void MinorClick()
    {
        audioSource.PlayOneShot(minorClick);
    }

    public void MajorClick()
    {
        audioSource.PlayOneShot(majorClick);
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
}
