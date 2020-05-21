using UnityEngine;


public class BehaviourAI : MonoBehaviour {

    //InputAI inputAI;
    MovementAI movementAI;
    ControllerAI controllerAI;

    BoxCollider boxCollider;

    //MOVE SECTION
    [SerializeField] float destinationPrecision = .25f;
    Vector3 currentDestination;

    bool hasReachedDestination = false;
    [SerializeField] AnimationCurve decelerationCurve;
    [SerializeField] LayerMask obstacleMask;

    float jumpTimer = 0f;
    float jumpVirtualPressDuration = .5f;
    bool isHoldingJump = false;

    //for debug purposes
    GameObject playerObj;
    float bufferedJumpTimer = 0f;
    float jumpBufferDuration = .2f;
    bool hasBufferedJump = false;

    GameObject lastObjectBeforeGap;


    float noInputAllowedTimer;
    float noInputAllowedDuration = .1f;
    bool noInputsAllowed = false;

    //[SerializeField] GameObject[] patrolWaypoints;

    void Start() {
        movementAI = GetComponent<MovementAI>();
        controllerAI = GetComponent<ControllerAI>();

        playerObj = FindObjectOfType<Player>().gameObject;
        boxCollider = GetComponent<BoxCollider>();
    }

    void Update() {

        if(isHoldingJump && (controllerAI.collisions.below || controllerAI.collisions.left || controllerAI.collisions.right)) {//if we've hit a surface we interrupt the current jump
            isHoldingJump = false;
            jumpTimer = 0;
        }

        if(Input.GetButtonDown("SideDash")) {
            //currentDestination = playerObj.transform.position;
            //hasReachedDestination = false;
            //if(alertState == AlertState.Default) movementAI.moveSpeed = 6f;
            //else if(alertState == AlertState.Suspicious) movementAI.moveSpeed = 9f;
            //else if(alertState == AlertState.Alert) movementAI.moveSpeed = 13f;
        }

        
        if(Time.frameCount%90 == 0) {
            currentDestination = playerObj.transform.position;
        }
        MoveTo(currentDestination);


        if(isHoldingJump) {
            jumpTimer += Time.deltaTime;
            if(jumpTimer >= jumpVirtualPressDuration) {
                jumpTimer = 0;
                isHoldingJump = false;
                movementAI.OnJumpInputUp();
            }
        }

        if(hasBufferedJump) {
            bufferedJumpTimer += Time.deltaTime;
            
            if(bufferedJumpTimer >= jumpBufferDuration) {
                movementAI.OnJumpInputDown();
                hasBufferedJump = false;
                bufferedJumpTimer = 0f;
                jumpVirtualPressDuration = 0.05f;
                isHoldingJump = true;
            }

        }

        if(noInputsAllowed) {
            noInputAllowedTimer += Time.deltaTime;

            if(noInputAllowedTimer >= noInputAllowedDuration) {
                noInputAllowedTimer = 0f;
                noInputsAllowed = false;
            }

        }


        movementAI.UpdateMovementAI();
    }

