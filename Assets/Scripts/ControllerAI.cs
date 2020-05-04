using UnityEngine;


public class ControllerAI : MonoBehaviour {

    BoxCollider boxCollider;
    RaycastOrigins raycastOrigins;

    public LayerMask collisionMask;

    const float skinWidth = .015f;

    int horizontalRayCount;
    int verticalRayCount;

    public float distanceBetweenRays = .25f;

    float horizontalRaySpacing;
    float verticalRaySpacing;

    public CollisionInfo collisions;

    Vector2 playerInput;
    bool playerIsHittingJump;
    bool playerDownwardDashing;

    void Start() {
        boxCollider = GetComponent<BoxCollider>();
        collisions.faceDir = 1;
        CalculateRaySpacing();
    }

    public void Move(Vector3 moveAmount, Vector2 input, bool hitJump, bool isDownwardDashing, bool isPassThrough) {
        UpdateRaycastOrigins();
        collisions.Reset();
        playerInput = input;
        playerIsHittingJump = hitJump;
        playerDownwardDashing = isDownwardDashing;

        if (moveAmount.x != 0f) collisions.faceDir = (int)Mathf.Sign(moveAmount.x);

        HorizontalCollisions(ref moveAmount);

        if (moveAmount.y != 0 && !isPassThrough) VerticalCollisions(ref moveAmount);

        collisions.lastFrameFinalVelocity = moveAmount / Time.deltaTime;
        transform.Translate(moveAmount);
        Physics.SyncTransforms();

        //this part needs to come after the physics sync so bounds is properly updated with frame values
        collisions.colliderBounds = boxCollider.bounds;//we're getting the value here so we can broadcast it to managers
        collisions.colliderBounds.Expand(-2f * skinWidth);
    }

    void VerticalCollisions(ref Vector3 moveAmount) {

        float directionY = Mathf.Sign(moveAmount.y);
        float rayLength = Mathf.Abs(moveAmount.y) + skinWidth;

        RaycastHit hit;

        for (int i = 0; i < verticalRayCount; i++) {

            Vector3 rayOrigin = directionY == -1 ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector3.right * (verticalRaySpacing * i + moveAmount.x);

            Debug.DrawRay(rayOrigin, Vector3.up * directionY * rayLength, Color.cyan);

            if (Physics.Raycast(rayOrigin, Vector3.up * directionY, out hit, rayLength, collisionMask)) {
                
                if (hit.collider.CompareTag("Through")) {
                    if (directionY == 1 || hit.distance == 0f) continue;//go through if coming from below
                    if (playerInput.y == -1 && playerIsHittingJump) {
                        collisions.justStartedPassingThrough = true;
                        continue;//go through if pressing "jump" while holding "down"

                    }
                }
                
                moveAmount.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;
                
                collisions.below = directionY == -1;
                collisions.above = directionY == 1;
                //print(collisions.below + "  " + Time.frameCount);

                if (collisions.below && playerInput.y == -1) collisions.crouching = true;//is the player crouching 

                //if (hit.collider.CompareTag("Deadly")) collisions.isDead = true;//if we hit something deadly, we're dead

                if (collisions.below && playerDownwardDashing) collisions.downwardDashHit = true;//if hitting the ground while using downward dash
            }
        }

    }

    void HorizontalCollisions(ref Vector3 moveAmount) {
        float initialRayLength = Mathf.Abs(moveAmount.x) + skinWidth;

        float directionX = collisions.faceDir;
        float rayLength = Mathf.Abs(moveAmount.x) + skinWidth;

        if (Mathf.Abs(moveAmount.x) < skinWidth) rayLength = 2f * skinWidth;//wall jump, need to probe outside a litte bit to find the wall
        

        RaycastHit hit;
        
        for (int i = 0; i < horizontalRayCount; i++) {

            Vector3 rayOrigin = directionX == -1 ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector3.up * (horizontalRaySpacing * i);

            Debug.DrawRay(rayOrigin, Vector3.right * directionX * rayLength, Color.cyan);

            if (Physics.Raycast(rayOrigin, Vector3.right * directionX, out hit, rayLength, collisionMask)) {
                moveAmount.x = (hit.distance - skinWidth) * directionX;
                rayLength = hit.distance;

                collisions.left = directionX == -1;
                collisions.right = directionX == 1;

                /*
                if (hit.collider.CompareTag("LevelEnd")) {
                    collisions.hasCompletedLevel = true;
                    return;
                }

                if (hit.collider.CompareTag("Deadly")) collisions.isDead = true;//if we hit something deadly, we're dead
                */
            }
        }
        
        
        if (Mathf.Abs(moveAmount.x) < skinWidth) initialRayLength = 2f * skinWidth;
        //this part is used to check if we can climb step
        if (collisions.left || collisions.right) {//if previous raycast loop found us against something

            Vector3 rayOrigin = directionX == -1 ? raycastOrigins.topLeft : raycastOrigins.topRight;//we want to test the top ray
            if (!Physics.Raycast(rayOrigin, Vector3.right * directionX, initialRayLength, collisionMask)) {
                collisions.canClimb = true;//if we're against a wall a top ray doesn't hit, we can climb
            }
        }
    }

    void UpdateRaycastOrigins() {
        Bounds bounds = boxCollider.bounds;
        bounds.Expand(skinWidth * -2f);

        raycastOrigins.bottomLeft = new Vector3(bounds.min.x, bounds.min.y, 0f);
        raycastOrigins.bottomRight = new Vector3(bounds.max.x, bounds.min.y, 0f);
        raycastOrigins.topLeft = new Vector3(bounds.min.x, bounds.max.y, 0f);
        raycastOrigins.topRight = new Vector3(bounds.max.x, bounds.max.y, 0f);
    }

    public void CalculateRaySpacing() {
        Bounds bounds = boxCollider.bounds;
        bounds.Expand(skinWidth * -2f);

        float boundsWidth = bounds.size.x;
        float boundsHeight = bounds.size.y;

        horizontalRayCount = Mathf.RoundToInt(boundsHeight / distanceBetweenRays);
        verticalRayCount = Mathf.RoundToInt(boundsWidth / distanceBetweenRays);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    struct RaycastOrigins {
        public Vector3 topLeft, topRight;
        public Vector3 bottomLeft, bottomRight;

    };

    public struct CollisionInfo {
        public bool above, below;
        public bool left, right;
        public bool crouching;
        public bool downwardDashHit;
        public bool isDead;

        public int faceDir;

        public Vector3 lastFrameFinalVelocity;//we use that to update camera position
        public bool justStartedPassingThrough;

        public bool hasCompletedLevel;
        public Bounds colliderBounds;

        public bool canClimb;

        public void Reset() {
            above = below = false;
            left = right = false;
            crouching = false;
            downwardDashHit = false;
            isDead = false;
            lastFrameFinalVelocity = Vector3.zero;
            justStartedPassingThrough = false;
            hasCompletedLevel = false;
            canClimb = false;
        }
    };
}
