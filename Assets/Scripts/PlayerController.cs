using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.InputSystem;

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
    [SerializeField] GameObject thrusterParent;
    [SerializeField] float thrusterSpeedDivisor = 40;
    [SerializeField] float maxThrusterSpeed = 20;
    [SerializeField] float thrusterVolumeDivisor = 4000;

    [Header("Collision")]
    [SerializeField] GameObject dieExplosion;
    [SerializeField] List<GameObject> hitExplosions;
    [SerializeField] GameObject collectEffect;
    [SerializeField] int forwardPercent = 30;
    [SerializeField] int backPercent = 30;
    [SerializeField] GameObject dronePrefab;
    [SerializeField] GameObject graphic;

    [Header("Ability")]
    [SerializeField] string shipModel;
    [SerializeField] int abilityPercent;
    [SerializeField] int activationTimePercent;
    [SerializeField] int minCooldown;
    [SerializeField] string abilityCaption;
    [SerializeField] GameObject aura;
    [SerializeField] ParticleSystem[] auraSystems;
    [SerializeField] LayerMask abilityMask;
    Collider[] surroundingColliders = new Collider[5000];
    

    [Header("UI/Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioSource fireSource;
    [SerializeField] AudioSource thrusterSource;
    [SerializeField] AudioClip[] hitSounds;
    [SerializeField] AudioClip collectSound;
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
    Vector3 startPos;
    Vector3 normalizedStartPos;
    Quaternion startRot;

    //Collisions/triggers
    float speedLossDivisor;
    int healthLossDivisor;
    float minHealthLoss;
    float deflectionForce;
    float health = 100;
    bool collided;
    public bool canCollide;
    bool canTrigger;
    public static SpawnManager spawnManager;
    public static int length;
    public static int playerNumber;
    public static int[] checkpointDistances;
    float[] collisionInitials;
    ParticleSystem.MainModule[] thrusters;
    int thrusterCount;
    int hitSoundsCount;
    public int collects;
    GameObject hitExplosionPrefab;
    public static int credits;
    public static int creditIncrease;

    //Ability
    IEnumerator ability;
    Coroutine runningAbility;
    InputAction abilityAction;
    int currentActivation;
    int currentCooldown;
    int auraCount;
    ParticleSystem.MainModule[] auraParticles;
    Ring ring;
    bool died;
    Coroutine resetting;
    public static WaitForSeconds oneSecond;
    public static WaitForSeconds twoSeconds;
    public static WaitForSeconds fifthSecond;
    public static WaitForSeconds oneAndHalfSecond;
    public static WaitForSeconds halfSecond;

    //Drone
    Transform droneGroup;
    public static int droneNumber;
    public static int[] checkpointDrones;
    int activeDrones;
    Drone[] droneScripts;
    GameObject[] drones;

    //UI
    public GameObject cameraObject;
    TMP_Text nameText;
    TMP_Text healthText;
    TMP_Text checkpointText;
    public static TMP_Text[] checkpointTexts;
    TMP_Text collectText;
    TMP_Text abilityText;
    TMP_Text posText;
    TMP_Text distanceText;
    TMP_Text speedText;
    TMP_Text timeText;
    TMP_Text caption;
    GameObject finishShip;
    TMP_Text finishText;
    TMP_Text countdownText;
    TMP_Text[] allText;
    GameObject finishBackground;
    public static float maxThrusterVolume;
    public static float audioVolume;
    public static bool showCaptions;
    bool raceMode;
    public static bool showArrow;

    void Awake()
    {
        dieExplosion.SetActive(false);
        hitExplosionPrefab = hitExplosions[0];
        hitExplosionPrefab.SetActive(false);
        aura.SetActive(false);
        collectEffect.SetActive(false);
        thrusterParent.SetActive(false);
        arrow.gameObject.SetActive(false);
        fireSource.volume = 0;
        thrusterSource.volume = 0;

        rb = gameObject.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        myTransform = transform;
        startPos = myTransform.position;
        startRot = myTransform.rotation;

        Transform thrusterTransform = thrusterParent.transform;
        thrusterCount = thrusterTransform.childCount;
        thrusters = new ParticleSystem.MainModule[thrusterCount];
        for(int i = 0; i < thrusterCount; i++) thrusters[i] = thrusterTransform.GetChild(i).GetComponent<ParticleSystem>().main;
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
                playerIndex = 0;
                break;
            case "2":
                playerIndex = 1;
                break;
            case "3":
                playerIndex = 2;
                break;
            case "4":
                playerIndex = 3;
                break;
        }
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

        Transform cameraStats = cameraObject.transform.GetChild(1);
        Transform farCanvas = cameraStats.GetChild(0);
        switch(shipModel)
        {
            case "Mercury":
                ability = SpeedUp();
                finishShip = farCanvas.GetChild(11).GetChild(0).gameObject;
                break;
            case "Venus":
                ability = Atmosphere();
                aura = Instantiate(aura);
                Atmosphere atmosphere = aura.GetComponent<Atmosphere>();
                atmosphere.target = myTransform;
                atmosphere.audioSource = audioSource;
                Transform graphic = aura.transform.GetChild(0);
                auraSystems = new ParticleSystem[] {graphic.GetComponent<ParticleSystem>(), graphic.GetChild(0).GetComponent<ParticleSystem>(), graphic.GetChild(1).GetComponent<ParticleSystem>(), graphic.GetChild(2).GetComponent<ParticleSystem>()};
                aura.SetActive(false);
                SpawnManager.followers[playerIndex].Add(aura.transform);
                finishShip = farCanvas.GetChild(11).GetChild(1).gameObject;
                break;
            case "Earth":
                ability = Regeneration();
                finishShip = farCanvas.GetChild(11).GetChild(2).gameObject;
                break;
            case "Mars":
                ability = DustStorm();
                finishShip = farCanvas.GetChild(11).GetChild(3).gameObject;
                break;
            case "Jupiter":
                ability = Juggernaut();
                collisionInitials = new float[] {speedLossDivisor, healthLossDivisor, minHealthLoss, deflectionForce};
                finishShip = farCanvas.GetChild(11).GetChild(4).gameObject;
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
                finishShip = farCanvas.GetChild(11).GetChild(5).gameObject;
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
                finishShip = farCanvas.GetChild(11).GetChild(6).gameObject;
                break;
            case "Neptune":
                ability = WindStorm();
                finishShip = farCanvas.GetChild(11).GetChild(7).gameObject;
                break;
        }
        finishShip.SetActive(false);

        //Spawn drones
        droneGroup = new GameObject("Drones " + playerNumberString).transform;
        droneGroup.gameObject.SetActive(false);
        droneScripts = new Drone[droneNumber];
        drones = new GameObject[droneNumber];
        for(int i = 0; i < droneNumber; i++)
        {
            GameObject drone = Instantiate(dronePrefab, droneGroup);
            Drone droneScript = drone.GetComponent<Drone>();
            droneScript.target = myTransform;
            droneScript.audioSource = audioSource;
            droneScripts[i] = droneScript;
            drones[i] = drone;
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
        nameText = farCanvas.GetChild(1).GetComponent<TMP_Text>();
        healthText = farCanvas.GetChild(2).GetComponent<TMP_Text>();
        checkpointText = farCanvas.GetChild(3).GetComponent<TMP_Text>();
        collectText = farCanvas.GetChild(4).GetComponent<TMP_Text>();
        abilityText = farCanvas.GetChild(5).GetComponent<TMP_Text>();
        posText = farCanvas.GetChild(6).GetComponent<TMP_Text>();
        distanceText = farCanvas.GetChild(7).GetComponent<TMP_Text>();
        speedText = farCanvas.GetChild(8).GetComponent<TMP_Text>();
        timeText = farCanvas.GetChild(9).GetComponent<TMP_Text>();
        caption = farCanvas.GetChild(10).GetComponent<TMP_Text>();
        finishText = farCanvas.GetChild(12).GetComponent<TMP_Text>();
        countdownText = cameraStats.GetChild(1).GetChild(0).GetComponent<TMP_Text>();
        
        finishBackground.SetActive(false);
        if(MenuManager.loaded) nameText.SetText(MenuManager.playerNames[playerIndex]);
        else nameText.SetText("Player " + playerNumberString);
        healthText.SetText("Health: 100.00");
        collectText.SetText("Space Junk: 0");
        abilityText.SetText("");
        posText.SetText("Pos: 0,0,0");
        distanceText.SetText("Distance: 0");
        speedText.SetText("Speed: 0.00");
        if(showCaptions) caption.SetText("");
        else caption.gameObject.SetActive(false);
        finishText.SetText("");
        spawnManager.finishTexts[playerIndex] = finishText;
        countdownText.SetText("");

        raceMode = spawnManager.raceMode;
        if(raceMode)
        {
            normalizedStartPos = startPos.normalized;
            if(showArrow)
            {
                arrow.GetChild(0).GetComponent<Canvas>().worldCamera = cameraObject.transform.GetChild(0).GetComponent<Camera>();
                arrow.gameObject.layer = playerIndex + 6;
            }
            checkpointText.SetText("Checkpoint: 0");
            timeText.SetText("Time: 0:00.000");
        }
        else
        {
            checkpointTexts[playerIndex] = checkpointText;
            credits = MenuManager.credits;
            checkpointText.SetText("Credits: " + credits);
            timeText.SetText("Time: " + spawnManager.minutes + ":00.000");
        }
        
        audioSource.volume = audioVolume;

        allText = new TMP_Text[]{nameText, checkpointText, healthText, speedText, collectText, abilityText, timeText, posText, distanceText};
        StartCoroutine(Countdown(3));
    }

    // Update is called once per frame
    void Update()
    {
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
                    ring.rotationOffset *= Quaternion.Euler(Vector3.right);
                    break;
                
                case "Uranus":
                    ring.rotationOffset *= Quaternion.Euler(new Vector3(0, 2, 0));
                    break;
            }
        }

        if(spawnManager.inCountdown) return;

        float speed = rb.velocity.magnitude;
        
        if(!rb.isKinematic)
        {
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
            switch(thrust)
            {
                case > 0:
                    if(glideSpeed != thrustSpeed) glideSpeed = thrustSpeed;
                    
                    if(thrustSpeed < accThreshold) thrustSpeed += accIncrement;
                    else if(thrustSpeed != maxThrustSpeed) thrustSpeed = maxThrustSpeed;
                    
                    if(decSpeed > decThreshold && dec != 1) decSpeed *= dec;
                    else if(decSpeed != initialThrustSpeed) decSpeed = initialThrustSpeed;
                    
                    break;

                case < 0:
                    /*if(glideSpeed != -decSpeed)
                    {
                        glideSpeed = -decSpeed;
                    }*/
                    if(glideSpeed != 0) glideSpeed = 0;

                    if(decSpeed < accThreshold) decSpeed += accIncrement;
                    else if(decSpeed != maxThrustSpeed) decSpeed = maxThrustSpeed;

                    if(thrustSpeed > decThreshold && dec != 1) thrustSpeed *= dec;
                    else if(thrustSpeed != initialThrustSpeed) thrustSpeed = initialThrustSpeed;

                    break;
                
                /*default:
                    float newGlide = glideSpeed - glideDrag * Time.deltaTime;
                    if(newGlide > 0) glideSpeed = newGlide;
                    else glideSpeed = 0;
                    /*switch(glideSpeed)
                    {
                        case > 0:
                            
                        case < 0:
                            newGlide = glideSpeed + glideDrag * Time.deltaTime;
                            if(newGlide < 0)
                            {
                                glideSpeed = newGlide;
                            }
                            else
                            {
                                glideSpeed = 0;
                            }
                            break;
                    }
                    break;*/
            }
        
            //Show speed in km/h, multiply by 2.23694 for mph
            speedText.SetText("Speed: " + (speed * 3.6f).ToString("0.00"));
        }

        //Change thruster effect based on speed
        float thrusterSpeed = speed / thrusterSpeedDivisor;
        if(thrusterSpeed > maxThrusterSpeed) thrusterSpeed = maxThrusterSpeed;
        else if(thrusterSpeed < 1) thrusterSpeed = 1;
        for(int i = 0; i < thrusterCount; i++)
        {
            thrusters[i].startSpeedMultiplier = thrusterSpeed;
        }
        float thrusterVolume = speed / thrusterVolumeDivisor;
        if(thrusterVolume > maxThrusterVolume) thrusterVolume = maxThrusterVolume;
        fireSource.volume = thrusterVolume;
        thrusterSource.volume = thrusterVolume;

        Vector3 position = myTransform.position;
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
    
        //Show time
        timeText.SetText(spawnManager.timeString);
        
        //Check if passed checkpoint
        if(raceMode)
        {
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
                        Finish();
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
        }
        else if(posMag >= 700000)
        {
            rb.AddForce(-myTransform.position.normalized * 2000, ForceMode.VelocityChange);
            StartCoroutine(ChangeTextColor(distanceText, fifthSecond, Color.white, Color.red));
            audioSource.PlayOneShot(errorSound);
        }

        //Show position in km, divide by 1609 for mi
        Vector3 kmPos = position / 1000;
        posText.SetText("Pos: " + kmPos.x.ToString("0") + "," + kmPos.y.ToString("0") + "," + kmPos.z.ToString("0"));

        //Show distance from origin
        distanceText.SetText("Distance: " + posMag.ToString("0"));
    }

    void LateUpdate()
    {
        if(rb.isKinematic) return;
        droneGroup.position = myTransform.position;

        if(!showArrow) return;
        arrow.localPosition = Vector3.zero;
        arrow.rotation = Quaternion.LookRotation(myTransform.position.normalized, Vector3.up);
        Vector3 rot = arrow.rotation.eulerAngles;
        arrow.rotation = Quaternion.Euler(rot.x, rot.y, myTransform.rotation.eulerAngles.z);
    }

    void FixedUpdate()
    {
        if(rb.isKinematic) return;

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

    void OnTriggerEnter(Collider other)
    {
        if(!canTrigger) return;

        //Collected powerup
        if(other.gameObject.CompareTag("Collectable"))
        {
            other.gameObject.SetActive(false);
            collects++;
            collectText.SetText("Space Junk: " + collects);
            StartCoroutine(ChangeTextColor(collectText, fifthSecond, Color.white, Color.green));
            if(raceMode)
            {
                int number = UnityEngine.Random.Range(0, 100);
                if(number < abilityPercent)
                {
                    abilityStrength += abilityIncrease;
                    if(showCaptions) StartCoroutine(ResetText(caption, abilityCaption));
                }
                else if(number < abilityPercent + activationTimePercent || cooldownTime <= minCooldown)
                {
                    activationTime++;
                    currentActivation++;
                    if(showCaptions) StartCoroutine(ResetText(caption, "+1 Ability Duration"));
                }
                else
                {
                    cooldownTime--;
                    if(currentCooldown > 1) currentCooldown--;
                    if(showCaptions) StartCoroutine(ResetText(caption, "-1 Ability Cooldown"));
                }
            }
            else
            {
                credits += creditIncrease;
                for(int i = 0; i < playerNumber; i++) checkpointTexts[i].SetText("Credits: " + credits);
                StartCoroutine(ChangeTextColor(checkpointText, fifthSecond, Color.white, Color.green));
            }
            if(!collectEffect.activeSelf)
            {
                collectEffect.SetActive(true);
                StartCoroutine(ResetEffect(collectEffect, fifthSecond));
            }
            audioSource.PlayOneShot(collectSound);
        }
    }

    /*void OnTriggerExit(Collider other)
    {
        if(!canTrigger)
        {
            return;
        }

        //Spawn new sections on trigger
        if(other.gameObject.CompareTag("Section"))
        {
            Vector3 sectionPos = other.transform.position;
            Vector3 posMag = rb.position;
            float myX = posMag.x;
            float myY = posMag.y;
            float myZ = posMag.z;
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
    }*/
    
    void OnCollisionEnter(Collision other)
    {
        if(collided || !canCollide) return;

        collided = true;
        float speed = rb.velocity.magnitude;
        float otherSpeed = other.rigidbody.velocity.magnitude;
        if(otherSpeed > speed) speed = otherSpeed;
        Vector3 otherPos = myTransform.InverseTransformPoint(other.transform.position);
        rb.AddTorque(new Vector3(-otherPos.y, otherPos.x, UnityEngine.Random.Range(-1f, 1)).normalized * speed * deflectionForce, ForceMode.VelocityChange);

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
        if(speed / healthLossDivisor > minHealthLoss) health -= speed / healthLossDivisor;
        else health -= minHealthLoss;

        //Player died
        switch(health)
        {
            case <= 0:
                canCollide = false;
                canTrigger = false;
                rb.isKinematic = true;

                dieExplosion.transform.position = other.GetContact(0).point;
                dieExplosion.SetActive(true);
                StartCoroutine(ResetEffect(dieExplosion, twoSeconds));

                died = true;
                currentActivation = 0;
                currentCooldown = 0;
                aura.SetActive(false);
                if(resetting != null) StopCoroutine(resetting);
                if(shipModel != "Venus")
                {
                    for(int i = 0; i < auraCount; i++)
                    {
                        auraParticles[i].loop = true;
                    }
                }
                else auraParticles[3].loop = true;

                health = 0;
                healthText.SetText("Health: 0.00");
                for(int i = 0; i < 9; i++)
                {
                    StartCoroutine(ChangeTextColor(allText[i], halfSecond, Color.white, Color.red));
                }
                for(int i = 0; i < activeDrones; i++) droneScripts[i].ResetDrone();

                //Respawning
                StartCoroutine(Respawn());
                return;

            case <= 10:
                if(healthText.color != Color.red) healthText.color = Color.red;
                break;
            
            default:
                StartCoroutine(ChangeTextColor(healthText, fifthSecond, healthText.color, Color.red, 2));
                break;
        }
        health = (float) Math.Round(health, 2);
        healthText.SetText("Health: " + health.ToString("0.00"));
    }

    IEnumerator Respawn()
    {
        graphic.SetActive(false);
        thrusterParent.SetActive(false);
        if(collectEffect.activeSelf) collectEffect.SetActive(false);
        if(showArrow) arrow.gameObject.SetActive(false);
        finishText.color = Color.red;
        finishText.SetText("You Died!");
        audioSource.PlayOneShot(dieSound);

        yield return oneAndHalfSecond;
        finishText.SetText("");
        finishText.color = Color.white;
        if(raceMode)
        {
            int playerCheckpoint = spawnManager.playerCheckpoints[playerIndex];
            switch(playerCheckpoint)
            {
                case 0:
                    rb.position = startPos;
                    rb.rotation = startRot;
                    spawnManager.SpawnSectionsAround(Vector3.zero, playerIndex);
                    break;
                default:
                    Vector3 newPos = startPos + normalizedStartPos * checkpointDistances[spawnManager.playerCheckpoints[playerIndex] - 1];
                    rb.position = newPos;
                    rb.rotation = startRot;
                    spawnManager.SpawnSectionsAround(new Vector3((int) Mathf.Round(newPos.x / length), (int) Mathf.Round(newPos.y / length), (int) Mathf.Round(newPos.z / length)), playerIndex);
                    break;
            }
        }
        else
        {
            rb.position = startPos;
            rb.rotation = startRot;
            spawnManager.SpawnSectionsAround(Vector3.zero, playerIndex);
            credits -= collects * creditIncrease;
            for(int i = 0; i < playerNumber; i++) checkpointTexts[i].SetText("Credits: " + credits);
        }
        collects = 0;
        collectText.SetText("Space Junk: 0");
        thrustSpeed = initialThrustSpeed;
        health = 100;
        healthText.SetText("Health: 100.00");

        graphic.SetActive(true);
        thrusterParent.SetActive(true);
        if(showArrow) arrow.gameObject.SetActive(true);
        rb.isKinematic = false;
        canTrigger = true;
        canCollide = true;
    }

    void OnCollisionExit(Collision other)
    {
        if(!canCollide) return;
        if(other.gameObject.CompareTag("Asteroid") || other.gameObject.layer == 6 || other.gameObject.CompareTag("Drone")) collided = false;
    }

    IEnumerator Countdown(int countdownTime)
    {
        spawnManager.initializedObjects ++;
        while(countdownTime > 0)
        {
            countdownText.SetText(countdownTime.ToString());
            countdownTime --;
            yield return oneSecond;
        }
        countdownText.SetText("Go!");
        yield return oneSecond;
        if(raceMode)
        {
            if(showArrow) arrow.gameObject.SetActive(true);
            activeDrones = checkpointDrones[0];
            for(int i = 0; i < activeDrones; i++)
            {
                drones[i].SetActive(true);
            }
        }
        else
        {
            for(int i = 0; i < droneNumber; i++)
            {
                drones[i].SetActive(true);
            }
        }
        countdownText.gameObject.SetActive(false);
        thrusterParent.SetActive(true);
        rb.isKinematic = false;
        canCollide = true;
        canTrigger = true;
    }

    IEnumerator ChangeTextColor(TMP_Text text, WaitForSeconds wait, Color originalColor, Color newColor, int amount = 1)
    {
        if(originalColor == newColor) originalColor = Color.white;
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
    }

    IEnumerator ResetEffect(GameObject effect, WaitForSeconds wait)
    {
        yield return wait;
        effect.SetActive(false);
    }

    IEnumerator ResetText(TMP_Text text, string phrase)
    {
        text.SetText(phrase);
        yield return oneSecond;
        if(text.text == phrase)
        {
            text.SetText("");
        }
    }

    public void Finish()
    {
        gameObject.SetActive(false);
        if(showCaptions) caption.gameObject.SetActive(false);
        abilityText.gameObject.SetActive(false);
        finishBackground.SetActive(true);
        finishShip.SetActive(true);
        if(!raceMode) timeText.SetText("Time: 0:00.000");
        else
        {
            spawnManager.PlayerFinish(playerIndex);
            if(MenuManager.loaded)
            {
                DateTime finishTime = DateTime.Now;
                int elapsedMilliseconds = spawnManager.milliseconds + spawnManager.seconds * 1000 + spawnManager.minutes * 60000;
                if(elapsedMilliseconds < MenuManager.times[MenuManager.difficulty] || MenuManager.times[MenuManager.difficulty] == 0)
                {
                    int index = MenuManager.difficulty;
                    MenuManager.times[index] = elapsedMilliseconds;
                    PlayerPrefs.SetInt("Time" + index, elapsedMilliseconds);
                    PlayerPrefs.SetString("Description" + index, ", " + MenuManager.playerNames[playerIndex] + "-" + shipModel + ", " + finishTime.Month.ToString("00") + "/" + finishTime.Day.ToString("00") + "/" + finishTime.Year);
                }
            }
        }
        if(collects != 0 && MenuManager.loaded)
        {
            MenuManager.collects += collects;
            PlayerPrefs.SetInt("Collects", MenuManager.collects);
        }
    }

    public void Teleport()
    {
        canTrigger = false;
        canCollide = false;
        int playerCheckpoint = spawnManager.playerCheckpoints[playerIndex];
        int direction = UnityEngine.Random.Range(0, 100);
        float distance;
        if(raceMode)
        {
            switch(playerCheckpoint)
            {
                case 0:
                    if(direction < forwardPercent)
                    {
                        AddCheckpoint(playerCheckpoint);
                        playerCheckpoint ++;
                        distance = UnityEngine.Random.Range(checkpointDistances[playerCheckpoint - 1] + 20, checkpointDistances[playerCheckpoint] - 20);
                        break;
                    }
                    distance = UnityEngine.Random.Range(0f, checkpointDistances[playerCheckpoint] - 20);
                    break;

                case 1:
                    if(direction < forwardPercent)
                    {
                        AddCheckpoint(playerCheckpoint);
                        playerCheckpoint ++;
                        distance = UnityEngine.Random.Range(checkpointDistances[playerCheckpoint - 1] + 20, checkpointDistances[playerCheckpoint] - 20);
                        break;
                    }
                    else if(direction < forwardPercent + backPercent)
                    {
                        playerCheckpoint --;
                        RemoveCheckpoint(playerCheckpoint);
                        distance = UnityEngine.Random.Range(0f, checkpointDistances[playerCheckpoint] - 5);
                        break;
                    }
                    distance = UnityEngine.Random.Range(checkpointDistances[playerCheckpoint - 1] + 20, checkpointDistances[playerCheckpoint] - 20);
                    break;
                
                case 8:
                    if(direction < backPercent)
                    {
                        playerCheckpoint --;
                        RemoveCheckpoint(playerCheckpoint);
                    }
                    distance = UnityEngine.Random.Range(checkpointDistances[playerCheckpoint - 1] + 20, checkpointDistances[playerCheckpoint] - 20);
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
                    distance = UnityEngine.Random.Range(checkpointDistances[playerCheckpoint - 1] + 20, checkpointDistances[playerCheckpoint] - 20);
                    break;
            }
        }
        else distance = UnityEngine.Random.Range(0f, 99080);

        Vector3 newPos = Quaternion.Euler(UnityEngine.Random.Range(0f, 360), UnityEngine.Random.Range(0f, 360), 0) * new Vector3(0, 0, distance);
        rb.position = newPos;
        spawnManager.SpawnSectionsAround(new Vector3(Mathf.RoundToInt(newPos.x / length), Mathf.RoundToInt(newPos.y / length), Mathf.RoundToInt(newPos.z / length)), playerIndex);
        audioSource.PlayOneShot(wormholeSound);
        canCollide = true;
        canTrigger = true;
    }

    void AddCheckpoint(int playerCheckpoint)
    {
        playerCheckpoint ++;
        if(spawnManager.playerCheckpoints[playerIndex] == playerCheckpoint) return;
        spawnManager.playerCheckpoints[playerIndex] = playerCheckpoint;
        checkpointText.SetText("Checkpoint: " + playerCheckpoint);
        int newDrones = checkpointDrones[playerCheckpoint];
        for(int i = activeDrones; i < newDrones; i++)
        {
            drones[i].gameObject.SetActive(true);
        }
        activeDrones = newDrones;
        StartCoroutine(ChangeTextColor(checkpointText, halfSecond, Color.white, Color.green));
        audioSource.PlayOneShot(checkpointSound);
    }

    void RemoveCheckpoint(int playerCheckpoint)
    {
        if(spawnManager.playerCheckpoints[playerIndex] == playerCheckpoint) return;
        spawnManager.playerCheckpoints[playerIndex] = playerCheckpoint;
        checkpointText.SetText("Checkpoint: " + playerCheckpoint);
        StartCoroutine(ChangeTextColor(checkpointText, halfSecond, Color.white, Color.red));
        audioSource.PlayOneShot(errorSound);
    }

    //Abilities
    IEnumerator SpeedUp()
    {
        //Activation
        ability = SpeedUp();
        audioSource.PlayOneShot(activationSound);
        aura.SetActive(true);
        rb.AddRelativeForce(Vector3.forward * (rb.velocity.magnitude + abilityStrength), ForceMode.VelocityChange);
        glideSpeed += abilityStrength;
        resetting = StartCoroutine(ResetAura(twoSeconds));

        //Cooldown
        while(currentCooldown > 0)
        {
            abilityText.SetText("Charging: " + currentCooldown);
            currentCooldown --;
            yield return oneSecond;
        }
        if(!died)
        {
            abilityText.SetText("Ability Charged!");
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
        for(int i = 0; i < 3; i++) auraParticles[i].duration = auraParticles[i].startLifetimeMultiplier = activationTime;
        aura.SetActive(true);
        audioSource.PlayOneShot(activationSound);
        while(currentActivation > 0)
        {
            abilityText.SetText("Activated: " + currentActivation);
            currentActivation --;
            yield return oneSecond;
        }
        auraParticles[2].loop = false;
        resetting = StartCoroutine(ResetAura(twoSeconds, 3));
        audioSource.PlayOneShot(expireSound);

        //Cooldown
        while(currentCooldown > 0)
        {
            abilityText.SetText("Charging: " + currentCooldown);
            currentCooldown --;
            yield return oneSecond;
        }
        if(!died)
        {
            abilityText.SetText("Ability Charged!");
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
        audioSource.PlayOneShot(activationSound);
        while(currentActivation > 0)
        {
            abilityText.SetText("Activated: " + currentActivation);
            currentActivation --;
            if(health + abilityStrength < 100) health += abilityStrength;
            else health = 100;
            health = (float) Math.Round(health, 2);
            healthText.SetText("Health: " + health.ToString("0.00"));
            yield return oneSecond;
        }
        for(int i = 0; i < auraCount; i++) auraParticles[i].loop = false;
        resetting = StartCoroutine(ResetAura(twoSeconds));
        audioSource.PlayOneShot(expireSound);

        //Cooldown
        while(currentCooldown > 0)
        {
            abilityText.SetText("Charging: " + currentCooldown);
            currentCooldown --;
            yield return oneSecond;
        }
        if(!died)
        {
            abilityText.SetText("Ability Charged!");
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
            abilityText.SetText("Activated: " + currentActivation);
            currentActivation --;
            yield return oneSecond;
        }
        for(int i = 0; i < auraCount; i++) auraParticles[i].loop = false;
        resetting = StartCoroutine(ResetAura(twoSeconds));
        audioSource.PlayOneShot(expireSound);
        for(int i = 0; i < droneNumber; i++) droneScripts[i].offset = 0;

        //Cooldown
        while(currentCooldown > 0)
        {
            abilityText.SetText("Charging: " + currentCooldown);
            currentCooldown --;
            yield return oneSecond;
        }
        if(!died)
        {
            abilityText.SetText("Ability Charged!");
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
        healthLossDivisor = 1000;
        minHealthLoss = 0;
        deflectionForce = 0;
        rb.mass *= abilityStrength;
        while(currentActivation > 0)
        {
            abilityText.SetText("Activated: " + currentActivation);
            currentActivation --;
            yield return oneSecond;
        }
        for(int i = 0; i < auraCount; i++) auraParticles[i].loop = false;
        resetting = StartCoroutine(ResetAura(twoSeconds));
        audioSource.PlayOneShot(expireSound);
        speedLossDivisor = collisionInitials[0];
        healthLossDivisor = (int) collisionInitials[1];
        minHealthLoss = collisionInitials[2];
        deflectionForce = collisionInitials[3];
        rb.mass /= abilityStrength;

        //Cooldown
        while(currentCooldown > 0)
        {
            abilityText.SetText("Charging: " + currentCooldown);
            currentCooldown --;
            yield return oneSecond;
        }
        if(!died)
        {
            abilityText.SetText("Ability Charged!");
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
        ring.hitForce = abilityStrength;
        aura.SetActive(true);
        audioSource.PlayOneShot(activationSound);
        while(currentActivation > 0)
        {
            abilityText.SetText("Activated: " + currentActivation);
            currentActivation --;
            yield return oneSecond;
        }
        for(int i = 0; i < auraCount; i++) auraParticles[i].loop = false;
        resetting = StartCoroutine(ResetAura(oneSecond));
        audioSource.PlayOneShot(expireSound);

        //Cooldown
        while(currentCooldown > 0)
        {
            abilityText.SetText("Charging: " + currentCooldown);
            currentCooldown --;
            yield return oneSecond;
        }
        if(!died)
        {
            abilityText.SetText("Ability Charged!");
            audioSource.PlayOneShot(chargeSound);
            yield return oneSecond;
        }
        abilityText.SetText("");
    }

    IEnumerator WindStorm()
    {
        //Activation
        ability = WindStorm();
        audioSource.PlayOneShot(activationSound);
        aura.SetActive(true);
        Array.Clear(surroundingColliders, 0, 5000);
        int colliders = Physics.OverlapSphereNonAlloc(myTransform.position, 5000, surroundingColliders, abilityMask);
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
                collider.GetComponent<Drone>().Explode(collider.transform.position, myTransform);
            }
            else
            {
                otherRb.AddForce((collider.transform.position - myTransform.position).normalized * abilityStrength, ForceMode.VelocityChange);
                otherRb.AddTorque(new Vector3(UnityEngine.Random.Range(0f, 360), UnityEngine.Random.Range(0f, 360), UnityEngine.Random.Range(0f, 360)) * abilityStrength, ForceMode.Impulse);
            }
        }
        resetting = StartCoroutine(ResetAura(oneSecond));

        //Cooldown
        while(currentCooldown > 0)
        {
            abilityText.SetText("Charging: " + currentCooldown);
            currentCooldown --;
            yield return oneSecond;
        }
        if(!died)
        {
            abilityText.SetText("Ability Charged!");
            audioSource.PlayOneShot(chargeSound);
            yield return oneSecond;
        }
        abilityText.SetText("");
    }

    IEnumerator ResetAura(WaitForSeconds wait, int startIndex = 0)
    {
        yield return wait;
        aura.SetActive(false);
        for(int i = startIndex; i < auraCount; i++) auraParticles[i].loop = true;
        if(!canCollide) canCollide = true;
    }
}

