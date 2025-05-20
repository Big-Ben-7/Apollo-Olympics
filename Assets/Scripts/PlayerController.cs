using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    [SerializeField] int abilityStrength2;
    [SerializeField] int accFuelLoss = 2;
    [SerializeField] int rotFuelLoss = 1;
    [SerializeField] float fuelRegeneration = 0.2f;

    [Header("Movement")]
    [SerializeField] GameObject thrusterParent;
    [SerializeField] float thrusterSpeedDivisor = 100;
    [SerializeField] float maxThrusterSpeed = 15;

    [Header("Collision")]
    [SerializeField] GameObject dieExplosion;
    [SerializeField] List<GameObject> hitExplosions;
    [SerializeField] GameObject collectEffect;
    [SerializeField] GameObject dronePrefab;
    [SerializeField] GameObject graphic;

    [Header("Ability")]
    [SerializeField] GameObject aura;
    [SerializeField] ParticleSystem[] auraSystems;
    [SerializeField] LayerMask abilityMask;
    

    [Header("UI/Audio")]
    [SerializeField] Color green;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioSource voiceSource;
    [SerializeField] AudioSource fireSource;
    [SerializeField] AudioClip[] hitSounds;
    [SerializeField] AudioClip noFuelSound;
    [SerializeField] AudioClip collectSound;
    [SerializeField] AudioClip checkpointSound;
    [SerializeField] AudioClip activationSound;
    [SerializeField] AudioClip expireSound;
    [SerializeField] AudioClip chargeSound;
    [SerializeField] AudioClip errorSound;
    [SerializeField] AudioClip wormholeSound;
    [SerializeField] AudioClip warningSound;
    [SerializeField] AudioClip dieSound;
    [SerializeField] AudioClip checkpointVoice;
    [SerializeField] AudioClip backCheckpointVoice;
    [SerializeField] AudioClip wormholeVoice;
    [SerializeField] AudioClip lowFuelVoice;
    [SerializeField] AudioClip noFuelVoice;
    [SerializeField] AudioClip lowHealthVoice;
    [SerializeField] AudioClip dieVoice;
    
    //Movement control
    Rigidbody rb;
    public Transform myTransform;
    public string playerNumberString;
    public int playerIndex;
    float initialThrustSpeed;
    float acc;
    float maxThrustSpeed;
    float thrustSpeed;
    float decSpeed;
    float glideSpeed;
    InputAction thrustAction;
    float thrust;
    InputAction pitchAction;
    InputAction yawAction;
    InputAction rollAction;
    Vector3 rotation;
    float rotationSpeed;
    float decMultiplier;
    public Vector3 startPos;
    public Vector3 normalizedStartPos;
    public Quaternion startRot;

    //Collisions/triggers
    float speedLossDivisor;
    int healthLossDivisor;
    float deflectionForce;
    float health = 100;
    float fuel = 100;
    bool collided;
    public bool canCollide;
    bool canTrigger;
    public static SpawnManager spawnManager;
    public static int length;
    public static int[] checkpointDistances;
    public static int lastCheckpointIndex;
    int secondLastCheckpointIndex;
    public static float lastCheckpointDistance;
    float[] collisionInitials;
    ParticleSystem.MainModule[] thrusters;
    int thrusterCount;
    int hitSoundsCount;
    public int collects;
    GameObject hitExplosionPrefab;
    int forwardPercent = 35;
    int backPercent = 25;
    int droneCaptionNumber;
    int crashNumber;
    int fuelNumber;
    int noFuelNumber;
    int deathNumber;
    bool regenerateFuel;

    //Ability
    IEnumerator ability;
    Coroutine runningAbility;
    InputAction abilityAction;
    int currentActivation;
    int currentCooldown;
    int auraCount;
    ParticleSystem.MainModule[] auraParticles;
    bool loopingAura = true;
    bool ringAbility;
    Atmosphere atmosphere;
    Ring ring;
    Vector3 ringRotation;
    bool died;
    Coroutine resetting;
    public static WaitForSeconds oneSecond;
    public static WaitForSeconds twoSeconds;
    public static WaitForSeconds threeSeconds;
    public static WaitForSeconds fifthSecond;
    public static WaitForSeconds halfSecond;
    public static WaitForSeconds threeTenthSeconds;
    Collider[] surroundingColliders = new Collider[10000];

    //Drone
    Transform droneGroup;
    public static int droneNumber;
    public static int[] checkpointDrones;
    int activeDrones;
    Drone[] droneScripts;
    GameObject[] drones;

    //UI
    public Transform uiCanvas;
    public Transform shipCanvas;
    TMP_Text healthLabel;
    Image healthBar;
    TMP_Text healthText;
    TMP_Text fuelLabel;
    Image fuelBar;
    TMP_Text fuelText;
    TMP_Text collectText;
    TMP_Text checkpointText;
    TMP_Text abilityText;
    TMP_Text nameText;
    TMP_Text posText;
    TMP_Text distanceText;
    TMP_Text speedText;
    TMP_Text timeText;
    TMP_Text caption;
    GameObject finishShip;
    GameObject confetti;
    TMP_Text finishText;
    GameObject finishCaption;
    TMP_Text deathText;
    TMP_Text countdownText;
    TMP_Text[] allText;
    int allTextLength;
    GameObject finishBackground;
    Coroutine settingCaption;
    public Transform arrow;
    public static string[] playerNames;
    public int shipIndex;
    public static float maxThrusterVolume;
    public static float thrusterVolumeDivisor;
    public static float audioVolume;
    public static float voiceVolume;
    public float stereoPan;
    public static bool tutorial;
    public static bool showCaptions;
    public static bool showArrow;
    public static bool paused;
    public bool kinematic = true;
    bool pauseTutorial;
    string tutorialText;
    int captionPriority = 0;
    bool errorPlaying;
    public bool inTutorial;
    bool canFinish = true;
    bool rotAllowed = true;
    bool thrustAllowed = true;
    bool abilityAllowed = true;
    public bool dronesConfused = true;
    bool debrisCollected = true;
    bool teleported = true;
    string checkpointEnd;
    string[] droneCaptions = new string[]
    {
        "Nice Move!",
        "Well Played!",
        "Wipeout!",
        "Clean Sweep!",
        "Bravo!",
        "Pirates Out!",
        "Bye-bye Pirates!",
        "Pirates Eliminated!"
    };
    string[] crashCaptions = new string[] 
    {
        "Watch Your Health!",
        "Warning: Low Health!",
        "Low On Health!"
    };
    string[] fuelCaptions = new string[]
    {
        "Warning: Low Fuel!\nCollect Space Debris!",
        "Low On Fuel!\nCollect Space Debris!"
    };
    string[] noFuelCaptions = new string[]
    {
        "Out Of Fuel!\n Collect Space Debris!",
        "No Fuel!\n Collect Space Debris!",
        "Fuel Tank Empty!\n Collect Space Debris!"
    };
    string[] deathCaptions = new string[] 
    {
        "Mission Impossible?",
        "Better luck next time...",
        "So close,\nyet so far...",
        "Lost in space...",
        "Never Give Up!",
        "Keep Going!",
        "Down,\nbut not out!",
        "Nice try.",
        "The best is\nyet to come...",
        "Time to\nbounce back?",
        "Persistance\nis key.",
        "Inspirational effort.",
    };
    string[] checkpointCaptions = new string[] 
    {
        "Mercury orbit", 
        "Venus orbit", 
        "Earth orbit", 
        "Mars orbit\nEntering asteroid belt", 
        "Jupiter orbit", 
        "Saturn orbit",
        "Uranus orbit",
        "Neptune orbit\nEntering Kuiper belt"
    };
    string[] backCaptions = new string[] 
    {
        "Mercury orbit", 
        "Venus orbit", 
        "Earth orbit", 
        "Mars orbit", 
        "Jupiter orbit", 
        "Saturn orbit",
        "Uranus orbit",
        "Neptune orbit"
    };
    string[] shipModels = new string[]
    {
        "Mercury",
        "Venus",
        "Earth",
        "Mars",
        "Jupiter",
        "Saturn",
        "Uranus",
        "Neptune"
    };
    Dictionary<string, string> wordNames = new Dictionary<string, string>
    {
        {".", "Period"},
        {",", "Comma"},
        {";", "Semicolon"},
        {"'", "Apostrophe"},
        {"`", "Backtick"},
    };
    void Awake()
    {
        dieExplosion.SetActive(false);
        hitExplosionPrefab = hitExplosions[0];
        hitExplosionPrefab.SetActive(false);
        aura.SetActive(false);
        collectEffect.SetActive(false);
        thrusterParent.SetActive(false);
        fireSource.volume = 0;

        rb = gameObject.GetComponent<Rigidbody>();
        rb.isKinematic = true;

        Transform thrusterTransform = thrusterParent.transform;
        thrusterCount = thrusterTransform.childCount;
        thrusters = new ParticleSystem.MainModule[thrusterCount];
        for(int i = 0; i < thrusterCount; i++) thrusters[i] = thrusterTransform.GetChild(i).GetComponent<ParticleSystem>().main;
        hitSoundsCount = hitSounds.Length;

        droneCaptionNumber = droneCaptions.Length;
        crashNumber = crashCaptions.Length;
        fuelNumber = fuelCaptions.Length;
        noFuelNumber = noFuelCaptions.Length;
        deathNumber = deathCaptions.Length;
        checkpointEnd = "/" + lastCheckpointIndex.ToString();
        secondLastCheckpointIndex = lastCheckpointIndex - 1;
    }

    //Player is tagged by SpawnManager after Awake
    void Start()
    {
        //Ability setup
        pitchAction = InputSystem.actions.FindAction("Player" + playerNumberString + "/Pitch");
        yawAction = InputSystem.actions.FindAction("Player" + playerNumberString + "/Yaw");
        rollAction = InputSystem.actions.FindAction("Player" + playerNumberString + "/Roll");
        thrustAction = InputSystem.actions.FindAction("Player" + playerNumberString + "/Thrust");
        abilityAction = InputSystem.actions.FindAction("Player" + playerNumberString + "/Ability");

        //Customize
        speed --;
        agility --;
        durability --;
        stability --;
        initialThrustSpeed = spawnManager.initialThrustSpeeds[speed];
        acc = spawnManager.accelerations[speed];
        maxThrustSpeed = spawnManager.maxThrustSpeeds[speed];
        rotationSpeed = spawnManager.rotationSpeeds[agility];
        decMultiplier = spawnManager.decMultipliers[agility];
        healthLossDivisor = spawnManager.healthLossDivisors[durability];
        speedLossDivisor = spawnManager.speedLossDivisors[stability];
        deflectionForce = spawnManager.deflectionForces[stability];
        thrustSpeed = decSpeed = initialThrustSpeed;

        switch(shipIndex)
        {
            case 0:
                ability = SpeedUp();
                finishShip = shipCanvas.GetChild(0).gameObject;
                loopingAura = false;
                break;
            case 1:
                ability = Atmosphere();
                aura = Instantiate(aura);
                atmosphere = aura.GetComponent<Atmosphere>();
                atmosphere.target = myTransform;
                atmosphere.hitForce = abilityStrength;
                atmosphere.audioSource = audioSource;
                Transform graphic = aura.transform.GetChild(0);
                auraSystems = new ParticleSystem[] {graphic.GetComponent<ParticleSystem>(), graphic.GetChild(0).GetComponent<ParticleSystem>(), graphic.GetChild(1).GetComponent<ParticleSystem>(), graphic.GetChild(2).GetComponent<ParticleSystem>()};
                aura.SetActive(false);
                SpawnManager.followers[playerIndex].Add(aura);
                finishShip = shipCanvas.GetChild(1).gameObject;
                loopingAura = false;
                break;
            case 2:
                ability = Regeneration();
                finishShip = shipCanvas.GetChild(2).gameObject;
                break;
            case 3:
                ability = DustStorm();
                finishShip = shipCanvas.GetChild(3).gameObject;
                break;
            case 4:
                ability = Juggernaut();
                collisionInitials = new float[] {speedLossDivisor, healthLossDivisor, deflectionForce, rb.mass};
                finishShip = shipCanvas.GetChild(4).gameObject;
                break;
            case 5:
                ability = Ring();
                aura = Instantiate(aura);
                ring = aura.GetComponent<Ring>();
                ring.target = myTransform;
                ring.hitForce = abilityStrength;
                ring.audioSource = audioSource;
                auraSystems = new ParticleSystem[] {aura.transform.GetChild(0).GetComponent<ParticleSystem>()};
                aura.SetActive(false);
                SpawnManager.followers[playerIndex].Add(aura);
                finishShip = shipCanvas.GetChild(5).gameObject;
                ringRotation = new Vector3(abilityStrength2, 0, 0);
                ringAbility = true;
                break;
            case 6:
                ability = Ring();
                aura = Instantiate(aura);
                ring = aura.GetComponent<Ring>();
                ring.target = myTransform;
                ring.hitForce = abilityStrength;
                ring.audioSource = audioSource;
                auraSystems = new ParticleSystem[] {aura.transform.GetChild(0).GetComponent<ParticleSystem>()};
                aura.SetActive(false);
                SpawnManager.followers[playerIndex].Add(aura);
                finishShip = shipCanvas.GetChild(6).gameObject;
                ringRotation = new Vector3(0, abilityStrength2, 0);
                ringAbility = true;
                break;
            case 7:
                ability = WindStorm();
                finishShip = shipCanvas.GetChild(7).gameObject;
                loopingAura = false;
                break;
        }
        confetti = shipCanvas.GetChild(8).gameObject;
        confetti.SetActive(false);
        spawnManager.confettis[playerIndex] = confetti;

        //Spawn drones
        droneGroup = new GameObject("Drones " + playerNumberString).transform;
        droneGroup.gameObject.SetActive(false);
        droneScripts = new Drone[droneNumber];
        drones = new GameObject[droneNumber];
        for(int i = 0; i < droneNumber; i++)
        {
            GameObject drone = drones[i] = Instantiate(dronePrefab, droneGroup);
            Drone droneScript = droneScripts[i] = drone.GetComponent<Drone>();
            droneScript.target = myTransform;
            droneScript.targetRb = rb;
            droneScript.playerController = this;
            droneScript.audioSource = audioSource;
            drone.SetActive(false);
            
        }
        droneGroup.gameObject.SetActive(true);
        SpawnManager.followers[playerIndex].Add(droneGroup.gameObject);

        //Aura setup
        if(loopingAura)
        {
            auraCount = auraSystems.Length;
            auraParticles = new ParticleSystem.MainModule[auraCount];
            for(int i = 0; i < auraCount; i++) auraParticles[i] = auraSystems[i].main;
        }

        //UI setup
        finishBackground = uiCanvas.GetChild(0).gameObject;
        Transform healthParent = uiCanvas.GetChild(1);
            healthLabel = healthParent.GetChild(0).GetComponent<TMP_Text>();
            healthBar = healthParent.GetChild(3).GetComponent<Image>();
            healthText = healthParent.GetChild(4).GetComponent<TMP_Text>();
        Transform fuelParent = uiCanvas.GetChild(2);
            fuelLabel = fuelParent.GetChild(0).GetComponent<TMP_Text>();
            fuelBar = fuelParent.GetChild(3).GetComponent<Image>();
            fuelText = fuelParent.GetChild(4).GetComponent<TMP_Text>();
        collectText = uiCanvas.GetChild(3).GetComponent<TMP_Text>();
        checkpointText = uiCanvas.GetChild(4).GetComponent<TMP_Text>();
        abilityText = uiCanvas.GetChild(5).GetComponent<TMP_Text>();
        nameText = uiCanvas.GetChild(6).GetComponent<TMP_Text>();
        posText = uiCanvas.GetChild(7).GetComponent<TMP_Text>();
        distanceText = uiCanvas.GetChild(8).GetComponent<TMP_Text>();
        speedText = uiCanvas.GetChild(9).GetComponent<TMP_Text>();
        timeText = uiCanvas.GetChild(10).GetComponent<TMP_Text>();
        spawnManager.captions[playerIndex] = caption = uiCanvas.GetChild(11).GetComponent<TMP_Text>();
        spawnManager.finishTexts[playerIndex] = finishText = uiCanvas.GetChild(12).GetComponent<TMP_Text>();
        finishCaption = uiCanvas.GetChild(13).gameObject;
        deathText = uiCanvas.GetChild(14).GetComponent<TMP_Text>();
        countdownText = uiCanvas.GetChild(15).GetComponent<TMP_Text>();
        
        finishBackground.SetActive(false);
        healthBar.fillAmount = fuelBar.fillAmount = 1;
        fuelText.SetText("100.00");
        healthText.SetText("100.00");
        collectText.SetText("Space Debris: 0");
        checkpointText.SetText("Checkpoint: 0" + checkpointEnd);
        abilityText.SetText("");
        nameText.SetText(playerNames[playerIndex]);
        Vector3 displayPos = rb.position / 600;
        posText.SetText("Pos: " + displayPos.x.ToString("0.0") + "," + displayPos.y.ToString("0.0") + "," + displayPos.z.ToString("0.0"));
        distanceText.SetText("Distance: " + displayPos.magnitude.ToString("0.0"));
        speedText.SetText("Speed: 0.00");
        caption.SetText("");
        finishText.SetText("");
        finishCaption.SetActive(false);
        deathText.SetText("");
        countdownText.SetText("");
        timeText.SetText("Time: 0:00.000");

        allText = new TMP_Text[]{nameText, checkpointText, healthLabel, healthText, fuelLabel, fuelText, speedText, collectText, abilityText, timeText, posText, distanceText};
        allTextLength = allText.Length;

        audioSource.volume = audioVolume;
        voiceSource.volume = voiceVolume;
        audioSource.panStereo = voiceSource.panStereo = fireSource.panStereo = stereoPan;
        spawnManager.effectSources[playerIndex] = audioSource;
        spawnManager.voiceSources[playerIndex] = voiceSource;

        StartCoroutine(Countdown(3));
    }

    // Update is called once per frame
    void Update()
    {
        if(paused) return;

        //Show time
        timeText.SetText(spawnManager.timeString);

        if(kinematic) return;

        //Activate special ability
        if(abilityAllowed)
        {
            if(abilityAction.WasPressedThisFrame())
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
                    if(!ringAbility || currentActivation == 0)
                    {
                        StartCoroutine(ChangeTextColor(abilityText, fifthSecond, Color.white, Color.red));
                        audioSource.PlayOneShot(errorSound);
                    }
                }
            }

            if(ringAbility && abilityAction.IsPressed() && currentActivation > 0) ring.rotationOffset *= Quaternion.Euler(ringRotation * Time.deltaTime);
        }

        //Movement input
        if(rotAllowed) rotation = new Vector3(-pitchAction.ReadValue<float>(), yawAction.ReadValue<float>(), -rollAction.ReadValue<float>());
        if(thrustAllowed) thrust = thrustAction.ReadValue<float>();

        //Thrust/glide control
        float accIncrement = acc * Time.deltaTime;
        float dec = 1;
        float decThreshold = initialThrustSpeed;
        if(decMultiplier != 1)
        {
            dec = Mathf.Pow(decMultiplier, Time.deltaTime * 60);
            decThreshold = initialThrustSpeed / dec;
        }
        float accThreshold = maxThrustSpeed - accIncrement;
        float accFuelIncrement = accFuelLoss * Time.deltaTime;
        float rotFuelIncrement = rotFuelLoss * Time.deltaTime;
        switch(thrust)
        {
            case > 0:
                if(fuel > accFuelIncrement)
                {
                    glideSpeed = thrustSpeed;
                    
                    if(thrustSpeed < accThreshold) thrustSpeed += accIncrement;
                    else thrustSpeed = maxThrustSpeed;
                    
                    fuel -= accFuelIncrement;
                }
                else fuel = 0;
                
                if(decSpeed > decThreshold && dec != 1) decSpeed *= dec;
                else decSpeed = initialThrustSpeed;
                break;

            case < 0:
                if(fuel > accFuelIncrement)
                {
                    glideSpeed = 0;

                    if(decSpeed < accThreshold) decSpeed += accIncrement;
                    else decSpeed = maxThrustSpeed;

                    fuel -= accFuelIncrement;
                }
                else fuel = 0;

                if(thrustSpeed > decThreshold && dec != 1) thrustSpeed *= dec;
                else thrustSpeed = initialThrustSpeed;
                break;
        
            case 0:
                if(rotation != Vector3.zero)
                {
                    if(fuel > rotFuelIncrement) fuel -= rotFuelIncrement;
                    else fuel = 0;
                }
                else if(regenerateFuel) fuel += fuelRegeneration * Time.deltaTime;
                break;
        }
        
        //Fuel management
        switch(fuel)
        {
            case < 0.005f:
                if(fuelText.text != "0.00")
                {
                    fuelText.SetText("0.00");
                    if(captionPriority <= 6)
                    {
                        if(settingCaption != null) StopCoroutine(settingCaption);
                        settingCaption = StartCoroutine(SetCaption(noFuelCaptions[UnityEngine.Random.Range(0, noFuelNumber)], threeSeconds, 6));
                    }
                    StartCoroutine(ChangeTextColor(caption, fifthSecond, Color.white, Color.red, 3));
                    audioSource.PlayOneShot(noFuelSound);
                    voiceSource.clip = noFuelVoice;
                    voiceSource.PlayDelayed(0.5f);
                }
                if(!regenerateFuel) regenerateFuel = true;
                break;
            case < 10:
                string fuelString = fuel.ToString("0.00");
                if(fuelText.text != "fuelString") fuelText.SetText(fuelString);
                break;
            case <= 20:
                if(fuelText.color != Color.red)
                {
                    fuelLabel.color = fuelText.color = Color.red;
                    if(showCaptions && captionPriority <= 5)
                    {
                        if(settingCaption != null) StopCoroutine(settingCaption);
                        settingCaption = StartCoroutine(SetCaption(fuelCaptions[UnityEngine.Random.Range(0, fuelNumber)], threeSeconds, 5));
                        StartCoroutine(ChangeTextColor(caption, fifthSecond, Color.white, Color.red, 2));
                    }
                    audioSource.PlayOneShot(warningSound);
                    voiceSource.clip = lowFuelVoice;
                    voiceSource.Play();
                }
                if(regenerateFuel)
                {
                    regenerateFuel = false;
                    fuel = 10;
                }
                fuelString = fuel.ToString("0.00");
                if(fuelText.text != "fuelString") fuelText.SetText(fuelString);
                break;
            default:
                if(fuelText.color != Color.white) fuelLabel.color = fuelText.color = Color.white;
                fuelString = fuel.ToString("0.00");
                if(fuelText.text != "fuelString") fuelText.SetText(fuelString);
                break;
        }

        Vector3 position = rb.position;
        float posMag = position.magnitude;

        //Check if passed section
        Vector3 chunk = new Vector3((int) Mathf.Round(position.x / length), (int) Mathf.Round(position.y / length), (int) Mathf.Round(position.z / length));
        Vector3 playerChunk = spawnManager.playerChunks[playerIndex];
        if(chunk != playerChunk)
        {
            if(chunk.x > playerChunk.x) spawnManager.SpawnSectionsRight(playerIndex);
            else if(chunk.x < playerChunk.x) spawnManager.SpawnSectionsLeft(playerIndex);

            if(chunk.y > playerChunk.y) spawnManager.SpawnSectionsUp(playerIndex);
            else if(chunk.y < playerChunk.y) spawnManager.SpawnSectionsDown(playerIndex);

            if(chunk.z > playerChunk.z) spawnManager.SpawnSectionsForward(playerIndex);
            else if(chunk.z < playerChunk.z) spawnManager.SpawnSectionsBack(playerIndex);
        }
        
        //Check if passed checkpoint
        int playerCheckpoint = spawnManager.playerCheckpoints[playerIndex];
        if(playerCheckpoint == 0)
        {
            if(posMag >= checkpointDistances[0]) AddCheckpoint(playerCheckpoint);
        }
        else if(playerCheckpoint == lastCheckpointIndex)
        {
            //Passed finish line
            if(posMag >= lastCheckpointDistance)
            {
                if(!canFinish)
                {
                    rb.AddForce(-rb.position.normalized * 2000, ForceMode.VelocityChange);
                    if(!errorPlaying)
                    {
                        errorPlaying = true;
                        StartCoroutine(ChangeTextColor(caption, fifthSecond, Color.white, Color.red, 2));
                        audioSource.PlayOneShot(errorSound);
                        StartCoroutine(ResetError());
                    }
                    return;
                }
                gameObject.SetActive(false);
                caption.gameObject.SetActive(false);
                abilityText.gameObject.SetActive(false);
                finishCaption.SetActive(true);
                finishBackground.SetActive(true);
                finishShip.SetActive(true);
                for(int i = 0; i < allTextLength; i++) if(allText[i].color != Color.white) allText[i].color = Color.white;
                spawnManager.PlayerFinish(playerIndex);

                if(tutorial)
                {
                    distanceText.SetText("Distance: 100.0");
                    return;
                }
                distanceText.SetText("Distance: 1000.0");
                DateTime finishTime = DateTime.Now;
                int elapsedMilliseconds = Mathf.RoundToInt(spawnManager.milliseconds) + spawnManager.seconds * 1000 + spawnManager.minutes * 60000;
                if(elapsedMilliseconds < MenuManager.times[MenuManager.difficulty] || MenuManager.times[MenuManager.difficulty] == 0)
                {
                    int index = MenuManager.difficulty;
                    MenuManager.times[index] = elapsedMilliseconds;
                    PlayerPrefs.SetInt("Time" + index.ToString(), elapsedMilliseconds);
                    PlayerPrefs.SetString("Description" + index.ToString(), " - " + playerNames[playerIndex] + ", " + shipModels[shipIndex] + ", " + finishTime.Month.ToString("00") + "/" + finishTime.Day.ToString("00"));
                }
                if(collects != 0)
                {
                    MenuManager.collects += collects;
                    PlayerPrefs.SetInt("Collects", MenuManager.collects);
                }
                return;
            }
            else if(posMag <= checkpointDistances[secondLastCheckpointIndex]) RemoveCheckpoint(secondLastCheckpointIndex);
        }
        else
        {
            int lastCheckpoint = playerCheckpoint - 1;
            if(posMag >= checkpointDistances[playerCheckpoint]) AddCheckpoint(playerCheckpoint);
            else if(posMag <= checkpointDistances[lastCheckpoint]) RemoveCheckpoint(lastCheckpoint);
        }

        //Show position
        Vector3 displayPos = position / 600;
        posText.SetText("Pos: " + displayPos.x.ToString("0.0") + "," + displayPos.y.ToString("0.0") + "," + displayPos.z.ToString("0.0"));

        //Show distance from origin
        distanceText.SetText("Distance: " + (posMag / 600).ToString("0.0"));

        //Show speed in km/h, multiply by 2.23694 for mph
        float shipSpeed = rb.velocity.magnitude;
        speedText.SetText("Speed: " + (shipSpeed * 9 / 2).ToString("0.00"));

        //Change thruster effect based on speed
        if(fuel > 0.005f && shipSpeed > 15)
        {
            float thrusterSpeed = (float) Math.Round(shipSpeed / thrusterSpeedDivisor, 4);
            if(thrusterSpeed > maxThrusterSpeed) thrusterSpeed = maxThrusterSpeed;
            for(int i = 0; i < thrusterCount; i++)
            {
                ParticleSystem.MainModule thruster = thrusters[i];
                if(thruster.maxParticles != 1000) thruster.maxParticles = 1000;
                thruster.startSpeedMultiplier = thrusterSpeed;
            }

            float thrusterVolume = (float) Math.Round(shipSpeed / thrusterVolumeDivisor, 4);
            if(thrusterVolume > maxThrusterVolume) thrusterVolume = maxThrusterVolume;
            fireSource.volume = thrusterVolume;
        }
        else
        {
            for(int i = 0; i < thrusterCount; i++)
            {
                ParticleSystem.MainModule thruster = thrusters[i];
                if(thruster.maxParticles != 0) thruster.maxParticles = 0;
                if(thruster.startSpeedMultiplier != 0) thruster.startSpeedMultiplier = 0;
            }
            fireSource.volume = 0;
        }
    }

    void LateUpdate()
    {
        if(paused) return;
        float fillAmount = (float) Math.Round(health / 100, 3);
        float currentFill = healthBar.fillAmount;
        if(currentFill != fillAmount) healthBar.fillAmount = Mathf.Lerp(currentFill, fillAmount, Time.deltaTime * 10);

        if(kinematic) return;
        droneGroup.position = myTransform.position;
        if(showArrow) arrow.rotation = Quaternion.LookRotation(myTransform.position.normalized, myTransform.up);
        
        fillAmount = (float) Math.Round(fuel / 100, 3);
        currentFill = fuelBar.fillAmount;
        if(currentFill != fillAmount) fuelBar.fillAmount = Mathf.Lerp(currentFill, fillAmount, Time.deltaTime * 10);
    }

    void FixedUpdate()
    {
        if(paused || kinematic) return;

        if(fuel < 0.005f)
        {
            rb.AddRelativeForce(Vector3.forward * glideSpeed, ForceMode.Acceleration);
            return;
        }

        //Roll, pitch, yaw
        if(rotation != Vector3.zero) rb.AddRelativeTorque(rotation * rotationSpeed, ForceMode.Acceleration);

        //Thrust and glide
        switch(thrust)
        {
            case > 0:
                rb.AddRelativeForce(Vector3.forward * thrustSpeed, ForceMode.Acceleration);
                break;
            case < 0:
                rb.AddRelativeForce(Vector3.back * decSpeed, ForceMode.Acceleration);
                break;
            case 0:
                rb.AddRelativeForce(Vector3.forward * glideSpeed, ForceMode.Acceleration);
                break;
        }
    }

    IEnumerator ResetError()
    {
        yield return threeTenthSeconds;
        errorPlaying = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if(!canTrigger) return;

        //Collected powerup
        if(other.gameObject.CompareTag("Collectable"))
        {
            if(other.name != "Collider") other.gameObject.SetActive(false);
            else other.transform.parent.gameObject.SetActive(false);
            collects++;
            collectText.SetText("Space Debris: " + collects.ToString());
            StartCoroutine(ChangeTextColor(collectText, fifthSecond, Color.white, green));
            int increase = UnityEngine.Random.Range(1, 21);
            float newFuel = fuel + increase;
            switch(newFuel)
            {
                case >= 100:
                    fuel = 100;
                    break;
                case >= 10:
                    if(regenerateFuel) regenerateFuel = false;
                    fuel = newFuel;
                    break;
                default:
                    fuel = newFuel;
                    break;
            }
            if(!debrisCollected)
            {
                debrisCollected = true;
                if(showCaptions)
                {
                    tutorialText = "+" + increase + " Fuel";
                    caption.SetText(tutorialText);
                }
            }
            else if(showCaptions && captionPriority <= 2)
            {
                if(settingCaption != null) StopCoroutine(settingCaption);
                settingCaption = StartCoroutine(SetCaption("+" + increase + " Fuel", oneSecond, 2));
            }

            if(!collectEffect.activeSelf)
            {
                collectEffect.SetActive(true);
                StartCoroutine(ResetEffect(collectEffect, fifthSecond));
            }
            audioSource.PlayOneShot(collectSound);
        }
    }
    
    void OnCollisionEnter(Collision other)
    {
        if(collided || !canCollide) return;

        collided = true;
        float shipSpeed = rb.velocity.magnitude;
        Rigidbody otherRb = other.rigidbody;
        float otherSpeed = otherRb.velocity.magnitude;
        if(otherSpeed > shipSpeed) shipSpeed = otherSpeed;

        Vector3 otherPos = otherRb.position;
        if(deflectionForce != 0)
        {
            rb.AddForce((rb.position - otherPos).normalized * shipSpeed, ForceMode.VelocityChange);
            Vector3 otherPosLocal = myTransform.InverseTransformPoint(otherPos);
            rb.AddRelativeTorque(new Vector3(otherPosLocal.y, otherPosLocal.x, UnityEngine.Random.Range(-0.5f, 0.5f)).normalized * shipSpeed * deflectionForce, ForceMode.VelocityChange);
        }

        if(!other.gameObject.CompareTag("Drone"))
        {
            GameObject explosion = null;
            foreach(GameObject hitExplosion in hitExplosions)
            {
                if(!hitExplosion.activeSelf)
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
            StartCoroutine(ResetEffect(explosion, twoSeconds));
        }
        
        //Lose speed on other
        if(speedLossDivisor != 1)
        {
            if(thrustSpeed > initialThrustSpeed * speedLossDivisor) thrustSpeed /= speedLossDivisor;
            else thrustSpeed = initialThrustSpeed;

            if(decSpeed > initialThrustSpeed * speedLossDivisor) decSpeed /= speedLossDivisor;
            else decSpeed = initialThrustSpeed;

            glideSpeed /= speedLossDivisor;
        }

        //Lose health on other
        if(shipSpeed / healthLossDivisor > 0.01f) health -= shipSpeed / healthLossDivisor;
        else health -= 0.01f;

        //Player died
        switch(health)
        {
            case <= 0:
                canCollide = canTrigger = false;
                rb.isKinematic = kinematic = pauseTutorial = true;

                dieExplosion.transform.position = other.GetContact(0).point;
                dieExplosion.SetActive(true);
                StartCoroutine(ResetEffect(dieExplosion, twoSeconds));

                died = true;
                currentActivation = currentCooldown = 0;
                aura.SetActive(false);
                if(resetting != null) StopCoroutine(resetting);
                if(loopingAura) for(int i = 0; i < auraCount; i++) auraParticles[i].loop = true;

                health = thrust = glideSpeed = 0;
                healthText.SetText("0.00");
                rotation = Vector3.zero;
                thrustSpeed = decSpeed = initialThrustSpeed;
                for(int i = 0; i < allTextLength; i++) StartCoroutine(ChangeTextColor(allText[i], halfSecond, Color.white, Color.red));
                for(int i = 0; i < activeDrones; i++) droneScripts[i].ResetDrone();

                graphic.SetActive(false);
                thrusterParent.SetActive(false);
                fireSource.volume = 0;
                if(collectEffect.activeSelf) collectEffect.SetActive(false);
                if(showArrow) arrow.gameObject.SetActive(false);
                if(settingCaption != null) StopCoroutine(settingCaption);
                caption.SetText("");
                captionPriority = 0;
                deathText.SetText(deathCaptions[UnityEngine.Random.Range(0, deathNumber)]);
                StartCoroutine(ChangeTextColor(deathText, fifthSecond, Color.red, Color.white, 3));
                audioSource.PlayOneShot(dieSound);
                voiceSource.clip = dieVoice;
                voiceSource.PlayDelayed(0.5f);
                StartCoroutine(Respawn());
                return;

            case <= 20:
                if(healthText.color != Color.red)
                {
                    healthText.color = healthLabel.color = Color.red;
                    if(showCaptions && captionPriority <= 5)
                    {
                        if(settingCaption != null) StopCoroutine(settingCaption);
                        settingCaption = StartCoroutine(SetCaption(crashCaptions[UnityEngine.Random.Range(0, crashNumber)], threeSeconds, 5));
                        StartCoroutine(ChangeTextColor(caption, fifthSecond, Color.white, Color.red, 2));
                    }
                    audioSource.PlayOneShot(warningSound);
                    voiceSource.clip = lowHealthVoice;
                    voiceSource.Play();
                }
                break;
            
            default:
                StartCoroutine(ChangeTextColor(healthText, fifthSecond, Color.white, Color.red, 2));
                StartCoroutine(ChangeTextColor(healthLabel, fifthSecond, Color.white, Color.red, 2));
                break;
        }
        health = (float) Math.Round(health, 2);
        healthText.SetText(health.ToString("0.00"));
    }

    IEnumerator Respawn()
    {
        yield return threeSeconds;
        deathText.SetText("");
        if(spawnManager.playerCheckpoints[playerIndex] == 0)
        {
            rb.position = startPos;
            rb.rotation = startRot;
            spawnManager.SpawnSectionsAround(Vector3.zero, playerIndex);
        }
        else
        {
            Vector3 newPos = startPos + normalizedStartPos * checkpointDistances[spawnManager.playerCheckpoints[playerIndex] - 1];
            rb.position = newPos;
            rb.rotation = startRot;
            spawnManager.SpawnSectionsAround(new Vector3((int) Mathf.Round(newPos.x / length), (int) Mathf.Round(newPos.y / length), (int) Mathf.Round(newPos.z / length)), playerIndex);
        }

        health = 100;
        healthText.SetText("100.00");
        if(fuel < 10)
        {
            fuel = 10;
            fuelText.SetText("10.00");
        }

        graphic.SetActive(true);
        thrusterParent.SetActive(true);
        if(showArrow) arrow.gameObject.SetActive(true);
        if(inTutorial)
        {
            caption.SetText(tutorialText);
            captionPriority = 4;
        }
        rb.isKinematic = kinematic = pauseTutorial = false;
        canCollide = canTrigger = true;
        if(collided) collided = false;
        rb.velocity = rb.angularVelocity = Vector3.zero;
    }

    void OnCollisionExit(Collision other)
    {
        if(!canCollide) return;
        collided = false;
    }

    IEnumerator Countdown(int countdownTime)
    {
        yield return null;
        spawnManager.initializedObjects ++;
        while(countdownTime > 0)
        {
            countdownText.SetText(countdownTime.ToString());
            countdownTime --;
            yield return oneSecond;
        }
        countdownText.SetText("Go!");
        yield return oneSecond;
        
        if(!tutorial)
        {
            activeDrones = checkpointDrones[0];
            for(int i = 0; i < activeDrones; i++) drones[i].SetActive(true);
        }
        else StartCoroutine(TutorialManager());
        if(showArrow) arrow.gameObject.SetActive(true);
        countdownText.gameObject.SetActive(false);
        thrusterParent.SetActive(true);
        rb.isKinematic = kinematic = false;
        canCollide = canTrigger = true;
        if(collided) collided = false;
    }

    IEnumerator TutorialManager()
    {
        inTutorial = true;
        rotAllowed = thrustAllowed = abilityAllowed = canFinish = false;
        captionPriority = 4;
        tutorialText = "Welcome to the\nApollo Olympics Tutorial!";
        caption.SetText(tutorialText);
        yield return threeSeconds;
        while(pauseTutorial) yield return null;

        rotAllowed = true;
        string key = InputControlPath.ToHumanReadableString(pitchAction.bindings[1].effectivePath, InputControlPath.HumanReadableStringOptions.UseShortNames | InputControlPath.HumanReadableStringOptions.OmitDevice);
        if(wordNames.ContainsKey(key)) key = wordNames[key];
        key = key.Replace("System", "Cmd").Replace("Control", "Ctrl");
        tutorialText = "Press " + key + " to pitch down";
        caption.SetText(tutorialText);
        while(rotation.x != 1) yield return null;
        StartCoroutine(ChangeTextColor(caption, halfSecond, Color.white, green));
        yield return halfSecond;
        while(pauseTutorial) yield return null;

        key = InputControlPath.ToHumanReadableString(pitchAction.bindings[2].effectivePath, InputControlPath.HumanReadableStringOptions.UseShortNames | InputControlPath.HumanReadableStringOptions.OmitDevice);
        if(wordNames.ContainsKey(key)) key = wordNames[key];
        key = key.Replace("System", "Cmd").Replace("Control", "Ctrl");
        tutorialText = "Press " + key + " to pitch up";
        caption.SetText(tutorialText);
        while(rotation.x != -1) yield return null;
        StartCoroutine(ChangeTextColor(caption, halfSecond, Color.white, green));
        yield return halfSecond;
        while(pauseTutorial) yield return null;

        key = InputControlPath.ToHumanReadableString(yawAction.bindings[1].effectivePath, InputControlPath.HumanReadableStringOptions.UseShortNames | InputControlPath.HumanReadableStringOptions.OmitDevice);
        if(wordNames.ContainsKey(key)) key = wordNames[key];
        key = key.Replace("System", "Cmd").Replace("Control", "Ctrl");
        tutorialText = "Press " + key + " to yaw left";
        caption.SetText(tutorialText);
        while(rotation.y != -1) yield return null;
        StartCoroutine(ChangeTextColor(caption, halfSecond, Color.white, green));
        yield return halfSecond;
        while(pauseTutorial) yield return null;

        key = InputControlPath.ToHumanReadableString(yawAction.bindings[2].effectivePath, InputControlPath.HumanReadableStringOptions.UseShortNames | InputControlPath.HumanReadableStringOptions.OmitDevice);
        if(wordNames.ContainsKey(key)) key = wordNames[key];
        key = key.Replace("System", "Cmd").Replace("Control", "Ctrl");
        tutorialText = "Press " + key + " to yaw right";
        caption.SetText(tutorialText);
        while(rotation.y != 1) yield return null;
        StartCoroutine(ChangeTextColor(caption, halfSecond, Color.white, green));
        yield return halfSecond;
        while(pauseTutorial) yield return null;

        key = InputControlPath.ToHumanReadableString(rollAction.bindings[1].effectivePath, InputControlPath.HumanReadableStringOptions.UseShortNames | InputControlPath.HumanReadableStringOptions.OmitDevice);
        if(wordNames.ContainsKey(key)) key = wordNames[key];
        key = key.Replace("System", "Cmd").Replace("Control", "Ctrl");
        tutorialText = "Press " + key + " to roll left";
        caption.SetText(tutorialText);
        while(rotation.z != 1) yield return null;
        StartCoroutine(ChangeTextColor(caption, halfSecond, Color.white, green));
        yield return halfSecond;
        while(pauseTutorial) yield return null;

        key = InputControlPath.ToHumanReadableString(rollAction.bindings[2].effectivePath, InputControlPath.HumanReadableStringOptions.UseShortNames | InputControlPath.HumanReadableStringOptions.OmitDevice);
        if(wordNames.ContainsKey(key)) key = wordNames[key];
        key = key.Replace("System", "Cmd").Replace("Control", "Ctrl");
        tutorialText = "Press " + key + " to roll right";
        caption.SetText(tutorialText);
        while(rotation.z != -1) yield return null;
        StartCoroutine(ChangeTextColor(caption, halfSecond, Color.white, green));
        yield return halfSecond;
        while(pauseTutorial) yield return null;

        thrustAllowed = true;
        key = InputControlPath.ToHumanReadableString(thrustAction.bindings[1].effectivePath, InputControlPath.HumanReadableStringOptions.UseShortNames | InputControlPath.HumanReadableStringOptions.OmitDevice);
        if(wordNames.ContainsKey(key)) key = wordNames[key];
        key = key.Replace("System", "Cmd").Replace("Control", "Ctrl");
        tutorialText = "Press " + key + " to thrust backward";
        caption.SetText(tutorialText);
        while(thrust != -1) yield return null;
        StartCoroutine(ChangeTextColor(caption, halfSecond, Color.white, green));
        yield return halfSecond;
        while(pauseTutorial) yield return null;

        key = InputControlPath.ToHumanReadableString(thrustAction.bindings[2].effectivePath, InputControlPath.HumanReadableStringOptions.UseShortNames | InputControlPath.HumanReadableStringOptions.OmitDevice);
        if(wordNames.ContainsKey(key)) key = wordNames[key];
        key = key.Replace("System", "Cmd").Replace("Control", "Ctrl");
        tutorialText = "Press " + key + " to thrust forward";
        caption.SetText(tutorialText);
        while(thrust != 1) yield return null;
        StartCoroutine(ChangeTextColor(caption, halfSecond, Color.white, green));
        yield return halfSecond;
        while(pauseTutorial) yield return null;

        activeDrones = 2;
        for(int i = 0; i < 2; i++)
        {
            droneScripts[i].tutorialSpawn = true;
            drones[i].SetActive(true);
        }
        tutorialText = "Thrust and roll to\nconfuse space pirates";
        caption.SetText(tutorialText);
        dronesConfused = false;
        while(!dronesConfused) yield return null;
        StartCoroutine(ChangeTextColor(caption, halfSecond, Color.white, green));
        if(showCaptions)
        {   
            tutorialText = droneCaptions[UnityEngine.Random.Range(0, droneCaptionNumber)];
            caption.SetText(tutorialText);
            yield return twoSeconds;
        }
        else yield return halfSecond;
        while(pauseTutorial) yield return null;

        abilityAllowed = true;
        key = InputControlPath.ToHumanReadableString(abilityAction.bindings[0].effectivePath, InputControlPath.HumanReadableStringOptions.UseShortNames | InputControlPath.HumanReadableStringOptions.OmitDevice);
        if(wordNames.ContainsKey(key)) key = wordNames[key];
        key = key.Replace("System", "Cmd").Replace("Control", "Ctrl");
        if(ringAbility) tutorialText = "Press " + key + " to activate ability\nHold " + key + " to rotate rings";
        else tutorialText = "Press " + key + " to activate ability";
        caption.SetText(tutorialText);
        while(!abilityAction.WasPressedThisFrame()) yield return null;
        StartCoroutine(ChangeTextColor(caption, halfSecond, Color.white, green));
        yield return halfSecond;
        while(pauseTutorial) yield return null;

        tutorialText = "Collect space debris for fuel";
        caption.SetText(tutorialText);
        debrisCollected = false;
        while(!debrisCollected) yield return null;
        StartCoroutine(ChangeTextColor(caption, halfSecond, Color.white, green));
        if(showCaptions) yield return oneSecond;
        else yield return halfSecond;
        while(pauseTutorial) yield return null;

        tutorialText = "Find a wormhole to teleport";
        caption.SetText(tutorialText);
        teleported = false;
        while(!teleported) yield return null;
        StartCoroutine(ChangeTextColor(caption, halfSecond, Color.white, green));
        canFinish = true;
        if(showCaptions) yield return twoSeconds;
        else yield return halfSecond;
        while(pauseTutorial) yield return null;

        tutorialText = "Controls and instructions\ncan be found in settings";
        caption.SetText(tutorialText);
        yield return threeSeconds;
        while(pauseTutorial) yield return null;

        tutorialText = "Now, race to the\nfinal red checkpoint!";
        caption.SetText(tutorialText);
        yield return threeSeconds;
        while(pauseTutorial) yield return null;

        inTutorial = false;
        if(captionPriority <= 4)
        {
            caption.SetText("");
            captionPriority = 0;
        }
    }

    IEnumerator ChangeTextColor(TMP_Text text, WaitForSeconds wait, Color originalColor, Color newColor, int amount = 1)
    {
        if(originalColor == newColor) originalColor = Color.white;
        switch(amount)
        {
            case 1:
                text.color = newColor;
                yield return wait;
                if((text == healthText || text == healthLabel) && shipIndex == 2 && currentActivation > 0) text.color = green;
                else if(text.color == newColor) text.color = originalColor;
                break;

            default:
                int amountMinusOne = amount - 1;
                for(int i = 0; i < amount; i++)
                {
                    text.color = newColor;
                    yield return wait;
                    if((text == healthText || text == healthLabel) && shipIndex == 2 && currentActivation > 0) text.color = green;
                    else if(text.color == newColor) text.color = originalColor;
                    if(i < amountMinusOne) yield return wait;
                }
                break;
        }
    }

    IEnumerator ResetEffect(GameObject effect, WaitForSeconds wait)
    {
        yield return wait;
        effect.SetActive(false);
    }

    IEnumerator SetCaption(string newText, WaitForSeconds wait, int priority)
    {
        if(inTutorial) pauseTutorial = true;
        if(captionPriority != priority) captionPriority = priority;
        if(caption.text != newText) caption.SetText(newText);

        yield return wait;
        if(inTutorial)
        {
            captionPriority = 4;
            caption.SetText(tutorialText);
            pauseTutorial = false;
        }
        else
        {
            captionPriority = 0;
            caption.SetText("");
        }
    }

    public IEnumerator SetDroneCaption(WaitForSeconds wait)
    {
        if(captionPriority > 1 || collided) yield break;
        captionPriority = 1;
        string phrase = droneCaptions[UnityEngine.Random.Range(0, droneCaptionNumber)];
        caption.SetText(phrase);
        yield return wait;
        if(caption.text == phrase)
        {
            captionPriority = 0;
            caption.SetText("");
        }
    }

    public void Teleport()
    {
        canCollide = canTrigger = false;
        int playerCheckpoint = spawnManager.playerCheckpoints[playerIndex];
        int direction = UnityEngine.Random.Range(0, 100);
        float distance;
        bool sameCheckpoint = false;

        switch(playerCheckpoint)
        {
            case 0:
                if(direction < forwardPercent)
                {
                    AddCheckpoint(playerCheckpoint, true);
                    distance = UnityEngine.Random.Range(checkpointDistances[0] + 200, checkpointDistances[1] - 200);
                    break;
                }
                sameCheckpoint = true;
                distance = UnityEngine.Random.Range(0, checkpointDistances[0] - 200);
                break;

            case 1:
                if(direction < backPercent)
                {
                    playerCheckpoint --;
                    RemoveCheckpoint(playerCheckpoint, true);
                    distance = UnityEngine.Random.Range(0, checkpointDistances[0] - 200);
                    break;
                }
                else if(!tutorial && direction < forwardPercent + backPercent)
                {
                    AddCheckpoint(playerCheckpoint, true);
                    distance = UnityEngine.Random.Range(checkpointDistances[1] + 200, checkpointDistances[2] - 200);
                    break;
                }
                sameCheckpoint = true;
                distance = UnityEngine.Random.Range(checkpointDistances[0] + 200, checkpointDistances[1] - 200);
                break;
            
            case 8:
                if(direction < backPercent)
                {
                    playerCheckpoint --;
                    RemoveCheckpoint(playerCheckpoint, true);
                }
                else sameCheckpoint = true;
                distance = UnityEngine.Random.Range(checkpointDistances[playerCheckpoint - 1] + 200, checkpointDistances[playerCheckpoint] - 200);
                break;

            default:
                if(direction < forwardPercent)
                {
                    AddCheckpoint(playerCheckpoint, true);
                    playerCheckpoint ++;
                }
                else if(direction < forwardPercent + backPercent)
                {
                    playerCheckpoint --;
                    RemoveCheckpoint(playerCheckpoint, true);
                }
                else sameCheckpoint = true;
                distance = UnityEngine.Random.Range(checkpointDistances[playerCheckpoint - 1] + 200, checkpointDistances[playerCheckpoint] - 200);
                break;
        }

        if(sameCheckpoint)
        {
            if(!teleported)
            {
                teleported = true;
                if(showCaptions)
                {
                    float mag = rb.position.magnitude;
                    if(mag > distance) tutorialText = "Teleported\n" + ((mag - distance) / 600).ToString("0.0") + " units back";
                    else tutorialText = "Teleported\n" + ((distance - mag) / 600).ToString("0.0") + " units ahead";
                    caption.SetText(tutorialText);
                }
            }
            else if(showCaptions && captionPriority <= 3)
            {
                float mag = rb.position.magnitude;
                if(mag > distance)
                {
                    if(settingCaption != null) StopCoroutine(settingCaption);
                    settingCaption = StartCoroutine(SetCaption("Teleported\n" + ((mag - distance) / 600).ToString("0.0") + " units back", twoSeconds, 3));
                }
                else
                {
                    if(settingCaption != null) StopCoroutine(settingCaption);
                    settingCaption = StartCoroutine(SetCaption("Teleported\n" + ((distance - mag) / 600).ToString("0.0") + " units ahead", twoSeconds, 3));
                }
            }
            voiceSource.clip = wormholeVoice;
            voiceSource.Play();
        }
        Vector3 newPos = Quaternion.Euler(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), 0) * new Vector3(0, 0, distance);
        rb.position = newPos;
        spawnManager.SpawnSectionsAround(new Vector3(Mathf.RoundToInt(newPos.x / length), Mathf.RoundToInt(newPos.y / length), Mathf.RoundToInt(newPos.z / length)), playerIndex);
        audioSource.PlayOneShot(wormholeSound);
        canCollide = canTrigger = true;
        if(collided) collided = false;
    }

    void AddCheckpoint(int playerCheckpoint, bool fromTeleport = false)
    {
        playerCheckpoint ++;
        if(spawnManager.playerCheckpoints[playerIndex] == playerCheckpoint) return;
        spawnManager.playerCheckpoints[playerIndex] = playerCheckpoint;
        checkpointText.SetText("Checkpoint: " + playerCheckpoint.ToString() + checkpointEnd);
        if(!tutorial)
        {
            int newDrones = checkpointDrones[playerCheckpoint];
            for(int i = activeDrones; i < newDrones; i++) drones[i].SetActive(true);
            activeDrones = newDrones;
            if(showCaptions && captionPriority <= 3)
            {
                if(fromTeleport)
                {
                    if(settingCaption != null) StopCoroutine(settingCaption);
                    settingCaption = StartCoroutine(SetCaption("Teleported past " + checkpointCaptions[playerCheckpoint - 1], twoSeconds, 3));
                }
                else
                {
                    if(settingCaption != null) StopCoroutine(settingCaption);
                    settingCaption = StartCoroutine(SetCaption("Passed " + checkpointCaptions[playerCheckpoint - 1], twoSeconds, 3));
                }
            }
        }
        else
        {
            if(fromTeleport)
            {
                if(!teleported)
                {
                    teleported = true;
                    if(showCaptions)
                    {
                        tutorialText = "Teleported past\nCheckpoint 1";
                        caption.SetText(tutorialText);
                    }
                }
                else if(showCaptions && captionPriority <= 3)
                {
                    if(settingCaption != null) StopCoroutine(settingCaption);
                    settingCaption = StartCoroutine(SetCaption("Teleported past\nCheckpoint 1", twoSeconds, 3));
                }
            }
            else if(showCaptions && captionPriority <= 3)
            {
                if(settingCaption != null) StopCoroutine(settingCaption);
                settingCaption = StartCoroutine(SetCaption("Passed Checkpoint 1", twoSeconds, 3));
            }
        }
        StartCoroutine(ChangeTextColor(checkpointText, halfSecond, Color.white, green));
        audioSource.PlayOneShot(checkpointSound);
        voiceSource.clip = checkpointVoice;
        voiceSource.PlayDelayed(0.2f);
    }

    void RemoveCheckpoint(int playerCheckpoint, bool fromTeleport = false)
    {
        if(spawnManager.playerCheckpoints[playerIndex] == playerCheckpoint) return;
        spawnManager.playerCheckpoints[playerIndex] = playerCheckpoint;
        checkpointText.SetText("Checkpoint: " + playerCheckpoint.ToString() + checkpointEnd);
        StartCoroutine(ChangeTextColor(checkpointText, halfSecond, Color.white, Color.red));
        if(tutorial)
        {
            if(fromTeleport)
            {
                if(!teleported)
                {
                    teleported = true;
                    if(showCaptions)
                    {
                        tutorialText = "Teleported back\nto Checkpoint 1";
                        caption.SetText(tutorialText);
                    }
                }
                else if(showCaptions && captionPriority <= 3)
                {
                    if(settingCaption != null) StopCoroutine(settingCaption);
                    settingCaption = StartCoroutine(SetCaption("Teleported back\nto Checkpoint 1", twoSeconds, 3));
                }
            }
            else if(showCaptions && captionPriority <= 3)
            {
                if(settingCaption != null) StopCoroutine(settingCaption);
                settingCaption = StartCoroutine(SetCaption("Returned to Checkpoint 1", twoSeconds, 3));
            }
        }
        else if(showCaptions && captionPriority <= 3)
        {
            if(fromTeleport)
            {
                if(settingCaption != null) StopCoroutine(settingCaption);
                settingCaption = StartCoroutine(SetCaption("Teleported back" + "\nto " + backCaptions[playerCheckpoint], twoSeconds, 3));
            }
            else
            {
                if(settingCaption != null) StopCoroutine(settingCaption);
                settingCaption = StartCoroutine(SetCaption("Returned to " + backCaptions[playerCheckpoint], twoSeconds, 3));
            }
        }
        audioSource.PlayOneShot(errorSound);
        voiceSource.clip = backCheckpointVoice;
        voiceSource.PlayDelayed(0.2f);
    }

    //Abilities
    IEnumerator SpeedUp()
    {
        //Activation
        died = false;
        ability = SpeedUp();
        audioSource.PlayOneShot(activationSound);
        aura.SetActive(true);
        rb.AddRelativeForce(Vector3.forward * (rb.velocity.magnitude + abilityStrength), ForceMode.VelocityChange);
        resetting = StartCoroutine(ResetAura(twoSeconds));

        //Cooldown
        while(currentCooldown > 0)
        {
            abilityText.SetText("Charging: " + currentCooldown.ToString());
            yield return oneSecond;
            if(currentCooldown > 0) currentCooldown --;
        }
        if(!died)
        {
            abilityText.SetText("Ability Charged");
            audioSource.PlayOneShot(chargeSound);
            yield return oneSecond;
        }
        abilityText.SetText("");
    }

    IEnumerator Atmosphere()
    {
        //Activation
        currentActivation = activationTime;
        died = false;
        ability = Atmosphere();
        aura.SetActive(true);
        audioSource.PlayOneShot(activationSound);
        while(currentActivation > 0)
        {
            abilityText.SetText("Activated: " + currentActivation.ToString());
            yield return oneSecond;
            currentActivation --;
        }
        audioSource.PlayOneShot(expireSound);
        aura.SetActive(false);

        //Cooldown
        while(currentCooldown > 0)
        {
            abilityText.SetText("Charging: " + currentCooldown.ToString());
            yield return oneSecond;
            if(currentCooldown > 0) currentCooldown --;
        }
        if(!died)
        {
            abilityText.SetText("Ability Charged");
            audioSource.PlayOneShot(chargeSound);
            yield return oneSecond;
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
        healthText.color = healthLabel.color = green;
        audioSource.PlayOneShot(activationSound);
        while(currentActivation > 0)
        {
            abilityText.SetText("Activated: " + currentActivation.ToString());
            if(health + abilityStrength < 100)
            {
                health += abilityStrength;
                health = (float) Math.Round(health, 2);
                healthText.SetText(health.ToString("0.00"));
            }
            else
            {
                health = 100;
                healthText.SetText("100.00");
            }
            yield return oneSecond;
            currentActivation --;
        }
        for(int i = 0; i < auraCount; i++) auraParticles[i].loop = false;
        resetting = StartCoroutine(ResetAura(twoSeconds));
        healthText.color = healthLabel.color = Color.white;
        audioSource.PlayOneShot(expireSound);

        //Cooldown
        while(currentCooldown > 0)
        {
            abilityText.SetText("Charging: " + currentCooldown.ToString());
            yield return oneSecond;
            if(currentCooldown > 0) currentCooldown --;
        }
        if(!died)
        {
            abilityText.SetText("Ability Charged");
            audioSource.PlayOneShot(chargeSound);
            yield return oneSecond;
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
        for(int i = 0; i < droneNumber; i++) droneScripts[i].offset = abilityStrength;
        while(currentActivation > 0)
        {
            abilityText.SetText("Activated: " + currentActivation.ToString());
            yield return oneSecond;
            currentActivation --;
        }
        for(int i = 0; i < auraCount; i++) auraParticles[i].loop = false;
        resetting = StartCoroutine(ResetAura(twoSeconds));
        audioSource.PlayOneShot(expireSound);
        for(int i = 0; i < droneNumber; i++) droneScripts[i].offset = 0;

        //Cooldown
        while(currentCooldown > 0)
        {
            abilityText.SetText("Charging: " + currentCooldown.ToString());
            yield return oneSecond;
            if(currentCooldown > 0) currentCooldown --;
        }
        if(!died)
        {
            abilityText.SetText("Ability Charged");
            audioSource.PlayOneShot(chargeSound);
            yield return oneSecond;
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
        healthLossDivisor = abilityStrength2;
        deflectionForce = 0;
        rb.mass = abilityStrength;
        while(currentActivation > 0)
        {
            abilityText.SetText("Activated: " + currentActivation.ToString());
            yield return oneSecond;
            currentActivation --;
        }
        for(int i = 0; i < auraCount; i++) auraParticles[i].loop = false;
        resetting = StartCoroutine(ResetAura(twoSeconds));
        audioSource.PlayOneShot(expireSound);
        speedLossDivisor = collisionInitials[0];
        healthLossDivisor = (int) collisionInitials[1];
        deflectionForce = collisionInitials[2];
        rb.mass = collisionInitials[3];

        //Cooldown
        while(currentCooldown > 0)
        {
            abilityText.SetText("Charging: " + currentCooldown.ToString());
            yield return oneSecond;
            if(currentCooldown > 0) currentCooldown --;
        }
        if(!died)
        {
            abilityText.SetText("Ability Charged");
            audioSource.PlayOneShot(chargeSound);
            yield return oneSecond;
        }
        abilityText.SetText("");
    }

    IEnumerator Ring()
    {
        //Activation
        currentActivation = activationTime;
        died = false;
        ability = Ring();
        aura.SetActive(true);
        audioSource.PlayOneShot(activationSound);
        while(currentActivation > 0)
        {
            abilityText.SetText("Activated: " + currentActivation.ToString());
            yield return oneSecond;
            currentActivation --;
        }
        for(int i = 0; i < auraCount; i++) auraParticles[i].loop = false;
        resetting = StartCoroutine(ResetAura(oneSecond));
        audioSource.PlayOneShot(expireSound);

        //Cooldown
        while(currentCooldown > 0)
        {
            abilityText.SetText("Charging: " + currentCooldown.ToString());
            yield return oneSecond;
            if(currentCooldown > 0) currentCooldown --;
        }
        if(!died)
        {
            abilityText.SetText("Ability Charged");
            audioSource.PlayOneShot(chargeSound);
            yield return oneSecond;
        }
        abilityText.SetText("");
    }

    IEnumerator WindStorm()
    {
        //Activation
        died = false;
        ability = WindStorm();
        audioSource.PlayOneShot(activationSound);
        aura.SetActive(true);
        int colliders = Physics.OverlapSphereNonAlloc(rb.position, abilityStrength2, surroundingColliders, abilityMask);
        for(int i = 0; i < colliders; i++)
        {
            Collider collider = surroundingColliders[i];
            Rigidbody otherRb = collider.attachedRigidbody;
            if(otherRb.isKinematic) continue;
            if(collider.gameObject.CompareTag("Drone")) collider.GetComponent<Drone>().Explode(otherRb.position);
            else
            {
                otherRb.AddForce((otherRb.position - rb.position).normalized * abilityStrength, ForceMode.VelocityChange);
                otherRb.AddTorque(new Vector3(UnityEngine.Random.Range(0, 720), UnityEngine.Random.Range(0, 720), UnityEngine.Random.Range(0, 720)) * abilityStrength, ForceMode.Impulse);
            }
        }
        resetting = StartCoroutine(ResetAura(oneSecond));

        //Cooldown
        while(currentCooldown > 0)
        {
            abilityText.SetText("Charging: " + currentCooldown.ToString());
            yield return oneSecond;
            if(currentCooldown > 0) currentCooldown --;
        }
        if(!died)
        {
            abilityText.SetText("Ability Charged");
            audioSource.PlayOneShot(chargeSound);
            yield return oneSecond;
        }
        abilityText.SetText("");
    }

    IEnumerator ResetAura(WaitForSeconds wait)
    {
        yield return wait;
        aura.SetActive(false);
        if(loopingAura) for(int i = 0; i < auraCount; i++) auraParticles[i].loop = true;
        if(!canCollide) canCollide = true;
    }
}

