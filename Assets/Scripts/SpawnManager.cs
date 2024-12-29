using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;
using System;
using UnityEngine.Rendering;

public class SpawnManager : MonoBehaviour
{
    [Header("Sections")]
    [SerializeField] int side = 3000;
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
    public bool raceMode;
    [SerializeField] int difficulty;
    [SerializeField] GameObject[] possibleAsteroids;
    [SerializeField] GameObject[] possibleCollectables;
    int asteroidCount;
    int collectableCount;
    [SerializeField] GameObject wormholePrefab;
    //Asteroid number must be divisible by 24
    [SerializeField] int[] asteroidNumbers = new int[] {24, 48, 72, 96};
    int asteroidNumber;
    [SerializeField] int collectAsteroidDivisor = 2;
    [SerializeField] int minAsteroidScale = 20;
    [SerializeField] int maxAsteroidScale = 200;
    [SerializeField] int[] collectableNumbers = new int[] {10, 8, 6, 4};
    int collectableNumber;
    [SerializeField] int collectCollectableMultiplier = 5;
    [SerializeField] int partDisablePercent = 50;
    [SerializeField] int maxMovementForce = 60;
    [SerializeField] float maxRotationForce = 0.2f;
    [SerializeField] float maxRotationSpeed = 30;
    [SerializeField] int wormholePercent = 5;
    [SerializeField] Vector3 wormholeProportions = new Vector3(0.5f, 0.4f, 0.8f);
    [SerializeField] int minWormholeScale = 1000;
    [SerializeField] int maxWormholeScale = 2000;
    int interactableNumber;

    [Header("Drones")]
    [SerializeField] int[] droneNumbers = new int[] {4, 8, 12, 16};
    int droneNumber;
    [SerializeField] int collectDroneDivisor = 2;
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
    bool finished;
    PlayerController[] playerControllers;

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
    Camera[] views = new Camera[4];
    CinemachineVirtualCamera[] thirdPersons = new CinemachineVirtualCamera[4];
    [SerializeField] AudioClip countdownSound;
    [SerializeField] AudioClip goSound;
    [SerializeField] AudioClip pauseSound;
    [SerializeField] GameObject eventSystem;
    public AudioSource audioSource;
    AudioSource backgroundMusic;
    AudioSource backgroundNoise;
    AudioSource[] allAudio;
    int audioCount;
    [SerializeField] GameObject menu;
    public int initializedObjects;
    bool initialized;
    bool cursorHide;
    InputAction pauseAction;
    WaitForSeconds oneSecond;
    WaitForSeconds twoSeconds;
    WaitForSecondsRealtime oneRealSecond = new WaitForSecondsRealtime(1);
    public TMP_Text[] finishTexts;
    public int milliseconds;
    public int seconds;
    public int minutes;
    public string timeString;
    public bool inCountdown = true;
    
