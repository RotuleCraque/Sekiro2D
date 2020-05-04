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
    //

    enum AlertState { Default, Suspicious, Alert };
    [SerializeField] AlertState alertState = AlertState.Default;

    float jumpTimer = 0f;
    float jumpVirtualPressDuration = .5f;
    bool isHoldingJump = false;

    //for debug purposes
    GameObject playerObj;
    float bufferedJumpTimer = 0f;
    float jumpBufferDuration = .1f;
    bool hasBufferedJump = false;

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
            currentDestination = playerObj.transform.position;
            hasReachedDestination = false;
            //if(alertState == AlertState.Default) movementAI.moveSpeed = 6f;
            //else if(alertState == AlertState.Suspicious) movementAI.moveSpeed = 9f;
            //else if(alertState == AlertState.Alert) movementAI.moveSpeed = 13f;
        }

        

        currentDestination = playerObj.transform.position;
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
        float distance01 = Mathf.Clamp01(distanceToDestinationProj / 3f);
        float inputX = Mathf.Lerp(0, 1, distance01) * directionSign;

        Vector2 directionalInput = new Vector2(inputX, 0);
        movementAI.SetDirectionalInput(directionalInput);


        Bounds bounds = boxCollider.bounds;
        bounds.Expand(-.02f);

        Vector3 rayOriginBottom = directionSign == 1 ? new Vector3(bounds.max.x, bounds.min.y, 0f) : new Vector3(bounds.min.x, bounds.min.y, 0f);
        Vector3 rayOriginTop = directionSign == 1 ? new Vector3(bounds.max.x, bounds.max.y, 0f) : new Vector3(bounds.min.x, bounds.max.y, 0f);

        Vector3 maxForwardProbePosition = rayOriginBottom + Vector3.right * inputX * movementAI.moveSpeed * .18f;//the point in front of us where scanning rays stop

        Debug.DrawRay(rayOriginBottom, Vector3.right * movementAI.moveSpeed * inputX * .18f, Color.red);

        //if we're running into a wall
        if(Physics.Raycast(rayOriginBottom, Vector3.right * directionSign, out RaycastHit hit, movementAI.moveSpeed * Mathf.Abs(inputX) * .18f, obstacleMask) && !isHoldingJump) {
            
            if(distanceToDestination - bounds.extents.x > hit.distance) {//if our target destination is further away than the wall in front of us, we jump
                isHoldingJump = true;
                jumpTimer = 0;

                //we need to adjust jumpVirtualPressDuration with regards to how high the obstacle is
                float deltaY = hit.collider.bounds.max.y - transform.position.y;
                float maxJumpHeightToObstacleHeightRatio = Mathf.Clamp01(deltaY * 1f / movementAI.maxJumpHeight);//we multiply by .8f so that the jump feels more tight
                jumpVirtualPressDuration = Mathf.Lerp(0, movementAI.timeToJumpApex, maxJumpHeightToObstacleHeightRatio);

                movementAI.OnJumpInputDown();
            }
        }

        float rayOriginTopToMaxForwardProbeLength = (maxForwardProbePosition - rayOriginTop).magnitude;
        Vector3 rayOriginTopToMaxForwardProbeDir = (maxForwardProbePosition - rayOriginTop) / rayOriginTopToMaxForwardProbeLength;//normalised vector
        Debug.DrawRay(rayOriginTop, rayOriginTopToMaxForwardProbeDir * (rayOriginTopToMaxForwardProbeLength + .2f), Color.green);
        //if we're running into no ground = gap
        if(!Physics.Raycast(rayOriginTop, rayOriginTopToMaxForwardProbeDir, out RaycastHit hit2, rayOriginTopToMaxForwardProbeLength + .02f, obstacleMask) && !hasBufferedJump) {
            if(controllerAI.collisions.below) {

                //if() {//if there's a platform on the other side of the gap, we roll-jump
                    
                    movementAI.OnRollInput();
                //}

                hasBufferedJump = true;
                
            }
            

        }

    }

    void OnDrawGizmosSelected() {

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(currentDestination, .25f);
    }

}
