using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;
using System;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SpawnManager : MonoBehaviour
{
    [Header("Sections")]
    [SerializeField] int side = 3500;
    int length;
    GameObject[] sectionPool;
    [SerializeField] Transform sectionParent;
    [SerializeField] GameObject sectionPrefab;
    Vector3[] sectionChunks;
    GameObject[][] childrenList;
    Rigidbody[][] rbList;
    int sectionNumber;

    [Header("Checkpoints")]
    public int[] playerCheckpoints;
    [SerializeField] int[] begCheckpointDistances;
    [SerializeField] int[] intCheckpointDistances;
    [SerializeField] int[] advCheckpointDistances;
    [SerializeField] int[] expCheckpointDistances;
    int[] checkpointDistances;
    [SerializeField] Transform checkpointParent;
    [SerializeField] GameObject checkpointPrefab;
    [SerializeField] Material finishMat;
    int checkpointCount;
    float lastCheckpointDistance;

    [Header("Interactables")]
    [SerializeField] int difficulty;
    [SerializeField] GameObject[] possibleAsteroids;
    [SerializeField] GameObject[] possibleCollectables;
    int asteroidCount;
    int collectableCount;
    [SerializeField] GameObject wormholePrefab;
    //Asteroid number must be divisible by 20
    [SerializeField] int[] asteroidNumbers = new int[] {20, 40, 60, 80};
    int asteroidNumber;
    [SerializeField] int minAsteroidScale = 250;
    [SerializeField] int maxAsteroidScale = 2500;
    [SerializeField] int[] collectableNumbers = new int[] {10, 8, 6, 4};
    int collectableNumber;
    [SerializeField] int partDisablePercent = 50;
    [SerializeField] int maxMovementForce1 = 70;
    [SerializeField] int maxMovementForce2 = 250;
    [SerializeField] int movementForce1Percent = 70;
    [SerializeField] float maxRotationForce = 0.2f;
    [SerializeField] float maxRotationSpeed = 30;
    [SerializeField] int wormholePercent = 5;
    [SerializeField] Vector3 wormholeProportions = new Vector3(0.5f, 0.4f, 0.8f);
    [SerializeField] int minWormholeScale = 1500;
    [SerializeField] int maxWormholeScale = 3000;
    int interactableNumber;

    [Header("Drones")]
    [SerializeField] int[] droneNumbers = new int[] {8, 16, 24, 32};
    int droneNumber;
    [SerializeField] int[] begCheckpointDrones;
    [SerializeField] int[] intCheckpointDrones;
    [SerializeField] int[] advCheckpointDrones;
    [SerializeField] int[] expCheckpointDrones;
    int[] checkpointDrones;
    int baseDisableNumber;
    int[] disableByCheckpoint;

    [Header("Players")]
    [SerializeField] int playerNumber = 1;
    [SerializeField] GameObject[] possiblePlayers;
    [SerializeField] int[] playerShips;
    GameObject[] players;
    public Vector3[] playerChunks;
    public List<int> playingPlayers = new List<int>();
    public static List<Transform>[] followers;
    bool finished = true;
    PlayerController[] playerControllers;
    public string[] playerNames;

    [Header("Customization")]
    public int[] initialThrustSpeeds;
    public int[] accelerations;
    public int[] maxThrustSpeeds;
    public float[] rotationSpeeds;
    public float[] decMultipliers;
    public int[] healthLossDivisors;
    public float[] speedLossDivisors;
    public float[] minHealthLosses;
    public float[] deflectionForces;

    [Header("Camera/UI/Audio")]
    [SerializeField] GameObject cameraPrefab;
    [SerializeField] LayerMask[] cameraMasks;
    [SerializeField] LayerMask[] finishMasks;
    [SerializeField] AudioClip countdownSound;
    [SerializeField] AudioClip goSound;
    [SerializeField] AudioClip pauseSound;
    [SerializeField] AudioClip finishSound;
    [SerializeField] AudioClip confettiSound;
    [SerializeField] AudioClip[] goVoices;
    [SerializeField] AudioClip[] firstVoices;
    [SerializeField] AudioClip[] secondVoices;
    [SerializeField] AudioClip[] thirdVoices;
    [SerializeField] AudioClip[] fourthVoices;
    [SerializeField] GameObject eventSystem;
    Camera[] views = new Camera[4];
    CinemachineVirtualCamera[] thirdPersons = new CinemachineVirtualCamera[4];
    public AudioSource audioSource;
    AudioSource backgroundMusic;
    AudioSource backgroundNoise;
    AudioSource[] allAudio;
    int audioCount;
    float voiceScale;
    float[] stereoPans;
    public int initializedObjects;
    bool cursorHide;
    InputAction pauseAction;
    WaitForSeconds oneSecond;
    WaitForSeconds oneAndHalfSeconds = new WaitForSeconds(1.5f);
    WaitForSeconds fiveSeconds = new WaitForSeconds(5);
    WaitForSecondsRealtime oneRealSecond = new WaitForSecondsRealtime(1);
    public TMP_Text[] finishTexts;
    public GameObject[] confettis;
    public float milliseconds;
    public int seconds;
    public int minutes;
    public string timeString;
    public bool paused = true;

    [Header("Menus")]
    [SerializeField] Canvas menu;
    [SerializeField] Canvas raceFinish;
    [SerializeField] Canvas loading;
    [SerializeField] Canvas pauseButton;
    [SerializeField] Canvas settings;
    [SerializeField] GameObject[] controls;
    [SerializeField] GameObject rebindErrorText;
    [SerializeField] GameObject[] settingWindows;
    int activePlayer;
    int activeSetting;

    [Header("Video/Audio Settings")]
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

    float damping;
    CinemachineFramingTransposer[] transposers;
    CinemachineSameAsFollowTarget[] camAims;
    public AudioSource[] effectSources;
    public AudioSource[] voiceSources;
    GameObject[] arrows;
    
    void Awake()
    {
        menu.enabled = raceFinish.enabled = loading.enabled = pauseButton.enabled = settings.enabled = false;
        length = side * 2;

        //Audio setup
        AudioSource[] audioSources = GetComponents<AudioSource>();
        audioSource = audioSources[0];
        backgroundMusic = audioSources[1];
        backgroundNoise = audioSources[2];

        //Getting variabes from main menu
        float fov, depth, music, thruster, effects, voice;
        playerNumber = MenuManager.playerNumber;
        playerShips = MenuManager.playerShips;
        difficulty = MenuManager.difficulty;
        playerNames = MenuManager.playerNames;
        effects = audioSource.volume = PlayerController.audioVolume = effectsSlider.value = MenuManager.effects;
        if(effects > 0.5f) PlayerController.warningScale = 1 / effects;
        else PlayerController.warningScale = 2;
        voice = PlayerController.voiceVolume = voiceSlider.value = MenuManager.voice;
        voiceScale = voice / effects;
        music = musicSlider.value = MenuManager.music;
        float bg = backgroundMusic.volume = music * 0.5f;
        if(bg < 0.1f) backgroundNoise.volume = bg;
        cursorHide = cursorToggle.isOn = MenuManager.cursorHide;
        if(cursorHide) Cursor.visible = false;
        CinemachineVirtualCamera view = cameraPrefab.transform.GetChild(0).GetChild(0).GetComponent<CinemachineVirtualCamera>();
        fov = view.m_Lens.FieldOfView = fovSlider.value = MenuManager.fov;
        depth = view.m_Lens.FarClipPlane = depthSlider.value = MenuManager.depth;
        damping = dampingSlider.value = MenuManager.damping;
        thruster = PlayerController.maxThrusterVolume = thrusterSlider.value = MenuManager.thruster;
        PlayerController.showCaptions = Drone.showCaptions = captionToggle.isOn = MenuManager.showCaptions;
        PlayerController.showArrow = arrowToggle.isOn = MenuManager.showArrow;

        fovText.SetText(fov.ToString("0"));
        depthText.SetText(depth.ToString("0"));
        musicText.SetText(music.ToString("0.00"));
        voiceText.SetText(voice.ToString("0.00"));
        thrusterText.SetText(thruster.ToString("0.00"));
        effectsText.SetText(effects.ToString("0.00"));
        dampingText.SetText(damping.ToString("0.00"));

        //Setting difficulty-dependent variables
        asteroidNumber = asteroidNumbers[difficulty];
        collectableNumber = collectableNumbers[difficulty];
        droneNumber = droneNumbers[difficulty];
        checkpointDistances = new int[][]{begCheckpointDistances, intCheckpointDistances, advCheckpointDistances, expCheckpointDistances}[difficulty];
        checkpointCount = checkpointDistances.Length;
        checkpointDrones = new int[][]{begCheckpointDrones, intCheckpointDrones, advCheckpointDrones, expCheckpointDrones}[difficulty];

        //Stats for each checkpoint
        baseDisableNumber = asteroidNumber * 2 / 5;
        disableByCheckpoint = new int[checkpointCount];
        int increment = baseDisableNumber / (checkpointCount - 1);
        for(int i = 0; i < checkpointCount; i++)
        {
            if(i == 4)
            {
                //Asteroid belt should have high asteroid number
                disableByCheckpoint[4] = increment;
                continue;
            }
            disableByCheckpoint[i] = baseDisableNumber - increment * i;
        }

        PlayerController.checkpointDistances = checkpointDistances;
        PlayerController.checkpointDrones = checkpointDrones;
        timeString = "Time: 0:00.000";
        interactableNumber = asteroidNumber + collectableNumber;
        asteroidCount = possibleAsteroids.Length;
        collectableCount = possibleCollectables.Length;

        //Initialize player variables
        PlayerController.spawnManager = this;
        PlayerController.length = length;
        PlayerController.droneNumber = droneNumber;
        PlayerController.oneSecond = oneSecond = MenuManager.oneSecond;
        PlayerController.twoSeconds = Drone.twoSeconds = MenuManager.twoSeconds;
        PlayerController.threeSeconds = new WaitForSeconds(3);
        PlayerController.fifthSecond = new WaitForSeconds(0.2f);
        PlayerController.halfSecond = MenuManager.halfSecond;
        pauseAction = InputSystem.actions.FindAction("Pause");
        StartCoroutine(Initialize());
    }
    IEnumerator Initialize()
    {
        while(SceneManager.GetActiveScene() != SceneManager.GetSceneByBuildIndex(1))
        {
            yield return null;
        }
        
        //Section spawning
        DebugManager.instance.enableRuntimeUI = false;
        sectionNumber = playerNumber * 27;
        sectionChunks = new Vector3[sectionNumber];
        sectionPool = new GameObject[sectionNumber];
        childrenList = new GameObject[sectionNumber][];
        rbList = new Rigidbody[sectionNumber][];
        Vector3 spawnPos = Vector3.zero;
        for(int i = 0; i < sectionNumber; i++)
        {
            switch(i)
            {
                case 0 or > 26:
                    break;
                case 1:
                    spawnPos = new Vector3(0, 0, length);
                    break;
                case 2:
                    spawnPos = new Vector3(0, 0, -length);
                    break;
                case 3:
                    spawnPos = new Vector3(length, 0, 0);
                    break;
                case 4:
                    spawnPos = new Vector3(-length, 0, 0);
                    break;
                case 5:
                    spawnPos = new Vector3(length, 0, length);
                    break;
                case 6:
                    spawnPos = new Vector3(-length, 0, length);
                    break;
                case 7:
                    spawnPos = new Vector3(length, 0, -length);
                    break;
                case 8:
                    spawnPos = new Vector3(-length, 0, -length);
                    break;
                case 9:
                    spawnPos = new Vector3(0, length, 0);
                    break;
                case 10:
                    spawnPos = new Vector3(0, length, length);
                    break;
                case 11:
                    spawnPos = new Vector3(0, length, -length);
                    break;
                case 12:
                    spawnPos = new Vector3(length, length, 0);
                    break;
                case 13:
                    spawnPos = new Vector3(-length, length, 0);
                    break;
                case 14:
                    spawnPos = new Vector3(length, length, length);
                    break;
                case 15:
                    spawnPos = new Vector3(-length, length, length);
                    break;
                case 16:
                    spawnPos = new Vector3(length, length, -length);
                    break;
                case 17:
                    spawnPos = new Vector3(-length, length, -length);
                    break;
                case 18:
                    spawnPos = new Vector3(0, -length, 0);
                    break;
                case 19:
                    spawnPos = new Vector3(0, -length, length);
                    break;
                case 20:
                    spawnPos = new Vector3(0, -length, -length);
                    break;
                case 21:
                    spawnPos = new Vector3(length, -length, 0);
                    break;
                case 22:
                    spawnPos = new Vector3(-length, -length, 0);
                    break;
                case 23:
                    spawnPos = new Vector3(length, -length, length);
                    break;
                case 24:
                    spawnPos = new Vector3(-length, -length, length);
                    break;
                case 25:
                    spawnPos = new Vector3(length, -length, -length);
                    break;
                case 26:
                    spawnPos = new Vector3(-length, -length, -length);
                    break;
            }

            //Setup sections
            GameObject section = sectionPool[i] = Instantiate(sectionPrefab, spawnPos, Quaternion.identity, sectionParent);
            section.SetActive(false);
            sectionChunks[i] = spawnPos / length;
            SetSection(i);
        }

        //Spawn checkpoints
        int lastCheckpoint = checkpointCount - 1;
        float newScale;
        for(int i = 0; i < lastCheckpoint; i++)
        {
            Transform checkpoint = Instantiate(checkpointPrefab, checkpointParent).transform;
            newScale = checkpointDistances[i] * 2;
            checkpoint.localScale = new Vector3(newScale, newScale, newScale);
        }
        newScale = checkpointDistances[lastCheckpoint] * 2;
        Transform finish = Instantiate(checkpointPrefab, checkpointParent).transform;
        finish.localScale = new Vector3(newScale, newScale, newScale);
        finish.GetComponent<Renderer>().sharedMaterial = finishMat;
        lastCheckpointDistance = checkpointDistances[lastCheckpoint];
        lastCheckpointDistance = lastCheckpointDistance * lastCheckpointDistance;

        //Spawn players
        players = confettis = arrows = new GameObject[playerNumber];
        playerChunks = new Vector3[playerNumber];
        playerCheckpoints = new int[playerNumber];
        followers = new List<Transform>[playerNumber];
        Transform[] cameraObjects = new Transform[playerNumber];
        finishTexts = new TMP_Text[playerNumber];
        playerControllers = new PlayerController[playerNumber];
        transposers = new CinemachineFramingTransposer[playerNumber];
        camAims = new CinemachineSameAsFollowTarget[playerNumber];
        effectSources = voiceSources = new AudioSource[playerNumber];

        Vector3[] startPos = new Vector3[] {new Vector3(0, 0, 300), new Vector3(0, 0, -300), new Vector3(300, 0, 0), new Vector3(-300, 0, 0)};
        Quaternion[] startRots = new Quaternion[] {Quaternion.identity, Quaternion.Euler(0, 180, 0), Quaternion.Euler(0, 90, 0), Quaternion.Euler(0, -90, 0)};
        Vector2 scaleVector;
        Vector2[] cameraRects;
        string[] playerStrings;
        Vector3 camOffset = Vector3.zero;
        switch(playerNumber)
        {
            case 1:
                scaleVector = Vector2.one;
                cameraRects = new Vector2[] {Vector2.zero};
                playerStrings = new string[] {"1"};
                stereoPans = new float[] {0};
                break;

            case 2:
                scaleVector = new Vector2(0.5f, 1);
                cameraRects = new Vector2[] {Vector2.zero, new Vector2(0.5f, 0)};
                playerStrings = new string[] {"1", "2"};
                stereoPans = new float[] {-0.9f, 0.9f};
                break;

            case 3:
                scaleVector = new Vector2(0.333f, 1);
                cameraRects = new Vector2[] {Vector2.zero, new Vector2(0.333f, 0), new Vector2(0.667f, 0)};
                playerStrings = new string[] {"1", "3", "2"};
                camOffset = new Vector3(0, 1, -5);
                stereoPans = new float[] {-0.9f, 0, 0.9f};
                break;
            
            default:
                scaleVector = new Vector2(0.5f, 0.5f);
                cameraRects = new Vector2[] {new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0.5f, 0)};
                playerStrings = new string[] {"1", "4", "3", "2"};
                camOffset = new Vector3(0, -1, 5);
                stereoPans = new float[] {-0.9f, 0.9f, -0.9f, 0.9f};
                break;
        }

        for(int i = 0; i < playerNumber; i++)
        {
            followers[i] = new List<Transform>();
            playerChunks[i] = Vector3.zero;
            playerCheckpoints[i] = 0;
            playingPlayers.Add(i);
            Transform cameraObject = cameraObjects[i] = Instantiate(cameraPrefab).transform;
            Transform viewObject = cameraObject.GetChild(0);
            Camera view = views[i] = viewObject.GetComponent<Camera>();
            GameObject thirdPersonObject = viewObject.GetChild(0).gameObject;
            CinemachineVirtualCamera viewCam = thirdPersons[i] = thirdPersonObject.GetComponent<CinemachineVirtualCamera>();
            view.cullingMask = cameraMasks[i];

            Vector3 pos = startPos[i];
            Quaternion rot = startRots[i];
            GameObject player = players[i] = Instantiate(possiblePlayers[playerShips[i]], pos, rot);
            PlayerController playerController = playerControllers[i] = player.GetComponent<PlayerController>();
            Transform playerTransform = playerController.myTransform = player.transform;
            playerController.playerNumberString = playerStrings[i];
            playerController.playerIndex = i;
            playerController.cameraStats = cameraObject.GetChild(1);
            playerController.startPos = pos;
            playerController.normalizedStartPos = pos.normalized;
            playerController.startRot = rot;
            playerController.stereoPan = stereoPans[i];

            Transform arrow = playerController.arrow = playerTransform.GetChild(3);
            arrows[i] = arrow.gameObject;
            Debug.Log(arrows[i]);
            arrow.gameObject.SetActive(false);
            GameObject arrowObject = arrow.GetChild(0).gameObject;
            arrowObject.GetComponent<Canvas>().worldCamera = view;
            arrowObject.layer = thirdPersonObject.layer = i + 9;
            Transform lookTarget = playerTransform.GetChild(2);
            lookTarget.localPosition += camOffset;
            viewCam.Follow = viewCam.LookAt = lookTarget;
            CinemachineFramingTransposer transposer = transposers[i] = viewCam.GetCinemachineComponent<CinemachineFramingTransposer>();
            CinemachineSameAsFollowTarget camAim = camAims[i] = viewCam.GetCinemachineComponent<CinemachineSameAsFollowTarget>();
            transposer.m_XDamping = transposer.m_YDamping = transposer.m_ZDamping = camAim.m_Damping = damping;
            view.rect = new Rect(cameraRects[i], scaleVector);
        }

        GetComponent<AudioListener>().enabled = true;
        eventSystem.SetActive(true);
        GC.Collect();

        //Start countdown
        while(initializedObjects < playerNumber)
        {
            yield return null;
        }
        yield return null;
        finished = false;

        //Audio setup
        allAudio = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
        audioCount = allAudio.Length;
        pauseButton.enabled = true;

        backgroundNoise.Play();
        backgroundMusic.Play();
        int countdownTime = 3;
        while(countdownTime > 0)
        {
            audioSource.PlayOneShot(countdownSound);
            yield return oneSecond;
            countdownTime--;
        }
        audioSource.PlayOneShot(goSound);
        audioSource.PlayOneShot(goVoices[UnityEngine.Random.Range(0, playerNumber)], voiceScale);
        yield return oneSecond;
        InputSystem.actions.Disable();
        InputSystem.actions.Enable();
        paused = PlayerController.paused = false;
    }
    void Update()
    {
        if(finished) return;
        if(pauseAction.WasPressedThisFrame() && Time.timeScale == 1) Pause();

        if(paused) return;
        //Update time
        milliseconds += Time.deltaTime * 1000;
        if(milliseconds >= 1000)
        {
            milliseconds = 0;
            seconds ++;
            if(seconds >= 60)
            {
                seconds = 0;
                minutes ++;
            }
        }
        timeString = "Time: " + minutes.ToString() + ":" + seconds.ToString("00") + "." + milliseconds.ToString("000");
    }

    public void Pause()
    {
        if(finished) return;
        paused = PlayerController.paused = true;
        menu.enabled = true;
        if(cursorHide) Cursor.visible = true;
        for(int i = 0; i < audioCount; i++)
        {
            allAudio[i].Pause();
        }
        audioSource.PlayOneShot(pauseSound);
        Time.timeScale = 0;
        GC.Collect();
    }

    public void Resume()
    {
        menu.enabled = false;
        if(cursorHide) Cursor.visible = false;
        Time.timeScale = 1;
        for(int i = 0; i < audioCount; i++)
        {
            allAudio[i].UnPause();
        }
        audioSource.PlayOneShot(pauseSound);
        paused = PlayerController.paused = false;
    }

    public void RacePlay()
    {
        audioSource.PlayOneShot(pauseSound);
        StartCoroutine(LoadScene(1));
    }

    public void Exit()
    {
        audioSource.PlayOneShot(pauseSound);
        MenuManager.fromGame = true;
        StartCoroutine(LoadScene(0));
    }

    IEnumerator LoadScene(int index)
    {
        loading.enabled = true;
        yield return oneRealSecond;
        paused = PlayerController.paused = false;
        Time.timeScale = 1;
        SceneManager.LoadSceneAsync(index);
    }

    void SetSection(int sectionIndex)
    {
        Transform parent = sectionPool[sectionIndex].transform;
        GameObject[] children = childrenList[sectionIndex] = new GameObject[interactableNumber];
        Rigidbody[] rbs = rbList[sectionIndex] = new Rigidbody[asteroidNumber];
        GameObject interactable;

        switch(sectionIndex)
        {
            case 0:
                parent.gameObject.SetActive(true);

                for(int i = 0; i < asteroidNumber; i++)
                {
                    //Random position, avoid center
                    int randomX = UnityEngine.Random.Range(500, side);
                    int randomY = UnityEngine.Random.Range(500, side);
                    int randomZ = UnityEngine.Random.Range(500, side);
                    if(UnityEngine.Random.Range(0, 2) == 0)
                    {
                        randomX *= -1;
                    }
                    if(UnityEngine.Random.Range(0, 2) == 0)
                    {
                        randomY *= -1;
                    }
                    if(UnityEngine.Random.Range(0, 2) == 0)
                    {
                        randomZ *= -1;
                    }

                    //Spawn random asteroid
                    int asteroidIndex = UnityEngine.Random.Range(0, asteroidCount);
                    interactable = children[i] = Instantiate(possibleAsteroids[asteroidIndex], new Vector3(randomX, randomY, randomZ), Quaternion.Euler(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360)), parent);

                    //Random scale
                    float newScale = UnityEngine.Random.Range(minAsteroidScale, maxAsteroidScale);
                    interactable.transform.localScale = new Vector3(UnityEngine.Random.Range(minAsteroidScale, maxAsteroidScale), UnityEngine.Random.Range(minAsteroidScale, maxAsteroidScale), UnityEngine.Random.Range(minAsteroidScale, maxAsteroidScale));
                    Rigidbody rb = rbs[i] = interactable.GetComponent<Rigidbody>();
                    rb.mass = newScale * 2;

                    //Variable interactable amount
                    if(i < baseDisableNumber)
                    {
                        interactable.SetActive(false);
                        continue;
                    }

                    //Random force
                    rb.AddRelativeForce(RandomForce(), ForceMode.VelocityChange);
                    rb.AddRelativeTorque(UnityEngine.Random.Range(-maxRotationForce, maxRotationForce), UnityEngine.Random.Range(-maxRotationForce, maxRotationForce), UnityEngine.Random.Range(-maxRotationForce, maxRotationForce), ForceMode.VelocityChange);
                }
            
                for(int i = asteroidNumber; i < interactableNumber; i++)
                {
                    //Random position, avoid center
                    int randomX = UnityEngine.Random.Range(500, side);
                    int randomY = UnityEngine.Random.Range(500, side);
                    int randomZ = UnityEngine.Random.Range(500, side);
                    if(UnityEngine.Random.Range(0, 2) == 0)
                    {
                        randomX *= -1;
                    }
                    if(UnityEngine.Random.Range(0, 2) == 0)
                    {
                        randomY *= -1;
                    }
                    if(UnityEngine.Random.Range(0, 2) == 0)
                    {
                        randomZ *= -1;
                    }

                    float interactableType = UnityEngine.Random.Range(0f, 100);
                    if(interactableType < wormholePercent)
                    {
                        //Spawn wormhole
                        interactable = Instantiate(wormholePrefab, new Vector3(randomX, randomY, randomZ), Quaternion.Euler(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360)), parent);
                        interactable.transform.localScale = wormholeProportions * UnityEngine.Random.Range(minWormholeScale, maxWormholeScale);
                    }
                    else
                    {
                        //Spawn random collectable
                        int collectableIndex = UnityEngine.Random.Range(0, collectableCount);
                        interactable = Instantiate(possibleCollectables[collectableIndex], new Vector3(randomX, randomY, randomZ), Quaternion.Euler(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360)), parent);
                        interactable.GetComponent<Collectable>().rotation = new Vector3(UnityEngine.Random.Range(-maxRotationSpeed, maxRotationSpeed), UnityEngine.Random.Range(-maxRotationSpeed, maxRotationSpeed), UnityEngine.Random.Range(-maxRotationSpeed, maxRotationSpeed));
                        
                        //Break down satellites
                        GameObject[] parts = GetDeepestChildren(interactable.transform);
                        int partNumber = parts.Length;
                        int disabledParts = 1;
                        if(partNumber > 2)
                        {
                            for(int j = 0; j < partNumber; j++)
                            {
                                if(UnityEngine.Random.Range(0, 100) < partDisablePercent && disabledParts != partNumber)
                                {
                                    Destroy(parts[j]);
                                    disabledParts ++;
                                }
                            }
                        }
                    }
                    children[i] = interactable;
                }
                break;
            
            case < 27:
                parent.gameObject.SetActive(true);

                for(int i = 0; i < asteroidNumber; i++)
                {
                    //Spawn random asteroid
                    int asteroidIndex = UnityEngine.Random.Range(0, asteroidCount);
                    interactable = children[i] = Instantiate(possibleAsteroids[asteroidIndex], Vector3.zero, Quaternion.Euler(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360)), parent);

                    //Random position and scale
                    interactable.transform.localPosition = new Vector3(UnityEngine.Random.Range(-side, side), UnityEngine.Random.Range(-side, side), UnityEngine.Random.Range(-side, side));
                    int newScale = UnityEngine.Random.Range(minAsteroidScale, maxAsteroidScale);
                    interactable.transform.localScale = new Vector3(UnityEngine.Random.Range(minAsteroidScale, maxAsteroidScale), UnityEngine.Random.Range(minAsteroidScale, maxAsteroidScale), UnityEngine.Random.Range(minAsteroidScale, maxAsteroidScale));
                    Rigidbody rb = rbs[i] = interactable.GetComponent<Rigidbody>();
                    rb.mass = newScale * 2;

                    //Variable interactable amount
                    if(i < baseDisableNumber)
                    {
                        interactable.SetActive(false);
                        continue;
                    }

                    //Random force
                    rb.AddRelativeForce(RandomForce(), ForceMode.VelocityChange);
                    rb.AddRelativeTorque(UnityEngine.Random.Range(-maxRotationForce, maxRotationForce), UnityEngine.Random.Range(-maxRotationForce, maxRotationForce), UnityEngine.Random.Range(-maxRotationForce, maxRotationForce), ForceMode.VelocityChange);
                }

                for(int i = asteroidNumber; i < interactableNumber; i++)
                {
                    float interactableType = UnityEngine.Random.Range(0f, 100);
                    if(interactableType < wormholePercent)
                    {
                        //Spawn wormhole
                        interactable = Instantiate(wormholePrefab, Vector3.zero, Quaternion.Euler(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360)), parent);
                        interactable.transform.localScale = wormholeProportions * UnityEngine.Random.Range(minWormholeScale, maxWormholeScale);
                    }
                    else
                    {
                        //Spawn random collectable
                        int collectableIndex = UnityEngine.Random.Range(0, collectableCount);
                        interactable = Instantiate(possibleCollectables[collectableIndex], Vector3.zero, Quaternion.Euler(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360)), parent);
                        interactable.GetComponent<Collectable>().rotation = new Vector3(UnityEngine.Random.Range(-maxRotationSpeed, maxRotationSpeed), UnityEngine.Random.Range(-maxRotationSpeed, maxRotationSpeed), UnityEngine.Random.Range(-maxRotationSpeed, maxRotationSpeed));
                        
                        //Break down satellites
                        GameObject[] parts = GetDeepestChildren(interactable.transform);
                        int partNumber = parts.Length;
                        int disabledParts = 1;
                        if(partNumber > 1)
                        {
                            for(int j = 0; j < partNumber; j++)
                            {
                                if(UnityEngine.Random.Range(0, 100) < partDisablePercent && disabledParts != partNumber)
                                {
                                    Destroy(parts[j]);
                                    disabledParts ++;
                                }
                            }
                        }
                    }

                    interactable.transform.localPosition = new Vector3(UnityEngine.Random.Range(-side, side), UnityEngine.Random.Range(-side, side), UnityEngine.Random.Range(-side, side));
                    children[i] = interactable;
                }
                break;
            
            default:
                for(int i = 0; i < asteroidNumber; i++)
                {
                    //Spawn random asteroid
                    int asteroidIndex = UnityEngine.Random.Range(0, asteroidCount);
                    interactable = children[i] = Instantiate(possibleAsteroids[asteroidIndex], Vector3.zero, Quaternion.Euler(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360)), parent);
                    interactable.SetActive(false);

                    //Random scale
                    int newScale = UnityEngine.Random.Range(minAsteroidScale, maxAsteroidScale);
                    interactable.transform.localScale = new Vector3(UnityEngine.Random.Range(minAsteroidScale, maxAsteroidScale), UnityEngine.Random.Range(minAsteroidScale, maxAsteroidScale), UnityEngine.Random.Range(minAsteroidScale, maxAsteroidScale));
                    Rigidbody rb = rbs[i] = interactable.GetComponent<Rigidbody>();
                    rb.mass = newScale * 2;
                }

                for(int i = asteroidNumber; i < interactableNumber; i++)
                {
                    float interactableType = UnityEngine.Random.Range(0f, 100);
                    if(interactableType < wormholePercent)
                    {
                        //Spawn wormhole
                        interactable = Instantiate(wormholePrefab, Vector3.zero, Quaternion.Euler(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360)), parent);
                        interactable.transform.localScale = wormholeProportions * UnityEngine.Random.Range(minWormholeScale, maxWormholeScale);
                        interactable.SetActive(false);
                    }
                    else
                    {
                        //Spawn random collectable
                        int collectableIndex = UnityEngine.Random.Range(0, collectableCount);
                        interactable = Instantiate(possibleCollectables[collectableIndex], Vector3.zero, Quaternion.Euler(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360)), parent);
                        interactable.GetComponent<Collectable>().rotation = new Vector3(UnityEngine.Random.Range(-maxRotationSpeed, maxRotationSpeed), UnityEngine.Random.Range(-maxRotationSpeed, maxRotationSpeed), UnityEngine.Random.Range(-maxRotationSpeed, maxRotationSpeed));
                        interactable.SetActive(false);
                        
                        //Break down satellites
                        GameObject[] parts = GetDeepestChildren(interactable.transform);
                        int partNumber = parts.Length;
                        int disabledParts = 1;
                        if(partNumber > 1)
                        {
                            for(int j = 0; j < partNumber; j++)
                            {
                                if(UnityEngine.Random.Range(0, 100) < partDisablePercent && disabledParts != partNumber)
                                {
                                    Destroy(parts[j]);
                                    disabledParts ++;
                                }
                            }
                        }
                    }
                    children[i] = interactable;
                }
                break;

        }

    }

    GameObject[] GetDeepestChildren(Transform parent)
    {
        List<GameObject> deepestChildren = new List<GameObject>();
        foreach(Transform child in parent)
        {
            if(child.name == "Collider") continue;
            if (child.childCount == 0)
            {
                //Child is deepest
                deepestChildren.Add(child.gameObject);
            }
            else
            {
                //Find deepest in child
                deepestChildren.AddRange(GetDeepestChildren(child));
            }
        }
        return deepestChildren.ToArray();
    }
    
    //Section spawning
    public void SpawnSectionsForward(int playerIndex)
    {
        Vector3[] newPosList = new Vector3[9];
        //Vector3 playerChunkPos = colliderPos + new Vector3(0, 0, side);
        playerChunks[playerIndex] += Vector3.forward;
        Vector3 playerChunkPos = playerChunks[playerIndex] * length;
        newPosList[0] = new Vector3(playerChunkPos.x, playerChunkPos.y, playerChunkPos.z + length);
        newPosList[1] = newPosList[0] + new Vector3(length, 0, 0);
        newPosList[2] = newPosList[0] + new Vector3(-length, 0, 0);
        newPosList[3] = newPosList[0] + new Vector3(0, length, 0);
        newPosList[4] = newPosList[0] + new Vector3(length, length, 0);
        newPosList[5] = newPosList[0] + new Vector3(-length, length, 0);
        newPosList[6] = newPosList[0] + new Vector3(0, -length, 0);
        newPosList[7] = newPosList[0] + new Vector3(length, -length, 0);
        newPosList[8] = newPosList[0] + new Vector3(-length, -length, 0);

        SpawnSections(newPosList, playerIndex);
    }
    public void SpawnSectionsBack(int playerIndex)
    {
        Vector3[] newPosList = new Vector3[9];
        //Vector3 playerChunkPos = colliderPos + new Vector3(0, 0, -side);
        playerChunks[playerIndex] += Vector3.back;
        Vector3 playerChunkPos = playerChunks[playerIndex] * length;
        newPosList[0] = new Vector3(playerChunkPos.x, playerChunkPos.y, playerChunkPos.z - length);
        newPosList[1] = newPosList[0] + new Vector3(length, 0, 0);
        newPosList[2] = newPosList[0] + new Vector3(-length, 0, 0);
        newPosList[3] = newPosList[0] + new Vector3(0, length, 0);
        newPosList[4] = newPosList[0] + new Vector3(length, length, 0);
        newPosList[5] = newPosList[0] + new Vector3(-length, length, 0);
        newPosList[6] = newPosList[0] + new Vector3(0, -length, 0);
        newPosList[7] = newPosList[0] + new Vector3(length, -length, 0);
        newPosList[8] = newPosList[0] + new Vector3(-length, -length, 0);

        SpawnSections(newPosList, playerIndex);
    }
    public void SpawnSectionsRight(int playerIndex)
    {
        Vector3[] newPosList = new Vector3[9];
        //Vector3 playerChunkPos = colliderPos + new Vector3(side, 0, 0);
        playerChunks[playerIndex] += Vector3.right;
        Vector3 playerChunkPos = playerChunks[playerIndex] * length;
        newPosList[0] = new Vector3(playerChunkPos.x + length, playerChunkPos.y, playerChunkPos.z);
        newPosList[1] = newPosList[0] + new Vector3(0, 0, length);
        newPosList[2] = newPosList[0] + new Vector3(0, 0, -length);
        newPosList[3] = newPosList[0] + new Vector3(0, length, 0);
        newPosList[4] = newPosList[0] + new Vector3(0, length, length);
        newPosList[5] = newPosList[0] + new Vector3(0, length, -length);
        newPosList[6] = newPosList[0] + new Vector3(0, -length, 0);
        newPosList[7] = newPosList[0] + new Vector3(0, -length, length);
        newPosList[8] = newPosList[0] + new Vector3(0, -length, -length);

        SpawnSections(newPosList, playerIndex);
    }
    public void SpawnSectionsLeft(int playerIndex)
    {
        Vector3[] newPosList = new Vector3[9];
        //Vector3 playerChunkPos = colliderPos + new Vector3(-side, 0, 0);
        playerChunks[playerIndex] += Vector3.left;
        Vector3 playerChunkPos = playerChunks[playerIndex] * length;
        newPosList[0] = new Vector3(playerChunkPos.x - length, playerChunkPos.y, playerChunkPos.z);
        newPosList[1] = newPosList[0] + new Vector3(0, 0, length);
        newPosList[2] = newPosList[0] + new Vector3(0, 0, -length);
        newPosList[3] = newPosList[0] + new Vector3(0, length, 0);
        newPosList[4] = newPosList[0] + new Vector3(0, length, length);
        newPosList[5] = newPosList[0] + new Vector3(0, length, -length);
        newPosList[6] = newPosList[0] + new Vector3(0, -length, 0);
        newPosList[7] = newPosList[0] + new Vector3(0, -length, length);
        newPosList[8] = newPosList[0] + new Vector3(0, -length, -length);

        SpawnSections(newPosList, playerIndex);
    }
    public void SpawnSectionsUp(int playerIndex)
    {
        Vector3[] newPosList = new Vector3[9];
        //Vector3 playerChunkPos = colliderPos + new Vector3(0, side, 0);
        playerChunks[playerIndex] += Vector3.up;
        Vector3 playerChunkPos = playerChunks[playerIndex] * length;
        newPosList[0] = new Vector3(playerChunkPos.x, playerChunkPos.y + length, playerChunkPos.z);
        newPosList[1] = newPosList[0] + new Vector3(length, 0, 0);
        newPosList[2] = newPosList[0] + new Vector3(-length, 0, 0);
        newPosList[3] = newPosList[0] + new Vector3(0, 0, length);
        newPosList[4] = newPosList[0] + new Vector3(length, 0, length);                
        newPosList[5] = newPosList[0] + new Vector3(-length, 0, length);
        newPosList[6] = newPosList[0] + new Vector3(0, 0, -length);
        newPosList[7] = newPosList[0] + new Vector3(length, 0, -length);                
        newPosList[8] = newPosList[0] + new Vector3(-length, 0, -length);

        SpawnSections(newPosList, playerIndex);
    }
    public void SpawnSectionsDown(int playerIndex)
    {
        Vector3[] newPosList = new Vector3[9];
        //Vector3 playerChunkPos = colliderPos + new Vector3(0, -side, 0);
        playerChunks[playerIndex] += Vector3.down;
        Vector3 playerChunkPos = playerChunks[playerIndex] * length;
        newPosList[0] = new Vector3(playerChunkPos.x, playerChunkPos.y - length, playerChunkPos.z);
        newPosList[1] = newPosList[0] + new Vector3(length, 0, 0);
        newPosList[2] = newPosList[0] + new Vector3(-length, 0, 0);
        newPosList[3] = newPosList[0] + new Vector3(0, 0, length);
        newPosList[4] = newPosList[0] + new Vector3(length, 0, length);                
        newPosList[5] = newPosList[0] + new Vector3(-length, 0, length);
        newPosList[6] = newPosList[0] + new Vector3(0, 0, -length);
        newPosList[7] = newPosList[0] + new Vector3(length, 0, -length);                
        newPosList[8] = newPosList[0] + new Vector3(-length, 0, -length);

        SpawnSections(newPosList, playerIndex);
    }

    public void SpawnSectionsAround(Vector3 playerChunk, int playerIndex)
    {
        playerChunks[playerIndex] = playerChunk;
        Vector3 playerChunkPos = playerChunk * length;
        Vector3[] newPosList = new Vector3[27];

        newPosList[0] = playerChunkPos;
        newPosList[1] = playerChunkPos + new Vector3(length, 0, 0);
        newPosList[2] = playerChunkPos + new Vector3(-length, 0, 0);
        newPosList[3] = playerChunkPos + new Vector3(0, 0, length);
        newPosList[4] = playerChunkPos + new Vector3(length, 0, length);                
        newPosList[5] = playerChunkPos + new Vector3(-length, 0, length);
        newPosList[6] = playerChunkPos + new Vector3(0, 0, -length);
        newPosList[7] = playerChunkPos + new Vector3(length, 0, -length);                
        newPosList[8] = playerChunkPos + new Vector3(-length, 0, -length);

        Vector3 baseChunkPos = new Vector3(playerChunkPos.x, playerChunkPos.y + length, playerChunkPos.z);
        newPosList[9] = baseChunkPos;
        newPosList[10] = baseChunkPos + new Vector3(length, 0, 0);
        newPosList[11] = baseChunkPos + new Vector3(-length, 0, 0);
        newPosList[12] = baseChunkPos + new Vector3(0, 0, length);
        newPosList[13] = baseChunkPos + new Vector3(length, 0, length);                
        newPosList[14] = baseChunkPos + new Vector3(-length, 0, length);
        newPosList[15] = baseChunkPos + new Vector3(0, 0, -length);
        newPosList[16] = baseChunkPos + new Vector3(length, 0, -length);                
        newPosList[17] = baseChunkPos + new Vector3(-length, 0, -length);
        
        baseChunkPos = new Vector3(playerChunkPos.x, playerChunkPos.y - length, playerChunkPos.z);
        newPosList[18] = baseChunkPos;
        newPosList[19] = baseChunkPos + new Vector3(length, 0, 0);
        newPosList[20] = baseChunkPos + new Vector3(-length, 0, 0);
        newPosList[21] = baseChunkPos + new Vector3(0, 0, length);
        newPosList[22] = baseChunkPos + new Vector3(length, 0, length);                
        newPosList[23] = baseChunkPos + new Vector3(-length, 0, length);
        newPosList[24] = baseChunkPos + new Vector3(0, 0, -length);
        newPosList[25] = baseChunkPos + new Vector3(length, 0, -length);                
        newPosList[26] = baseChunkPos + new Vector3(-length, 0, -length);
        
        SpawnSections(newPosList, playerIndex, 27);
    }
    
    void SpawnSections(Vector3[] newPosList, int playerIndex, int newPosCount = 9)
    {
        int stage = 0;
        int disableNumber = disableByCheckpoint[playerCheckpoints[playerIndex]];
        for(int i = 0; i < sectionNumber; i++)
        {
            GameObject section = sectionPool[i];

            //Section is unused
            if(CanDestroySection(section))
            {
                //Section should be reused
                if(stage < newPosCount)
                {
                    bool valid = true;
                    Vector3 newPos = newPosList[stage];

                    //Check if a section is already there
                    Vector3 newChunk = newPos / length;
                    for(int j = 0; j < sectionNumber; j++)
                    {
                        if(newChunk == sectionChunks[j] && sectionPool[j].activeSelf)
                        {
                            valid = false;
                            break;
                        }
                    }
                    if(valid)
                    {
                        section.transform.position = newPos;
                        sectionChunks[i] = newChunk;

                        //Randomize section
                        GameObject[] children = childrenList[i];
                        Rigidbody[] rbs = rbList[i];
                        for(int j = 0; j < asteroidNumber; j++)
                        {
                            GameObject child = children[j];

                            //Variable interactable amount
                            if(j < disableNumber)
                            {
                                continue;
                            }

                            //Random position and force
                            Vector3 newPosition = new Vector3(UnityEngine.Random.Range(-side, side), UnityEngine.Random.Range(-side, side), UnityEngine.Random.Range(-side, side));
                            if((newPos + newPosition).sqrMagnitude < lastCheckpointDistance)
                            {
                                child.transform.localPosition = newPosition;
                                if(!child.activeSelf) child.SetActive(true);
                                Rigidbody rb = rbs[j];
                                rb.velocity = rb.angularVelocity = Vector3.zero;
                                rb.AddRelativeForce(RandomForce(), ForceMode.VelocityChange);
                                rb.AddRelativeTorque(UnityEngine.Random.Range(-maxRotationForce, maxRotationForce), UnityEngine.Random.Range(-maxRotationForce, maxRotationForce), UnityEngine.Random.Range(-maxRotationForce, maxRotationForce), ForceMode.VelocityChange);
                            }
                            else if(child.activeSelf) child.SetActive(false);
                        }
                        for(int j = asteroidNumber; j < interactableNumber; j++)
                        {
                            //Random position
                            GameObject child = children[j];
                            Vector3 newPosition = new Vector3(UnityEngine.Random.Range(-side, side), UnityEngine.Random.Range(-side, side), UnityEngine.Random.Range(-side, side));
                            if((newPos + newPosition).sqrMagnitude < lastCheckpointDistance)
                            {
                                child.transform.localPosition = newPosition;
                                if(!child.activeSelf) child.SetActive(true);
                            }
                            else if(child.activeSelf) child.SetActive(false);
                        }
                    }
                    else i--;
                    stage++;
                }
                
                //Deactivate unused section
                else StartCoroutine(DeactivateSection(i));
            }

            //Section is inactive - activate to reuse
            else if(!section.activeSelf && stage < newPosCount)
            {
                bool valid = true;
                Vector3 newPos = newPosList[stage];

                //Check if a section is already there
                Vector3 newChunk = newPos / length;
                for(int j = 0; j < sectionNumber; j++)
                {
                    if(newChunk == sectionChunks[j])
                    {
                        if(!sectionPool[j].activeSelf)
                        {
                            StartCoroutine(ActivateSection(j, disableNumber));
                        }
                        valid = false;
                        break;
                    }
                }
                if(valid)
                {
                    section.transform.position = newPos;
                    sectionChunks[i] = newChunk;
                    StartCoroutine(ActivateSection(i, disableNumber));
                }
                else i--;
                stage++;
            }
        }
    }

    bool CanDestroySection(GameObject section)
    {
        if(section.activeSelf)
        {
            Vector3 sectionChunk = section.transform.position / length;
            foreach(int j in playingPlayers)
            {
                Vector3 playerChunk = playerChunks[j];
                if(playerChunk.x >= sectionChunk.x - 1 && playerChunk.x <= sectionChunk.x + 1 && playerChunk.z >= sectionChunk.z - 1 && playerChunk.z <= sectionChunk.z + 1 && playerChunk.y >= sectionChunk.y - 1 && playerChunk.y <= sectionChunk.y + 1)
                {
                    return false;
                }
            }
            return true;
        }
        return false;
    }

    void DestroySection(int sectionIndex)
    {
        GameObject section = sectionPool[sectionIndex];
        if(section.activeSelf)
        {
            bool valid = true;
            Vector3 sectionChunk = section.transform.position / length;
            foreach(int j in playingPlayers)
            {
                Vector3 playerChunk = playerChunks[j];
                if(playerChunk.x >= sectionChunk.x - 1 && playerChunk.x <= sectionChunk.x + 1 && playerChunk.z >= sectionChunk.z - 1 && playerChunk.z <= sectionChunk.z + 1 && playerChunk.y >= sectionChunk.y - 1 && playerChunk.y <= sectionChunk.y + 1)
                {
                    valid = false;
                    break;
                }
            }
            if(valid)
            {
                StartCoroutine(DeactivateSection(sectionIndex));
            }
        }
    }

    public void PlayerFinish(int playerIndex)
    {
        playingPlayers.Remove(playerIndex);
        for(int i = 0; i < sectionNumber; i++)
        {
            DestroySection(i);
        }
        List<Transform> playerFollowers = followers[playerIndex];
        foreach(Transform playerFollower in playerFollowers) playerFollower.gameObject.SetActive(false);
        views[playerIndex].cullingMask = finishMasks[playerIndex];
        thirdPersons[playerIndex].m_Lens.FieldOfView = 35;
        audioSource.panStereo = stereoPans[playerIndex];
        int playersLeft = playingPlayers.Count;
        switch(playerNumber - playersLeft)
        {
            case 1:
                finishTexts[playerIndex].SetText("1st Place");
                audioSource.PlayOneShot(firstVoices[playerIndex], voiceScale);
                break;
            case 2:
                finishTexts[playerIndex].SetText("2nd Place");
                audioSource.PlayOneShot(secondVoices[playerIndex], voiceScale);
                break;
            case 3:
                finishTexts[playerIndex].SetText("3rd Place");
                audioSource.PlayOneShot(thirdVoices[playerIndex], voiceScale);
                break;
            default:
                finishTexts[playerIndex].SetText("4th Place");
                audioSource.PlayOneShot(fourthVoices[playerIndex], voiceScale);
                break;
        }
        audioSource.panStereo = 0;

        //Game over
        if(playersLeft == 0)
        {
            finished = true;
            pauseButton.enabled = false;
            MenuManager.races++;
            PlayerPrefs.SetInt("Races", MenuManager.races);
            StartCoroutine(RaceFinish());
        }
        audioSource.PlayOneShot(goSound);
        audioSource.PlayOneShot(finishSound);
        StartCoroutine(FinishCoroutine(confettis[playerIndex]));
    }

    IEnumerator FinishCoroutine(GameObject confetti)
    {
        yield return oneSecond;
        confetti.SetActive(true);
        for(int i = 0; i < 3; i++)
        {
            audioSource.PlayOneShot(confettiSound, 0.7f);
            yield return oneAndHalfSeconds;
        };
        confetti.SetActive(false);
    }

    IEnumerator RaceFinish()
    {
        yield return fiveSeconds;
        if(menu.enabled) menu.enabled = false;
        raceFinish.enabled = true;
        if(cursorHide) Cursor.visible = true;
        audioSource.PlayOneShot(pauseSound);
    }

    IEnumerator ActivateSection(int sectionIndex, int disableNumber)
    {
        GameObject[] children = childrenList[sectionIndex];
        Rigidbody[] rbs = rbList[sectionIndex];
        GameObject section = sectionPool[sectionIndex];
        section.SetActive(true);
        Vector3 sectionPos = section.transform.position;
        for(int i = 0; i < asteroidNumber; i++)
        {
            GameObject child = children[i];

            //Variable interactable amount
            if(i < disableNumber)
            {
                continue;
            }

            //Random position and force
            Vector3 newPos = new Vector3(UnityEngine.Random.Range(-side, side), UnityEngine.Random.Range(-side, side), UnityEngine.Random.Range(-side, side));
            if((sectionPos + newPos).sqrMagnitude < lastCheckpointDistance)
            {
                child.transform.localPosition = newPos;
                child.SetActive(true);
                Rigidbody rb = rbs[i];
                rb.velocity = rb.angularVelocity = Vector3.zero;
                rb.AddRelativeForce(RandomForce(), ForceMode.VelocityChange);
                rb.AddRelativeTorque(UnityEngine.Random.Range(-maxRotationForce, maxRotationForce), UnityEngine.Random.Range(-maxRotationForce, maxRotationForce), UnityEngine.Random.Range(-maxRotationForce, maxRotationForce), ForceMode.VelocityChange);
            }

            if(i == 20 || i == 40 || i == 60 || i == 80)
            {
                yield return null;
            }
        }
        for(int i = asteroidNumber; i < interactableNumber; i++)
        {
            //Random position
            GameObject child = children[i];
            Vector3 newPos = new Vector3(UnityEngine.Random.Range(-side, side), UnityEngine.Random.Range(-side, side), UnityEngine.Random.Range(-side, side));
            if((sectionPos + newPos).sqrMagnitude < lastCheckpointDistance)
            {
                child.transform.localPosition = newPos;
                child.SetActive(true);
            }
        }
    }

    IEnumerator DeactivateSection(int sectionIndex)
    {
        GameObject[] children = childrenList[sectionIndex];
        for(int i = 0; i < interactableNumber; i++)
        {
            if(children[i].activeSelf) children[i].SetActive(false);
            if(i == 20 || i == 40 || i == 60) yield return null;
        }
        sectionPool[sectionIndex].SetActive(false);
    }

    Vector3 RandomForce()
    {
        float maxMovementForce;
        if(UnityEngine.Random.Range(0, 100) < movementForce1Percent) maxMovementForce = maxMovementForce1;
        else maxMovementForce = maxMovementForce2;
        return new Vector3(UnityEngine.Random.Range(-maxMovementForce, maxMovementForce), UnityEngine.Random.Range(-maxMovementForce, maxMovementForce), UnityEngine.Random.Range(-maxMovementForce, maxMovementForce));
    }

    
    //Settings functions
    public void Settings()
    {
        string rebinds = PlayerPrefs.GetString("Bindings");
        if(!string.IsNullOrEmpty(rebinds)) InputSystem.actions.LoadBindingOverridesFromJson(rebinds);
        activePlayer = 0;
        for(int i = 1; i < 4; i++)
        {
            controls[i].SetActive(false);
        }
        controls[0].SetActive(true);
        activeSetting = 0;
        settingWindows[0].SetActive(true);
        settingWindows[1].SetActive(false);
        menu.enabled = false;
        settings.enabled = true;
        MajorClick();
    }

    public void ExitSettings()
    {
        settings.enabled = false;
        menu.enabled = true;
        MajorClick();
    }

    public void NextSetting(int increment)
    {
        activeSetting += increment;
        if(activeSetting > 1) activeSetting = 0;
        if(activeSetting < 0) activeSetting = 1;
        if(activeSetting == 0)
        {
            string rebinds = PlayerPrefs.GetString("Bindings");
            if(!string.IsNullOrEmpty(rebinds)) InputSystem.actions.LoadBindingOverridesFromJson(rebinds);
        }
        for(int i = 0; i < 2; i++) settingWindows[i].SetActive(false);
        settingWindows[activeSetting].SetActive(true);
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

    public void ChangeFOV()
    {
        float fov = fovSlider.value;
        foreach(int i in playingPlayers) thirdPersons[i].m_Lens.FieldOfView = fov;
        fovText.SetText(fov.ToString("0"));
        PlayerPrefs.SetFloat("Fov", fov);
    }

    public void ChangeDepth()
    {
        float depth = depthSlider.value;
        foreach(int i in playingPlayers) thirdPersons[i].m_Lens.FarClipPlane = depth;
        depthText.SetText(depth.ToString("0"));
        PlayerPrefs.SetFloat("Depth", depth);
    }

    public void ChangeDamping()
    {
        damping = dampingSlider.value;
        foreach(int i in playingPlayers)
        {
            CinemachineFramingTransposer transposer = transposers[i];
            transposer.m_XDamping = transposer.m_YDamping = transposer.m_ZDamping = camAims[i].m_Damping = damping;
        }
        dampingText.SetText(damping.ToString("0.00"));
        PlayerPrefs.SetFloat("Damping", damping);
    }

    public void ChangeMusic()
    {
        float music = musicSlider.value;
        float bg = backgroundMusic.volume = music * 0.5f;
        if(bg < 0.1f) backgroundNoise.volume = bg;
        musicText.SetText(music.ToString("0.00"));
        PlayerPrefs.SetFloat("Music", music);
    }

    public void ChangeVoice()
    {
        float voice = voiceSlider.value;
        foreach(int i in playingPlayers) voiceSources[i].volume = voice;
        voiceText.SetText(voice.ToString("0.00"));
        PlayerPrefs.SetFloat("Voice", voice);
    }

    public void ChangeThruster()
    {
        float thruster = PlayerController.maxThrusterVolume = thrusterSlider.value;
        thrusterText.SetText(thruster.ToString("0.00"));
        PlayerPrefs.SetFloat("Thruster", thruster);
    }

    public void ChangeEffects()
    {
        float effects = effectsSlider.value;
        foreach(int i in playingPlayers) effectSources[i].volume = effects;
        if(effects * 2 > 1) PlayerController.warningScale = 1 / effects;
        else PlayerController.warningScale = 2;
        effectsText.SetText(effects.ToString("0.00"));
        PlayerPrefs.SetFloat("Effects", effects);
    }

    public void ChangeCaptions()
    {
        bool showCaptions = PlayerController.showCaptions = Drone.showCaptions = captionToggle.isOn;
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
        bool showArrow = PlayerController.showArrow = arrowToggle.isOn;
        foreach(int i in playingPlayers) if(!playerControllers[i].kinematic) arrows[i].SetActive(true);
        if(showArrow) PlayerPrefs.SetInt("ShowArrow", 1);
        else PlayerPrefs.SetInt("ShowArrow", 0);
        MinorClick();
    }

    public void ResetSettings()
    {
        finished = true;

        PlayerPrefs.SetFloat("Fov", 40);
        PlayerPrefs.SetFloat("Depth", 7000);
        PlayerPrefs.SetFloat("Damping", 0.1f);
        PlayerPrefs.SetFloat("Music", 0.1f);
        PlayerPrefs.SetInt("ShowCaptions", 1);
        PlayerPrefs.SetInt("CursorHide", 0);
        PlayerPrefs.SetInt("ShowArrow", 1);
        PlayerPrefs.SetFloat("Voice", 0.4f);
        PlayerPrefs.SetFloat("Thruster", 0.2f);
        PlayerPrefs.SetFloat("Effects", 0.4f);

        fovSlider.value = 40;
        fovText.SetText("40");
        depthSlider.value = 7000;
        depthText.SetText("7000");
        dampingSlider.value = 0.1f;
        dampingText.SetText("0.10");
        musicSlider.value = 0.1f;
        musicText.SetText("0.10");
        captionToggle.isOn = arrowToggle.isOn = true;
        cursorToggle.isOn = false;
        voiceSlider.value = 0.4f;
        voiceText.SetText("0.40");
        thrusterSlider.value = 0.2f;
        thrusterText.SetText("0.20");
        effectsSlider.value = 0.4f;
        effectsText.SetText("0.40");

        finished = false;
        MinorClick();
    }

    public void SaveBindings()
    {
        PlayerPrefs.SetString("Bindings", InputSystem.actions.SaveBindingOverridesAsJson());
    }

    public void MajorClick()
    {
        audioSource.PlayOneShot(goSound);
    }

    public void MinorClick()
    {
        if(!finished) audioSource.PlayOneShot(countdownSound);
    }
}
