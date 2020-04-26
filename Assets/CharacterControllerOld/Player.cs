using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class Player : MonoBehaviour {

    [Header("Base Move Data")]
    [Tooltip("Time to reach full speed while airborne")]
    public float accelerationTimeAirborne = .15f;
    [Tooltip("Time to reach full speed while grounded")]
    public float accelerationTimeGrounded = .09f;
    [Tooltip("Base move speed")]
    public float moveSpeed = 6f;
    [Tooltip("Curve to filter horizontal inputs")]
    public AnimationCurve moveSpeedInputFilterCurve;
    [Tooltip("Curve to filter vertical inputs")]
    public AnimationCurve verticalInputFilterCurve;
    Vector3 velocity;
    float velocityXSmoothing;
    Vector2 directionalInput;
    float currentMoveSpeed;
    float dirX = 1f;//this is used to prevent sign(0) from messing with negative inputs when idle

    [Header("Jump Data")]
    [Tooltip("Maximum jump height")]
    public float maxJumpHeight = 4f;
    [Tooltip("Minimum jump height")]
    public float minJumpHeight = 1f;
    [Tooltip("Time to reach apex of jump")]
    public float timeToJumpApex = .4f;
    [Tooltip("XY amplitude of wall climbing jump")]
    public Vector2 wallJumpClimb = new Vector2(8f, 15f);
    [Tooltip("XY amplitude of jump off wall")]
    public Vector2 wallJumpOff = new Vector2(9f, 6f);
    [Tooltip("XY amplitude of leaping from wall")]
    public Vector2 wallJumpLeap = new Vector2(18f, 18f);
    [Tooltip("Time during which jumping is still possible after falling")]
    public float jumpingTimeFrameAfterFalling = .08f;
    [Tooltip("XY amplitude of jump while rolling")]
    public Vector2 rollJump = new Vector2(40f, 20f);
    float gravity;
    float maxJumpVelocity;
    float minJumpVelocity;
    bool playerPressedJump;
    float timeSinceLastJumpInput;
    float timeSinceLastFall;
    bool groundedAtStartOfFrame;
    bool performedBufferedJump;

    [Header("Wall Slide Data")]
    [Tooltip("Maximum speed while sliding down wall without input")]
    public float wallSlideSpeedMax = 3f;
    [Tooltip("Maximum speed while sliding down wall and holding stick down")]
    public float wallSlideSpeedMaxWithInput = 5f;
    [Tooltip("Time to reach full speed while sliding down wall holding stick down")]
    public float wallSlideAcceleration = .1f;
    [Tooltip("Time it takes to unstick from a wall")]
    public float wallStickTime = .25f;
    float timeToWallUnstick;
    bool wallSliding;
    int wallDirX;
    float velocityYWallSlideSmoothing;

    [Header("Crouch Data")]
    [Tooltip("Duration of crouch animation")]
    public float crouchVisualsTime = .15f;
    [Tooltip("How much faster is standing up animation from crouching animation")]
    public float standingUpSpeedModifier = 1.2f;
    float jumpingWhileCrouchingHackTime = .08f;
    float timeSpentCrouching;

    [Header("Downward Dash Data")]
    [Tooltip("Y impulse when downward dashing")]
    public float downwardDashVelocityY = -30f;
    [Tooltip("Time it takes for downward dash to enter power mode")]
    public float timeToPowerDownwardDash = .9f;
    [Tooltip("Time it takes for speed to go back to max value after power dash")]
    public float speedRecoveryTimeAfterDownwardDash = 0.7f;
    [Tooltip("Player speed right after landing power dash")]
    public float postDownwardDashMoveSpeed = 2f;
    [Tooltip("Jump height multiplier right after landing power dash")]
    public float postDownwardDashJumpHeightMultiplier = .3f;
    [Tooltip("Curve to control speed recovery")]
    public AnimationCurve speedRecoveryCurve;
    bool isDownwardDashing;
    bool isPowerDownwardDashing;
    float timeSinceDownwardDashing;
    float timeSinceLastDownwardDashHit;
    bool hasRecentlyDownwardDashedHit;
    float currentJumpHeightMultiplier;

    PlayerController controller;

    MasterManager masterManager;

    [Header("Roll")]
    [Tooltip("Max roll speed")]
    public float rollingMaxSpeed = 30f;
    [Tooltip("Roll base duration")]
    public float timeToFullRoll = .4f;
    [Tooltip("Roll duration variation from input")]
    public float rollTimeVariationFromInput = .3f;
    [Tooltip("Roll speed curve")]
    public AnimationCurve rollSpeedCurve;
    [Tooltip("Roll animation curve")]
    public AnimationCurve rollAnimationCurve;
    bool isRolling;
    float timeSinceStartedRolling;
    float rollDirection;
    bool playerPressedRoll;
    float timeSinceLastRollInput;
    float rollBufferTime = .08f;

    [Header("Neutralise Move")]
    [Tooltip("Layers that will be hit")]
    public LayerMask hitMask;
    [Tooltip("Offset from player's origin to box center")]
    public Vector2 hitBoxOffset = new Vector2(1f, .9f);
    [Tooltip("Half extents of the hit box")]
    public Vector3 hitBoxHalfSize = new Vector3(.5f, .9f, 1f);
    [Tooltip("Hitting animation curve")]
    public AnimationCurve hitAnimCurve;
    [Tooltip("Hit duration")]
    public float timeToFullHit = .05f;
    float hitBufferTime = .08f;
    float timeSinceLastHitInput;
    float hitDir;
    float hitAnimTime = .1f;
    float timeSinceLastHitAnim;
    float timeSinceLastHitGameplay;
    int hitID;//used to tell hits apart to prevent unwanted multiple projectile hits
    int previousHitID;


    [Header("Edge Climbing")]
    [Tooltip("Time to climb an edge")]
    public float timeToClimbEdge = .5f;
    [Tooltip("Edge climbing speed curve")]
    public AnimationCurve edgeClimbCurve;
    [Tooltip("Edge climbing max speed")]
    public float edgeClimbSpeedMax = 10f;
    float timeSinceStartedClimbingEdge;
    

    [Header("DEBUG")]
    public bool customFrameRate;
    public int targetFrameRate = 30;
    public bool projectileInvincibility;

    BoxCollider playerCollider;
    Renderer playerRenderer;

    GameObject uglyNose;//temp nose
    Renderer uglyNoseRenderer;//remove ASAP
    GameObject uglyArm;
    Renderer uglyArmRenderer;

    //BoxCollider projectileStopper;//used to more easily intercept high-speed projectiles

    float passingThroughTimeFrame = .05f;
    float timeSinceStartedPassingThrough;
    bool isPassingThrough;
    bool wasPassingThroughLastFrame;
    
    bool isLevelComplete;

    bool receivedDeadlyHit;

#if UNITY_EDITOR
    [HideInInspector]//public so we can set it when we spawn player from editorManager
    public Vector3 lastRegisteredSpawnerPos;
#else
    Vector3 lastRegisteredSpawnerPos;
#endif

    void Start() {
        DontDestroyOnLoad(this);

        controller = GetComponent<PlayerController>();
        gravity = -(2f * maxJumpHeight) / (timeToJumpApex * timeToJumpApex);
        minJumpVelocity = Mathf.Sqrt(2f * Mathf.Abs(gravity * minJumpHeight));
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;


        currentMoveSpeed = moveSpeed;//at start, move speed is max (decreased when landing from dash)
        currentJumpHeightMultiplier = 1f;//same as above but for jump height

        playerCollider = GetComponent<BoxCollider>();
        playerRenderer = GetComponent<Renderer>();

        masterManager = FindObjectOfType<MasterManager>();

        uglyNose = transform.GetChild(0).gameObject;//abomination
        uglyNoseRenderer = uglyNose.GetComponent<Renderer>();
        uglyArm = transform.GetChild(1).gameObject;//abomination
        uglyArmRenderer = uglyArm.GetComponent<Renderer>();


        //this is the projectile stopper
        //projectileStopper = transform.GetChild(2).GetComponent<BoxCollider>();
        //if (projectileStopper.enabled) projectileStopper.enabled = false;//if active, deactivate it

#if UNITY_EDITOR//that's taken care by master manager when scene is loaded
        lastRegisteredSpawnerPos = FindObjectOfType<PlayerSpawner>().transform.position;//initialise spawner pos
#endif 

        masterManager.LevelRestartEvent += RestartLevel;//subscribe restartlevel to event from master manager
    }

    public void InitializeNewLevel(Vector3 spawnerPos) {
        ResetVariables();
        lastRegisteredSpawnerPos = spawnerPos;
        transform.position = lastRegisteredSpawnerPos;
    }

    public void UpdatePlayer(ref Vector3 playerPos, ref Bounds playerBounds, ref Vector3 playerVelocity, ref Vector3 lastFrameVelocity) {
        //inputs are processed right before UpdatePlayer()

#if UNITY_EDITOR
        if (customFrameRate) {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFrameRate;
        } else if(QualitySettings.vSyncCount != 1) QualitySettings.vSyncCount = 1;
#endif

        groundedAtStartOfFrame = controller.collisions.below;//did we end last frame on the ground ?
        
        //this bit is used to fix the bug at high frame rates that caused the player to not be able to pass through thin ground
        //if we started passing through last frame, trigger small time window during which we won't detect vertical collisions
        if (!wasPassingThroughLastFrame && controller.collisions.justStartedPassingThrough) {
            wasPassingThroughLastFrame = true;
            isPassingThrough = true;
            timeSinceStartedPassingThrough = passingThroughTimeFrame;
        } else wasPassingThroughLastFrame = false;
        

        HandleRollBuffer();
        HandleJumpBuffer();
        HandleHitWhileRollingBuffer();
        CalculateVelocity();

        HandleRolling();
        HandleWallSliding();
        HandleStepClimbing();
        HandleCrouching();
        HandleHittingAnim();

        controller.Move(velocity * Time.deltaTime, directionalInput, playerPressedJump, isDownwardDashing, isPassingThrough);

        HandleLevelComplete();
        HandleHitting();
        HandleDownwardDash();//has to come after controller.move otherwise downward dashing bools are reset to false
        HandleTemporaryStatAlteration();//modulate move speed & jump height based on actions

        if (controller.collisions.above || controller.collisions.below) velocity.y = 0;

        //if we fell right this frame, trigger time frame during which jumping is still possible
        if (groundedAtStartOfFrame && !controller.collisions.below && !playerPressedJump) timeSinceLastFall = jumpingTimeFrameAfterFalling;

        playerPressedJump = false;//may need a "reset" function if more variables need to be reset
        playerPressedRoll = false;
        performedBufferedJump = false;

        SortOutCountdowns();

        ApplyPlayerDirectionAnim();//sickening

        //pass in frame parameters to master manager for the rest of the update
        playerPos = transform.position;
        playerBounds = controller.collisions.colliderBounds;
        playerVelocity = velocity;
        lastFrameVelocity = controller.collisions.lastFrameFinalVelocity;

        HandlePlayerDeath();//have we died during the frame ?
    }

    void SortOutCountdowns() {
        if (timeSinceLastJumpInput > 0f) {
            //keeps track of how long it has been since last jump input (clamped)
            timeSinceLastJumpInput -= Time.deltaTime;
            timeSinceLastJumpInput = timeSinceLastJumpInput < 0f ? 0f : timeSinceLastJumpInput;
        }

        if (timeSinceLastRollInput > 0f) {
            //keeps track of how long it has been since last roll input (clamped)
            timeSinceLastRollInput -= Time.deltaTime;
            timeSinceLastRollInput = timeSinceLastRollInput < 0f ? 0f : timeSinceLastRollInput;
        }

        if (timeSinceLastFall > 0f) {
            //how long since last fall
            timeSinceLastFall -= Time.deltaTime;
            timeSinceLastFall = timeSinceLastFall < 0f ? 0f : timeSinceLastFall;
        }

        if (timeSinceLastHitInput > 0f) {
            //how long since last hit while rolling
            timeSinceLastHitInput -= Time.deltaTime;
            timeSinceLastHitInput = timeSinceLastHitInput < 0f ? 0f : timeSinceLastHitInput;
        }

        if (timeSinceLastHitGameplay > 0f) {
            timeSinceLastHitGameplay -= Time.deltaTime;
            timeSinceLastHitGameplay = timeSinceLastHitGameplay < 0f ? 0f : timeSinceLastHitGameplay;
            //if (timeSinceLastHitGameplay == 0f) projectileStopper.enabled = false;//deactivate projectile stopper
        }

        if (timeSinceStartedPassingThrough > 0f) {//keeps track of how long it's been since we passed through a thin ground
            timeSinceStartedPassingThrough -= Time.deltaTime;
            if (timeSinceStartedPassingThrough < 0f) isPassingThrough = false;
        }
    }

    public void ReceiveFatalHit() {
        if (projectileInvincibility) return;
        TriggerPlayerDeath();
    }

    void TriggerPlayerDeath() {
        timeSinceLastHitGameplay = 0f;//this is to stop hitting when we're dead
        //projectileStopper.enabled = false;//deactivate projectile stopper if we're dead
        ToggleRendererAndCollider(false);
        ToggleArmAndNoseRenderer(false);//nose horror
        masterManager.RegisterPlayerDeath();
    }

    void HandlePlayerDeath() {
        if (controller.collisions.isDead) {//if we've hit something lethal, trigger death
            TriggerPlayerDeath();
        }
    }

    void HandleLevelComplete() {
        if (controller.collisions.hasCompletedLevel) masterManager.TriggerNextLevel();
    }

    void HandleDownwardDash() {
        if (controller.collisions.downwardDashHit && isPowerDownwardDashing) {
            masterManager.PlayScreenShake();//plays a screenshake
            masterManager.SetDownwardDashColor(0f);//resets colour change
            hasRecentlyDownwardDashedHit = true;//that'll start the timer which controls the limited speed after landing
            timeSinceLastDownwardDashHit = 0f;

            masterManager.GenerateImpactProjectiles(transform.position, dirX);
        }

        if (isDownwardDashing) {
            if (!isPowerDownwardDashing) {
                timeSinceDownwardDashing += Time.deltaTime;
                timeSinceDownwardDashing = timeSinceDownwardDashing > timeToPowerDownwardDash ? timeToPowerDownwardDash : timeSinceDownwardDashing;
                if (timeSinceDownwardDashing == timeToPowerDownwardDash) isPowerDownwardDashing = true;//we're power dashing
            }
            
            float powerDownwardDashRatio = timeSinceDownwardDashing / timeToPowerDownwardDash;
            masterManager.SetDownwardDashColor(powerDownwardDashRatio);
        }

        if (controller.collisions.downwardDashHit || wallSliding) {
            isDownwardDashing = false;//resets state as we'll resolve the impact early next frame thanks to controller.collisions.downwardDashHit
            timeSinceDownwardDashing = 0f;//resets timer
            isPowerDownwardDashing = false;//we're not power downward dashing anymore

            masterManager.SetDownwardDashColor(0f);//resets colour change
        }
    }

    void HandleTemporaryStatAlteration() {
        if (playerPressedRoll) {//remove stat penalty when rolling after power dash
            hasRecentlyDownwardDashedHit = false;
            currentMoveSpeed = moveSpeed;
            currentJumpHeightMultiplier = 1f;
        }

        if (hasRecentlyDownwardDashedHit) {//should limit jump height as well, this block needs to be taken out of CalculateVelocity()
            timeSinceLastDownwardDashHit += Time.deltaTime;
            float speedRecoveryRatio = Mathf.Clamp01(timeSinceLastDownwardDashHit / speedRecoveryTimeAfterDownwardDash);
            speedRecoveryRatio = speedRecoveryCurve.Evaluate(speedRecoveryRatio);
            currentMoveSpeed = Mathf.Lerp(postDownwardDashMoveSpeed, moveSpeed, speedRecoveryRatio);
            currentJumpHeightMultiplier = speedRecoveryRatio == 1f ? 1f : 0f;//if we're at the end of recovery, can jump again

            if (speedRecoveryRatio == 1f) {
                hasRecentlyDownwardDashedHit = false;
            }
        }
    }

#region inputs
    public void SetDirectionalInput(Vector2 input) {
        directionalInput.x = moveSpeedInputFilterCurve.Evaluate(Mathf.Abs(input.x)) * Mathf.Sign(input.x);
        directionalInput.y = verticalInputFilterCurve.Evaluate(Mathf.Abs(input.y)) * Mathf.Sign(input.y);

        if (directionalInput.x != 0f) dirX = Mathf.Sign(directionalInput.x);//store the sign of last x input that was not 0
    }

    public void OnJumpInputDown() {

        playerPressedJump = true;//this is used for climbing down "through" obstacles
        timeSinceLastJumpInput = jumpingWhileCrouchingHackTime;

        if (wallSliding) {
            timeSinceLastJumpInput = 0f;//if we're jumping, no need to buffer a jump
            if (wallDirX == directionalInput.x) {
                if (controller.collisions.canClimb) {//if we're climbing an edge
                    velocity.x = 0f;//only go vertical
                    velocity.y = wallJumpClimb.y;
                } else {//if we're simply climbing against the wall
                    velocity.x = -wallDirX * wallJumpClimb.x;
                    velocity.y = wallJumpClimb.y;
                }

            } else if (directionalInput.x == 0) {
                velocity.x = -wallDirX * wallJumpOff.x;
                velocity.y = wallJumpOff.y;
            } else {
                velocity.x = -wallDirX * wallJumpLeap.x;
                velocity.y = wallJumpLeap.y;
            }
        }

        if (isRolling) {//if we're rolling
            if (controller.collisions.below || timeSinceLastFall > 0f) {//and there's something below or we just fell
                timeSinceLastJumpInput = 0f;//if we're jumping, no need to buffer a jump
                velocity.x = dirX * rollJump.x;
                velocity.y = rollJump.y;
            }

        } else
        //if grounded and not crouching, or if we just fell, jump
        //don't jump if we've just passed through thin ground
        if (((controller.collisions.below && !controller.collisions.crouching) || timeSinceLastFall > 0f) && !isPassingThrough) {
            timeSinceLastJumpInput = 0f;//if we're jumping, no need to buffer a jump
            velocity.y = maxJumpVelocity * currentJumpHeightMultiplier;
        }

        if (!controller.collisions.below && !wallSliding && directionalInput.y == -1f && !isDownwardDashing) {//downward dash
            velocity.y = downwardDashVelocityY;
            isDownwardDashing = true;
        }


    }

    public void OnJumpInputUp() {
        if(velocity.y > minJumpVelocity) velocity.y = minJumpVelocity * currentJumpHeightMultiplier;
    }

    public void OnRollInput() {
        if (!controller.collisions.below) {//trigger roll buffer only if we're in the air
            timeSinceLastRollInput = rollBufferTime;
        }

        if (controller.collisions.below && !isRolling) {//start roll if we're on ground and not already rolling
            isRolling = true;
            timeSinceStartedRolling = 0f;
            rollDirection = dirX;
            playerPressedRoll = true;
        }
    }

    public void OnHitInput() {
        if (controller.collisions.crouching) return;//disable hitting while crouching

        if(isRolling) {//can't hit while rolling, but hit is buffered
            timeSinceLastHitInput = hitBufferTime;
            return;
        }

        timeSinceLastHitGameplay = timeToFullHit;
        
        //projectileStopper.enabled = true;

        //this serves for the animation
        timeSinceLastHitAnim = hitAnimTime;
        hitDir = dirX;//player orientation when at the time of hit

        //generate a random number different from the last one
        do { hitID = UnityEngine.Random.Range(1, int.MaxValue); }//0 is for projectiles not hit by player
        while (hitID == previousHitID);
        previousHitID = hitID;//set previous random number to new one for next input call

    }
#endregion

#region buffers
    void HandleRollBuffer() {
        if (controller.collisions.below && !isRolling && timeSinceLastRollInput > 0f) {//roll if we're on ground, not already rolling and still in time frame
            isRolling = true;
            timeSinceStartedRolling = 0f;
            rollDirection = dirX;
            playerPressedRoll = true;
            timeSinceLastRollInput = 0f;
        }
    }

    void HandleHitWhileRollingBuffer() {
        if (timeSinceLastHitInput > 0f && !controller.collisions.crouching) {

            timeSinceLastHitInput = 0f;

            timeSinceLastHitGameplay = timeToFullHit;

            //projectileStopper.enabled = true;

            //this serves for the animation
            timeSinceLastHitAnim = hitAnimTime;
            hitDir = dirX;//player orientation when at the time of hit

            //generate a random number different from the last one
            do { hitID = UnityEngine.Random.Range(1, int.MaxValue); }//0 is for projectiles not hit by player
            while (hitID == previousHitID);
            previousHitID = hitID;//set previous random number to new one for next input call
        }
    }

    void HandleJumpBuffer() {
        //this is used to buffer a jump right before exiting crouching
        //ALSO it happens to allow players to buffer a jump just as they're about to land, amazing QoL improvement
        if (timeSinceLastJumpInput > 0f && !playerPressedJump && !isPassingThrough) {
            if (controller.collisions.below && !controller.collisions.crouching) {
                velocity.y = maxJumpVelocity * currentJumpHeightMultiplier;
                timeSinceLastJumpInput = 0f;
                performedBufferedJump = true;
            }
            
            if (wallSliding) {//takes care of walljumping as well
                timeSinceLastJumpInput = 0f;
                performedBufferedJump = true;

                if (wallDirX == directionalInput.x) {
                    if (controller.collisions.canClimb) {
                        velocity.x = -wallDirX * 0f;
                        velocity.y = wallJumpClimb.y;
                    } else {
                        velocity.x = -wallDirX * wallJumpClimb.x;
                        velocity.y = wallJumpClimb.y;
                    }
                } else if (directionalInput.x == 0) {
                    velocity.x = -wallDirX * wallJumpOff.x;
                    velocity.y = wallJumpOff.y;
                } else {
                    velocity.x = -wallDirX * wallJumpLeap.x;
                    velocity.y = wallJumpLeap.y;
                }
            }
            
        }
    }
#endregion

    void HandleRolling() {
        
        if (isRolling && (playerPressedJump || wallSliding)) {//if we were rolling and jumped/started wallsliding
            isRolling = false;//not rolling anymore
            ApplyCrouchAnim(0f);//reset player scale to 1
        }
        

        if (isRolling) {
            velocityXSmoothing = 0f;//necessary to prevent potential drifting after roll

            //1 if no x input while rolling, 1f + rollTimeVariationFromInput if x input opposite roll direction
            //1f - rollTimeVariationFromInput if x input in same direction as roll
            //in a nutshell, roll will last longer if stick is held in the same direction as roll, shorter if stick held opposite
            float rollTimeModifier = Mathf.Lerp(1f + rollTimeVariationFromInput,
                1f - rollTimeVariationFromInput, (directionalInput.x * rollDirection + 1f) * .5f);

            timeSinceStartedRolling += Time.deltaTime * rollTimeModifier;
            float rollRatio = timeSinceStartedRolling / timeToFullRoll;
            rollRatio = rollRatio > 1f ? 1f : rollRatio;//clamp at 1

            velocity.x = Mathf.Max(rollingMaxSpeed * rollSpeedCurve.Evaluate(rollRatio), moveSpeed) * rollDirection;

            if (rollRatio == 1f) {
                isRolling = false;//end of roll

                //to be framerate independent, we need to account for the time lost on clamping rollRatio
                float delta = timeSinceStartedRolling - timeToFullRoll;
                
                float targetVelocityX = Mathf.Abs(directionalInput.x) * dirX * currentMoveSpeed;

                //simply add the amount of movement that would have been performed once the roll is over
                velocity.x += Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing,
                    accelerationTimeGrounded, moveSpeed, delta);
            }

            float animRatio = rollAnimationCurve.Evaluate(rollRatio);
            ApplyCrouchAnim(animRatio);
        }
    }

    void CalculateVelocity() {

        float targetVelocityX = 0f;

        if (!controller.collisions.crouching) {//can't move if we're crouching
            targetVelocityX = Mathf.Abs(directionalInput.x) * dirX * currentMoveSpeed;
        }

        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing,
            controller.collisions.below ? accelerationTimeGrounded : accelerationTimeAirborne);

        velocity.y += gravity * Time.deltaTime;
    }

    void HandleStepClimbing() {
        if (directionalInput.x == wallDirX && directionalInput.x != 0f && controller.collisions.canClimb && 
            /*!controller.collisions.below &&*/ !playerPressedJump) {

            //increase climb speed according to time we've spent climbing the edge
            timeSinceStartedClimbingEdge -= Time.deltaTime;
            float climbRatio = Mathf.Clamp01(timeSinceStartedClimbingEdge / timeToClimbEdge);
            climbRatio = edgeClimbCurve.Evaluate(climbRatio);

            float climbVelocityY = Mathf.Lerp(0f, edgeClimbSpeedMax, climbRatio);

            if (velocity.y < climbVelocityY) velocity.y = climbVelocityY;

        } else {
            timeSinceStartedClimbingEdge = timeToClimbEdge;//reset timer
        }
    }
    
    void HandleWallSliding() {
        wallDirX = controller.collisions.left ? -1 : 1;
        wallSliding = false;
        if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && !playerPressedJump && !performedBufferedJump) {
            wallSliding = true;
            
            if(velocity.y <= -wallSlideSpeedMax) {//slide at wallSlideSpeedMax without input, accelerate to wallSlideSpeedMaxWithInput if pressing down
                float targetVelocityY = Mathf.Lerp(-wallSlideSpeedMax, -wallSlideSpeedMaxWithInput,//lerp if we're holding stick downwards
                    Mathf.Clamp01(-directionalInput.y));

                velocity.y = Mathf.SmoothDamp(velocity.y - gravity * Time.deltaTime, targetVelocityY,
                    ref velocityYWallSlideSmoothing, wallSlideAcceleration);//need to subtract gravity * deltatime which is added in CalculateVelocity()
                //maybe we could simply not add it if we're wallsliding
            }
            

            if (timeToWallUnstick > 0f) {
                velocityXSmoothing = 0f;
                velocity.x = 0f;

                if (directionalInput.x != wallDirX && directionalInput.x != 0f) timeToWallUnstick -= Time.deltaTime;
                else timeToWallUnstick = wallStickTime;

            } else timeToWallUnstick = wallStickTime;
        }
    }

    void HandleCrouching() {
        if (controller.collisions.crouching) {
            timeSpentCrouching += Time.deltaTime;
            timeSpentCrouching = timeSpentCrouching > crouchVisualsTime ? crouchVisualsTime : timeSpentCrouching;
            float crouchRatio = timeSpentCrouching / crouchVisualsTime;
            ApplyCrouchAnim(crouchRatio);
        } else {

            if (timeSpentCrouching > 0f) {
                timeSpentCrouching -= Time.deltaTime * standingUpSpeedModifier;//standing up is faster than crouching when standingUpSpeedModifier > 1
                timeSpentCrouching = timeSpentCrouching < 0f ? 0f : timeSpentCrouching;
                float standingUpRatio = timeSpentCrouching / crouchVisualsTime;
                ApplyCrouchAnim(standingUpRatio);
            }
        }
    }

    void HandleHitting() {
        if(timeSinceLastHitGameplay > 0f) {
            Vector3 boxOrigin = new Vector3(transform.position.x + dirX * hitBoxOffset.x,
            transform.position.y + hitBoxOffset.y, transform.position.z);
            Collider[] items = Physics.OverlapBox(boxOrigin, hitBoxHalfSize, Quaternion.identity, hitMask);//gather all the things hit in front of player
            if (items.Length != 0) masterManager.SortOutHitItems(items, dirX, hitID, velocity.x);//send things to be sorted out
        }
    }

    void HandleHittingAnim() {
        if(timeSinceLastHitAnim > 0f) {
            timeSinceLastHitAnim -= Time.deltaTime;
            float animRatio = 1f - (timeSinceLastHitAnim / hitAnimTime);//full arm extension at 1, no arm extension at 0
            animRatio = animRatio > 1f ? 1f : animRatio;//clamp below 1

            ApplyHitAnim(animRatio);
        }
    }

    void ApplyCrouchAnim(float animRatio) {//this would utlimately be replaced by an actual animation
        float scaleY = Mathf.Lerp(1f, .4f, animRatio);
        transform.localScale = new Vector3(transform.localScale.x, scaleY, transform.localScale.z);
        Physics.SyncTransforms();//synchronise collider change
        controller.CalculateRaySpacing();
    }

    void ApplyPlayerDirectionAnim() {//temp and ugly
        float nosePosX = Mathf.Lerp(-.2f, .2f, (dirX + 1f) * .5f);
        uglyNose.transform.localPosition = new Vector3(nosePosX, 1.35f, 0f);
    }

    void ApplyHitAnim(float animRatio) {
        float armPosX = Mathf.Lerp(0f, .45f * hitDir, hitAnimCurve.Evaluate(animRatio));
        uglyArm.transform.localPosition = new Vector3(armPosX, .8f, 0f);
    }
    
    void ToggleRendererAndCollider(bool toggleState) {
        playerCollider.enabled = toggleState;
        playerRenderer.enabled = toggleState;
    }

    void ToggleArmAndNoseRenderer(bool toggleState) {
        uglyNoseRenderer.enabled = toggleState;
        uglyArmRenderer.enabled = toggleState;
    }

    void RestartLevel() {//should probably be handled with an event

        ToggleRendererAndCollider(true);

        ToggleArmAndNoseRenderer(true);//remove with ugly nose
        transform.position = lastRegisteredSpawnerPos;//respawn at last registered spawner

        ApplyCrouchAnim(0f);//that shouldn't always be necessary, but just in case
        ApplyHitAnim(0f);//same as above
        ResetVariables();
    }

    void ResetVariables() {
        velocity = Vector3.zero;
        velocityXSmoothing = 0f;
        currentMoveSpeed = moveSpeed;
        dirX = 1f;
        playerPressedJump = false;
        timeSinceLastJumpInput = 0f;
        timeToWallUnstick = 0f;
        velocityYWallSlideSmoothing = 0f;
        timeSpentCrouching = 0f;
        isDownwardDashing = false;
        isPowerDownwardDashing = false;
        timeSinceDownwardDashing = 0f;
        timeSinceLastDownwardDashHit = 0f;
        hasRecentlyDownwardDashedHit = false;
        currentJumpHeightMultiplier = 1f;
        isRolling = false;
        timeSinceStartedRolling = 0f;
        playerPressedRoll = false;
        timeSinceLastRollInput = 0f;
        timeSinceLastHitInput = 0f;
        timeSinceLastHitAnim = 0f;
        isPassingThrough = false;
        timeSinceStartedPassingThrough = 0f;
        wasPassingThroughLastFrame = false;
        performedBufferedJump = false;
        timeSinceStartedClimbingEdge = 0f;
        timeSinceLastHitGameplay = 0f;
        hitID = 0;
        previousHitID = 0;
    }


    void OnDrawGizmos() {
        Gizmos.color = Color.black;
        Vector3 boxOrigin = new Vector3(transform.position.x + dirX * hitBoxOffset.x, transform.position.y + hitBoxOffset.y, transform.position.z);
        Vector3 boxHalfExtents = hitBoxHalfSize * 2f;
        Gizmos.DrawWireCube(boxOrigin, boxHalfExtents);
    }
}
