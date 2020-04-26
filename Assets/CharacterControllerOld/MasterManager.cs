using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class MasterManager : MonoBehaviour {

    //HitManager hitManager;
    //ProjectileManager projectileManager;
    //ManagerFX effectsManager;
    LevelManager levelManager;
    //AIManager AIManager;

    Player player;
    PlayerInput playerInput;

    Camera mainCam;
    //CameraController cameraController;
    PlayerSpawner playerSpawner;

    public event Action LevelRestartEvent;

    public Vector3 currentFramePlayerPos;
    public Bounds currentFramePlayerBounds;
    public Vector3 currentFramePlayerVelocity;
    public Vector3 lastFramePlayerVelocity;

    bool playerDied;

    void Start() {
        DontDestroyOnLoad(this);

        SceneManager.sceneLoaded += SetUpNewLevel;

        //references to player scripts
        GameObject playerObj = FindObjectOfType<Player>().gameObject;
        player = playerObj.GetComponent<Player>();
        playerInput = playerObj.GetComponent<PlayerInput>();

        //cameraController = FindObjectOfType<CameraController>();
        playerSpawner = FindObjectOfType<PlayerSpawner>();

        //hitManager is child 0
        //projectileManager child 1
        //FXmanager child 2
        //levelManager child 3
        //hitManager = transform.GetChild(0).GetComponent<HitManager>();
        //projectileManager = transform.GetChild(1).GetComponent<ProjectileManager>();
        //effectsManager = transform.GetChild(2).GetComponent<ManagerFX>();
        //levelManager = transform.GetChild(3).GetComponent<LevelManager>();
        //AIManager = transform.GetChild(4).GetComponent<AIManager>();
    }

    void Update() {
        MasterUpdate();
    }

    public void RegisterPlayerDeath() {
        playerDied = true;
    }

    public void PlayScreenShake() {
        //effectsManager.PlayScreenShake();
    }

    public void SetDownwardDashColor(float ratio) {
        //effectsManager.SetDownwardDashColor(ratio);
    }

    public void GenerateImpactProjectiles(Vector3 position, float directionX) {
        //projectileManager.GenerateImpactProjectiles(position, directionX);
    }

    public void SortOutHitItems(Collider[] hits, float directionX, int hitID, float playerVelocityX) {
        for (int i = 0; i < hits.Length; i++) {//check all the items registered
            //if (hits[i].CompareTag("Projectile"))//if item has tag "projectile", projectilemanager deals with it
                //projectileManager.ReceiveProjectileHit(hits[i], directionX, hitID, playerVelocityX);
            //else//otherwise, hitmanager takes care of it
                //hitManager.RegisterHit(hits[i]);
        }
    }

    public void UpdatePlayerData(Vector3 playerPos, Bounds playerBounds, Vector3 playerVelocity) {
        currentFramePlayerPos = playerPos;
        currentFramePlayerBounds = playerBounds;
        currentFramePlayerVelocity = playerVelocity;
    }

    public void TriggerNextLevel() {
        //levelManager.GoToNextLevel();
    }

    void SetUpNewLevel(Scene scene, LoadSceneMode mode) {
        //re-reference all the things that were destroyed
        mainCam = Camera.main;
        //cameraController = FindObjectOfType<CameraController>();
        playerSpawner = FindObjectOfType<PlayerSpawner>();

        //distribute data to set up new level
        player.InitializeNewLevel(playerSpawner.transform.position);//set player position and give ref for camera update
        //effectsManager.InitializeNewLevel(mainCam);//this is for screenshakes
        //cameraController.SetCameraPosition(playerSpawner.transform.position);//teleport camera
        //AIManager.GetArrayOfAIAgents();//find all AIs in the scene
    }

    void MasterUpdate() {

        if (!playerDied) {//if player isn't currently dead, update it
            playerInput.PlayerInputUpdate();
            player.UpdatePlayer(ref currentFramePlayerPos, ref currentFramePlayerBounds,
                ref currentFramePlayerVelocity, ref lastFramePlayerVelocity);

            //AI updates here
        } else if(Input.GetButtonDown("Jump")) {//need input manager or something
            LevelRestartEvent?.Invoke();//invoke level restart so all subscribed entities are reset
            //cameraController.SetCameraPosition(playerSpawner.transform.position);
            playerDied = false;
        }

        //cameraController.UpdateCameraPosition(currentFramePlayerPos, lastFramePlayerVelocity);//update camera position
        //AIManager.UpdateAIManager(currentFramePlayerBounds);
        //projectileManager.UpdateActiveProjectiles();
    }
}