    void Awake()
    {
        menu.SetActive(false);
        length = side * 2;

        //Audio setup
        AudioSource[] audioSources = GetComponents<AudioSource>();
        audioSource = audioSources[0];
        backgroundMusic = audioSources[1];
        backgroundNoise = audioSources[2];

        //Getting variabes from main menu
        if(MenuManager.loaded)
        {
            raceMode = MenuManager.raceMode;
            playerNumber = MenuManager.playerNumber;
            playerShips = MenuManager.playerShips;
            difficulty = MenuManager.difficulty;
            audioSource.volume = PlayerController.audioVolume = MenuManager.effects;
            backgroundMusic.volume = MenuManager.music * 0.5f;
            backgroundNoise.volume = MenuManager.noise;
            cursorHide = MenuManager.cursorHide;
            if(cursorHide)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            CinemachineVirtualCamera view = cameraPrefab.transform.GetChild(0).GetChild(0).GetComponent<CinemachineVirtualCamera>();
            view.m_Lens.FieldOfView = MenuManager.fov;
            view.m_Lens.FarClipPlane = MenuManager.depth;
            PlayerController.maxThrusterVolume = MenuManager.thruster;
            PlayerController.showCaptions = Drone.showCaptions = MenuManager.showCaptions;
            PlayerController.showArrow = MenuManager.showArrow;
        }
        else
        {
            PlayerController.audioVolume = 0.2f;
            PlayerController.maxThrusterVolume = 0.2f;
            PlayerController.showCaptions = Drone.showCaptions = PlayerController.showArrow = true;
        }

        //Setting difficulty-dependent variables
        asteroidNumber = asteroidNumbers[difficulty];
        collectableNumber = collectableNumbers[difficulty];
        droneNumber = droneNumbers[difficulty];
        checkpointDistances = new int[][]{begCheckpointDistances, intCheckpointDistances, advCheckpointDistances, expCheckpointDistances}[difficulty];
        checkpointCount = checkpointDistances.Length;
        if(raceMode)
        {
            checkpointDrones = new int[][]{begCheckpointDrones, intCheckpointDrones, advCheckpointDrones, expCheckpointDrones}[difficulty];

            //Stats for each checkpoint
            baseDisableNumber = asteroidNumber / 3;
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
        }
        else
        {
            asteroidNumber /= collectAsteroidDivisor;
            collectableNumber *= collectCollectableMultiplier;
            droneNumber /= collectDroneDivisor;
            minutes = 1;

            PlayerController.checkpointTexts = new TMP_Text[playerNumber];
            PlayerController.playerNumber = playerNumber;
            PlayerController.creditIncrease = difficulty + 1;
            PlayerController.showArrow = false;
        }
        interactableNumber = asteroidNumber + collectableNumber;
        asteroidCount = possibleAsteroids.Length;
        collectableCount = possibleCollectables.Length;

        //Initialize player variables
        PlayerController.spawnManager = this;
        PlayerController.length = length;
        PlayerController.droneNumber = droneNumber;
        PlayerController.oneSecond = oneSecond = MenuManager.oneSecond;
        PlayerController.twoSeconds = Drone.twoSeconds = twoSeconds = MenuManager.twoSeconds;
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
        if(raceMode)
        {
            for(int i = 0; i < lastCheckpoint; i++)
            {
                Transform checkpoint = Instantiate(checkpointPrefab, checkpointParent).transform;
                newScale = checkpointDistances[i] * 2;
                checkpoint.localScale = new Vector3(newScale, newScale, newScale);
            }
        }
        newScale = checkpointDistances[lastCheckpoint] * 2;
        Transform finish = Instantiate(checkpointPrefab, checkpointParent).transform;
        finish.localScale = new Vector3(newScale, newScale, newScale);
        finish.GetComponent<Renderer>().material = finishMat;
        lastCheckpointDistance = PlayerController.lastCheckpointDistance = checkpointDistances[lastCheckpoint];
        lastCheckpointDistance = lastCheckpointDistance * lastCheckpointDistance;

        //Spawn players
        players = new GameObject[playerNumber];
        playerChunks = new Vector3[playerNumber];
        playerCheckpoints = new int[playerNumber];
        followers = new List<Transform>[playerNumber];
        Transform[] cameraObjects = new Transform[playerNumber];
        finishTexts = new TMP_Text[playerNumber];
        playerControllers = new PlayerController[playerNumber];

        Vector3[] startPos = new Vector3[] {new Vector3(0, 0, 300), new Vector3(0, 0, -300), new Vector3(300, 0, 0), new Vector3(-300, 0, 0)};
        Quaternion[] startRots = new Quaternion[] {Quaternion.identity, Quaternion.Euler(0, 180, 0), Quaternion.Euler(0, 90, 0), Quaternion.Euler(0, -90, 0)};
        Vector2 scaleVector;
        Vector2[] cameraRects;
        string[] tags;
        switch(playerNumber)
        {
            case 1:
                scaleVector = Vector2.one;
                cameraRects = new Vector2[] {Vector2.zero};
                tags = new string[] {"1"};
                break;

            case 2:
                scaleVector = new Vector2(0.5f, 1);
                cameraRects = new Vector2[] {Vector2.zero, new Vector2(0.5f, 0)};
                tags = new string[] {"1", "2"};
                break;

            case 3:
                scaleVector = new Vector2(0.333f, 1);
                cameraRects = new Vector2[] {Vector2.zero, new Vector2(0.333f, 0), new Vector2(0.667f, 0)};
                tags = new string[] {"1", "3", "2"};
                break;
            
            default:
                scaleVector = new Vector2(0.5f, 0.5f);
                cameraRects = new Vector2[] {new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0.5f, 0)};
                tags = new string[] {"1", "4", "3", "2"};
                break;
        }

