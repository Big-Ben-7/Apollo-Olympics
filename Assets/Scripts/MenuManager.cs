using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

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
    [SerializeField] TMP_Text creditsText;
    [SerializeField] TMP_Text collectsText;
    [SerializeField] TMP_Text begTime;
    [SerializeField] TMP_Text intTime;
    [SerializeField] TMP_Text advTime;
    [SerializeField] TMP_Text expTime;

    [Header("Game Settings")]
    [SerializeField] TMP_Text creditsText2;
    [SerializeField] TMP_Text raceText;
    [SerializeField] TMP_Text collectionText;
    [SerializeField] Slider difficultySlider;
    [SerializeField] TMP_Text[] difficulties;
    [SerializeField] Slider playersSlider;
    [SerializeField] TMP_Text[] playerNumbers;
    [SerializeField] TMP_Text playText;
    [SerializeField] Color normalColor;
    [SerializeField] Color selectedColor;
    [SerializeField] GameObject racePlay;
    [SerializeField] GameObject collectionPlay;

    [Header("Customization")]
    [SerializeField] GameObject[] ships;
    int shipCount;
    int activeShip;
    int activePlayer;
    [SerializeField] TMP_Text playerText;
    [SerializeField] TMP_InputField nameInput;
    [SerializeField] GameObject nameEdit;
    [SerializeField] GameObject nameEnter;
    GameObject nameObject;
    TMP_Text[] selectTexts;
    Transform[] models;
    GameObject[] abilities;
    [SerializeField] AudioClip[] abilitySounds;
    [SerializeField] AudioClip debuffSound;
    Transform activeModel;
    int rotationSpeed;
    Coroutine resetting;
    public static string[] playerNames = new string[4];

    [Header("Settings")]
    [SerializeField] Slider fovSlider;
    [SerializeField] Slider depthSlider;
    [SerializeField] Slider dampingSlider;
    [SerializeField] Toggle captionToggle;
    [SerializeField] Toggle cursorToggle;
    [SerializeField] Toggle arrowToggle;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider noiseSlider;
    [SerializeField] Slider thrusterSlider;
    [SerializeField] Slider effectsSlider;
    [SerializeField] TMP_Text fovText;

    [SerializeField] TMP_Text depthText;
    [SerializeField] TMP_Text dampingText;
    [SerializeField] TMP_Text musicText;
    [SerializeField] TMP_Text noiseText;
    [SerializeField] TMP_Text thrusterText;
    [SerializeField] TMP_Text effectsText;

    [SerializeField] GameObject[] controls;

    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioSource backgroundMusic;
    [SerializeField] AudioClip majorClick;
    [SerializeField] AudioClip minorClick;
    [SerializeField] AudioClip errorSound;
    [SerializeField] AudioClip buySound;
    
    public static bool loaded;
    public static int playerNumber;
    public static bool raceMode;
    public static int difficulty;
    public static int[] playerShips = new int[4];
    public static int[] times = new int[4];
    public static int collects;
    public static int races;
    public static int credits;
    public static float fov;
    public static float depth;
    public static float damping;
    public static bool showCaptions;
    public static bool cursorHide;
    public static bool showArrow;
    public static float music;
    public static float noise;
    public static float thruster;
    public static float effects;
    public static WaitForSeconds halfSecond = new WaitForSeconds(0.5f);
    public static WaitForSeconds oneSecond = new WaitForSeconds(1);
    public static WaitForSeconds twoSeconds = new WaitForSeconds(2);
    WaitForSeconds fiveSeconds = new WaitForSeconds(5);

    void Awake()
    {
        loaded = false;

        for(int i = 0; i < 4; i++)
        {
            times[i] = PlayerPrefs.GetInt("Time" + i.ToString());
            playerShips[i] = PlayerPrefs.GetInt("Ship" + i.ToString());
            playerNames[i] = PlayerPrefs.GetString("Name" + i.ToString(), "Player " + (i + 1).ToString());
        }

        //Get main menu variables
        races = PlayerPrefs.GetInt("Races");
        racesText.SetText("Races: " + races.ToString());
        credits = PlayerPrefs.GetInt("Credits", 500);
        string newCredits = "Credits: " + credits.ToString();
        creditsText.SetText(newCredits);
        creditsText2.SetText(newCredits);
        collects = PlayerPrefs.GetInt("Collects");
        collectsText.SetText("Space Junk Collected: " + collects.ToString());
        TimeSpan begSpan = TimeSpan.FromMilliseconds(times[0]);
        begTime.SetText("Beginner: " + begSpan.Minutes.ToString() + ":" + begSpan.Seconds.ToString("00") + "." + begSpan.Milliseconds.ToString("000") + PlayerPrefs.GetString("Description0"));
        TimeSpan intSpan = TimeSpan.FromMilliseconds(times[1]);
        intTime.SetText("Intermediate: " + intSpan.Minutes.ToString() + ":" + intSpan.Seconds.ToString("00") + "." + intSpan.Milliseconds.ToString("000") + PlayerPrefs.GetString("Description1"));
        TimeSpan advSpan = TimeSpan.FromMilliseconds(times[2]);
        advTime.SetText("Advanced: " + advSpan.Minutes.ToString() + ":" + advSpan.Seconds.ToString("00") + "." + advSpan.Milliseconds.ToString("000") + PlayerPrefs.GetString("Description2"));
        TimeSpan expSpan = TimeSpan.FromMilliseconds(times[3]);
        expTime.SetText("Expert: " + expSpan.Minutes.ToString() + ":" + expSpan.Seconds.ToString("00") + "." + expSpan.Milliseconds.ToString("000") + PlayerPrefs.GetString("Description3"));
        gameSettings.SetActive(false);
        customization.SetActive(false);
        settings.SetActive(false);
        vaSettings.SetActive(false);
        mainMenu.SetActive(true);
        controlSettings.SetActive(true);

        //Get customization variables
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

        //Get game settings variables
        if(PlayerPrefs.GetInt("Mode") == 0)
        {
            raceMode = true;
            raceText.fontStyle = FontStyles.UpperCase | FontStyles.Underline;
            raceText.color = selectedColor;
            collectionText.fontStyle = FontStyles.Normal;
            collectionText.color = normalColor;
            racePlay.SetActive(true);
            collectionPlay.SetActive(false);
            playText.SetText("Play  -  100 credits");
        }
        else
        {
            raceMode = false;
            raceText.fontStyle = FontStyles.Normal;
            raceText.color = normalColor;
            collectionText.fontStyle = FontStyles.UpperCase | FontStyles.Underline;
            collectionText.color = selectedColor;
            racePlay.SetActive(false);
            collectionPlay.SetActive(true);
            playText.SetText("Play");
        }
        playerNumber = PlayerPrefs.GetInt("PlayerNumber", 1);
        playersSlider.value = playerNumber;
        for(int i = 0; i < 4; i++) playerNumbers[i].color = normalColor;
        playerNumbers[playerNumber - 1].color = selectedColor;
        difficulty = PlayerPrefs.GetInt("Difficulty");
        difficultySlider.value = difficulty;
        for(int i = 0; i < 4; i++) difficulties[i].color = normalColor;
        difficulties[difficulty].color = selectedColor;
        nameObject = nameInput.gameObject;
        nameInput.text = "";

        //Get settings variables
        fov = PlayerPrefs.GetFloat("Fov", 35);
        fovSlider.value = fov;
        fovText.SetText(fov.ToString("0.#"));
        depth = PlayerPrefs.GetFloat("Depth", 7000);
        depthSlider.value = depth;
        depthText.SetText(depth.ToString("0"));
        damping = PlayerPrefs.GetFloat("Damping", 0.1f);
        dampingSlider.value = damping;
        dampingText.SetText(damping.ToString("0.#"));
        music = PlayerPrefs.GetFloat("Music", 0.2f);
        musicSlider.value = music;
        musicText.SetText(music.ToString("0.##"));
        if(PlayerPrefs.GetInt("ShowCaptions", 1) == 0)
        {
            showCaptions = false;
            captionToggle.isOn = false;
        }
        else
        {
            showCaptions = true;
            captionToggle.isOn = true;
        }
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
        if(PlayerPrefs.GetInt("ShowArrow", 1) == 0)
        {
            showArrow = false;
            arrowToggle.isOn = false;
        }
        else
        {
            showArrow = true;
            arrowToggle.isOn = true;
        }
        noise = PlayerPrefs.GetFloat("Noise", 0.2f);
        noiseSlider.value = noise;
        noiseText.SetText(noise.ToString("0.##"));
        thruster = PlayerPrefs.GetFloat("Thruster", 0.2f);
        thrusterSlider.value = thruster;
        thrusterText.SetText(thruster.ToString("0.##"));
        effects = PlayerPrefs.GetFloat("Effects", 0.2f);
        effectsSlider.value = effects;
        effectsText.SetText(effects.ToString("0.##"));
        string rebinds = PlayerPrefs.GetString("Bindings");
        if(!string.IsNullOrEmpty(rebinds)) InputSystem.actions.LoadBindingOverridesFromJson(rebinds);

        DebugManager.instance.enableRuntimeUI = false;
        backgroundMusic.Play();
        loaded = true;
    }

    void Update()
    {
        if(customization.activeSelf)
        {
            activeModel.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }

    public void ShowMain()
    {
        gameSettings.SetActive(false);
        customization.SetActive(false);
        settings.SetActive(false);
        loading.SetActive(false);
        mainMenu.SetActive(true);
        MajorClick();
    }

    public void GameSettings()
    {
        mainMenu.SetActive(false);
        gameSettings.SetActive(true);
        MajorClick();
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
        if(activeShip > 3)
        {
            if(rotationSpeed != 40) rotationSpeed = 40;
        }
        else if(rotationSpeed != 80) rotationSpeed = 80;
        nameObject.SetActive(false);
        nameEnter.SetActive(false);
        nameEdit.SetActive(true);
        playerText.enabled = true;
        customization.SetActive(true);
        mainMenu.SetActive(false);
        MajorClick();
    }

    public void Settings()
    {
        string rebinds = PlayerPrefs.GetString("Bindings");
        if(!string.IsNullOrEmpty(rebinds)) InputSystem.actions.LoadBindingOverridesFromJson(rebinds);
        mainMenu.SetActive(false);
        activePlayer = 0;
        for(int i = 1; i < 4; i++)
        {
            controls[i].SetActive(false);
        }
        controls[0].SetActive(true);
        settings.SetActive(true);
        MajorClick();
    }

    public void ChangeMode(bool racing)
    {
        if(racing)
        {
            raceMode = true;
            raceText.fontStyle = FontStyles.UpperCase | FontStyles.Underline;
            raceText.color = selectedColor;
            collectionText.fontStyle = FontStyles.Normal;
            collectionText.color = normalColor;
            racePlay.SetActive(true);
            collectionPlay.SetActive(false);
            playText.SetText("Play  -  100 credits");
            PlayerPrefs.SetInt("Mode", 0);
        }
        else
        {
            raceMode = false;
            raceText.fontStyle = FontStyles.Normal;
            raceText.color = normalColor;
            collectionText.fontStyle = FontStyles.UpperCase | FontStyles.Underline;
            collectionText.color = selectedColor;
            racePlay.SetActive(false);
            collectionPlay.SetActive(true);
            playText.SetText("Play");
            PlayerPrefs.SetInt("Mode", 1);
        }
        MinorClick();
    }

    public void Play()
    {
        if(raceMode)
        {
            if(credits < 100)
            {
                StartCoroutine(ChangeText(playText, halfSecond, Color.white, Color.red, "Play  -  100 credits", "Not enough credits", FontStyles.Italic));
                audioSource.PlayOneShot(errorSound);
                return;
            }
            credits -= 100;
            string newCredits = "Credits: " + credits.ToString();
            creditsText.SetText(newCredits);
            creditsText2.SetText(newCredits);
            PlayerPrefs.SetInt("Credits", credits);
            audioSource.PlayOneShot(buySound);
        }
        gameSettings.SetActive(false);
        loading.SetActive(true);
        MajorClick();
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
        MinorClick();
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
        for(int i = 0; i < shipCount; i++) selectTexts[i].SetText("Select");
        selectTexts[activeShip].SetText("Selected");
        MinorClick();
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
        if(activeShip > 3)
        {
            if(rotationSpeed != 40) rotationSpeed = 40;
        }
        else if(rotationSpeed != 80) rotationSpeed = 80;
    }

    void SelectShip()
    {
        playerShips[activePlayer] = activeShip;
        for(int i = 0; i < shipCount; i++)
        {
            selectTexts[i].SetText("Select");
        }
        selectTexts[activeShip].SetText("Selected");
        PlayerPrefs.SetInt("Ship" + activePlayer.ToString(), activeShip);
        MajorClick();
    }

    public void EditName()
    {
        playerText.enabled = false;
        nameInput.text = "";
        nameEdit.SetActive(false);
        nameObject.SetActive(true);
        nameEnter.SetActive(true);
        MinorClick();
    }

    public void ChangeName()
    {
        nameObject.SetActive(false);
        nameEnter.SetActive(false);
        string newName = nameInput.text;
        bool valid = true;
        for(int i = 0; i < 4; i++)
        {
            if(playerNames[i] == newName && i != activePlayer)
            {
                valid = false;
                break;
            }
        }
        if(newName != "" && valid)
        {
            playerText.SetText(newName);
            playerNames[activePlayer] = newName;
            PlayerPrefs.SetString("Name" + activePlayer.ToString(), newName);
            MinorClick();
        }
        else
        {
            if(string.IsNullOrEmpty(newName)) StartCoroutine(ChangeText(playerText, halfSecond, Color.white, Color.red, playerNames[activePlayer], "Invalid name", FontStyles.Italic));
            else StartCoroutine(ChangeText(playerText, halfSecond, Color.white, Color.red, playerNames[activePlayer], "Name taken", FontStyles.Italic));
            audioSource.PlayOneShot(errorSound);
        }
        playerText.enabled = true;
        nameEdit.SetActive(true);
    }

    public void Ability()
    {
        GameObject activeAbility = abilities[activeShip];
        if(!activeAbility.activeSelf || activeShip == 7)
        {
            switch(activeShip)
            {
                case 0:
                    resetting = StartCoroutine(ResetEffect(activeAbility, twoSeconds));
                    break;
                case 1:
                    resetting = StartCoroutine(ResetEffect(activeAbility, fiveSeconds, true));
                    break;
                case 7:
                    activeAbility.SetActive(false);
                    resetting = StartCoroutine(ResetEffect(activeAbility, twoSeconds));
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
        if(controlSettings.activeSelf)
        {
            PlayerPrefs.SetString("Bindings", InputSystem.actions.SaveBindingOverridesAsJson());
            controlSettings.SetActive(false);
            vaSettings.SetActive(true);
        }
        else
        {
            string rebinds = PlayerPrefs.GetString("Bindings");
            if(!string.IsNullOrEmpty(rebinds)) InputSystem.actions.LoadBindingOverridesFromJson(rebinds);
            vaSettings.SetActive(false);
            controlSettings.SetActive(true);
        }
        MinorClick();
    }

    public void ChangeFOV()
    {
        fov = fovSlider.value;
        fovText.SetText(fov.ToString("0.#"));
        PlayerPrefs.SetFloat("Fov", fov);
    }

    public void ChangeDepth()
    {
        depth = depthSlider.value;
        depthText.SetText(depth.ToString("0"));
        PlayerPrefs.SetFloat("Depth", depth);
    }

    public void ChangeDamping()
    {
        damping = dampingSlider.value;
        dampingText.SetText(damping.ToString("0.#"));
        PlayerPrefs.SetFloat("Damping", damping);
    }

    public void ChangeMusic()
    {
        music = backgroundMusic.volume = musicSlider.value;
        musicText.SetText(music.ToString("0.##"));
        PlayerPrefs.SetFloat("Music", music);
    }

    public void ChangeNoise()
    {
        noise = noiseSlider.value;
        noiseText.SetText(noise.ToString("0.##"));
        PlayerPrefs.SetFloat("Noise", noise);
    }

    public void ChangeThruster()
    {
        thruster = thrusterSlider.value;
        thrusterText.SetText(thruster.ToString("0.##"));
        PlayerPrefs.SetFloat("Thruster", thruster);
    }

    public void ChangeEffects()
    {
        effects = audioSource.volume = effectsSlider.value;
        effectsText.SetText(effects.ToString("0.##"));
        PlayerPrefs.SetFloat("Effects", effects);
    }

    public void ChangeCaptions()
    {
        showCaptions = captionToggle.isOn;
        if(showCaptions) PlayerPrefs.SetInt("ShowCaptions", 1);
        else PlayerPrefs.SetInt("ShowCaptions", 0);
        MinorClick();
    }

    public void ChangeCursor()
    {
        cursorHide = cursorToggle.isOn;
        if(cursorHide) PlayerPrefs.SetInt("CursorHide", 1);
        else PlayerPrefs.SetInt("CursorHide", 0);
        MinorClick();
    }

    public void ChangeArrow()
    {
        showArrow = arrowToggle.isOn;
        if(showArrow) PlayerPrefs.SetInt("ShowArrow", 1);
        else PlayerPrefs.SetInt("ShowArrow", 0);
        MinorClick();
    }

    public void ChangeDifficulty()
    {
        difficulty = (int) difficultySlider.value;
        PlayerPrefs.SetInt("Difficulty", difficulty);
        for(int i = 0; i < 4; i++) difficulties[i].color = normalColor;
        difficulties[difficulty].color = selectedColor;
        MinorClick();
    }

    public void ChangePlayerNumber()
    {
        playerNumber = (int) playersSlider.value;
        PlayerPrefs.SetInt("PlayerNumber", playerNumber);
        for(int i = 0; i < 4; i++) playerNumbers[i].color = normalColor;
        playerNumbers[playerNumber - 1].color = selectedColor;
        MinorClick();
    }

    public void ResetPrefs()
    {
        loaded = false;

        for(int i = 0; i < 4; i++)
        {
            PlayerPrefs.SetInt("Time" + i.ToString(), 0);
            times[i] = 0;
            PlayerPrefs.SetString("Description" + i.ToString(), "");
            PlayerPrefs.SetInt("Ship" + i.ToString(), 0);
            playerShips[i] = 0;
            PlayerPrefs.SetString("Name" + i.ToString(), "Player " + (i + 1).ToString());
            playerNames[i] = "Player " + (i + 1).ToString();
        }
        PlayerPrefs.SetInt("Mode", 0);
        PlayerPrefs.SetInt("PlayerNumber", 1);
        PlayerPrefs.SetInt("Difficulty", 0);
        PlayerPrefs.SetInt("Races", 0);
        PlayerPrefs.SetInt("Credits", 500);
        PlayerPrefs.SetInt("Collects", 0);
        PlayerPrefs.SetFloat("Fov", 35);
        PlayerPrefs.SetFloat("Depth", 7000);
        PlayerPrefs.SetFloat("Damping", 0.1f);
        PlayerPrefs.SetFloat("Music", 0.2f);
        PlayerPrefs.SetInt("ShowCaptions", 1);
        PlayerPrefs.SetInt("CursorHide", 1);
        PlayerPrefs.SetInt("ShowArrow", 1);
        PlayerPrefs.SetFloat("Noise", 0.2f);
        PlayerPrefs.SetFloat("Thruster", 0.2f);
        PlayerPrefs.SetFloat("Effects", 0.2f);

        races = 0;
        racesText.SetText("Races: 0");
        credits = 500;
        creditsText.SetText("Credits: 500");
        creditsText2.SetText("Credits: 500");
        collects = 0;
        collectsText.SetText("Space Junk Collected: 0");
        begTime.SetText("Beginner: 0:00:000");
        intTime.SetText("Intermediate: 0:00:000");
        advTime.SetText("Advanced: 0:00:000");
        expTime.SetText("Expert: 0:00:000");
        raceMode = true;
        raceText.fontStyle = FontStyles.UpperCase | FontStyles.Underline;
        raceText.color = selectedColor;
        collectionText.fontStyle = FontStyles.Normal;
        collectionText.color = normalColor;
        racePlay.SetActive(true);
        collectionPlay.SetActive(false);
        playText.SetText("Play  -  100 credits");
        playersSlider.value = 1;
        for(int i = 1; i < 4; i++) playerNumbers[i].color = normalColor;
        playerNumbers[0].color = selectedColor;
        difficultySlider.value = 0;
        for(int i = 1; i < 4; i++) difficulties[i].color = normalColor;
        difficulties[0].color = selectedColor;
        fovSlider.value = 35;
        fovText.SetText("35");
        depthSlider.value = 7000;
        depthText.SetText("7000");
        dampingSlider.value = 0.1f;
        dampingText.SetText("0.1");
        musicSlider.value = 0.2f;
        musicText.SetText("0.2");
        showCaptions = captionToggle.isOn = cursorHide = cursorToggle.isOn = showArrow = arrowToggle.isOn = true;
        noiseSlider.value = 0.2f;
        noiseText.SetText("0.2");
        thrusterSlider.value = 0.2f;
        thrusterText.SetText("0.2");
        effectsSlider.value = 0.2f;
        effectsText.SetText("0.2");
        foreach(InputActionMap map in InputSystem.actions.actionMaps) map.RemoveAllBindingOverrides();
        PlayerPrefs.SetString("Bindings", InputSystem.actions.SaveBindingOverridesAsJson());

        loaded = true;
        MajorClick();
    }

    public void ResetSettings()
    {
        PlayerPrefs.SetFloat("Fov", 35);
        PlayerPrefs.SetFloat("Depth", 7000);
        PlayerPrefs.SetFloat("Damping", 0.1f);
        PlayerPrefs.SetFloat("Music", 0.2f);
        PlayerPrefs.SetInt("ShowCaptions", 1);
        PlayerPrefs.SetInt("CursorHide", 1);
        PlayerPrefs.SetInt("ShowArrow", 1);
        PlayerPrefs.SetFloat("Noise", 0.2f);
        PlayerPrefs.SetFloat("Thruster", 0.2f);
        PlayerPrefs.SetFloat("Effects", 0.2f);

        fovSlider.value = 35;
        fovText.SetText("35");
        depthSlider.value = 7000;
        depthText.SetText("7000");
        dampingSlider.value = 0.1f;
        dampingText.SetText("0.1");
        musicSlider.value = 0.2f;
        musicText.SetText("0.2");
        showCaptions = captionToggle.isOn = cursorHide = cursorToggle.isOn = showArrow = arrowToggle.isOn = true;
        noiseSlider.value = 0.2f;
        noiseText.SetText("0.2");
        thrusterSlider.value = 0.2f;
        thrusterText.SetText("0.2");
        effectsSlider.value = 0.2f;
        effectsText.SetText("0.2");
        MinorClick();
    }

    public void NextPlayerControls(int increment)
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
        for(int i = 0; i < 4; i++) controls[i].SetActive(false);
        controls[activePlayer].SetActive(true);
        MinorClick();
    }

    public void ResetControls()
    {
        InputSystem.actions.FindActionMap("Player" + (activePlayer + 1).ToString()).RemoveAllBindingOverrides();
        MinorClick();
    }

    public void MinorClick()
    {
        if(loaded) audioSource.PlayOneShot(minorClick);
    }

    public void MajorClick()
    {
        if(loaded) audioSource.PlayOneShot(majorClick);
    }

    IEnumerator ResetEffect(GameObject effect, WaitForSeconds wait, bool sound = false)
    {
        yield return wait;
        effect.SetActive(false);
        if(sound)
        {
            audioSource.PlayOneShot(debuffSound);
        }
    }

    IEnumerator ChangeText(TMP_Text text, WaitForSeconds wait, Color originalColor, Color newColor, string originalString = "", string newString = "", FontStyles style = FontStyles.Normal, int amount = 1)
    {
        if(originalString != newString) text.text = newString;
        if(originalColor == newColor) originalColor = Color.white;
        if(text.fontStyle != style) text.fontStyle = style;
        switch(amount)
        {
            case 1:
                text.color = newColor;
                yield return wait;
                if(text.color == newColor) text.color = originalColor;
                break;

            default:
                int amountMinusOne = amount - 1;
                for(int i = 0; i < amount; i++)
                {
                    text.color = newColor;
                    yield return wait;
                    if(text.color == newColor) text.color = originalColor;
                    if(i < amountMinusOne) yield return wait;
                }
                break;
        }
        if(originalString != newString) text.text = originalString;
        if(text.fontStyle != FontStyles.Normal) text.fontStyle = FontStyles.Normal;
    }

    IEnumerator LoadGame()
    {
        yield return new WaitForSeconds(1);
        AsyncOperation load = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        while(!load.isDone)
        {
            yield return null;
        }
        SceneManager.UnloadSceneAsync(0);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(1));
    }
}
