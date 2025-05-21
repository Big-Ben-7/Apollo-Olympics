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
    [SerializeField] GameObject intro;
    [SerializeField] Canvas mainMenu;
    [SerializeField] Canvas gameSettings;
    [SerializeField] Canvas customization;
    [SerializeField] GameObject shipParent;
    [SerializeField] Canvas settings;
    [SerializeField] Canvas resetMenu;
    [SerializeField] GameObject loading;
    [SerializeField] GameObject instructions;

    [Header("Main Menu")]
    [SerializeField] TMP_Text racesText;
    [SerializeField] TMP_Text collectsText;
    [SerializeField] TMP_Text begTime;
    [SerializeField] TMP_Text intTime;
    [SerializeField] TMP_Text advTime;
    [SerializeField] TMP_Text expTime;
    [SerializeField] TMP_Text[] manualTexts;
    [SerializeField] Canvas manualCanvas;
    [SerializeField] GameObject manualBack;
    [SerializeField] GameObject manualNext;
    [SerializeField] GameObject[] manualClose;
    int lastManualIndex;
    int manualIndex = 0;

    [Header("Game Settings")]
    [SerializeField] Slider difficultySlider;
    [SerializeField] TMP_Text[] difficulties;
    [SerializeField] Slider playersSlider;
    [SerializeField] TMP_Text[] playerNumbers;
    [SerializeField] Color normalColor;
    [SerializeField] Color selectedColor;
    [SerializeField] GameObject[] gameSettingWindows;
    int activeGameSetting;

    [Header("Customization")]
    [SerializeField] GameObject[] ships;
    [SerializeField] TMP_Text playerText;
    [SerializeField] TMP_InputField nameInput;
    [SerializeField] GameObject nameEdit;
    [SerializeField] GameObject nameEnter;
    TMP_Text namePlaceholder;
    GameObject nameObject;
    TMP_Text[] selectTexts;
    Transform[] models;
    GameObject[] abilities;
    [SerializeField] AudioClip[] abilitySounds;
    [SerializeField] AudioClip debuffSound;
    Transform activeModel;
    int shipCount;
    int activeShip;
    int activePlayer;
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
    [SerializeField] Slider voiceSlider;
    [SerializeField] Slider thrusterSlider;
    [SerializeField] Slider effectsSlider;
    [SerializeField] TMP_Text fovText;
    [SerializeField] TMP_Text depthText;
    [SerializeField] TMP_Text dampingText;
    [SerializeField] TMP_Text musicText;
    [SerializeField] TMP_Text voiceText;
    [SerializeField] TMP_Text thrusterText;
    [SerializeField] TMP_Text effectsText;

    [SerializeField] GameObject[] controls;
    [SerializeField] GameObject rebindErrorText;
    [SerializeField] GameObject[] settingWindows;
    int activeSetting;

    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioSource backgroundMusic;
    [SerializeField] AudioClip majorClick;
    [SerializeField] AudioClip minorClick;
    [SerializeField] AudioClip errorSound;
    
    bool loaded;
    public static bool fromGame;
    public static bool replay;
    public static bool tutorial;
    public static int playerNumber;
    public static int difficulty;
    public static int[] playerShips = new int[4];
    public static int[] times = new int[4];
    public static int collects;
    public static int races;
    public static float fov;
    public static float depth;
    public static float damping;
    public static bool showCaptions;
    public static bool cursorHide;
    public static bool showArrow;
    public static float music;
    public static float voice;
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
        collects = PlayerPrefs.GetInt("Collects");
        collectsText.SetText("Space Debris Collected: " + collects.ToString());
        TimeSpan begSpan = TimeSpan.FromMilliseconds(times[0]);
        begTime.SetText("Beginner: " + begSpan.Minutes.ToString() + ":" + begSpan.Seconds.ToString("00") + "." + begSpan.Milliseconds.ToString("000") + PlayerPrefs.GetString("Description0"));
        TimeSpan intSpan = TimeSpan.FromMilliseconds(times[1]);
        intTime.SetText("Intermediate: " + intSpan.Minutes.ToString() + ":" + intSpan.Seconds.ToString("00") + "." + intSpan.Milliseconds.ToString("000") + PlayerPrefs.GetString("Description1"));
        TimeSpan advSpan = TimeSpan.FromMilliseconds(times[2]);
        advTime.SetText("Advanced: " + advSpan.Minutes.ToString() + ":" + advSpan.Seconds.ToString("00") + "." + advSpan.Milliseconds.ToString("000") + PlayerPrefs.GetString("Description2"));
        TimeSpan expSpan = TimeSpan.FromMilliseconds(times[3]);
        expTime.SetText("Expert: " + expSpan.Minutes.ToString() + ":" + expSpan.Seconds.ToString("00") + "." + expSpan.Milliseconds.ToString("000") + PlayerPrefs.GetString("Description3"));
        lastManualIndex = manualTexts.Length - 1;
        manualClose[0].SetActive(false);
        manualClose[1].SetActive(false);
        manualNext.SetActive(true);

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
        namePlaceholder = nameInput.placeholder.GetComponent<TMP_Text>();
        nameObject = nameInput.gameObject;
        nameInput.text = "";

        //Get game settings variables
        playersSlider.value = PlayerPrefs.GetInt("PlayerNumber", 1);
        difficultySlider.value = PlayerPrefs.GetInt("Difficulty");
        ChangePlayerNumber();
        ChangeDifficulty();
        NextGameSetting(PlayerPrefs.GetInt("Tutorial"));

        //Get settings variables
        fovSlider.value = PlayerPrefs.GetFloat("Fov", 60);
        depthSlider.value = PlayerPrefs.GetFloat("Depth", 8000);
        dampingSlider.value = PlayerPrefs.GetFloat("Damping", 0.2f);
        musicSlider.value = PlayerPrefs.GetFloat("Music", 0.4f);
        if(PlayerPrefs.GetInt("ShowCaptions", 1) == 0) captionToggle.isOn = false;
        else captionToggle.isOn = true;
        if(PlayerPrefs.GetInt("CursorHide") == 0) cursorToggle.isOn = false;
        else cursorToggle.isOn = true;
        if(PlayerPrefs.GetInt("ShowArrow", 1) == 0) arrowToggle.isOn = false;
        else arrowToggle.isOn = true;
        voiceSlider.value = PlayerPrefs.GetFloat("Voice", 0.4f);
        thrusterSlider.value = PlayerPrefs.GetFloat("Thruster", 0.4f);
        effectsSlider.value = PlayerPrefs.GetFloat("Effects", 0.4f);
        ChangeFOV();
        ChangeDepth();
        ChangeDamping();
        ChangeMusic();
        ChangeCaptions();
        ChangeCursor();
        ChangeArrow();
        ChangeVoice();
        ChangeThruster();
        ChangeEffects();

        intro.SetActive(false);
        instructions.SetActive(false);
        shipParent.SetActive(false);
        mainMenu.enabled = gameSettings.enabled = customization.enabled = settings.enabled = resetMenu.enabled = manualCanvas.enabled = false;
        
        if(fromGame)
        {
            mainMenu.enabled = true;
            backgroundMusic.Play();
            fromGame = false;
        }
        else if(replay)
        {
            gameSettings.enabled = true;
            backgroundMusic.Play();
            replay = false;
        }
        else intro.SetActive(true);

        DebugManager.instance.enableRuntimeUI = false;
        if(!Cursor.visible) Cursor.visible = true;
        loaded = true;
    }

    void Update()
    {
        if(shipParent.activeSelf) activeModel.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }

    public void Enter()
    {
        intro.SetActive(false);
        ShowMain();
        if(PlayerPrefs.GetInt("FirstPlay", 1) == 1)
        {
            StartCoroutine(OpenManual());
            PlayerPrefs.SetInt("FirstPlay", 0);
        }
        //instructions.SetActive(true);
        //MajorClick();
    }

    IEnumerator OpenManual()
    {
        yield return oneSecond;
        ToggleManual();
    }

    public void NextManual(int increment)
    {
        manualIndex += increment;
        if (manualIndex < 0)
        {
            manualIndex = 0;
            return;
        }
        else if (manualIndex == 0)
        {
            manualBack.SetActive(false);
            manualClose[0].SetActive(true);
        }
        else if (manualIndex == lastManualIndex)
        {
            manualNext.SetActive(false);
            manualClose[1].SetActive(true);
        }
        else if (manualIndex > lastManualIndex)
        {
            manualIndex = lastManualIndex;
            return;
        }
        else
        {
            if (!manualNext.activeSelf)
            {
                manualClose[1].SetActive(false);
                manualNext.SetActive(true);
            }
            else if (!manualBack.activeSelf)
            {
                manualClose[0].SetActive(false);
                manualBack.SetActive(true);
            }
        }
        for (int i = 0; i <= lastManualIndex; i++) manualTexts[i].enabled = false;
        manualTexts[manualIndex].enabled = true;
        MinorClick();
    }

    public void ToggleManual()
    {
        for(int i = 0; i <= lastManualIndex; i++) manualTexts[i].enabled = false;
        manualTexts[manualIndex].enabled = true;
        manualCanvas.enabled = !manualCanvas.enabled;
        MinorClick();
    }

    public void ShowMain()
    {
        //instructions.SetActive(false);
        //loading.SetActive(false);
        shipParent.SetActive(false);
        gameSettings.enabled = customization.enabled = settings.enabled = false;
        mainMenu.enabled = true;
        if(!backgroundMusic.isPlaying) backgroundMusic.Play();
        MajorClick();
    }

    public void GameSettings()
    {
        mainMenu.enabled = false;
        gameSettings.enabled = true;
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
        shipParent.SetActive(true);
        mainMenu.enabled = false;
        customization.enabled = true;
        MajorClick();
    }

    public void Settings()
    {
        string rebinds = PlayerPrefs.GetString("Bindings");
        if(!string.IsNullOrEmpty(rebinds)) InputSystem.actions.LoadBindingOverridesFromJson(rebinds);
        activePlayer = 0;
        for(int i = 1; i < 4; i++) controls[i].SetActive(false);
        controls[0].SetActive(true);
        activeSetting = 0;
        for(int i = 1; i < 3; i++) settingWindows[i].SetActive(false);
        settingWindows[0].SetActive(true);
        mainMenu.enabled = false;
        settings.enabled = true;
        MajorClick();
    }

    public void ResetMenu()
    {
        resetMenu.enabled = !resetMenu.enabled;
        MajorClick();
    }

    public void Play()
    {
        gameSettings.enabled = false;
        if(manualCanvas.enabled) manualCanvas.enabled = false;
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

    public void SelectShip()
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
        namePlaceholder.SetText(playerNames[activePlayer]);
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
        if(newName == "") MinorClick();
        else if(valid)
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

    public void NextGameSetting(int newActive)
    {
        activeGameSetting = newActive;
        for(int i = 0; i < 2; i++) gameSettingWindows[i].SetActive(false);
        gameSettingWindows[activeGameSetting].SetActive(true);
        if(activeGameSetting == 0) tutorial = false;
        else tutorial = true;
        PlayerPrefs.SetInt("Tutorial", activeGameSetting);
        MinorClick();
    }

    public void NextSetting(int increment)
    {
        activeSetting += increment;
        if(activeSetting > 2) activeSetting = 0;
        if(activeSetting < 0) activeSetting = 2;
        if(activeSetting == 0)
        {
            string rebinds = PlayerPrefs.GetString("Bindings");
            if(!string.IsNullOrEmpty(rebinds)) InputSystem.actions.LoadBindingOverridesFromJson(rebinds);
        }
        for(int i = 0; i < 3; i++) settingWindows[i].SetActive(false);
        settingWindows[activeSetting].SetActive(true);
        MinorClick();
    }

    public void ChangeFOV()
    {
        fov = Mathf.Round(fovSlider.value);
        fovText.SetText(fov.ToString());
        PlayerPrefs.SetFloat("Fov", fov);
    }

    public void ChangeDepth()
    {
        depth = Mathf.Round(depthSlider.value);
        depthText.SetText(depth.ToString());
        PlayerPrefs.SetFloat("Depth", depth);
    }

    public void ChangeDamping()
    {
        damping = (float) Math.Round(dampingSlider.value, 2);
        dampingText.SetText(damping.ToString("0.00"));
        PlayerPrefs.SetFloat("Damping", damping);
    }

    public void ChangeMusic()
    {
        music = backgroundMusic.volume = (float) Math.Round(musicSlider.value, 2);
        musicText.SetText(music.ToString("0.00"));
        PlayerPrefs.SetFloat("Music", music);
    }

    public void ChangeVoice()
    {
        voice = (float) Math.Round(voiceSlider.value, 2);
        voiceText.SetText(voice.ToString("0.00"));
        PlayerPrefs.SetFloat("Voice", voice);
    }

    public void ChangeThruster()
    {
        thruster = (float) Math.Round(thrusterSlider.value, 2);
        thrusterText.SetText(thruster.ToString("0.00"));
        PlayerPrefs.SetFloat("Thruster", thruster);
    }

    public void ChangeEffects()
    {
        effects = audioSource.volume = (float) Math.Round(effectsSlider.value, 2);
        effectsText.SetText(effects.ToString("0.00"));
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
        PlayerPrefs.SetInt("Races", 0);
        PlayerPrefs.SetInt("Collects", 0);

        races = 0;
        racesText.SetText("Races: 0");
        collects = 0;
        collectsText.SetText("Space Debris Collected: 0");
        begTime.SetText("Beginner: 0:00:000");
        intTime.SetText("Intermediate: 0:00:000");
        advTime.SetText("Advanced: 0:00:000");
        expTime.SetText("Expert: 0:00:000");
        playersSlider.value = 1;
        difficultySlider.value = 0;
        NextGameSetting(0);

        fovSlider.value = 60;
        depthSlider.value = 8000;
        dampingSlider.value = 0.2f;
        musicSlider.value = voiceSlider.value = thrusterSlider.value = effectsSlider.value = 0.4f;
        captionToggle.isOn = arrowToggle.isOn = true;
        cursorToggle.isOn = false;
        foreach(InputActionMap map in InputSystem.actions.actionMaps) map.RemoveAllBindingOverrides();
        SaveBindings();
        string rebinds = PlayerPrefs.GetString("Bindings");
        if(!string.IsNullOrEmpty(rebinds)) InputSystem.actions.LoadBindingOverridesFromJson(rebinds);

        loaded = true;
        ResetMenu();
    }

    public void ResetSettings()
    {
        loaded = false;

        fovSlider.value = 60;
        depthSlider.value = 8000;
        dampingSlider.value = 0.2f;
        musicSlider.value = voiceSlider.value = thrusterSlider.value = effectsSlider.value = 0.4f;
        captionToggle.isOn = arrowToggle.isOn = true;
        cursorToggle.isOn = false;

        loaded = true;
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

    public void StartRebind()
    {
        if(rebindErrorText.activeSelf) rebindErrorText.SetActive(false);
        MinorClick();
    }

    public void ResetControls()
    {
        InputSystem.actions.FindActionMap("Player" + (activePlayer + 1).ToString()).RemoveAllBindingOverrides();
        InputSystem.actions.FindActionMap("General").RemoveAllBindingOverrides();
        SaveBindings();
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
        if(originalString != newString) text.SetText(newString);
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
        if(originalString != newString) text.SetText(originalString);
        if(text.fontStyle != FontStyles.Normal) text.fontStyle = FontStyles.Normal;
    }

    IEnumerator LoadGame()
    {
        yield return new WaitForSeconds(1);
        gameObject.tag = "Untagged";
        AsyncOperation load = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        while(!load.isDone)
        {
            yield return null;
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(1));
        SceneManager.UnloadSceneAsync(0);
    }

    public void SaveBindings()
    {
        PlayerPrefs.SetString("Bindings", InputSystem.actions.SaveBindingOverridesAsJson());
    }
}