        for(int i = 0; i < playerNumber; i++)
        {
            playerChunks[i] = Vector3.zero;
            playerCheckpoints[i] = 0;
            playingPlayers.Add(i);
            Transform cameraObject = cameraObjects[i] = Instantiate(cameraPrefab).transform;
            Transform viewObject = cameraObject.GetChild(0);
            Camera view = views[i] = viewObject.GetComponent<Camera>();
            GameObject thirdPersonObject = viewObject.GetChild(0).gameObject;
            thirdPersonObject.layer = i + 9;
            CinemachineVirtualCamera viewCam = thirdPersons[i] = thirdPersonObject.GetComponent<CinemachineVirtualCamera>();
            view.cullingMask = cameraMasks[i];
            followers[i] = new List<Transform>() {cameraObjects[i]};

            Vector3 pos = startPos[i];
            Quaternion rot = startRots[i];
            GameObject player = players[i] = Instantiate(possiblePlayers[playerShips[i]], pos, rot);
            player.tag = tags[i];
            PlayerController playerController = playerControllers[i] = player.GetComponent<PlayerController>();
            playerController.cameraObject = cameraObject;
            playerController.startPos = pos;
            playerController.normalizedStartPos = pos.normalized;
            playerController.startRot = rot;
            viewCam.Follow = viewCam.LookAt = player.transform.GetChild(2);
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

        //Audio setup
        allAudio = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
        List<AudioSource> audioSources = new List<AudioSource>(allAudio);
        audioSources.Remove(audioSource);
        allAudio = audioSources.ToArray();
        audioCount = allAudio.Length;

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
        InputSystem.actions.Disable();
        InputSystem.actions.Enable();
        inCountdown = false;
    }
    void Update()
    {
        if(pauseAction.WasPressedThisFrame() && !finished)
        {
            switch(Time.timeScale)
            {
                case 0:
                    Resume();
                    break;
                case 1:
                    Pause();
                    break;
            }
        }

        //Update time
        if(inCountdown) return;
        if(raceMode)
        {
            milliseconds += (int) (Time.deltaTime * 1000);
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
        }
        else
        {
            milliseconds -= (int) (Time.deltaTime * 1000);
            if(milliseconds < 0)
            {
                milliseconds = 999;
                seconds --;
                if(seconds < 0)
                {
                    seconds = 59;
                    minutes --;
                    if(minutes < 0)
                    {
                        //Game over
                        inCountdown = true;
                        sectionParent.gameObject.SetActive(false);
                        int[] collectsList = new int[playerNumber];
                        int[] sortedList = new int[playerNumber];
                        for(int i = 0; i < playerNumber; i++)
                        {
                            collectsList[i] = sortedList[i] = playerControllers[i].collects;
                        }
                        Array.Sort(sortedList);
                        Array.Reverse(sortedList);
                        for(int i = 0; i < playerNumber; i++)
                        {
                            playerControllers[i].Finish();
                            List<Transform> playerFollowers = followers[i];
                            int playerFollowerNumber = playerFollowers.Count;
                            for(int j = 1; j < playerFollowerNumber; j++)
                            {
                                playerFollowers[j].gameObject.SetActive(false);
                            }
                            switch(Array.IndexOf(sortedList, collectsList[i]))
                            {
                                case 0:
                                    finishTexts[i].SetText("1st Place!");
                                    break;
                                case 1:
                                    finishTexts[i].SetText("2nd Place!");
                                    break;
                                case 2:
                                    finishTexts[i].SetText("3rd Place!");
                                    break;
                                default:
                                    finishTexts[i].SetText("4th Place!");
                                    break;
                            }
                        }
                        audioSource.PlayOneShot(goSound);
                        StartCoroutine(FinishMenu());

                        if(MenuManager.loaded)
                        {
                            int credits = PlayerController.credits;
                            if(MenuManager.credits != credits)
                            {
                                MenuManager.credits = credits;
                                PlayerPrefs.SetInt("Credits", credits);
                            }
                        }
                        return;
                    }
                }
            }
        }
        timeString = "Time: " + minutes + ":" + seconds.ToString("00") + "." + milliseconds.ToString("000");
    }

