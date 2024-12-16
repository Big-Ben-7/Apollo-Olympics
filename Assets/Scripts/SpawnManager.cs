
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Pool;
using Cinemachine;
using UnityEngine.Analytics;
using JetBrains.Annotations;
using System.Runtime.CompilerServices;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.LowLevel;
using TMPro;

public class SpawnManager : MonoBehaviour
{
    [Header("Sections")]
    [SerializeField] int side = 900;
    int length;
    [SerializeField] int colliderWidth = 500;
    GameObject[] sectionPool;
    [SerializeField] Transform sectionParent;
    [SerializeField] GameObject sectionPrefab;
    Vector3[] sectionChunks;

    [Header("Checkpoints")]
    public int[] playerCheckpoints;
    [SerializeField] int[] checkpointDistances = new int[] {7500, 15000, 22500, 30000, 40000, 50000, 65000, 80000, 100000};
    [SerializeField] Transform checkpointParent;
    [SerializeField] GameObject checkpointPrefab;
    [SerializeField] Material finishMat;
    int checkpointCount;

    [Header("Interactables")]
    [SerializeField] int difficulty;
    [SerializeField] GameObject[] possibleAsteroids;
    [SerializeField] GameObject[] possibleCollectables;
    [SerializeField] GameObject wormholePrefab;
    //Interactable number must be divisible by 24
    [SerializeField] int maxInteractables = 48;
    [SerializeField] int interactableIncrease = 24;
    [SerializeField] int collectablePercent = 20;
    [SerializeField] int collectableDecrease = 5;
    [SerializeField] float wormholePercent = 1;
    [SerializeField] float wormholeDecrease = 0.2f;

    [Header("Drones")]
    [SerializeField] int droneNumber = 4;
    [SerializeField] int droneIncrease = 4;
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

