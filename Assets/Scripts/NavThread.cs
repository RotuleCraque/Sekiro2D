using UnityEngine;

public class NavThread : MonoBehaviour {


    public Vector3[] navPoints = new Vector3[2];

    [HideInInspector] public Vector2[] allNavPoints;


    void OnValidate() {

        int numPoints = 0;

        for (int i = 0; i < navPoints.Length; i++) {

            navPoints[i].z = Mathf.Max(0, Mathf.RoundToInt(navPoints[i].z));

            numPoints++;

            if(i < navPoints.Length - 1) {//for all the points except the last one
                numPoints += (int)navPoints[i].z - 1;
            }
            
        }

        allNavPoints = new Vector2[numPoints];
        int currentIndex = 0;
        for (int i = 0; i < navPoints.Length; i++) {
            allNavPoints[currentIndex] = new Vector2(navPoints[i].x, navPoints[i].y);
            currentIndex++;

            if(i < navPoints.Length - 1) {
                Vector2 currentNavPointToNextNavPoint = (new Vector2(navPoints[i+1].x, navPoints[i+1].y)) - (new Vector2(navPoints[i].x, navPoints[i].y));
                for (int j = 1; j < navPoints[i].z; j++) {

                    

                    allNavPoints[currentIndex] = new Vector2(navPoints[i].x, navPoints[i].y) + Vector2.one * j * currentNavPointToNextNavPoint / navPoints[i].z;

                    currentIndex++;
                }
            }
        }
    }


    void OnDrawGizmos() {
        

        Gizmos.color = Color.white;
        for (int i = 0; i < allNavPoints.Length; i++) {
            Gizmos.DrawWireSphere(allNavPoints[i], .15f);
            if(i < allNavPoints.Length -1) {
                Gizmos.DrawLine(allNavPoints[i], allNavPoints[i + 1]);
            }
        }


        Gizmos.color = Color.black;

        for (int i = 0; i < navPoints.Length; i++) {
            Gizmos.DrawSphere(new Vector3(navPoints[i].x, navPoints[i].y, 0), .35f);

            
        }

    }


}