    public void Pause()
    {
        menu.SetActive(true);
        if(cursorHide)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        audioSource.PlayOneShot(pauseSound);
        for(int i = 0; i < audioCount; i++)
        {
            allAudio[i].Pause();
        }
        StartCoroutine(PauseSounds());
        Time.timeScale = 0;
        GC.Collect();
    }

    IEnumerator PauseSounds()
    {
        yield return oneRealSecond;
        audioSource.Pause();
    }

    public void Resume()
    {
        menu.SetActive(false);
        if(cursorHide)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        Time.timeScale = 1;
        audioSource.UnPause();
        for(int i = 0; i < audioCount; i++)
        {
            allAudio[i].UnPause();
        }
        audioSource.PlayOneShot(pauseSound);
    }

    public void Restart()
    {
        audioSource.PlayOneShot(pauseSound);
        StartCoroutine(LoadScene(1));
    }

    public void Exit()
    {
        audioSource.PlayOneShot(pauseSound);
        StartCoroutine(LoadScene(0));
    }

    IEnumerator LoadScene(int index)
    {
        yield return oneRealSecond;
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
                    float randomX = UnityEngine.Random.Range(500, side);
                    float randomY = UnityEngine.Random.Range(500, side);
                    float randomZ = UnityEngine.Random.Range(500, side);
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
                    interactable = children[i] = Instantiate(possibleAsteroids[asteroidIndex], new Vector3(randomX, randomY, randomZ), Quaternion.Euler(UnityEngine.Random.Range(0f, 360), UnityEngine.Random.Range(0f, 360), UnityEngine.Random.Range(0f, 360)), parent);

                    //Random scale
                    float newScale = UnityEngine.Random.Range(minAsteroidScale, maxAsteroidScale);
                    interactable.transform.localScale = new Vector3(UnityEngine.Random.Range(minAsteroidScale, maxAsteroidScale), UnityEngine.Random.Range(minAsteroidScale, maxAsteroidScale), UnityEngine.Random.Range(minAsteroidScale, maxAsteroidScale));
                    Rigidbody rb = rbs[i] = interactable.GetComponent<Rigidbody>();
                    rb.mass = newScale * 2;

                    //Variable interactable amount
                    if(raceMode && i < baseDisableNumber)
                    {
                        interactable.SetActive(false);
                        continue;
                    }

                    //Random force
                    rb.AddRelativeForce(UnityEngine.Random.Range(-maxMovementForce, maxMovementForce), UnityEngine.Random.Range(-maxMovementForce, maxMovementForce), UnityEngine.Random.Range(-maxMovementForce, maxMovementForce), ForceMode.VelocityChange);
                    rb.AddRelativeTorque(UnityEngine.Random.Range(-maxRotationForce, maxRotationForce), UnityEngine.Random.Range(-maxRotationForce, maxRotationForce), UnityEngine.Random.Range(-maxRotationForce, maxRotationForce), ForceMode.VelocityChange);
                }
            
