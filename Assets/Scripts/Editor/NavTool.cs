using UnityEngine;
using UnityEditor;

public class NavTool : EditorWindow {

    float maxProbeRadius = 8f;
    float maxVerticalDistance = 6f;
    LayerMask obstacleMask;

    [MenuItem("CustomTools/NavTool")]
    public static void DisplayToolWindow() {
        GetWindow<NavTool>("NavTool");

    }

    void OnGUI() {
        if(GUILayout.Button("Nav Wall Pairing")) {
            NavWallPairing();
        }
    }


    void NavWallPairing() {
        
        NavWall[] navWalls = FindObjectsOfType<NavWall>();

        //for all the navWalls in the scene we check if any of the other navWalls is close enough and properly oriented to be a twin
        for (int i = 0; i < navWalls.Length; i++) {

            for (int j = 0; j < navWalls.Length; j++) {

                if(navWalls[i] == navWalls[j]) continue;

                Bounds bounds = navWalls[i].GetComponent<BoxCollider>().bounds;
                Bounds potentialTwinBounds = navWalls[j].GetComponent<BoxCollider>().bounds;

                Vector3 currentCenterTop = new Vector3(bounds.center.x, bounds.max.y, 0f);
                Vector3 potentialCenterTop = new Vector3(potentialTwinBounds.center.x, potentialTwinBounds.max.y, 0f);

                float distSqr = (currentCenterTop - potentialCenterTop).sqrMagnitude;

                
                
                if(distSqr <= maxProbeRadius*maxProbeRadius) {//this navWall is close enough

                    //but is it too high/low?
                    if(Mathf.Abs((currentCenterTop - potentialCenterTop).y) > maxVerticalDistance) return;
                    
                    float potentialTwinDirectionSign = Mathf.Sign((potentialCenterTop - currentCenterTop).x);

                    
                    bounds.Expand(-.02f);
                    
                    potentialTwinBounds.Expand(-.02f);

                    Vector3 rayOrigin = potentialTwinDirectionSign >= 1f ? new Vector3(bounds.max.x, bounds.max.y, 0f) : new Vector3(bounds.min.x, bounds.max.y, 0f);
                    Vector3 rayDestination = potentialTwinDirectionSign >= 1f ? 
                        new Vector3(potentialTwinBounds.min.x, potentialTwinBounds.max.y, 0f) : new Vector3(potentialTwinBounds.max.x, potentialTwinBounds.max.y, 0f);


                    obstacleMask = LayerMask.GetMask("Obstacle");
                    if(Physics.Linecast(rayOrigin, rayDestination, out RaycastHit hit, obstacleMask)) {
                        
                        if(hit.collider == navWalls[j].GetComponent<BoxCollider>()) {//no obstacle between navWall and candidate, twin found

                            bounds.Expand(.02f);
                            potentialTwinBounds.Expand(.02f);

                            navWalls[i].cornerPosition = potentialTwinDirectionSign >= 1f ? new Vector3(bounds.max.x, bounds.max.y, 0f) : new Vector3(bounds.min.x, bounds.max.y, 0f);
                            navWalls[i].twinCornerPosition = potentialTwinDirectionSign >= 1f ?
                                 new Vector3(potentialTwinBounds.min.x, potentialTwinBounds.max.y, 0f) : new Vector3(potentialTwinBounds.max.x, potentialTwinBounds.max.y, 0f);
                        }
                    }

                }
                
            }
            
        }
    }

}