    void MoveTo(Vector3 destination) {
        Vector3 currentPosToDestination = destination - transform.position;
        

        //let's try having the target position be the point on the ground below the given position
        if(Physics.Raycast(destination, Vector3.down, out RaycastHit hit0, 1000f, obstacleMask)) {//maybe the layer mask is incorrect and we only want to detect environmental obstacles, not players
            //we've filled in hit0
        }

        Vector3 currentPosToDestinationProj = hit0.point - transform.position;

        float distanceToDestination = currentPosToDestination.magnitude;
        float distanceToDestinationProj = currentPosToDestinationProj.magnitude;
        if(distanceToDestinationProj < destinationPrecision) {// if we're close enough from target, consider we've reached it
            
            movementAI.SetDirectionalInput(Vector2.zero);//set input to 0 so AI doesn't drift from destination
            hasReachedDestination = true;
            return;
        }

        float directionSign = Mathf.Sign(currentPosToDestination.x);
        float distance01 = Mathf.Clamp01(distanceToDestinationProj / 3f);//we're lerping the input over the arbitrary value of 3 meters
        float inputX = Mathf.Lerp(0, 1, distance01) * directionSign;

        Vector2 directionalInput = new Vector2(inputX, 0);
        movementAI.SetDirectionalInput(directionalInput);


        Bounds bounds = boxCollider.bounds;
        bounds.Expand(-.02f);

        Vector3 rayOriginBottom = directionSign == 1 ? new Vector3(bounds.max.x, bounds.min.y, 0f) : new Vector3(bounds.min.x, bounds.min.y, 0f);
        Vector3 rayOriginTop = directionSign == 1 ? new Vector3(bounds.max.x, bounds.max.y, 0f) : new Vector3(bounds.min.x, bounds.max.y, 0f);

        //Vector3 maxForwardProbePositionWall = rayOriginBottom + Vector3.right * inputX * movementAI.moveSpeed * .18f;//the point in front of us where wall scanning ray stops
        Vector3 maxForwardProbePositionGap = rayOriginBottom + Vector3.right * inputX * movementAI.moveSpeed * .08f;//the point in front of us where gap scanning ray stops

        Debug.DrawRay(rayOriginBottom, Vector3.right * movementAI.moveSpeed * inputX * .18f, Color.red);

        //if we're running into a wall
        if(Physics.Raycast(rayOriginBottom, Vector3.right * directionSign, out RaycastHit hit, movementAI.moveSpeed * Mathf.Abs(inputX) * .18f, obstacleMask) && !isHoldingJump) {
            
            //we've hit a wall, now we should check:
            //is our target right in front of the wall? if so we've probably reached destination and don't need to do anything
            //is our target below? is there a line of sight? if that's the case we'll probably want to slide down or jump
            //if no line of sight check navthread to see if we're on the right path
            //if on the right path, slide down

            //if our target is above, check if line of sight
            //if yes, check if walljump on same wall brings us to the top, if so do it
            //else check if other wall opposite ours and if close enough
            //if so climb doing zigzag walljumps

            if(distanceToDestination - bounds.extents.x > hit.distance) {//if our target destination is further away than the wall in front of us, we jump
                isHoldingJump = true;
                jumpTimer = 0;

                //we need to adjust jumpVirtualPressDuration with regards to how high the obstacle is
                float deltaY = hit.collider.bounds.max.y - transform.position.y;
                float maxJumpHeightToObstacleHeightRatio = Mathf.Clamp01(deltaY * 1f / movementAI.maxJumpHeight);//we can multiply here if needed to make jump tighter
                jumpVirtualPressDuration = Mathf.Lerp(0, movementAI.timeToJumpApex, maxJumpHeightToObstacleHeightRatio);

                if(!noInputsAllowed) {
                    movementAI.OnJumpInputDown();
                    noInputsAllowed = true;
                }
            }
        }

        float rayOriginTopToMaxForwardProbeLength = (maxForwardProbePositionGap - rayOriginTop).magnitude;
        Vector3 rayOriginTopToMaxForwardProbeDir = (maxForwardProbePositionGap - rayOriginTop) / rayOriginTopToMaxForwardProbeLength;//normalised vector
        Debug.DrawRay(rayOriginTop, rayOriginTopToMaxForwardProbeDir * (rayOriginTopToMaxForwardProbeLength + .2f), Color.green);
        //if there's something at our feet, we store it
        if(Physics.Raycast(rayOriginTop, rayOriginTopToMaxForwardProbeDir, out RaycastHit hit2, rayOriginTopToMaxForwardProbeLength + .02f, obstacleMask) && !hasBufferedJump) {
            
            lastObjectBeforeGap = hit2.collider.gameObject;

        } else {//if we're running into no ground = gap
            if(controllerAI.collisions.below) {
                if(lastObjectBeforeGap != null) {//preventing errors on start
                    if(lastObjectBeforeGap.TryGetComponent<NavWall>(out NavWall currentWall)) {
                        //if the twin wall is higher -> long press jump
                        //if it's roughly the same height or lower -> roll jump
                        //if there's none, short press jump

                        float currentToTwinDistanceY = currentWall.twinCornerPosition.y - currentWall.cornerPosition.y;
                        if(currentToTwinDistanceY > 1f) {//twin is too high
                            float maxJumpHeightToObstacleHeightRatio = Mathf.Clamp01(Mathf.Abs(currentToTwinDistanceY) * 1f / movementAI.maxJumpHeight);//we can multiply here if needed to make jump tighter
                            jumpVirtualPressDuration = Mathf.Lerp(0, movementAI.timeToJumpApex, maxJumpHeightToObstacleHeightRatio);

                            if(!noInputsAllowed) {
                                movementAI.OnJumpInputDown();
                                noInputsAllowed = true;
                            }
                        } else {
                            if(!noInputsAllowed) {
                                movementAI.OnRollInput();
                                hasBufferedJump = true;
                                noInputsAllowed = true;
                            }
                        }
                    } else {
                        if(!noInputsAllowed) {
                            hasBufferedJump = true;
                            noInputsAllowed = true;
                        }
                    }
                }

                
                
            }
        }



    }

    bool IsPlayerInLineOfSight(GameObject player) {


        return true;
    }




    void OnDrawGizmosSelected() {

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(currentDestination, .25f);
    }

}
