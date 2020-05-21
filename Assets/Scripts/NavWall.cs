using UnityEngine;

public class NavWall : MonoBehaviour {

    public Vector3 cornerPosition;

    public Vector3 twinCornerPosition;
    

    void OnDrawGizmos() {
        Gizmos.color = Color.white;
        Gizmos.DrawLine(cornerPosition, twinCornerPosition);

        Gizmos.color = Color.black;

        Gizmos.DrawSphere(cornerPosition, .25f);
        Gizmos.DrawSphere(twinCornerPosition, .25f);

        
    }

}
