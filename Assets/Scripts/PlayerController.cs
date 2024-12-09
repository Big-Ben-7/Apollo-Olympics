using System.Collections;
using System.Collections.Generic;
using System.Data;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;
using TMPro;
using System;
using System.Xml;
using UnityEngine.SceneManagement;
using Unity.VisualScripting.AssemblyQualifiedNameParser;
using System.Linq.Expressions;
using UnityEngine.Profiling;
using Unity.VisualScripting.Dependencies.NCalc;
using System.Resources;
using UnityEngine.UI;
using UnityEditor.Rendering;
using UnityEditor;
using Unity.Mathematics;
using System.Linq;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] int speed;
    [SerializeField] int agility;
    [SerializeField] int durability;
    [SerializeField] int stability;
    [SerializeField] int activationTime;
    [SerializeField] int cooldownTime;
    [SerializeField] float abilityStrength;
    [SerializeField] float abilityIncrease;

    [Header("Movement")]
    [SerializeField] float decMultiplier;
    [SerializeField] float glideDrag;
    [SerializeField] GameObject thrusterParent;
    [SerializeField] float thrusterSpeedDivisor;
    [SerializeField] float maxThrusterSpeed;
    [SerializeField] float thrusterVolumeDivisor;
    [SerializeField] float maxThrusterVolume;

    [Header("Collision")]
    [SerializeField] GameObject dieExplosion;
    [SerializeField] List<GameObject> hitExplosions;
    [SerializeField] int forwardPercent;
    [SerializeField] int backPercent;
    GameObject hitExplosionPrefab;

    [Header("Ability")]
    [SerializeField] string shipModel;
    [SerializeField] int abilityPercent;
    [SerializeField] int activationTimePercent;
    [SerializeField] int minCooldown;
    [SerializeField] GameObject aura;
    [SerializeField] ParticleSystem[] auraSystems;

    [Header("Drone spawning")]
    [SerializeField] GameObject dronePrefab;

    [Header("UI/Audio")]
    [SerializeField] AudioSource fireSource;
    [SerializeField] AudioSource thrusterSource;
    [SerializeField] AudioClip[] hitSounds;
    [SerializeField] AudioClip checkpointSound;
    [SerializeField] AudioClip activationSound;
    [SerializeField] AudioClip expireSound;
    [SerializeField] AudioClip chargeSound;
    [SerializeField] AudioClip errorSound;
    [SerializeField] AudioClip wormholeSound;
    [SerializeField] AudioClip dieSound;
    [SerializeField] Transform arrow;
    
    //Movement control
    Rigidbody rb;
    Transform myTransform;
    string playerNumberString;
    int playerIndex;
    float initialThrustSpeed;
    float acc;
    float maxThrustSpeed;
    float yawSpeed;
    float pitchSpeed;
    float rollSpeed;
    float thrustSpeed;
    float decSpeed;
    float glideSpeed;
    float yaw;
    float pitch;
    float roll;
    float thrust;
    Vector3 startPos;
    Vector3 normalizedStartPos;
    Quaternion startRot;

    //Collisions/triggers
    float speedLossDivisor;
    int healthLossDivisor;
    float minHealthLoss;
    int deflectionForce;
    float health = 100;
    bool collided;
    bool sectionFront;
    bool sectionSide;
    bool sectionUp;
    bool checkpointTriggered;
    public bool canCollide;
    bool canTrigger;
    public static SpawnManager spawnManager;
    public static int length;
    public static int side;
    public static int playerNumber;
    public static int[] checkpointDistances;
    public static float[] checkpointRadii;
    public static int colliderWidth;
    public static AudioSource audioSource;
    float[] collisionInitials;
    ParticleSystem.MainModule[] thrusters;
    int thrusterCount;
    int hitSoundsCount;
    int collects;

    //Ability
    IEnumerator ability;
    Coroutine runningAbility;
    KeyCode abilityKey;
    int currentActivation;
    int currentCooldown;
    int auraCount;
    ParticleSystem.MainModule[] auraParticles;
    Ring ring;
    bool died;
    Coroutine resetting;

    //Drone
    Transform droneGroup;
    public static int droneNumber;
    public static int[] checkpointDrones;
    int activeDrones;
    Drone[] drones;

    //UI
    public GameObject cameraObject;
    TMP_Text nameText;
    TMP_Text checkpointText;
    TMP_Text healthText;
    TMP_Text collectText;
    TMP_Text abilityText;
    TMP_Text posText;
    TMP_Text distanceText;
    TMP_Text speedText;
    TMP_Text timeText;
    TMP_Text countdownText;
    TMP_Text finishText;
    TMP_Text[] allText;
    GameObject finishBackground;
    DateTime startTime;

    void Awake()
    {
        dieExplosion.SetActive(false);
        hitExplosionPrefab = hitExplosions[0];
        hitExplosionPrefab.SetActive(false);
        aura.SetActive(false);
        thrusterParent.SetActive(false);
        arrow.gameObject.SetActive(false);
        fireSource.volume = 0;
        thrusterSource.volume = 0;

        rb = gameObject.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        myTransform = transform;
        startPos = rb.position;
        normalizedStartPos = startPos.normalized;
        startRot = rb.rotation;

        Transform thrusterTransform = thrusterParent.transform;
        thrusterCount = thrusterTransform.childCount;
        thrusters = new ParticleSystem.MainModule[thrusterCount];
        for(int i = 0; i < thrusterCount; i++)
        {
            thrusters[i] = thrusterTransform.GetChild(i).GetComponent<ParticleSystem>().main;
        }
        hitSoundsCount = hitSounds.Length;
    }

    //Player is tagged by SpawnManager after Awake
    void Start()
    {
        //Ability setup
        playerNumberString = gameObject.tag;
        switch(playerNumberString)
        {
            case "1":
                abilityKey = KeyCode.R;
                playerIndex = 0;
                break;
            case "2":
                abilityKey = KeyCode.Comma;
                playerIndex = 1;
                break;
            case "3":
                abilityKey = KeyCode.Y;
                playerIndex = 2;
                break;
            case "4":
                abilityKey = KeyCode.C;
                playerIndex = 3;
                break;
        }

        //Customize
        speed -= 1;
        agility -= 1;
        durability -= 1;
        stability -= 1;
        initialThrustSpeed = spawnManager.initialThrustSpeeds[speed];
        acc = spawnManager.accelerations[speed];
        maxThrustSpeed = spawnManager.maxThrustSpeeds[speed];
        rollSpeed = spawnManager.rollSpeeds[agility];
        pitchSpeed = spawnManager.pitchSpeeds[agility];
        yawSpeed = spawnManager.yawSpeeds[agility];
        healthLossDivisor = spawnManager.healthLossDivisors[durability];
        minHealthLoss = spawnManager.minHealthLosses[durability];
        speedLossDivisor = spawnManager.speedLossDivisors[stability];
        deflectionForce = spawnManager.deflectionForces[stability];
        thrustSpeed = initialThrustSpeed;
        decSpeed = thrustSpeed;

        switch(shipModel)
        {
            case "Mercury":
                ability = SpeedUp();
                break;
            case "Venus":
                ability = Atmosphere();
                aura = Instantiate(aura);
                aura.GetComponent<Atmosphere>().target = myTransform;
                Transform graphic = aura.transform.GetChild(0);
                auraSystems = new ParticleSystem[] {graphic.GetComponent<ParticleSystem>(), graphic.GetChild(0).GetComponent<ParticleSystem>(), graphic.GetChild(1).GetComponent<ParticleSystem>(), graphic.GetChild(2).GetComponent<ParticleSystem>()};
                aura.SetActive(false);
                SpawnManager.followers[playerIndex].Add(aura.transform);
                break;
            case "Earth":
                ability = Regeneration();
                break;
            case "Mars":
                ability = DustStorm();
                break;
            case "Jupiter":
                ability = Juggernaut();
                collisionInitials = new float[] {speedLossDivisor, healthLossDivisor, minHealthLoss, deflectionForce};
                break;
            case "Saturn" or "Uranus":
                ability = Ring();
                aura = Instantiate(aura);
                ring = aura.GetComponent<Ring>();
                ring.target = myTransform;
                auraSystems = new ParticleSystem[] {aura.transform.GetChild(0).GetComponent<ParticleSystem>()};
                aura.SetActive(false);
                SpawnManager.followers[playerIndex].Add(aura.transform);
                break;
            case "Neptune":
                ability = WindStorm();
                break;
        }

        //Spawn drones
        droneGroup = new GameObject("Drones " + playerNumberString).transform;
        droneGroup.gameObject.SetActive(false);
        drones = new Drone[droneNumber];
        for(int i = 0; i < droneNumber; i++)
        {
            GameObject drone = Instantiate(dronePrefab, droneGroup);
            Drone droneScript = drone.GetComponent<Drone>();
            droneScript.target = myTransform;
            drones[i] = droneScript;
            drone.SetActive(false);
            
        }
        droneGroup.gameObject.SetActive(true);
        SpawnManager.followers[playerIndex].Add(droneGroup);

        //Aura setup
        auraCount = auraSystems.Length;
        auraParticles = new ParticleSystem.MainModule[auraCount];
        for(int i = 0; i < auraCount; i++)
        {
            auraParticles[i] = auraSystems[i].main;
        }

        //Text setup
        Transform cameraStats = cameraObject.transform.GetChild(1);
        Transform farCanvas = cameraStats.GetChild(0);
        finishBackground = farCanvas.GetChild(0).gameObject;
        nameText = checkpointText = farCanvas.GetChild(1).GetComponent<TMP_Text>();
        checkpointText = farCanvas.GetChild(2).GetComponent<TMP_Text>();
        healthText = farCanvas.GetChild(3).GetComponent<TMP_Text>();
        collectText = farCanvas.GetChild(4).GetComponent<TMP_Text>();
        abilityText = farCanvas.GetChild(5).GetComponent<TMP_Text>();
        posText = farCanvas.GetChild(6).GetComponent<TMP_Text>();
        distanceText = farCanvas.GetChild(7).GetComponent<TMP_Text>();
        speedText = farCanvas.GetChild(8).GetComponent<TMP_Text>();
        timeText = farCanvas.GetChild(9).GetComponent<TMP_Text>();
        finishText = farCanvas.GetChild(10).GetComponent<TMP_Text>();
        countdownText = cameraStats.GetChild(1).GetChild(0).GetComponent<TMP_Text>();
        
        finishBackground.SetActive(false);
        nameText.SetText(MenuManager.playerNames[playerIndex]);
        checkpointText.SetText("Checkpoint: 0");
        healthText.SetText("Health: 100.00");
        collectText.SetText("Space Junk: 0");
        abilityText.SetText("");
        posText.SetText("Pos: 0.0,0.0,0.0");
        distanceText.SetText("Distance: 0.00");
        speedText.SetText("Speed: 0.00");
        timeText.SetText("Time: 0:00.000");
        countdownText.SetText("");
        finishText.SetText("");

        allText = new TMP_Text[]{checkpointText, healthText, speedText, collectText, abilityText, timeText, posText, distanceText};

        StartCoroutine(TextUpdate());
        StartCoroutine(Countdown(3));
    }

    // Update is called once per frame
    void Update()
    {
        float speed = rb.velocity.magnitude;
        
        if(!rb.isKinematic)
        {
            //Movement input
            yaw = Input.GetAxis("Yaw" + playerNumberString);
            pitch = Input.GetAxis("Pitch" + playerNumberString);
            roll = Input.GetAxis("Roll" + playerNumberString);
            thrust = Input.GetAxis("Thrust" + playerNumberString);

            //Thrust/glide control
            float accIncrement = acc * Time.deltaTime;
            float dec = Mathf.Pow(decMultiplier, Time.deltaTime * 60);
            float accThreshold = maxThrustSpeed - accIncrement;
            float decThreshold = initialThrustSpeed / dec;
            switch(thrust)
            {
                case > 0:
                    if(glideSpeed != thrustSpeed)
                    {
                        glideSpeed = thrustSpeed;
                    }
                    if(thrustSpeed < accThreshold)
                    {
                        thrustSpeed += accIncrement;
                    }
                    else if(thrustSpeed != maxThrustSpeed)
                    {
                        thrustSpeed = maxThrustSpeed;
                    }
                    if(decSpeed > decThreshold)
                    {
                        decSpeed *= dec;
                    }
                    else if(decSpeed != initialThrustSpeed)
                    {
                        decSpeed = initialThrustSpeed;
                    }
                    break;

                case < 0:
                    if(glideSpeed != -decSpeed)
                    {
                        glideSpeed = -decSpeed;
                    }
                    if(decSpeed < accThreshold)
                    {
                        decSpeed += accIncrement;
                    }
                    else if(decSpeed != maxThrustSpeed)
                    {
                        decSpeed = maxThrustSpeed;
                    }
                    if(thrustSpeed > decThreshold)
                    {
                        thrustSpeed *= dec;
                    }
                    else if(thrustSpeed != initialThrustSpeed)
                    {
                        thrustSpeed = initialThrustSpeed;
                    }
                    break;
                
                default:
                    float newGlide = glideSpeed - glideDrag * Time.deltaTime;
                    if(newGlide > 0)
                    {
                        glideSpeed = newGlide;
                    }
                    else
                    {
                        glideSpeed = 0;
                    }
                    break;
            }
        }

        //Change thruster effect based on speed
        float thrusterSpeed = speed / thrusterSpeedDivisor;
        if(thrusterSpeed > maxThrusterSpeed)
        {
            thrusterSpeed = maxThrusterSpeed;
        }
        else if(thrusterSpeed < 1)
        {
            thrusterSpeed = 1;
        }
        for(int i = 0; i < thrusterCount; i++)
        {
            ParticleSystem.MainModule thruster = thrusters[i];
            thruster.startSpeedMultiplier = thrusterSpeed;
        }
        float thrusterVolume = speed / thrusterVolumeDivisor;
        if(thrusterVolume > maxThrusterVolume)
        {
            thrusterVolume = maxThrusterVolume;
        }
        fireSource.volume = thrusterVolume;
        thrusterSource.volume = thrusterVolume;

        //Activate special ability
        if(Input.GetKeyDown(abilityKey))
        {
            if(currentCooldown == 0)
            {   
                currentCooldown = cooldownTime;
                if(runningAbility != null)
                {
                    StopCoroutine(runningAbility);
                }
                runningAbility = StartCoroutine(ability);
            }
            else
            {
                if((shipModel != "Saturn" && shipModel != "Uranus") || currentActivation == 0)
                {
                    StartCoroutine(ChangeTextColor(abilityText, 0.2f, Color.white, Color.red));
                    audioSource.PlayOneShot(errorSound);
                }
            }
        }

        if(Input.GetKey(abilityKey) && currentActivation > 0)
        {
            switch(shipModel)
            {
                case "Saturn":
                    ring.rotationOffset *= Quaternion.Euler(Vector3.right);
                    break;
                
                case "Uranus":
                    ring.rotationOffset *= Quaternion.Euler(new Vector3(0, 2, 0));
                    break;
            }
        }
    }

    void LateUpdate()
    {
        if(rb.isKinematic)
        {
            return;
        }
        droneGroup.localPosition = myTransform.localPosition;
        arrow.localPosition = Vector3.zero;
        arrow.rotation = Quaternion.LookRotation(rb.position.normalized, transform.up);
    }

    void FixedUpdate()
    {
        if(rb.isKinematic)
        {
            return;
        }

        //Roll, pitch, yaw
        if(yaw != 0 || pitch != 0 || roll != 0)
        {
            rb.AddRelativeTorque(-pitchSpeed * pitch, yawSpeed * yaw, -rollSpeed * roll, ForceMode.Acceleration);
        }

        //Thrust and glide
        switch(thrust)
        {
            case > 0:
                rb.AddRelativeForce(Vector3.forward * thrustSpeed * thrust, ForceMode.Acceleration);
                break;
            case < 0:
                rb.AddRelativeForce(Vector3.forward * decSpeed * thrust, ForceMode.Acceleration);
                break;
            case 0:
                rb.AddRelativeForce(Vector3.forward * glideSpeed, ForceMode.Acceleration);
                break;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(!canTrigger)
        {
            return;
        }

        //Passed section boundary
        if(other.gameObject.CompareTag("Section") && (sectionFront || sectionSide || sectionUp))
        {
            sectionFront = false;
            sectionSide = false;
            sectionUp = false;
        }

        else if(other.gameObject.CompareTag("Checkpoint") && checkpointTriggered)
        {
            checkpointTriggered = false;
        }
    
        //Passed finish line
        else if(other.gameObject.CompareTag("FinishLine"))
        {
            int playingPlayers = spawnManager.PlayerFinish(playerIndex);
            DateTime finishTime = DateTime.Now;
            TimeSpan elapsedTime = finishTime - startTime;
            //transform.parent = null;
            gameObject.SetActive(false);
            finishBackground.SetActive(true);
            audioSource.PlayOneShot(checkpointSound);
            int finishedPlayers = playerNumber - playingPlayers;
            switch(finishedPlayers)
            {
                case 1:
                    finishText.SetText("1st\nPlace!");
                    break;
                case 2:
                    finishText.SetText("2nd\nPlace!");
                    break;
                case 3:
                    finishText.SetText("3rd\nPlace!");
                    break;
                default:
                    finishText.SetText(finishedPlayers + "th\nPlace!");
                    break;
            }
            int elapsedMilliseconds = (int) Math.Round(elapsedTime.TotalMilliseconds);
            if(MenuManager.loaded && (elapsedMilliseconds < MenuManager.times[MenuManager.difficulty] || MenuManager.times[MenuManager.difficulty] == 0))
            {
                int index = MenuManager.difficulty;
                MenuManager.times[index] = elapsedMilliseconds;
                PlayerPrefs.SetInt("Time" + index, elapsedMilliseconds);
                PlayerPrefs.SetString("Description" + index, " (" + MenuManager.playerNames[playerIndex] + ", " + shipModel + " - " + finishTime.Month.ToString("00") + "/" + finishTime.Day.ToString("00") + "/" + finishTime.Year + ")");
            }
            MenuManager.collects += collects;
            PlayerPrefs.SetInt("Collects", MenuManager.collects);
            timeText.SetText("Time: " + elapsedTime.Minutes + ":" + elapsedTime.Seconds.ToString("00") + "." + elapsedTime.Milliseconds.ToString("000"));
        }

        //collects powerup
        else if(other.gameObject.CompareTag("Collectable"))
        {
            other.gameObject.SetActive(false);
            collects++;
            collectText.SetText("Space Junk: " + collects);
            StartCoroutine(ChangeTextColor(collectText, 0.2f, Color.white, Color.green));
            int number = UnityEngine.Random.Range(0, 100);
            if(number < abilityPercent + activationTimePercent)
            {
                activationTime++;
                currentActivation++;
            }
            else if(number < abilityPercent || cooldownTime <= minCooldown)
            {
                abilityStrength += abilityIncrease;
            }
            else
            {
                cooldownTime--;
                if(currentCooldown > 1)
                {
                    currentCooldown--;
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(!canTrigger)
        {
            return;
        }

        //Spawn new sections on trigger
        if(other.gameObject.CompareTag("Section"))
        {
            Vector3 sectionPos = other.transform.position;
            Vector3 pos = rb.position;
            float myX = pos.x;
            float myY = pos.y;
            float myZ = pos.z;
            if(sectionPos.x == myX - side || sectionPos.x == myX + side)
            {
                rb.position += Vector3.right;
            }
            if(sectionPos.z == myZ - side || sectionPos.z == myZ + side)
            {
                rb.position += Vector3.forward;
            }
            if(sectionPos.y == myY - side || sectionPos.y == myY + side)
            {
                rb.position += Vector3.up;
            }
            switch(other.name)
            {
                //Player moved forward or back
                case "SectionFront":
                    if(!sectionFront && myX > sectionPos.x - side && myX < sectionPos.x + side && myY > sectionPos.y - side && myY < sectionPos.y + side)
                    {
                        sectionFront = true;
                        if(myZ >= sectionPos.z)
                        {
                            spawnManager.SpawnSectionsForward(sectionPos, playerIndex);
                        }
                        else
                        {
                            spawnManager.SpawnSectionsBack(sectionPos, playerIndex);
                        }
                    }
                    break;

                //Player moved left or right
                case "SectionSide":
                    if(!sectionSide && myZ > sectionPos.z - side && myZ < sectionPos.z + side && myY > sectionPos.y - side && myY < sectionPos.y + side)
                    {
                        sectionSide = true;
                        if(myX >= sectionPos.x)
                        {
                            spawnManager.SpawnSectionsRight(sectionPos, playerIndex);
                        }
                        else
                        {
                            spawnManager.SpawnSectionsLeft(sectionPos, playerIndex);
                        }
                    }
                    break;

                //Player moved up or down
                case "SectionUp":
                    if(!sectionUp && myX > sectionPos.x - side && myX < sectionPos.x + side && myZ > sectionPos.z - side && myZ < sectionPos.z + side)
                    {
                        sectionUp = true;
                        if(myY >= sectionPos.y)
                        {
                            spawnManager.SpawnSectionsUp(sectionPos, playerIndex);
                        }
                        else
                        {
                            spawnManager.SpawnSectionsDown(sectionPos, playerIndex);
                        }
                    }
                    break;
            }
        }
    
        //Passed checkpoint
        else if(other.gameObject.CompareTag("Checkpoint") && !checkpointTriggered)
        {
            Vector3 pos = rb.position;
            checkpointTriggered = true;
            int playerCheckpoint = 0;
            switch(other.transform.parent.parent.name)
            {
                case "0":
                    playerCheckpoint = 0;
                    break;
                case "1":
                    playerCheckpoint = 1;
                    break;
                case "2":
                    playerCheckpoint = 2;
                    break;
                case "3":
                    playerCheckpoint = 3;
                    break;
                case "4":
                    playerCheckpoint = 4;
                    break;
                case "5":
                    playerCheckpoint = 5;
                    break;
                case "6":
                    playerCheckpoint = 6;
                    break;
                case "7":
                    playerCheckpoint = 7;
                    break;
            }
            if(Vector3.Angle(rb.position, rb.velocity) <= 90)
            {
                //Player moved out
                AddCheckpoint(playerCheckpoint);
            }
            else
            {
                //Player moved in
                RemoveCheckpoint(playerCheckpoint);
            }
        }
    }
    
    void OnCollisionEnter(Collision other)
    {
        if(collided || !canCollide)
        {
            return;
        }

        collided = true;
        float speed = rb.velocity.magnitude + other.gameObject.GetComponent<Rigidbody>().velocity.magnitude;
        Vector3 otherPos = other.transform.position;
        Vector3 pos = rb.position;
        rb.AddTorque(new Vector3(otherPos.y - pos.y, otherPos.x - pos.x, UnityEngine.Random.Range(-1f, 1)).normalized * speed);
        rb.angularVelocity *= deflectionForce;

        if(!other.gameObject.CompareTag("Drone"))
        {
            GameObject explosion = null;
            foreach(GameObject hitExplosion in hitExplosions)
            {
                if(!hitExplosion.activeInHierarchy)
                {
                    explosion = hitExplosion;
                    break;
                }
            }
            if(explosion == null)
            {
                explosion = Instantiate(hitExplosionPrefab, myTransform);
                hitExplosions.Add(explosion);
            }
            explosion.transform.position = other.GetContact(0).point;
            explosion.SetActive(true);
            audioSource.PlayOneShot(hitSounds[UnityEngine.Random.Range(0, hitSoundsCount)]);
            StartCoroutine(ResetEffect(explosion));
        }
        
        //Lose speed on other
        if(thrustSpeed > initialThrustSpeed * speedLossDivisor)
        {
            thrustSpeed /= speedLossDivisor;
        }
        else
        {
            thrustSpeed = initialThrustSpeed;
        }
        if(decSpeed > initialThrustSpeed * speedLossDivisor)
        {
            decSpeed /= speedLossDivisor;
        }
        else
        {
            decSpeed = initialThrustSpeed;
        }
        glideSpeed /= speedLossDivisor;

        //Lose health on other
        if(speed / healthLossDivisor > minHealthLoss)
        {
            health -= speed / healthLossDivisor;
        }
        else
        {
            health -= minHealthLoss;
        }

        //Player died
        switch(health)
        {
            case <= 0:
                canCollide = false;
                canTrigger = false;
                rb.isKinematic = true;

                dieExplosion.transform.position = other.GetContact(0).point;
                dieExplosion.SetActive(true);
                StartCoroutine(ResetEffect(dieExplosion));

                died = true;
                currentActivation = 0;
                currentCooldown = 0;
                aura.SetActive(false);
                if(resetting != null)
                {
                    StopCoroutine(resetting);
                }
                if(shipModel != "Venus")
                {
                    for(int i = 0; i < auraCount; i++)
                    {
                        auraParticles[i].loop = true;
                    }
                }
                else
                {
                    auraParticles[3].loop = true;
                }

                for(int i = 0; i < allText.Length; i++)
                {
                    StartCoroutine(ChangeTextColor(allText[i], 0.5f, Color.white, Color.red));
                }

                //Respawning
                int playerCheckpoint = spawnManager.playerCheckpoints[playerIndex];
                /*myTransform.parent = null;
                foreach(Transform follower in SpawnManager.followers[playerIndex])
                {
                    follower.parent = null;
                }*/
                switch(playerCheckpoint)
                {
                    case 0:
                        rb.position = startPos;
                        spawnManager.SpawnSectionsAround(Vector3.zero, playerIndex);
                        break;
                    default:
                        Vector3 newPos = startPos + normalizedStartPos * (checkpointDistances[spawnManager.playerCheckpoints[playerIndex] - 1] + colliderWidth);
                        rb.position = newPos;
                        spawnManager.SpawnSectionsAround(new Vector3((int) Mathf.Round(newPos.x / length), (int) Mathf.Round(newPos.y / length), (int) Mathf.Round(newPos.z / length)), playerIndex);
                        break;
                }

                for(int i = 0; i < activeDrones; i++)
                {
                    droneGroup.GetChild(i).GetComponent<Drone>().ResetDrone();
                }

                rb.rotation = startRot;
                thrustSpeed = initialThrustSpeed;
                health = 100;
                audioSource.PlayOneShot(dieSound);

                rb.isKinematic = false;
                canTrigger = true;
                canCollide = true;
                break;

            case <= 10:
                healthText.color = Color.red;
                break;
            
            default:
                StartCoroutine(ChangeTextColor(healthText, 0.2f, healthText.color, Color.red, 2));
                break;
        }
        health = (float) Math.Round(health, 2);
        healthText.SetText("Health: " + health.ToString("0.00"));
    }

    void OnCollisionExit(Collision other)
    {
        if(!canCollide)
        {
            return;
        }

        if(other.gameObject.CompareTag("Asteroid") || other.gameObject.layer == 6 || other.gameObject.CompareTag("Drone"))
        {
            collided = false;
        }
    }

    IEnumerator Countdown(int countdownTime)
    {
        spawnManager.initializedPlayers += 1;
        while(countdownTime > 0)
        {
            countdownText.SetText("" + countdownTime);
            countdownTime --;
            yield return new WaitForSeconds(1);
        }
        arrow.gameObject.SetActive(true);
        countdownText.SetText("Go!");
        yield return new WaitForSeconds(1);
        for(int i = 0; i < checkpointDrones[0]; i++)
        {
            droneGroup.GetChild(activeDrones).gameObject.SetActive(true);
            activeDrones++;
        }
        countdownText.gameObject.SetActive(false);
        thrusterParent.SetActive(true);
        rb.isKinematic = false;
        canCollide = true;
        canTrigger = true;
        StartCoroutine(TimeUpdate());
    }

    //Updates speed but not as fast as update
    IEnumerator TextUpdate()
    {
        while(true)
        {
            //Show speed in km/h, multiply by 2.23694 for mph
            speedText.SetText("Speed: " + (rb.velocity * 3.6f).magnitude.ToString("0.00"));

            //Show position in km, divide by 1609 for mi
            Vector3 pos = rb.position / 1000;
            posText.SetText("Pos: " + pos.x.ToString("0.0") + "," + pos.y.ToString("0.0") + "," + pos.z.ToString("0.0"));

            //Show distance from origin
            distanceText.SetText("Distance: " + new Vector2(pos.x, pos.z).magnitude.ToString("0.00"));
            yield return new WaitForSeconds(0.02f);
        }
    }

    IEnumerator TimeUpdate()
    {
        startTime = DateTime.Now;
        while(true)
        {
            //Show time
            TimeSpan elapsedTime = DateTime.Now - startTime;
            timeText.SetText("Time: " + elapsedTime.Minutes + ":" + elapsedTime.Seconds.ToString("00") + "." + elapsedTime.Milliseconds.ToString("000"));

            yield return new WaitForSeconds(0.001f);
        }
    }

    IEnumerator ChangeTextColor(TMP_Text text, float time, Color originalColor, Color newColor, int amount = 1)
    {
        if(originalColor == newColor)
        {
            originalColor = Color.white;
        }
        switch(amount)
        {
            case 1:
                text.color = newColor;
                yield return new WaitForSeconds(time);
                if(text.color == newColor)
                {
                    text.color = originalColor;
                }
                break;

            default:
                int amountMinusOne = amount - 1;
                for(int i = 0; i < amount; i++)
                {
                    text.color = newColor;
                    yield return new WaitForSeconds(time);
                    if(text.color == newColor)
                    {
                        text.color = originalColor;
                    }
                    if(i < amountMinusOne)
                    {
                        yield return new WaitForSeconds(time);
                    }
                }
                break;
        }
    }

    IEnumerator ResetEffect(GameObject effect, int waitTime = 2)
    {
        yield return new WaitForSeconds(waitTime);
        effect.SetActive(false);
    }

    public void Teleport()
    {
        canTrigger = false;
        canCollide = false;
        int playerCheckpoint = spawnManager.playerCheckpoints[playerIndex];
        int direction = UnityEngine.Random.Range(0, 100);
        float distance;
        switch(playerCheckpoint)
        {
            case 0:
                if(direction < forwardPercent)
                {
                    AddCheckpoint(playerCheckpoint);
                    playerCheckpoint += 1;
                    distance = UnityEngine.Random.Range(checkpointRadii[playerCheckpoint - 1] + colliderWidth + 20, checkpointDistances[playerCheckpoint] - colliderWidth - 20);
                    break;
                }
                distance = UnityEngine.Random.Range(0f, checkpointDistances[playerCheckpoint] - colliderWidth - 20);
                break;

            case 1:
                if(direction < forwardPercent)
                {
                    AddCheckpoint(playerCheckpoint);
                    playerCheckpoint += 1;
                    distance = UnityEngine.Random.Range(checkpointRadii[playerCheckpoint - 1] + colliderWidth + 20, checkpointDistances[playerCheckpoint] - colliderWidth - 20);
                    break;
                }
                else if(direction < forwardPercent + backPercent)
                {
                    playerCheckpoint -= 1;
                    RemoveCheckpoint(playerCheckpoint);
                    distance = UnityEngine.Random.Range(0f, checkpointDistances[playerCheckpoint] - colliderWidth - 5);
                    break;
                }
                distance = UnityEngine.Random.Range(checkpointRadii[playerCheckpoint - 1] + colliderWidth + 20, checkpointDistances[playerCheckpoint] - colliderWidth - 20);
                break;
            
            case 8:
                if(direction < backPercent)
                {
                    playerCheckpoint -= 1;
                    RemoveCheckpoint(playerCheckpoint);
                }
                distance = UnityEngine.Random.Range(checkpointRadii[playerCheckpoint - 1] + colliderWidth + 20, checkpointDistances[playerCheckpoint] - colliderWidth - 20);
                break;

            default:
                if(direction < forwardPercent)
                {
                    AddCheckpoint(playerCheckpoint);
                    playerCheckpoint += 1;
                }
                else if(direction < forwardPercent + backPercent)
                {
                    playerCheckpoint -= 1;
                    RemoveCheckpoint(playerCheckpoint);
                }
                distance = UnityEngine.Random.Range(checkpointRadii[playerCheckpoint - 1] + colliderWidth + 20, checkpointDistances[playerCheckpoint] - colliderWidth - 20);
                break;
        }
        Vector3 newPos = Quaternion.Euler(UnityEngine.Random.Range(0f, 360), UnityEngine.Random.Range(0f, 360), 0) * new Vector3(0, 0, distance);
        //myTransform.parent = null;
        rb.position = newPos;
        spawnManager.SpawnSectionsAround(new Vector3(Mathf.RoundToInt(newPos.x / length), Mathf.RoundToInt(newPos.y / length), Mathf.RoundToInt(newPos.z / length)), playerIndex);
        audioSource.PlayOneShot(wormholeSound);
        canCollide = true;
        canTrigger = true;
    }

    void AddCheckpoint(int playerCheckpoint)
    {
        playerCheckpoint += 1;
        if(spawnManager.playerCheckpoints[playerIndex] != playerCheckpoint)
        {
            spawnManager.playerCheckpoints[playerIndex] = playerCheckpoint;
            //spawnManager.SpawnCheckpoints(playerIndex, playerCheckpoint);
            checkpointText.SetText("Checkpoint: " + playerCheckpoint);
        }
        int dronesToActivate = checkpointDrones[playerCheckpoint] - activeDrones;
        for(int i = 0; i < dronesToActivate; i++)
        {
            droneGroup.GetChild(activeDrones).gameObject.SetActive(true);
            activeDrones++;
        }
        StartCoroutine(ChangeTextColor(checkpointText, 0.5f, Color.white, Color.green));
        audioSource.PlayOneShot(checkpointSound);
    }

    void RemoveCheckpoint(int playerCheckpoint)
    {
        if(spawnManager.playerCheckpoints[playerIndex] != playerCheckpoint)
        {
            spawnManager.playerCheckpoints[playerIndex] = playerCheckpoint;
            //spawnManager.SpawnCheckpoints(playerIndex, playerCheckpoint - 1);
            checkpointText.SetText("Checkpoint: " + playerCheckpoint);
        }
        StartCoroutine(ChangeTextColor(checkpointText, 0.5f, Color.white, Color.red));
        audioSource.PlayOneShot(errorSound);
    }

    //Abilities
    IEnumerator SpeedUp()
    {
        //Activation
        ability = SpeedUp();
        audioSource.PlayOneShot(activationSound);
        aura.SetActive(true);
        rb.AddRelativeForce(Vector3.forward * abilityStrength, ForceMode.VelocityChange);
        resetting = StartCoroutine(ResetAura(2));

        //Cooldown
        while(currentCooldown > 0)
        {
            abilityText.SetText("Charging: " + currentCooldown);
            currentCooldown --;
            yield return new WaitForSeconds(1);
        }
        if(!died)
        {
            abilityText.SetText("Ability Charged!");
            audioSource.PlayOneShot(chargeSound);
            yield return new WaitForSeconds(1);
        }
        abilityText.SetText("");
    }

    IEnumerator Atmosphere()
    {
        //Activation
        currentActivation = activationTime;
        died = false;
        ability = Atmosphere();
        for(int i = 0; i < 3; i++)
        {
            auraParticles[i].duration = auraParticles[i].startLifetimeMultiplier = activationTime;
        }
        aura.SetActive(true);
        audioSource.PlayOneShot(activationSound);
        while(currentActivation > 0)
        {
            abilityText.SetText("Activated: " + currentActivation);
            currentActivation --;
            yield return new WaitForSeconds(1);
        }
        auraParticles[2].loop = false;
        resetting = StartCoroutine(ResetAura(2, 3));
        audioSource.PlayOneShot(expireSound);

        //Cooldown
        while(currentCooldown > 0)
        {
            abilityText.SetText("Charging: " + currentCooldown);
            currentCooldown --;
            yield return new WaitForSeconds(1);
        }
        if(!died)
        {
            abilityText.SetText("Ability Charged!");
            audioSource.PlayOneShot(chargeSound);
            yield return new WaitForSeconds(1);
        }
        abilityText.SetText("");
    }

    IEnumerator Regeneration()
    {
        //Activation
        currentActivation = activationTime;
        died = false;
        ability = Regeneration();
        aura.SetActive(true);
        audioSource.PlayOneShot(activationSound);
        while(currentActivation > 0)
        {
            abilityText.SetText("Activated: " + currentActivation);
            currentActivation --;
            if(health + abilityStrength < 100)
            {
                health += abilityStrength;
            }
            else
            {
                health = 100;
            }
            health = (float) Math.Round(health, 2);
            healthText.SetText("Health: " + health.ToString("0.00"));
            yield return new WaitForSeconds(1);
        }
        for(int i = 0; i < auraCount; i++)
        {
            auraParticles[i].loop = false;
        }
        resetting = StartCoroutine(ResetAura(2));
        audioSource.PlayOneShot(expireSound);

        //Cooldown
        while(currentCooldown > 0)
        {
            abilityText.SetText("Charging: " + currentCooldown);
            currentCooldown --;
            yield return new WaitForSeconds(1);
        }
        if(!died)
        {
            abilityText.SetText("Ability Charged!");
            audioSource.PlayOneShot(chargeSound);
            yield return new WaitForSeconds(1);
        }
        abilityText.SetText("");
    }

    IEnumerator DustStorm()
    {
        //Activation
        currentActivation = activationTime;
        died = false;
        ability = DustStorm();
        aura.SetActive(true);
        audioSource.PlayOneShot(activationSound);
        for(int i = 0; i < droneNumber; i++)
        {
            drones[i].offset = abilityStrength;
        }
        while(currentActivation > 0)
        {
            abilityText.SetText("Activated: " + currentActivation);
            currentActivation --;
            yield return new WaitForSeconds(1);
        }
        for(int i = 0; i < auraCount; i++)
        {
            auraParticles[i].loop = false;
        }
        resetting = StartCoroutine(ResetAura(2));
        audioSource.PlayOneShot(expireSound);
        for(int i = 0; i < droneNumber; i++)
        {
            drones[i].offset = 0;
        }

        //Cooldown
        while(currentCooldown > 0)
        {
            abilityText.SetText("Charging: " + currentCooldown);
            currentCooldown --;
            yield return new WaitForSeconds(1);
        }
        if(!died)
        {
            abilityText.SetText("Ability Charged!");
            audioSource.PlayOneShot(chargeSound);
            yield return new WaitForSeconds(1);
        }
        abilityText.SetText("");
    }

    IEnumerator Juggernaut()
    {
        //Activation
        currentActivation = activationTime;
        died = false;
        ability = Juggernaut();
        aura.SetActive(true);
        audioSource.PlayOneShot(activationSound);
        speedLossDivisor = 1;
        healthLossDivisor = 1000;
        minHealthLoss = 0;
        deflectionForce = 1;
        rb.mass *= abilityStrength;
        while(currentActivation > 0)
        {
            abilityText.SetText("Activated: " + currentActivation);
            currentActivation --;
            yield return new WaitForSeconds(1);
        }
        for(int i = 0; i < auraCount; i++)
        {
            auraParticles[i].loop = false;
        }
        resetting = StartCoroutine(ResetAura(2));
        audioSource.PlayOneShot(expireSound);
        speedLossDivisor = collisionInitials[0];
        healthLossDivisor = (int) collisionInitials[1];
        minHealthLoss = collisionInitials[2];
        deflectionForce = (int) collisionInitials[3];
        rb.mass /= abilityStrength;

        //Cooldown
        while(currentCooldown > 0)
        {
            abilityText.SetText("Charging: " + currentCooldown);
            currentCooldown --;
            yield return new WaitForSeconds(1);
        }
        if(!died)
        {
            abilityText.SetText("Ability Charged!");
            audioSource.PlayOneShot(chargeSound);
            yield return new WaitForSeconds(1);
        }
        abilityText.SetText("");
    }

    IEnumerator Ring()
    {
        //Activation
        currentActivation = activationTime;
        died = false;
        ability = Ring();
        ring.hitForce = abilityStrength;
        aura.SetActive(true);
        audioSource.PlayOneShot(activationSound);
        while(currentActivation > 0)
        {
            abilityText.SetText("Activated: " + currentActivation);
            currentActivation --;
            yield return new WaitForSeconds(1);
        }
        for(int i = 0; i < auraCount; i++)
        {
            auraParticles[i].loop = false;
        }
        resetting = StartCoroutine(ResetAura(1));
        audioSource.PlayOneShot(expireSound);

        //Cooldown
        while(currentCooldown > 0)
        {
            abilityText.SetText("Charging: " + currentCooldown);
            currentCooldown --;
            yield return new WaitForSeconds(1);
        }
        if(!died)
        {
            abilityText.SetText("Ability Charged!");
            audioSource.PlayOneShot(chargeSound);
            yield return new WaitForSeconds(1);
        }
        abilityText.SetText("");
    }

    IEnumerator WindStorm()
    {
        //Activation
        ability = WindStorm();
        audioSource.PlayOneShot(activationSound);
        aura.SetActive(true);
        Collider[] colliders = Physics.OverlapSphere(rb.position, 1000);
        for(int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            Rigidbody otherRb = collider.GetComponent<Rigidbody>();
            if(otherRb == null || collider.gameObject == gameObject || otherRb.isKinematic)
            {
                continue;
            }
            if(collider.gameObject.CompareTag("Drone"))
            {
                collider.gameObject.GetComponent<Drone>().Explode(collider.transform.position, transform);
            }
            Vector3 direction = collider.transform.position - rb.position;
            otherRb.AddForce(direction * (abilityStrength / direction.magnitude), ForceMode.Impulse);
        }
        resetting = StartCoroutine(ResetAura(1));

        //Cooldown
        while(currentCooldown > 0)
        {
            abilityText.SetText("Charging: " + currentCooldown);
            currentCooldown --;
            yield return new WaitForSeconds(1);
        }
        if(!died)
        {
            abilityText.SetText("Ability Charged!");
            audioSource.PlayOneShot(chargeSound);
            yield return new WaitForSeconds(1);
        }
        abilityText.SetText("");
    }

    IEnumerator ResetAura(int waitTime, int startIndex = 0)
    {
        yield return new WaitForSeconds(waitTime);
        aura.SetActive(false);
        for(int i = startIndex; i < auraCount; i++)
        {
            auraParticles[i].loop = true;
        }
        if(!canCollide)
        {
            canCollide = true;
        }
    }
}