    [Header("Customization")]
    public int[] initialThrustSpeeds;
    public int[] accelerations;
    public int[] maxThrustSpeeds;
    public float[] rotationSpeeds;
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
    AudioSource audioSource;
    AudioSource backgroundMusic;
    AudioSource backgroundNoise;
    AudioSource[] allAudio;
    int audioCount;
    [SerializeField] GameObject menu;
    public int initializedPlayers;
    bool cursorHide;
    
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
            playerNumber = MenuManager.playerNumber;
            playerShips = MenuManager.playerShips;
            difficulty = MenuManager.difficulty;
            audioSource.volume = MenuManager.effects * 0.4f;
            backgroundMusic.volume = MenuManager.music * 0.2f;
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
        }
        else
        {
            PlayerController.maxThrusterVolume = 0.3f;
        }

        //Setting difficulty-dependent variables
        maxInteractables += interactableIncrease * difficulty;
        droneNumber += droneIncrease * difficulty;
        collectablePercent -= collectableDecrease * difficulty;
        wormholePercent -= wormholeDecrease * difficulty;
        checkpointDrones = new int[][]{begCheckpointDrones, intCheckpointDrones, advCheckpointDrones, expCheckpointDrones}[difficulty];

        //Stats for each checkpoint
        baseDisableNumber = maxInteractables / 3;
        checkpointCount = checkpointDistances.Length;
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

        //Initialize player variables
        PlayerController.spawnManager = this;
        PlayerController.length = length;
        PlayerController.side = side;
        PlayerController.playerNumber = playerNumber;
        PlayerController.checkpointDistances = checkpointDistances;
        PlayerController.droneNumber = droneNumber;
        PlayerController.checkpointDrones = checkpointDrones;
        PlayerController.audioSource = Drone.audioSource = Ring.audioSource = Atmosphere.audioSource = audioSource;
        StartCoroutine(Initialize());
    }
    IEnumerator Initialize()
    {
        while(SceneManager.GetActiveScene() != SceneManager.GetSceneByName("Play"))
        {
            yield return null;
        }
        
        //Section spawning
        UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;
        int sectionCount = playerNumber * 27;
        sectionChunks = new Vector3[sectionCount];
        sectionPool = new GameObject[sectionCount];
        Vector3 spawnPos = Vector3.zero;
        for(int i = 0; i < sectionCount; i++)
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
            sectionPool[i] = Instantiate(sectionPrefab, spawnPos, Quaternion.identity, sectionParent);
            sectionPool[i].SetActive(false);
            SectionSpawner.side = side;
            sectionPool[i].GetComponent<SectionSpawner>().initialDisableNumber = baseDisableNumber;
            sectionChunks[i] = spawnPos / length;

            Transform child1 = sectionPool[i].transform.GetChild(0);
            Transform child2 = sectionPool[i].transform.GetChild(1);
            Transform child3 = sectionPool[i].transform.GetChild(2);

            child1.localScale = new Vector3(length + colliderWidth, length + colliderWidth, colliderWidth);
            child1.localPosition = new Vector3(0, 0, side);
            
            child2.localScale = new Vector3(colliderWidth, length + colliderWidth, length + colliderWidth);
            child2.localPosition = new Vector3(side, 0, 0);
            
            child3.localScale = new Vector3(length + colliderWidth, colliderWidth, length + colliderWidth);
            child3.localPosition = new Vector3(0, side, 0);
            SetSection(sectionPool[i].transform);
        }

        //Spawn initial sections
        sectionPool[0].GetComponent<SectionSpawner>().avoidCenter = true;
        /*for(int i = 0; i < 5; i++)
        {
            sectionPool[i].GetComponent<SectionSpawner>().initialEnable = true;
            sectionPool[i].SetActive(true);
            sectionPool[i].GetComponent<SectionSpawner>().initialEnable = false;
        }*/
        for(int i = 0; i < 27; i++)
        {
            sectionPool[i].SetActive(true);
        }
        sectionPool[0].GetComponent<SectionSpawner>().avoidCenter = false;

        //Spawn checkpoints
        int lastCheckpoint = checkpointCount - 1;
        int newScale;
        for(int i = 0; i < lastCheckpoint; i++)
        {
            Transform checkpoint = Instantiate(checkpointPrefab, checkpointParent).transform;
            newScale = checkpointDistances[i] * 2;
            checkpoint.localScale = new Vector3(newScale, newScale, newScale);
        }
        Transform finish = Instantiate(checkpointPrefab, checkpointParent).transform;
        newScale = checkpointDistances[lastCheckpoint] * 2;
        finish.transform.localScale = new Vector3(newScale, newScale, newScale);
        finish.GetComponent<Renderer>().material = finishMat;

        //Spawn players
        players = new GameObject[playerNumber];
        playerChunks = new Vector3[playerNumber];
        playerCheckpoints = new int[playerNumber];
        followers = new List<Transform>[playerNumber];
        Transform[] cameraObjects = new Transform[playerNumber];
        for(int i = 0; i < playerNumber; i++)
        {
            playerChunks[i] = Vector3.zero;
            playerCheckpoints[i] = 0;
            playingPlayers.Add(i);
            cameraObjects[i] = Instantiate(cameraPrefab).transform;
            Transform viewObject = cameraObjects[i].GetChild(0);
            views[i] = viewObject.GetComponent<Camera>();
            GameObject thirdPersonObject = viewObject.GetChild(0).gameObject;
            thirdPersonObject.layer = i + 9;
            thirdPersons[i] = thirdPersonObject.GetComponent<CinemachineVirtualCamera>();
            views[i].cullingMask = cameraMasks[i];
            followers[i] = new List<Transform>() {cameraObjects[i]};
        }
        GameObject player1;
        GameObject player2;
        GameObject player3;
        GameObject player4;
        switch(playerNumber)
        {
            case 1:
                player1 = Instantiate(possiblePlayers[playerShips[0]], new Vector3(0, 0, 20), Quaternion.identity);
                player1.tag = "1";
                players[0] = player1;

                //Set up cameras
                players[0].GetComponent<PlayerController>().cameraObject = cameraObjects[0].gameObject;
                CinemachineVirtualCamera viewCam = thirdPersons[0];
                viewCam.Follow = viewCam.LookAt = player1.transform.GetChild(2);
                viewCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_TrackedObjectOffset = Vector3.zero;
                viewCam.GetCinemachineComponent<CinemachineSameAsFollowTarget>().m_Damping = 0.15f;
                views[0].rect = new Rect(Vector2.zero, new Vector2(1, 1));
                break;

            case 2:
                player1 = Instantiate(possiblePlayers[playerShips[0]], new Vector3(0, 0, 20), Quaternion.identity);
                player2 = Instantiate(possiblePlayers[playerShips[1]], new Vector3(0, 0, -20), Quaternion.Euler(0, 180, 0));
                player1.tag = "1";
                player2.tag = "2";
                players[0] = player1;
                players[1] = player2;

                //Set up cameras
                for(int i = 0; i < 2; i++)
                {
                    players[i].GetComponent<PlayerController>().cameraObject = cameraObjects[i].gameObject;
                    viewCam = thirdPersons[i];
                    viewCam.Follow = viewCam.LookAt = players[i].transform.GetChild(2);
                    viewCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_TrackedObjectOffset = Vector3.back;
                    viewCam.GetCinemachineComponent<CinemachineSameAsFollowTarget>().m_Damping = 0.15f;
                }
                Vector2 scaleVector = new Vector2(0.5f, 1);
                views[0].rect = new Rect(Vector2.zero, scaleVector);
                views[1].rect = new Rect(new Vector2(0.5f, 0), scaleVector);
                break;

            case 3:
                player1 = Instantiate(possiblePlayers[playerShips[0]], new Vector3(0, 0, 20), Quaternion.identity);
                player2 = Instantiate(possiblePlayers[playerShips[2]], new Vector3(-20, 0, 0), Quaternion.Euler(0, 270, 0));
                player3 = Instantiate(possiblePlayers[playerShips[1]], new Vector3(20, 0, 0), Quaternion.Euler(0, 90, 0));
                player1.tag = "1";
                player2.tag = "2";
                player3.tag = "3";
                players[0] = player1;
                players[1] = player2;
                players[2] = player3;

                //Set up cameras
                for(int i = 0; i < 3; i++)
                {
                    players[i].GetComponent<PlayerController>().cameraObject = cameraObjects[i].gameObject;
                    viewCam = thirdPersons[i];
                    viewCam.Follow = viewCam.LookAt = players[i].transform.GetChild(2);
                    viewCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_TrackedObjectOffset = new Vector3(0, 0, -2);
                    viewCam.GetCinemachineComponent<CinemachineSameAsFollowTarget>().m_Damping = 0.1f;
                }
                scaleVector = new Vector2(0.333f, 1);
                views[0].rect = new Rect(Vector2.zero, scaleVector);
                views[1].rect = new Rect(new Vector2(0.667f, 0), scaleVector);
                views[2].rect = new Rect(new Vector2(0.333f, 0), scaleVector);
                break;

            case 4:
                player1 = Instantiate(possiblePlayers[playerShips[0]], new Vector3(0, 0, 20), Quaternion.identity);
                player2 = Instantiate(possiblePlayers[playerShips[3]], new Vector3(-20, 0, 0), Quaternion.Euler(0, 270, 0));
                player3 = Instantiate(possiblePlayers[playerShips[1]], new Vector3(20, 0, 0), Quaternion.Euler(0, 90, 0));
                player4 = Instantiate(possiblePlayers[playerShips[2]], new Vector3(0, 0, -20), Quaternion.Euler(0, 180, 0));
                player1.tag = "1";
                player2.tag = "2";
                player3.tag = "3";
                player4.tag = "4";
                players[0] = player1;
                players[1] = player2;
                players[2] = player3;
                players[3] = player4;

                //Set up cameras
                for(int i = 0; i < 4; i++)
                {
                    players[i].GetComponent<PlayerController>().cameraObject = cameraObjects[i].gameObject;
                    viewCam = thirdPersons[i];
                    viewCam.Follow = viewCam.LookAt = players[i].transform.GetChild(2);
                    viewCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_TrackedObjectOffset = Vector3.zero;
                    viewCam.GetCinemachineComponent<CinemachineSameAsFollowTarget>().m_Damping = 0.15f;
                }
                scaleVector = new Vector2(0.5f, 0.5f);
                views[0].rect = new Rect(new Vector2(0, 0.5f), scaleVector);
                views[1].rect = new Rect(new Vector2(0.5f, 0), scaleVector);
                views[2].rect = new Rect(new Vector2(0.5f, 0.5f), scaleVector);
                views[3].rect = new Rect(Vector2.zero, scaleVector);;
                break;
        }

        //Audio setup
        allAudio = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
        List<AudioSource> audioSources = new List<AudioSource>(allAudio);
        audioSources.Remove(audioSource);
        allAudio = audioSources.ToArray();
        audioCount = allAudio.Length;
        gameObject.AddComponent<AudioListener>();
        eventSystem.SetActive(true);

        StartCoroutine(Countdown(3));
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return) && !finished)
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
    }

    IEnumerator PauseSounds()
    {
        yield return new WaitForSecondsRealtime(1);
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
        yield return new WaitForSecondsRealtime(1);
        Time.timeScale = 1;
        SceneManager.LoadSceneAsync(index);
    }

    void SetSection(Transform parent)
    {
        for(int i = 0; i < maxInteractables; i++)
        {
            //Asteroid or collectable
            float interactableType = UnityEngine.Random.Range(0f, 100);
            int asteroidNumber = possibleAsteroids.Length;
            int collectableNumber = possibleCollectables.Length;
            if(interactableType < collectablePercent)
            {
                //Spawn random collectable
                int collectableIndex = UnityEngine.Random.Range(0, collectableNumber);
                Instantiate(possibleCollectables[collectableIndex], parent);
            }
            else if(interactableType < collectablePercent + wormholePercent)
            {
                //Spawn wormhole
                Instantiate(wormholePrefab, parent);
            }
            else
            {
                //Spawn random asteroid
                int asteroidIndex = UnityEngine.Random.Range(0, asteroidNumber);
                Instantiate(possibleAsteroids[asteroidIndex], parent);
            }
        }
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
        
        SpawnSections(newPosList, playerIndex);
    }
    
    void SpawnSections(Vector3[] newPosList, int playerIndex)
    {
        int stage = 0;
        int disableNumber = disableByCheckpoint[playerCheckpoints[playerIndex]];
        for(int i = 0; i < sectionPool.Length; i++)
        {
            GameObject section = sectionPool[i];

            //Deactivate unused sections
            DestroySection(section);

            //Activate new sections
            if(!section.activeInHierarchy && stage < newPosList.Length)
            {
                bool valid = true;
                Vector3 newPos = newPosList[stage];

                //Check if a section is already there
                Vector3 newChunk = newPos / length;
                for(int j = 0; j < sectionChunks.Length; j++)
                {
                    if(newChunk == sectionChunks[j])
                    {
                        if(!sectionPool[j].activeInHierarchy)
                        {
                            sectionPool[j].SetActive(true);
                        }
                        valid = false;
                        break;
                    }
                }
                switch(valid)
                {
                    case true:
                        section.transform.position = newPos;
                        section.GetComponent<SectionSpawner>().initialDisableNumber = disableNumber;
                        sectionChunks[Array.IndexOf(sectionPool, section)] = newChunk;
                        section.SetActive(true);
                        break;
                    
                    case false:
                        i--;
                        break;
                }
                stage++;
            }
        }
        //Transform newParent = sectionPool[Array.IndexOf(sectionChunks, playerChunks[playerIndex])].transform;
        //players[playerIndex].transform.parent = newParent;
        //foreach(Transform follower in followers[playerIndex])
        //{
        //    follower.parent = newParent;
        //}
    }

    void DestroySection(GameObject section)
    {
        if(section.activeInHierarchy)
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
                section.SetActive(false);
            }
        }
    }

    public int PlayerFinish(int playerIndex)
    {
        playingPlayers.Remove(playerIndex);
        for(int i = 0; i < sectionPool.Length; i++)
        {
            DestroySection(sectionPool[i]);
        }
        List<Transform> playerFollowers = followers[playerIndex];
        //playerFollowers[0].parent = null;
        for(int i = 1; i < playerFollowers.Count; i++)
        {
            //playerFollowers[i].parent = null;
            playerFollowers[i].gameObject.SetActive(false);
        }
        //Game over
        if(playingPlayers.Count == 0)
        {
            finished = true;
            MenuManager.races++;
            PlayerPrefs.SetInt("Races", MenuManager.races);
            StartCoroutine(FinishMenu());
        }
        return playingPlayers.Count;
    }

    IEnumerator FinishMenu()
    {
        yield return new WaitForSeconds(1);
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
    }

    IEnumerator Countdown(int countdownTime)
    {
        while(initializedPlayers < playerNumber)
        {
            yield return null;
        }
        backgroundNoise.Play();
        backgroundMusic.Play();
        while(countdownTime > 0)
        {
            audioSource.PlayOneShot(countdownSound);
            yield return new WaitForSeconds(1);
            countdownTime--;
        }
        audioSource.PlayOneShot(goSound);
    }
}