                for(int i = asteroidNumber; i < interactableNumber; i++)
                {
                    //Random position, avoid center
                    float randomX = UnityEngine.Random.Range(500, side);
                    float randomY = UnityEngine.Random.Range(500, side);
                    float randomZ = UnityEngine.Random.Range(500, side);
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
                        interactable = Instantiate(wormholePrefab, new Vector3(randomX, randomY, randomZ), Quaternion.Euler(UnityEngine.Random.Range(0f, 360), UnityEngine.Random.Range(0f, 360), UnityEngine.Random.Range(0f, 360)), parent);
                        interactable.transform.localScale = wormholeProportions * UnityEngine.Random.Range(minWormholeScale, maxWormholeScale);
                    }
                    else
                    {
                        //Spawn random collectable
                        int collectableIndex = UnityEngine.Random.Range(0, collectableCount);
                        interactable = Instantiate(possibleCollectables[collectableIndex], new Vector3(randomX, randomY, randomZ), Quaternion.Euler(UnityEngine.Random.Range(0f, 360), UnityEngine.Random.Range(0f, 360), UnityEngine.Random.Range(0f, 360)), parent);
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
                    interactable = children[i] = Instantiate(possibleAsteroids[asteroidIndex], Vector3.zero, Quaternion.Euler(UnityEngine.Random.Range(0f, 360), UnityEngine.Random.Range(0f, 360), UnityEngine.Random.Range(0f, 360)), parent);

                    //Random position and scale
                    interactable.transform.localPosition = new Vector3(UnityEngine.Random.Range((float) -side, side), UnityEngine.Random.Range((float) -side, side), UnityEngine.Random.Range((float) -side, side));
                    float newScale = UnityEngine.Random.Range(minAsteroidScale, maxAsteroidScale);
                    interactable.transform.localScale = new Vector3(UnityEngine.Random.Range(minAsteroidScale, maxAsteroidScale), UnityEngine.Random.Range(minAsteroidScale, maxAsteroidScale), UnityEngine.Random.Range(minAsteroidScale, maxAsteroidScale));
                    Rigidbody rb = rbs[i] = interactable.GetComponent<Rigidbody>();
                    rb.mass = newScale * 2;

                    //Variable interactable amount
                    if(raceMode && i < baseDisableNumber)
                    {
                        interactable.SetActive(false);
                        continue;
                    }

                    //Random force
                    rb.AddRelativeForce(UnityEngine.Random.Range(-maxMovementForce, maxMovementForce), UnityEngine.Random.Range(-maxMovementForce, maxMovementForce), UnityEngine.Random.Range(-maxMovementForce, maxMovementForce), ForceMode.VelocityChange);
                    rb.AddRelativeTorque(UnityEngine.Random.Range(-maxRotationForce, maxRotationForce), UnityEngine.Random.Range(-maxRotationForce, maxRotationForce), UnityEngine.Random.Range(-maxRotationForce, maxRotationForce), ForceMode.VelocityChange);
                }

