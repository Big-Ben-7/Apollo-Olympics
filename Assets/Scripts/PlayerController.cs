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
    [SerializeField] float fuelRegeneration = 0.05f;

    [Header("Movement")]
    [SerializeField] GameObject thrusterParent;
    [SerializeField] float thrusterSpeedDivisor = 40;
    [SerializeField] float maxThrusterSpeed = 20;
    [SerializeField] float thrusterVolumeDivisor = 8000;

    [Header("Collision")]
    [SerializeField] GameObject dieExplosion;
    [SerializeField] List<GameObject> hitExplosions;
    [SerializeField] GameObject collectEffect;
    [SerializeField] GameObject dronePrefab;
    [SerializeField] GameObject graphic;

    [Header("Ability")]
    [SerializeField] string shipModel;
    [SerializeField] GameObject aura;
    [SerializeField] ParticleSystem[] auraSystems;
    [SerializeField] LayerMask abilityMask;
    

    [Header("UI/Audio")]
    [SerializeField] Color green;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioSource voiceSource;
    [SerializeField] AudioSource fireSource;
    [SerializeField] AudioSource thrusterSource;
    [SerializeField] AudioClip[] hitSounds;
    [SerializeField] AudioClip noFuelSound;
    [SerializeField] AudioClip noFuelVoice;
    [SerializeField] AudioClip collectSound;
    [SerializeField] AudioClip checkpointSound;
    [SerializeField] AudioClip checkpointVoice;
    [SerializeField] AudioClip backCheckpointVoice;
    [SerializeField] AudioClip activationSound;
    [SerializeField] AudioClip activationVoice;
    [SerializeField] AudioClip expireSound;
    [SerializeField] AudioClip expireVoice;
    [SerializeField] AudioClip chargeSound;
    [SerializeField] AudioClip chargeVoice;
    [SerializeField] AudioClip pirateVoice;
    [SerializeField] AudioClip errorSound;
    [SerializeField] AudioClip wormholeSound;
    [SerializeField] AudioClip wormholeVoice;
    [SerializeField] AudioClip warningSound;
    [SerializeField] AudioClip lowHealthVoice;
    [SerializeField] AudioClip lowFuelVoice;
    [SerializeField] AudioClip dieSound;
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
    float minHealthLoss;
    float deflectionForce;
    float health = 100;
    float fuel = 100;
    bool collided;
    public bool canCollide;
    bool canTrigger;
    public static SpawnManager spawnManager;
    public static int length;
    public static int[] checkpointDistances;
    float[] collisionInitials;
    ParticleSystem.MainModule[] thrusters;
    int thrusterCount;
    int hitSoundsCount;
    public int collects;
    GameObject hitExplosionPrefab;
    int forwardPercent = 35;
    int backPercent = 25;
    int extraFuelPercent = 30;
    string[] droneCaptions = new string[]
    {
        "Nice Move!",
        "Well Played!",
        "Got 'em!",
        "Wipeout!",
        "Clean Sweep!",
        "Impressive!",
        "Bravo!",
        "Pirates Out!",
        "Bye-bye Pirates!",
        "Well Done!",
        "Outstanding!",
        "Terriffic!",
        "Awesome!",
        "Amazing!",
        "WOW!",
        "Let's Go!"
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
    int droneCaptionNumber;
    int crashNumber;
    int fuelNumber;
    int noFuelNumber;
    int deathNumber;

    //Ability
    IEnumerator ability;
    Coroutine runningAbility;
    InputAction abilityAction;
    int currentActivation;
    int currentCooldown;
    int auraCount;
    ParticleSystem.MainModule[] auraParticles;
    Atmosphere atmosphere;
    Ring ring;
    bool died;
    Coroutine resetting;
    public static WaitForSeconds oneSecond;
    public static WaitForSeconds twoSeconds;
    public static WaitForSeconds threeSeconds;
    public static WaitForSeconds fifthSecond;
    public static WaitForSeconds halfSecond;
    Collider[] surroundingColliders = new Collider[10000];

    //Drone
    Transform droneGroup;
    public static int droneNumber;
    public static int[] checkpointDrones;
    int activeDrones;
    Drone[] droneScripts;
    GameObject[] drones;

    //UI
    public Transform cameraStats;
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
    public Transform arrow;
    public static float maxThrusterVolume;
    public static float audioVolume;
    public static float voiceVolume;
    float warningScale;
    public static bool showCaptions;
    public static bool showArrow;
    public static bool paused = true;
    bool kinematic = true;
    string[] checkpointCaptions = new string[] 
    {
        "Passed Mercury Orbit", 
        "Passed Venus Orbit", 
        "Passed Earth Orbit", 
        "Passed Mars Orbit\nEntering Asteroid Belt", 
        "Passed Jupiter Orbit", 
        "Passed Saturn Orbit",
        "Passed Uranus Orbit",
        "Passed Neptune Orbit\nEntering Kuiper Belt"
    };    string[] backCaptions = new string[] 
    {
        "Returned To Mercury Orbit", 
        "Returned To Venus Orbit", 
        "Returned To Earth Orbit", 
        "Returned To Mars Orbit", 
        "Returned To Jupiter Orbit", 
        "Returned To Saturn Orbit",
        "Returned To Uranus Orbit",
        "Returned To Neptune Orbit"
    };
    int captionPriority = 0;

    void Awake()
    {
        dieExplosion.SetActive(false);
        hitExplosionPrefab = hitExplosions[0];
        hitExplosionPrefab.SetActive(false);
        aura.SetActive(false);
        collectEffect.SetActive(false);
        thrusterParent.SetActive(false);
        fireSource.volume = 0;
        thrusterSource.volume = 0;

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
        minHealthLoss = spawnManager.minHealthLosses[durability];
        speedLossDivisor = spawnManager.speedLossDivisors[stability];
        deflectionForce = spawnManager.deflectionForces[stability];
        thrustSpeed = initialThrustSpeed;
        decSpeed = thrustSpeed;

        Transform farCanvas = cameraStats.GetChild(0);
        Transform shipParent = farCanvas.GetChild(12);
        switch(shipModel)
        {
            case "Mercury":
                ability = SpeedUp();
                finishShip = shipParent.GetChild(0).gameObject;
                break;
            case "Venus":
                ability = Atmosphere();
                aura = Instantiate(aura);
                atmosphere = aura.GetComponent<Atmosphere>();
                atmosphere.target = myTransform;
                atmosphere.audioSource = audioSource;
                Transform graphic = aura.transform.GetChild(0);
                auraSystems = new ParticleSystem[] {graphic.GetComponent<ParticleSystem>(), graphic.GetChild(0).GetComponent<ParticleSystem>(), graphic.GetChild(1).GetComponent<ParticleSystem>(), graphic.GetChild(2).GetComponent<ParticleSystem>()};
                aura.SetActive(false);
                SpawnManager.followers[playerIndex].Add(aura.transform);
                finishShip = shipParent.GetChild(1).gameObject;
                break;
            case "Earth":
                ability = Regeneration();
                finishShip = shipParent.GetChild(2).gameObject;
                break;
            case "Mars":
                ability = DustStorm();
                finishShip = shipParent.GetChild(3).gameObject;
                break;
            case "Jupiter":
                ability = Juggernaut();
                collisionInitials = new float[] {speedLossDivisor, healthLossDivisor, minHealthLoss, deflectionForce, rb.mass};
                finishShip = shipParent.GetChild(4).gameObject;
                break;
            case "Saturn":
                ability = Ring();
                aura = Instantiate(aura);
                ring = aura.GetComponent<Ring>();
                ring.target = myTransform;
                ring.audioSource = audioSource;
                auraSystems = new ParticleSystem[] {aura.transform.GetChild(0).GetComponent<ParticleSystem>()};
                aura.SetActive(false);
                SpawnManager.followers[playerIndex].Add(aura.transform);
                finishShip = shipParent.GetChild(5).gameObject;
                break;
            case "Uranus":
                ability = Ring();
                aura = Instantiate(aura);
                ring = aura.GetComponent<Ring>();
                ring.target = myTransform;
                ring.audioSource = audioSource;
                auraSystems = new ParticleSystem[] {aura.transform.GetChild(0).GetComponent<ParticleSystem>()};
                aura.SetActive(false);
                SpawnManager.followers[playerIndex].Add(aura.transform);
                finishShip = shipParent.GetChild(6).gameObject;
                break;
            case "Neptune":
                ability = WindStorm();
                finishShip = shipParent.GetChild(7).gameObject;
                break;
        }
        finishShip.SetActive(false);
        confetti = shipParent.GetChild(8).gameObject;
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
        SpawnManager.followers[playerIndex].Add(droneGroup);

        //Aura setup
        auraCount = auraSystems.Length;
        auraParticles = new ParticleSystem.MainModule[auraCount];
        for(int i = 0; i < auraCount; i++)
        {
            auraParticles[i] = auraSystems[i].main;
        }

        //UI setup
        finishBackground = farCanvas.GetChild(0).gameObject;
        Transform healthParent = farCanvas.GetChild(1);
            healthLabel = healthParent.GetChild(0).GetComponent<TMP_Text>();
            healthBar = healthParent.GetChild(3).GetComponent<Image>();
            healthText = healthParent.GetChild(4).GetComponent<TMP_Text>();
        Transform fuelParent = farCanvas.GetChild(2);
            fuelLabel = fuelParent.GetChild(0).GetComponent<TMP_Text>();
            fuelBar = fuelParent.GetChild(3).GetComponent<Image>();
            fuelText = fuelParent.GetChild(4).GetComponent<TMP_Text>();
        collectText = farCanvas.GetChild(3).GetComponent<TMP_Text>();
        checkpointText = farCanvas.GetChild(4).GetComponent<TMP_Text>();
        abilityText = farCanvas.GetChild(5).GetComponent<TMP_Text>();
        nameText = farCanvas.GetChild(6).GetComponent<TMP_Text>();
        posText = farCanvas.GetChild(7).GetComponent<TMP_Text>();
        distanceText = farCanvas.GetChild(8).GetComponent<TMP_Text>();
        speedText = farCanvas.GetChild(9).GetComponent<TMP_Text>();
        timeText = farCanvas.GetChild(10).GetComponent<TMP_Text>();
        caption = farCanvas.GetChild(11).GetComponent<TMP_Text>();
        finishText = farCanvas.GetChild(13).GetComponent<TMP_Text>();
        finishCaption = farCanvas.GetChild(14).gameObject;
        deathText = farCanvas.GetChild(15).GetComponent<TMP_Text>();
        countdownText = cameraStats.GetChild(1).GetChild(0).GetComponent<TMP_Text>();
        
        finishBackground.SetActive(false);
        healthBar.fillAmount = fuelBar.fillAmount = 1;
        fuelText.SetText("100.00");
        healthText.SetText("100.00");
        collectText.SetText("Space Debris: 0");
        checkpointText.SetText("Checkpoint: 0");
        abilityText.SetText("");
        nameText.SetText(spawnManager.playerNames[playerIndex]);
        posText.SetText("Pos: 0.0,0.0,0.0");
        distanceText.SetText("Distance: 0.0");
        speedText.SetText("Speed: 0.00");
        caption.SetText("");
        finishText.SetText("");
        spawnManager.finishTexts[playerIndex] = finishText;
        finishCaption.SetActive(false);
        deathText.SetText("");
        countdownText.SetText("");
        timeText.SetText(spawnManager.timeString);
        
        audioSource.volume = audioVolume;
        voiceSource.volume = voiceVolume;
        if(audioVolume * 2 > 1) warningScale = 1 / audioVolume;
        else warningScale = 2;

        allText = new TMP_Text[]{nameText, checkpointText, healthLabel, healthText, fuelLabel, fuelText, speedText, collectText, abilityText, timeText, posText, distanceText};
        allTextLength = allText.Length;
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
                if((shipModel != "Saturn" && shipModel != "Uranus") || currentActivation == 0)
                {
                    StartCoroutine(ChangeTextColor(abilityText, fifthSecond, Color.white, Color.red));
                    audioSource.PlayOneShot(errorSound);
                }
            }
        }

        if(abilityAction.IsPressed() && currentActivation > 0)
        {
            switch(shipModel)
            {
                case "Saturn":
                    ring.rotationOffset *= Quaternion.Euler(new Vector3(abilityStrength2, 0, 0) * Time.deltaTime);
                    break;
                
                case "Uranus":
                    ring.rotationOffset *= Quaternion.Euler(new Vector3(0, abilityStrength2, 0) * Time.deltaTime);
                    break;
            }
        }

        //Movement input
        rotation = new Vector3(-pitchAction.ReadValue<float>(), yawAction.ReadValue<float>(), -rollAction.ReadValue<float>());
        thrust = thrustAction.ReadValue<float>();

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
        float fuelRegIncrement = fuelRegeneration * Time.deltaTime;
        switch(thrust)
        {
            case > 0:
                if(fuel > accFuelIncrement)
                {
                    if(glideSpeed != thrustSpeed) glideSpeed = thrustSpeed;
                    
                    if(thrustSpeed < accThreshold) thrustSpeed += accIncrement;
                    else if(thrustSpeed != maxThrustSpeed) thrustSpeed = maxThrustSpeed;
                    
                    fuel -= accFuelIncrement;
                }
                else fuel = 0;
                
                if(decSpeed > decThreshold && dec != 1) decSpeed *= dec;
                else if(decSpeed != initialThrustSpeed) decSpeed = initialThrustSpeed;
                break;

            case < 0:
                if(fuel > accFuelIncrement)
                {
                    if(glideSpeed != 0) glideSpeed = 0;

                    if(decSpeed < accThreshold) decSpeed += accIncrement;
                    else if(decSpeed != maxThrustSpeed) decSpeed = maxThrustSpeed;

                    fuel -= accFuelIncrement;
                }
                else fuel = 0;

                if(thrustSpeed > decThreshold && dec != 1) thrustSpeed *= dec;
                else if(thrustSpeed != initialThrustSpeed) thrustSpeed = initialThrustSpeed;
                break;
        
            case 0:
                if(rotation != Vector3.zero)
                {
                    if(fuel > rotFuelIncrement) fuel -= rotFuelIncrement;
                    else fuel = 0;
                }
                else if(fuel < 100 - fuelRegIncrement) fuel += fuelRegIncrement;
                else fuel = 100;
                break;
        }
        
        //Fuel management
        switch(fuel)
        {
            case < 0.005f:
                if(fuelText.text != "0.00")
                {
                    fuelText.SetText("0.00");
                    StartCoroutine(SetCaption(noFuelCaptions[UnityEngine.Random.Range(0, noFuelNumber)], threeSeconds, 5));
                    StartCoroutine(ChangeTextColor(caption, fifthSecond, Color.white, Color.red, 3));
                    audioSource.PlayOneShot(noFuelSound);
                    voiceSource.clip = noFuelVoice;
                    voiceSource.PlayDelayed(0.5f);
                }
                break;
            case <= 20:
                if(fuelText.color != Color.red)
                {
                    fuelLabel.color = fuelText.color = Color.red;
                    if(showCaptions)
                    {
                        StartCoroutine(SetCaption(fuelCaptions[UnityEngine.Random.Range(0, fuelNumber)], threeSeconds, 4));
                        StartCoroutine(ChangeTextColor(caption, fifthSecond, Color.white, Color.red, 2));
                    }
                    audioSource.PlayOneShot(warningSound, warningScale);
                    voiceSource.clip = lowFuelVoice;
                    voiceSource.Play();
                }
                string fuelString = fuel.ToString("0.00");
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
        switch(playerCheckpoint)
        {
            case 0:
                if(posMag >= checkpointDistances[0]) AddCheckpoint(playerCheckpoint);
                break;
            
            case 8:
                //Passed finish line
                if(posMag >= checkpointDistances[8])
                {
                    gameObject.SetActive(false);
                    caption.gameObject.SetActive(false);
                    abilityText.gameObject.SetActive(false);
                    finishCaption.SetActive(true);
                    finishBackground.SetActive(true);
                    finishShip.SetActive(true);
                    for(int i = 0; i < allTextLength; i++) if(allText[i].color != Color.white) allText[i].color = Color.white;
                    spawnManager.PlayerFinish(playerIndex);
                    DateTime finishTime = DateTime.Now;
                    int elapsedMilliseconds = Mathf.RoundToInt(spawnManager.milliseconds) + spawnManager.seconds * 1000 + spawnManager.minutes * 60000;
                    if(elapsedMilliseconds < MenuManager.times[MenuManager.difficulty] || MenuManager.times[MenuManager.difficulty] == 0)
                    {
                        int index = MenuManager.difficulty;
                        MenuManager.times[index] = elapsedMilliseconds;
                        PlayerPrefs.SetInt("Time" + index.ToString(), elapsedMilliseconds);
                        PlayerPrefs.SetString("Description" + index.ToString(), " - " + spawnManager.playerNames[playerIndex] + ", " + shipModel + ", " + finishTime.Month.ToString("00") + "/" + finishTime.Day.ToString("00"));
                    }
                    if(collects != 0)
                    {
                        MenuManager.collects += collects;
                        PlayerPrefs.SetInt("Collects", MenuManager.collects);
                    }
                    return;
                }
                else if(posMag <= checkpointDistances[7]) RemoveCheckpoint(7);
                break;

            default:
                int lastCheckpoint = playerCheckpoint - 1;
                if(posMag >= checkpointDistances[playerCheckpoint]) AddCheckpoint(playerCheckpoint);
                else if(posMag <= checkpointDistances[lastCheckpoint]) RemoveCheckpoint(lastCheckpoint);
                break;
        }

        //Show position in km, divide by 1609 for mi
        Vector3 kmPos = position / 1000;
        posText.SetText("Pos: " + kmPos.x.ToString("0.0") + "," + kmPos.y.ToString("0.0") + "," + kmPos.z.ToString("0.0"));

        //Show distance from origin
        distanceText.SetText("Distance: " + (posMag / 1000).ToString("0.0"));

        //Show speed in km/h, multiply by 2.23694 for mph
        float shipSpeed = rb.velocity.magnitude;
        speedText.SetText("Speed: " + (shipSpeed * 3.6f).ToString("0.00"));

        //Change thruster effect based on speed
        if(fuel > 0 && shipSpeed != 0)
        {
            float thrusterSpeed = shipSpeed / thrusterSpeedDivisor;
            if(thrusterSpeed > maxThrusterSpeed) thrusterSpeed = maxThrusterSpeed;
            for(int i = 0; i < thrusterCount; i++)
            {
                ParticleSystem.MainModule thruster = thrusters[i];
                if(thruster.startLifetimeMultiplier != 1) thruster.startLifetimeMultiplier = 1;
                thruster.startSpeedMultiplier = thrusterSpeed;
            }

            float thrusterVolume = shipSpeed / thrusterVolumeDivisor;
            if(thrusterVolume > maxThrusterVolume) thrusterVolume = maxThrusterVolume;
            fireSource.volume = thrusterSource.volume = thrusterVolume;
        }
        else
        {
            for(int i = 0; i < thrusterCount; i++)
            {
                ParticleSystem.MainModule thruster = thrusters[i];
                if(thruster.startLifetimeMultiplier != 0) thruster.startLifetimeMultiplier = 0;
            }
            fireSource.volume = thrusterSource.volume = 0;
        }
    }

    void LateUpdate()
    {
        if(paused || kinematic) return;
        droneGroup.position = myTransform.position;

        float fillAmount = (float) Math.Round(fuel / 100, 3);
        if(fuelBar.fillAmount != fillAmount) fuelBar.fillAmount = fillAmount;

        if(!showArrow) return;
        arrow.rotation = Quaternion.LookRotation(myTransform.position.normalized, myTransform.up);
    }

    void FixedUpdate()
    {
        if(paused || kinematic) return;

        if(fuel == 0)
        {
            rb.AddRelativeForce(Vector3.forward * glideSpeed, ForceMode.Acceleration);
            return;
        }

        //Roll, pitch, yaw
        if(rotation != Vector3.zero)
        {
            rb.AddRelativeTorque(rotation * rotationSpeed, ForceMode.Acceleration);
        }

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
            int number = UnityEngine.Random.Range(0, 100);
            if(number < extraFuelPercent)
            {
                if(fuel < 90) fuel += 10;
                else fuel = 100;
                if(showCaptions) StartCoroutine(SetCaption("+10 Fuel", oneSecond, 2));
            }
            else
            {
                if(fuel < 95) fuel += 5;
                else fuel = 100;
                if(showCaptions) StartCoroutine(SetCaption("+5 Fuel", oneSecond, 2));
            }
            if(!collectEffect.activeSelf)
            {
                collectEffect.SetActive(true);
                StartCoroutine(ResetEffect(collectEffect, fifthSecond));
            }
            audioSource.PlayOneShot(collectSound, 0.8f);
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
        if(shipSpeed / healthLossDivisor > minHealthLoss) health -= shipSpeed / healthLossDivisor;
        else health -= minHealthLoss;

        //Player died
        switch(health)
        {
            case <= 0:
                canCollide = canTrigger = false;
                rb.isKinematic = kinematic = true;

                dieExplosion.transform.position = other.GetContact(0).point;
                dieExplosion.SetActive(true);
                StartCoroutine(ResetEffect(dieExplosion, twoSeconds));

                died = true;
                currentActivation = currentCooldown = 0;
                aura.SetActive(false);
                if(resetting != null) StopCoroutine(resetting);
                if(shipModel != "Venus")
                {
                    for(int i = 0; i < auraCount; i++)
                    {
                        auraParticles[i].loop = true;
                    }
                }

                health = 0;
                healthText.SetText("0.00");
                healthBar.fillAmount = 0;
                thrustSpeed = initialThrustSpeed;
                for(int i = 0; i < allTextLength; i++) StartCoroutine(ChangeTextColor(allText[i], halfSecond, Color.white, Color.red));
                for(int i = 0; i < activeDrones; i++) droneScripts[i].ResetDrone();

                StartCoroutine(Respawn());
                return;

            case <= 20:
                if(healthText.color != Color.red)
                {
                    healthText.color = healthLabel.color = Color.red;
                    if(showCaptions)
                    {
                        StartCoroutine(SetCaption(crashCaptions[UnityEngine.Random.Range(0, crashNumber)], threeSeconds, 4));
                        StartCoroutine(ChangeTextColor(caption, fifthSecond, Color.white, Color.red, 2));
                    }
                    audioSource.PlayOneShot(warningSound, warningScale);
                    voiceSource.clip = lowHealthVoice;
                    voiceSource.Play();
                }
                break;
            
            default:
                StartCoroutine(ChangeTextColor(healthText, fifthSecond, Color.white, Color.red, 2));
                StartCoroutine(ChangeTextColor(healthLabel, fifthSecond, Color.white, Color.red, 2));
                break;
        }
        health = (float) Math.Round(health, 1);
        healthText.SetText(health.ToString("0.00"));
        healthBar.fillAmount = health / 100;
    }

    IEnumerator Respawn()
    {
        graphic.SetActive(false);
        thrusterParent.SetActive(false);
        if(collectEffect.activeSelf) collectEffect.SetActive(false);
        if(showArrow) arrow.gameObject.SetActive(false);
        caption.SetText("");
        captionPriority = 0;
        deathText.SetText(deathCaptions[UnityEngine.Random.Range(0, deathNumber)]);
        StartCoroutine(ChangeTextColor(deathText, fifthSecond, Color.red, Color.white, 3));
        audioSource.PlayOneShot(dieSound);
        voiceSource.clip = dieVoice;
        voiceSource.PlayDelayed(0.5f);

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
        healthBar.fillAmount = 1;

        graphic.SetActive(true);
        thrusterParent.SetActive(true);
        if(showArrow) arrow.gameObject.SetActive(true);
        rb.isKinematic = kinematic = false;
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
        if(showArrow) arrow.gameObject.SetActive(true);
        activeDrones = checkpointDrones[0];
        for(int i = 0; i < activeDrones; i++)
        {
            drones[i].SetActive(true);
        }
        
        countdownText.gameObject.SetActive(false);
        thrusterParent.SetActive(true);
        rb.isKinematic = kinematic = false;
        canCollide = canTrigger = true;
        if(collided) collided = false;
    }

    IEnumerator ChangeTextColor(TMP_Text text, WaitForSeconds wait, Color originalColor, Color newColor, int amount = 1)
    {
        if(originalColor == newColor) originalColor = Color.white;
        switch(amount)
        {
            case 1:
                text.color = newColor;
                yield return wait;
                if((text == healthText || text == healthLabel) && shipModel == "Earth" && currentActivation > 0) text.color = green;
                else if(text.color == newColor) text.color = originalColor;
                break;

            default:
                int amountMinusOne = amount - 1;
                for(int i = 0; i < amount; i++)
                {
                    text.color = newColor;
                    yield return wait;
                    if((text == healthText || text == healthLabel) && shipModel == "Earth" && currentActivation > 0) text.color = green;
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

    IEnumerator SetCaption(string phrase, WaitForSeconds wait, int priority)
    {
        if(captionPriority > priority) yield break;
        if(captionPriority != priority) captionPriority = priority;
        caption.SetText(phrase);
        yield return wait;
        if(caption.text == phrase)
        {
            captionPriority = 0;
            caption.SetText("");
        }
    }

    public IEnumerator SetDroneCaption(WaitForSeconds wait)
    {
        if(captionPriority >= 1 || collided) yield break;
        captionPriority = 1;
        string phrase = droneCaptions[UnityEngine.Random.Range(0, droneCaptionNumber)];
        caption.SetText(phrase);
        voiceSource.clip = pirateVoice;
        voiceSource.PlayDelayed(0.5f);
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
                    AddCheckpoint(playerCheckpoint);
                    distance = UnityEngine.Random.Range(checkpointDistances[0] + 300, checkpointDistances[1] - 300);
                    break;
                }
                sameCheckpoint = true;
                distance = UnityEngine.Random.Range(0, checkpointDistances[0] - 300);
                break;

            case 1:
                if(direction < forwardPercent)
                {
                    AddCheckpoint(playerCheckpoint);
                    distance = UnityEngine.Random.Range(checkpointDistances[1] + 300, checkpointDistances[2] - 300);
                    break;
                }
                else if(direction < forwardPercent + backPercent)
                {
                    playerCheckpoint --;
                    RemoveCheckpoint(playerCheckpoint);
                    distance = UnityEngine.Random.Range(0, checkpointDistances[0] - 300);
                    break;
                }
                sameCheckpoint = true;
                distance = UnityEngine.Random.Range(checkpointDistances[0] + 300, checkpointDistances[1] - 300);
                break;
            
            case 8:
                if(direction < backPercent)
                {
                    playerCheckpoint --;
                    RemoveCheckpoint(playerCheckpoint);
                }
                else sameCheckpoint = true;
                distance = UnityEngine.Random.Range(checkpointDistances[playerCheckpoint - 1] + 300, checkpointDistances[playerCheckpoint] - 300);
                break;

            default:
                if(direction < forwardPercent)
                {
                    AddCheckpoint(playerCheckpoint);
                    playerCheckpoint ++;
                }
                else if(direction < forwardPercent + backPercent)
                {
                    playerCheckpoint --;
                    RemoveCheckpoint(playerCheckpoint);
                }
                else sameCheckpoint = true;
                distance = UnityEngine.Random.Range(checkpointDistances[playerCheckpoint - 1] + 300, checkpointDistances[playerCheckpoint] - 300);
                break;
        }
        if(sameCheckpoint && showCaptions)
        {
            float mag = rb.position.magnitude;
            if(mag > distance) StartCoroutine(SetCaption("Teleported\n" + ((mag - distance) / 1000).ToString("0.0") + " units back", twoSeconds, 3));
            else StartCoroutine(SetCaption("Teleported\n" + ((distance - mag) / 1000).ToString("0.0") + " units ahead", twoSeconds, 3));
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

    void AddCheckpoint(int playerCheckpoint)
    {
        if(showCaptions) StartCoroutine(SetCaption(checkpointCaptions[playerCheckpoint], threeSeconds, 3));
        playerCheckpoint ++;
        if(spawnManager.playerCheckpoints[playerIndex] == playerCheckpoint) return;
        spawnManager.playerCheckpoints[playerIndex] = playerCheckpoint;
        checkpointText.SetText("Checkpoint: " + playerCheckpoint.ToString());
        int newDrones = checkpointDrones[playerCheckpoint];
        for(int i = activeDrones; i < newDrones; i++)
        {
            drones[i].gameObject.SetActive(true);
        }
        activeDrones = newDrones;
        StartCoroutine(ChangeTextColor(checkpointText, halfSecond, Color.white, green));
        audioSource.PlayOneShot(checkpointSound);
        voiceSource.clip = checkpointVoice;
        voiceSource.PlayDelayed(0.2f);
    }

    void RemoveCheckpoint(int playerCheckpoint)
    {
        if(showCaptions) StartCoroutine(SetCaption(backCaptions[playerCheckpoint], threeSeconds, 3));
        if(spawnManager.playerCheckpoints[playerIndex] == playerCheckpoint) return;
        spawnManager.playerCheckpoints[playerIndex] = playerCheckpoint;
        checkpointText.SetText("Checkpoint: " + playerCheckpoint.ToString());
        StartCoroutine(ChangeTextColor(checkpointText, halfSecond, Color.white, Color.red));
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
        voiceSource.clip = activationVoice;
        voiceSource.PlayDelayed(0.2f);
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
            audioSource.PlayOneShot(chargeSound, 0.7f);
            voiceSource.clip = chargeVoice;
            voiceSource.Play();
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
        atmosphere.hitForce = abilityStrength;
        for(int i = 0; i < 3; i++) auraParticles[i].duration = auraParticles[i].startLifetimeMultiplier = activationTime;
        auraParticles[3].duration = activationTime - 1;
        aura.SetActive(true);
        audioSource.PlayOneShot(activationSound);
        voiceSource.clip = activationVoice;
        voiceSource.PlayDelayed(0.2f);
        while(currentActivation > 0)
        {
            abilityText.SetText("Activated: " + currentActivation.ToString());
            yield return oneSecond;
            currentActivation --;
        }
        audioSource.PlayOneShot(expireSound);
        voiceSource.clip = expireVoice;
        voiceSource.Play();
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
            audioSource.PlayOneShot(chargeSound, 0.7f);
            voiceSource.clip = chargeVoice;
            voiceSource.Play();
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
        voiceSource.clip = activationVoice;
        voiceSource.PlayDelayed(0.2f);
        while(currentActivation > 0)
        {
            abilityText.SetText("Activated: " + currentActivation.ToString());
            if(health + abilityStrength < 100)
            {
                health += abilityStrength;
                health = (float) Math.Round(health, 1);
                healthText.SetText(health.ToString("0.00"));
                healthBar.fillAmount = health / 100;
            }
            else
            {
                health = 100;
                healthText.SetText("100.00");
                healthBar.fillAmount = 1;
            }
            yield return oneSecond;
            currentActivation --;
        }
        for(int i = 0; i < auraCount; i++) auraParticles[i].loop = false;
        resetting = StartCoroutine(ResetAura(twoSeconds));
        healthText.color = healthLabel.color = Color.white;
        audioSource.PlayOneShot(expireSound);
        voiceSource.clip = expireVoice;
        voiceSource.Play();

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
            audioSource.PlayOneShot(chargeSound, 0.7f);
            voiceSource.clip = chargeVoice;
            voiceSource.Play();
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
        voiceSource.clip = activationVoice;
        voiceSource.PlayDelayed(0.2f);
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
        voiceSource.clip = expireVoice;
        voiceSource.Play();
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
            audioSource.PlayOneShot(chargeSound, 0.7f);
            voiceSource.clip = chargeVoice;
            voiceSource.Play();
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
        voiceSource.clip = activationVoice;
        voiceSource.PlayDelayed(0.2f);
        speedLossDivisor = 1;
        healthLossDivisor = abilityStrength2;
        minHealthLoss = 0;
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
        voiceSource.clip = expireVoice;
        voiceSource.Play();
        speedLossDivisor = collisionInitials[0];
        healthLossDivisor = (int) collisionInitials[1];
        minHealthLoss = collisionInitials[2];
        deflectionForce = collisionInitials[3];
        rb.mass = collisionInitials[4];

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
            audioSource.PlayOneShot(chargeSound, 0.7f);
            voiceSource.clip = chargeVoice;
            voiceSource.Play();
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
        ring.hitForce = abilityStrength;
        aura.SetActive(true);
        audioSource.PlayOneShot(activationSound);
        voiceSource.clip = activationVoice;
        voiceSource.PlayDelayed(0.2f);
        while(currentActivation > 0)
        {
            abilityText.SetText("Activated: " + currentActivation.ToString());
            yield return oneSecond;
            currentActivation --;
        }
        for(int i = 0; i < auraCount; i++) auraParticles[i].loop = false;
        resetting = StartCoroutine(ResetAura(oneSecond));
        audioSource.PlayOneShot(expireSound);
        voiceSource.clip = expireVoice;
        voiceSource.Play();

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
            audioSource.PlayOneShot(chargeSound, 0.7f);
            voiceSource.clip = chargeVoice;
            voiceSource.Play();
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
        voiceSource.clip = activationVoice;
        voiceSource.PlayDelayed(0.2f);
        aura.SetActive(true);
        int colliders = Physics.OverlapSphereNonAlloc(rb.position, abilityStrength2, surroundingColliders, abilityMask);
        for(int i = 0; i < colliders; i++)
        {
            Collider collider = surroundingColliders[i];
            Rigidbody otherRb = collider.attachedRigidbody;
            if(otherRb.isKinematic || collider.gameObject == gameObject)
            {
                continue;
            }
            if(collider.gameObject.CompareTag("Drone"))
            {
                collider.GetComponent<Drone>().Explode(otherRb.position);
            }
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
            audioSource.PlayOneShot(chargeSound, 0.7f);
            voiceSource.clip = chargeVoice;
            voiceSource.Play();
            yield return oneSecond;
        }
        abilityText.SetText("");
    }

    IEnumerator ResetAura(WaitForSeconds wait)
    {
        yield return wait;
        aura.SetActive(false);
        for(int i = 0; i < auraCount; i++) auraParticles[i].loop = true;
        if(!canCollide) canCollide = true;
    }
}

