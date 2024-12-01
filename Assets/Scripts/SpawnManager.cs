
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
using UnityEngine.UIElements;
using Cinemachine;
using UnityEngine.Analytics;
using JetBrains.Annotations;
using System.Runtime.CompilerServices;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.LowLevel;

public class SpawnManager : MonoBehaviour
{
    [Header("Sections")]
    [SerializeField] int side = 600;
    int length;
    [SerializeField] int colliderWidth = 500;
    GameObject[] sectionPool;
    [SerializeField] Transform sectionParent;
    [SerializeField] GameObject sectionPrefab;
    Vector3[] sectionChunks;

    [Header("Checkpoints")]
    GameObject[] checkpointPool;
    public int[] playerCheckpoints;
    [SerializeField] int[] checkpointDistances;
    float[] checkpointLengths;
    float[] checkpointRadii;
    [SerializeField] Transform checkpointParent;
    [SerializeField] GameObject checkpointGraphic;
    [SerializeField] GameObject checkpointTrigger;
    [SerializeField] Material checkpointMat;
    [SerializeField] Material finishMat;
    [SerializeField] float polygonSides = 36;
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
    Vector3[] playerChunks;
    public List<int> playingPlayers = new List<int>();
    public static List<Transform>[] followers;

    [Header("Camera/Audio")]
    [SerializeField] GameObject cameraPrefab;
    [SerializeField] LayerMask[] cameraMasks;
    Camera[] views = new Camera[4];
    CinemachineVirtualCamera[] thirdPersons = new CinemachineVirtualCamera[4];
    [SerializeField] AudioClip startSound;
    [SerializeField] AudioClip countdownSound;
    [SerializeField] AudioClip goSound;
    [SerializeField] AudioClip pauseSound;
    AudioSource audioSource;
    AudioSource backgroundMusic;
    AudioSource backgroundNoise;
    List<AudioSource> allAudio = new List<AudioSource>();
    
    [Header("UI")]
    [SerializeField] GameObject menu;
    