                for(int i = asteroidNumber; i < interactableNumber; i++)
                {
                    float interactableType = UnityEngine.Random.Range(0f, 100);
                    if(interactableType < wormholePercent)
                    {
                        //Spawn wormhole
                        interactable = Instantiate(wormholePrefab, Vector3.zero, Quaternion.Euler(UnityEngine.Random.Range(0f, 360), UnityEngine.Random.Range(0f, 360), UnityEngine.Random.Range(0f, 360)), parent);
                        interactable.transform.localScale = wormholeProportions * UnityEngine.Random.Range(minWormholeScale, maxWormholeScale);
                    }
                    else
                    {
                        //Spawn random collectable
                        int collectableIndex = UnityEngine.Random.Range(0, collectableCount);
                        interactable = Instantiate(possibleCollectables[collectableIndex], Vector3.zero, Quaternion.Euler(UnityEngine.Random.Range(0f, 360), UnityEngine.Random.Range(0f, 360), UnityEngine.Random.Range(0f, 360)), parent);
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

                    interactable.transform.localPosition = new Vector3(UnityEngine.Random.Range((float) -side, side), UnityEngine.Random.Range((float) -side, side), UnityEngine.Random.Range((float) -side, side));
                    children[i] = interactable;
                }
                break;
            
            default:
                for(int i = 0; i < asteroidNumber; i++)
                {
                    //Spawn random asteroid
                    int asteroidIndex = UnityEngine.Random.Range(0, asteroidCount);
                    interactable = children[i] = Instantiate(possibleAsteroids[asteroidIndex], Vector3.zero, Quaternion.Euler(UnityEngine.Random.Range(0f, 360), UnityEngine.Random.Range(0f, 360), UnityEngine.Random.Range(0f, 360)), parent);
                    interactable.SetActive(false);

                    //Random scale
                    float newScale = UnityEngine.Random.Range(minAsteroidScale, maxAsteroidScale);
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
                        interactable = Instantiate(wormholePrefab, Vector3.zero, Quaternion.Euler(UnityEngine.Random.Range(0f, 360), UnityEngine.Random.Range(0f, 360), UnityEngine.Random.Range(0f, 360)), parent);
                        interactable.transform.localScale = wormholeProportions * UnityEngine.Random.Range(minWormholeScale, maxWormholeScale);
                        interactable.SetActive(false);
                    }
                    else
                    {
                        //Spawn random collectable
                        int collectableIndex = UnityEngine.Random.Range(0, collectableCount);
                        interactable = Instantiate(possibleCollectables[collectableIndex], Vector3.zero, Quaternion.Euler(UnityEngine.Random.Range(0f, 360), UnityEngine.Random.Range(0f, 360), UnityEngine.Random.Range(0f, 360)), parent);
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
        int disableNumber = 0;
        if(raceMode) disableNumber = disableByCheckpoint[playerCheckpoints[playerIndex]];
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
                            if(raceMode && j < disableNumber)
                            {
                                continue;
                            }

                            //Random position and force
                            Vector3 newPosition = new Vector3(UnityEngine.Random.Range((float) -side, side), UnityEngine.Random.Range((float) -side, side), UnityEngine.Random.Range((float) -side, side));
                            if((newPos + newPosition).sqrMagnitude < lastCheckpointDistance)
                            {
                                child.transform.localPosition = newPosition;
                                if(!child.activeSelf) child.SetActive(true);
                                Rigidbody rb = rbs[j];
                                rb.velocity = rb.angularVelocity = Vector3.zero;
                                rb.AddRelativeForce(UnityEngine.Random.Range(-maxMovementForce, maxMovementForce), UnityEngine.Random.Range(-maxMovementForce, maxMovementForce), UnityEngine.Random.Range(-maxMovementForce, maxMovementForce), ForceMode.VelocityChange);
                                rb.AddRelativeTorque(UnityEngine.Random.Range(-maxRotationForce, maxRotationForce), UnityEngine.Random.Range(-maxRotationForce, maxRotationForce), UnityEngine.Random.Range(-maxRotationForce, maxRotationForce), ForceMode.VelocityChange);
                            }
                            else if(child.activeSelf) child.SetActive(false);
                        }
                        for(int j = asteroidNumber; j < interactableNumber; j++)
                        {
                            //Random position
                            GameObject child = children[j];
                            Vector3 newPosition = new Vector3(UnityEngine.Random.Range((float) -side, side), UnityEngine.Random.Range((float) -side, side), UnityEngine.Random.Range((float) -side, side));
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
        int playerFollowerNumber = playerFollowers.Count;
        for(int i = 1; i < playerFollowerNumber; i++)
        {
            playerFollowers[i].gameObject.SetActive(false);
        }
        
        int playersLeft = playingPlayers.Count;
        switch(playerNumber - playersLeft)
        {
            case 1:
                finishTexts[playerIndex].SetText("1st Place!");
                break;
            case 2:
                finishTexts[playerIndex].SetText("2nd Place!");
                break;
            case 3:
                finishTexts[playerIndex].SetText("3rd Place!");
                break;
            default:
                finishTexts[playerIndex].SetText("4th Place!");
                break;
        }

        //Game over
        if(playersLeft == 0)
        {
            finished = true;
            MenuManager.races++;
            PlayerPrefs.SetInt("Races", MenuManager.races);
            StartCoroutine(FinishMenu());
        }

        audioSource.PlayOneShot(goSound);
    }

    IEnumerator FinishMenu()
    {
        yield return twoSeconds;
        Transform menuTransform = menu.transform.GetChild(1);
        menuTransform.GetChild(2).gameObject.SetActive(false);
        menuTransform.GetChild(3).gameObject.SetActive(true);
        menuTransform.GetChild(1).GetChild(1).GetComponent<TMP_Text>().SetText("Game Over");
        menu.SetActive(true);
        if(cursorHide)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
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
            if(raceMode && i < disableNumber)
            {
                continue;
            }

            //Random position and force
            Vector3 newPos = new Vector3(UnityEngine.Random.Range((float) -side, side), UnityEngine.Random.Range((float) -side, side), UnityEngine.Random.Range((float) -side, side));
            if((sectionPos + newPos).sqrMagnitude < lastCheckpointDistance)
            {
                child.transform.localPosition = newPos;
                child.SetActive(true);
                Rigidbody rb = rbs[i];
                rb.velocity = rb.angularVelocity = Vector3.zero;
                rb.AddRelativeForce(UnityEngine.Random.Range(-maxMovementForce, maxMovementForce), UnityEngine.Random.Range(-maxMovementForce, maxMovementForce), UnityEngine.Random.Range(-maxMovementForce, maxMovementForce), ForceMode.VelocityChange);
                rb.AddRelativeTorque(UnityEngine.Random.Range(-maxRotationForce, maxRotationForce), UnityEngine.Random.Range(-maxRotationForce, maxRotationForce), UnityEngine.Random.Range(-maxRotationForce, maxRotationForce), ForceMode.VelocityChange);
            }

            if(i == 10 || i == 20 || i == 30 || i == 40 || i == 50 || i == 60 || i == 70 || i == 80 || i == 90 || i == 100)
            {
                yield return null;
            }
        }
        for(int i = asteroidNumber; i < interactableNumber; i++)
        {
            //Random position
            GameObject child = children[i];
            Vector3 newPos = new Vector3(UnityEngine.Random.Range((float) -side, side), UnityEngine.Random.Range((float) -side, side), UnityEngine.Random.Range((float) -side, side));
            if((sectionPos + newPos).sqrMagnitude < lastCheckpointDistance)
            {
                child.transform.localPosition = newPos;
                child.SetActive(true);
            }

            if(i == 10 || i == 20 || i == 30 || i == 40 || i == 50 || i == 60 || i == 70 || i == 80 || i == 90 || i == 100)
            {
                yield return null;
            }
        }
    }

    IEnumerator DeactivateSection(int sectionIndex)
    {
        GameObject[] children = childrenList[sectionIndex];
        for(int i = 0; i < interactableNumber; i++)
        {
            if(children[i].activeSelf) children[i].SetActive(false);
            if(i == 20 || i == 40 || i == 60 || i == 80 || i == 100) yield return null;
        }
        sectionPool[sectionIndex].SetActive(false);
    }
}