    void Awake()
    {
        menu.SetActive(false);
        length = side * 2;

        //Getting number of players
        if(MenuManager.playerNumber != 0)
        {
            playerNumber = MenuManager.playerNumber;
        }

        //Getting ship of each player
        if(MenuManager.playerShips != null)
        {
            playerShips = MenuManager.playerShips;
        }

        //Set difficulty-dependent variables
        if(MenuManager.difficulty != 0)
        {
            difficulty = MenuManager.difficulty;
        }
        int difficultyMinusOne = difficulty - 1;
        maxInteractables += interactableIncrease * difficultyMinusOne;
        droneNumber += droneIncrease * difficultyMinusOne;
        collectablePercent -= collectableDecrease * difficultyMinusOne;
        wormholePercent -= wormholeDecrease * difficultyMinusOne;
        checkpointDrones = new int[][]{begCheckpointDrones, intCheckpointDrones, advCheckpointDrones, expCheckpointDrones}[difficultyMinusOne];

        //Stats for each checkpoint
        baseDisableNumber = maxInteractables / 3;
        checkpointCount = checkpointDistances.Length;
        disableByCheckpoint = new int[checkpointCount];
        int increment = baseDisableNumber / (checkpointCount - 1);
        checkpointLengths = new float[checkpointCount];
        checkpointRadii = new float[checkpointCount];
        for(int i = 0; i < checkpointCount; i++)
        {
            disableByCheckpoint[i] = baseDisableNumber - i * increment;
            int distance = checkpointDistances[i];
            float checkpointSide = distance * Mathf.Tan(Mathf.Deg2Rad * (180 / polygonSides));
            checkpointLengths[i] = 2 * checkpointSide;
            checkpointRadii[i] = Mathf.Sqrt(checkpointSide * checkpointSide + (float) distance * (float) distance);
        }
        //Asteroid belt should have high asteroid number
        disableByCheckpoint[4] = increment;

        //Initialize player variables
        PlayerController.spawnManager = this;
        PlayerController.length = length;
        PlayerController.side = side;
        PlayerController.playerNumber = playerNumber;
        PlayerController.checkpointDistances = checkpointDistances;
        PlayerController.checkpointRadii = checkpointRadii;
        PlayerController.colliderWidth = colliderWidth;
        PlayerController.droneNumber = droneNumber;
        PlayerController.checkpointDrones = checkpointDrones;
        AudioSource[] audioSources = GetComponents<AudioSource>();
        audioSource = audioSources[0];
        PlayerController.audioSource = Drone.audioSource = Ring.audioSource = Atmosphere.audioSource = audioSource;
        backgroundMusic = audioSources[1];
        backgroundNoise = audioSources[2];

        StartCoroutine(Initialize());
    }
    IEnumerator Initialize()
    {
        while(SceneManager.GetActiveScene() != SceneManager.GetSceneByName("Play"))
        {
            yield return null;
        }
        
        //Section spawning
        int sectionNumber = playerNumber * 27;
        sectionChunks = new Vector3[sectionNumber];
        sectionPool = new GameObject[sectionNumber];
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
        for(int i = 0; i < 27; i++)
        {
            sectionPool[i].SetActive(true);
        }
        sectionPool[0].GetComponent<SectionSpawner>().avoidCenter = false;

        //Spawn checkpoints
        int checkpointNumber = playerNumber * 6;
        checkpointPool = new GameObject[checkpointNumber];
        for(int i = 0; i < checkpointNumber; i++)
        {
            switch(i)
            {
                case 0:
                    checkpointPool[0] = Instantiate(checkpointGraphic, checkpointParent);
                    SetCheckpoint(checkpointPool[0].transform, 0);
                    break;
                case 1:
                    checkpointPool[1] = Instantiate(checkpointGraphic, new Vector3(0, length, 0), Quaternion.identity, checkpointParent);
                    SetCheckpoint(checkpointPool[1].transform, 0);
                    break;
                case 2:
                    checkpointPool[2] = Instantiate(checkpointGraphic, new Vector3(0, -length, 0), Quaternion.identity, checkpointParent);
                    SetCheckpoint(checkpointPool[2].transform, 0);
                    break;
                default:
                    checkpointPool[i] = Instantiate(checkpointGraphic, checkpointParent);
                    checkpointPool[i].SetActive(false);
                    SetCheckpoint(checkpointPool[i].transform, 0);
                    break;

            }
        }

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
            cameraObjects[i] = Instantiate(cameraPrefab, transform).transform;
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
                player1 = Instantiate(possiblePlayers[playerShips[0]], new Vector3(0, 0, 5), Quaternion.identity);
                player1.tag = "1";
                players[0] = player1;

                //Set up cameras
                Transform cameraObject = cameraObjects[0];
                cameraObject.parent = players[0].transform;
                cameraObject.localPosition = Vector3.zero;
                players[0].GetComponent<PlayerController>().cameraObject = cameraObjects[0].gameObject;
                CinemachineVirtualCamera viewCam = thirdPersons[0];
                viewCam.Follow = viewCam.LookAt = player1.transform.GetChild(2);
                viewCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_TrackedObjectOffset = Vector3.zero;
                viewCam.GetCinemachineComponent<CinemachineSameAsFollowTarget>().m_Damping = 0.15f;
                views[0].rect = new Rect(Vector2.zero, new Vector2(1, 1));
                break;

            case 2:
                player1 = Instantiate(possiblePlayers[playerShips[0]], new Vector3(0, 0, 5), Quaternion.identity);
                player2 = Instantiate(possiblePlayers[playerShips[1]], new Vector3(0, 0, -5), Quaternion.Euler(0, 180, 0));
                player1.tag = "1";
                player2.tag = "2";
                players[0] = player1;
                players[1] = player2;

                //Set up cameras
                for(int i = 0; i < 2; i++)
                {
                    cameraObject = cameraObjects[i];
                    cameraObject.parent = players[i].transform;
                    cameraObject.localPosition = Vector3.zero;
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
                player1 = Instantiate(possiblePlayers[playerShips[0]], new Vector3(0, 0, 5), Quaternion.identity);
                player2 = Instantiate(possiblePlayers[playerShips[2]], new Vector3(-5, 0, 0), Quaternion.Euler(0, 270, 0));
                player3 = Instantiate(possiblePlayers[playerShips[1]], new Vector3(5, 0, 0), Quaternion.Euler(0, 90, 0));
                player1.tag = "1";
                player2.tag = "2";
                player3.tag = "3";
                players[0] = player1;
                players[1] = player2;
                players[2] = player3;

                //Set up cameras
                for(int i = 0; i < 3; i++)
                {
                    cameraObject = cameraObjects[i];
                    cameraObject.parent = players[i].transform;
                    cameraObject.localPosition = Vector3.zero;
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
                player1 = Instantiate(possiblePlayers[playerShips[0]], new Vector3(0, 0, 5), Quaternion.identity);
                player2 = Instantiate(possiblePlayers[playerShips[3]], new Vector3(-5, 0, 0), Quaternion.Euler(0, 270, 0));
                player3 = Instantiate(possiblePlayers[playerShips[1]], new Vector3(5, 0, 0), Quaternion.Euler(0, 90, 0));
                player4 = Instantiate(possiblePlayers[playerShips[2]], new Vector3(0, 0, -5), Quaternion.Euler(0, 180, 0));
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
                    cameraObject = cameraObjects[i];
                    cameraObject.parent = players[i].transform;
                    cameraObject.localPosition = Vector3.zero;
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
        AudioSource[] audioArray = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
        foreach(AudioSource audio in audioArray)
        {
            if(audio == audioSource)
            {
                continue;
            }
            allAudio.Add(audio);
        }
        gameObject.AddComponent<AudioListener>();

        StartCoroutine(Countdown(3));
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
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
        audioSource.PlayOneShot(pauseSound);
        foreach(AudioSource audio in allAudio)
        {
            audio.Pause();
        }
        Time.timeScale = 0;
    }

    public void Resume()
    {
        menu.SetActive(false);
        audioSource.PlayOneShot(pauseSound);
        Time.timeScale = 1;
        foreach(AudioSource audio in allAudio)
        {
            audio.UnPause();
        }
    }

    public void Exit()
    {
        audioSource.PlayOneShot(pauseSound);
        SceneManager.LoadSceneAsync(0);
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

    void SetCheckpoint(Transform parent, int checkpointIndex)
    {
        parent.name = "" + checkpointIndex;
        int distance = checkpointDistances[checkpointIndex];
        float objectLength = checkpointLengths[checkpointIndex];
        float baseAngle = 360 / polygonSides;
        switch(parent.childCount)
        {
            case 0:
                for(int i = 0; i < polygonSides; i++)
                {
                    Transform checkpoint = Instantiate(checkpointTrigger, parent.position, Quaternion.Euler(0, baseAngle * i, 0), parent).transform;
                    Transform collider = checkpoint.GetChild(0);
                    collider.localScale = new Vector3(objectLength, length, colliderWidth);
                    collider.localPosition = new Vector3(0, 0, distance);
                }
                break;

            default:
                int finishDistance = distance + colliderWidth / 2;
                for(int i = 0; i < polygonSides; i++)
                {
                    Transform checkpoint = parent.GetChild(i);
                    checkpoint.rotation = Quaternion.Euler(0, baseAngle * i, 0);
                    Transform collider = checkpoint.GetChild(0);
                    collider.localScale = new Vector3(objectLength, length, colliderWidth);
                    switch(checkpointIndex == checkpointCount - 1)
                    {
                        case false:
                            collider.localPosition = new Vector3(0, 0, distance);
                            collider.GetComponent<Renderer>().material = checkpointMat;
                            collider.tag = "Checkpoint";
                            break;

                        case true:
                            collider.localPosition = new Vector3(0, 0, finishDistance);
                            collider.GetComponent<Renderer>().material = finishMat;
                            collider.tag = "FinishLine";
                            break;
                    }
                }
                break;
        }
    }
    
    //Section spawning
    public void SpawnSectionsForward(Vector3 colliderPos, int playerIndex)
    {
        Vector3[] newPosList = new Vector3[9];
        Vector3 playerChunkPos = colliderPos + new Vector3(0, 0, side);
        playerChunks[playerIndex] = playerChunkPos / length;
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
    public void SpawnSectionsBack(Vector3 colliderPos, int playerIndex)
    {
        Vector3[] newPosList = new Vector3[9];
        Vector3 playerChunkPos = colliderPos + new Vector3(0, 0, -side);
        playerChunks[playerIndex] = playerChunkPos / length;
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
    public void SpawnSectionsRight(Vector3 colliderPos, int playerIndex)
    {
        Vector3[] newPosList = new Vector3[9];
        Vector3 playerChunkPos = colliderPos + new Vector3(side, 0, 0);
        playerChunks[playerIndex] = playerChunkPos / length;
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
    public void SpawnSectionsLeft(Vector3 colliderPos, int playerIndex)
    {
        Vector3[] newPosList = new Vector3[9];
        Vector3 playerChunkPos = colliderPos + new Vector3(-side, 0, 0);
        playerChunks[playerIndex] = playerChunkPos / length;
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
    public void SpawnSectionsUp(Vector3 colliderPos, int playerIndex)
    {
        Vector3[] newPosList = new Vector3[9];
        Vector3 playerChunkPos = colliderPos + new Vector3(0, side, 0);
        playerChunks[playerIndex] = playerChunkPos / length;
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
        MoveCheckpoint(playerIndex, (int) (playerChunkPos.y / length + 1));
    }
    public void SpawnSectionsDown(Vector3 colliderPos, int playerIndex)
    {
        Vector3[] newPosList = new Vector3[9];
        Vector3 playerChunkPos = colliderPos + new Vector3(0, -side, 0);
        playerChunks[playerIndex] = playerChunkPos / length;
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
        MoveCheckpoint(playerIndex, (int) (playerChunkPos.y / length - 1));
    }

    public void SpawnSectionsAround(Vector3 playerChunk, int playerIndex)
    {
        int originalYChunk = (int) playerChunks[playerIndex].y;
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
        
        //Move checkpoints and sections
        int playerYChunk = (int) playerChunk.y;
        if(originalYChunk > playerYChunk + 1 || originalYChunk < playerYChunk - 1)
        {
            MoveCheckpoint(playerIndex, playerYChunk);
        }
        playerYChunk += 1;
        if(originalYChunk > playerYChunk + 1 || originalYChunk < playerYChunk - 1)
        {
            MoveCheckpoint(playerIndex, playerYChunk);
        }
        playerYChunk -= 2;
        if(originalYChunk > playerYChunk + 1 || originalYChunk < playerYChunk - 1)
        {
            MoveCheckpoint(playerIndex, playerYChunk);
        }
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
        Transform newParent = sectionPool[Array.IndexOf(sectionChunks, playerChunks[playerIndex])].transform;
        players[playerIndex].transform.parent = newParent;
        foreach(Transform follower in followers[playerIndex])
        {
            follower.parent = newParent;
        }
    }
    
    void MoveCheckpoint(int playerIndex, int newYChunk)
    {
        int baseCheckpointIndex = playerCheckpoints[playerIndex];
        int[] checkpointIndexes = new int[]{baseCheckpointIndex, baseCheckpointIndex - 1};
        if(baseCheckpointIndex == 0)
        {
            checkpointIndexes = new int[]{baseCheckpointIndex};
        }
        int stage = 0;

        for(int i = 0; i < checkpointPool.Length; i++)
        {
            GameObject checkpoint = checkpointPool[i];

            //Deactivate unused checkpoints
            DestroyCheckpoint(checkpoint);

            //Activate sections
            if(!checkpoint.activeInHierarchy && stage < checkpointIndexes.Length)
            {
                int checkpointIndex = checkpointIndexes[stage];
                bool valid = true;
                foreach(int j in playingPlayers)
                {
                    if(j == playerIndex)
                    {
                        continue;
                    }
                    int playerCheckpoint = playerCheckpoints[j];
                    int playerYChunk = (int) playerChunks[j].y;
                    if(playerYChunk <= newYChunk + 1 && playerYChunk >= newYChunk - 1 && (playerCheckpoint == checkpointIndex || playerCheckpoint == checkpointIndex + 1))
                    {
                        valid = false;
                        break;
                    }
                }
                switch(valid)
                {
                    case true:
                        checkpoint.transform.position = new Vector3(0, newYChunk * length, 0);
                        if(checkpoint.name != "" + checkpointIndex)
                        {
                            SetCheckpoint(checkpoint.transform, checkpointIndex);
                        }
                        checkpoint.SetActive(true);
                        break;

                    case false:
                        i--;
                        break;
                }
                stage++;
            }
        }
    }
    
    public void SpawnCheckpoints(int playerIndex, int checkpointIndex)
    {
        int baseYChunk = (int) playerChunks[playerIndex].y;
        int[] newYChunks = new int[]{baseYChunk, baseYChunk + 1, baseYChunk - 1};
        int stage = 0;

        for(int i = 0; i < checkpointPool.Length; i++)
        {
            GameObject checkpoint = checkpointPool[i];

            //Deactivate unused checkpoints
            DestroyCheckpoint(checkpoint);

            //Activate sections
            if(!checkpoint.activeInHierarchy && stage < newYChunks.Length && checkpointIndex >= 0)
            {
                int newYChunk = newYChunks[stage];
                bool valid = true;
                foreach(int j in playingPlayers)
                {
                    if(j == playerIndex)
                    {
                        continue;
                    }
                    int playerCheckpoint = playerCheckpoints[j];
                    int playerYChunk = (int) playerChunks[j].y;
                    if(playerYChunk <= newYChunk + 1 && playerYChunk >= newYChunk - 1 && (playerCheckpoint == checkpointIndex || playerCheckpoint == checkpointIndex + 1))
                    {
                        valid = false;
                        break;
                    }
                }
                switch(valid)
                {
                    case true:
                        checkpoint.transform.position = new Vector3(0, newYChunk * length, 0);
                        if(checkpoint.name != "" + checkpointIndex)
                        {
                            SetCheckpoint(checkpoint.transform, checkpointIndex);
                        }
                        checkpoint.SetActive(true);
                        break;

                    case false:
                        i--;
                        break;
                }
                stage++;
            }
        }
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
    
    void DestroyCheckpoint(GameObject checkpoint)
    {
        if(checkpoint.activeInHierarchy)
        {
            int checkpointYChunk = Mathf.RoundToInt(checkpoint.transform.position.y / length);
            bool valid = true;
            foreach(int j in playingPlayers)
            {
                int playerCheckpoint = playerCheckpoints[j];
                int playerYChunk = (int) playerChunks[j].y;
                string checkpointName = checkpoint.name;
                if(playerYChunk <= checkpointYChunk + 1 && playerYChunk >= checkpointYChunk - 1 && (checkpointName == "" + playerCheckpoint || checkpointName == "" + (playerCheckpoint - 1)))
                {
                    valid = false;
                    break;
                }
            }
            if(valid)
            {
                checkpoint.SetActive(false);
            }
        }
    }

    public int PlayerFinish(int playerIndex)
    {
        playingPlayers.Remove(playerIndex);
        Vector3 playerChunk = playerChunks[playerIndex];
        int playerYChunk = (int) playerChunk.y;
        int playerCheckpoint = playerCheckpoints[playerIndex];
        foreach(GameObject section in sectionPool)
        {
            if(section.activeInHierarchy)
            {
                Vector3 sectionChunk = section.transform.position / length;
                if(playerChunk.x >= sectionChunk.x - 1 && playerChunk.x <= sectionChunk.x + 1 && playerChunk.z >= sectionChunk.z - 1 && playerChunk.z <= sectionChunk.z + 1 && playerChunk.y >= sectionChunk.y - 1 && playerChunk.y <= sectionChunk.y + 1)
                {
                    section.SetActive(false);
                }
            }
        }
        foreach(GameObject checkpoint in checkpointPool)
        {
            if(checkpoint.activeInHierarchy)
            {
                int checkpointYChunk = Mathf.RoundToInt(checkpoint.transform.position.y / length);
                string checkpointName = checkpoint.name;
                if(playerYChunk <= checkpointYChunk + 1 && playerYChunk >= checkpointYChunk - 1 && (checkpointName == "" + playerCheckpoint || checkpointName == "" + (playerCheckpoint - 1)))
                {
                    checkpoint.SetActive(false);
                }
            }
        }
        foreach(Transform follower in followers[playerIndex])
        {
            follower.gameObject.SetActive(false);
        }
        return playingPlayers.Count;
    }

    IEnumerator Countdown(int countdownTime)
    {
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
