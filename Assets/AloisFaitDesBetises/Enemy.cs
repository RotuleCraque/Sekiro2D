using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class Enemy : MonoBehaviour
{

    public Material shinobiMat;
    public float timeToImage = 1f;
    bool doTimer = false;
    float timer;

    public AnimationCurve curve;

    // Start is called before the first frame update
    void Start()
    {
        shinobiMat.SetFloat("_Opacity", 0);
    }

    // Update is called once per frame
    void Update()
    {
        if(doTimer) {
            timer += Time.deltaTime;

            float ratio = Mathf.Min(1f, timer / timeToImage);
            float amount = curve.Evaluate(ratio);
            shinobiMat.SetFloat("_Opacity", amount);

        }
       
    }

    public void UpdateScale(){

        if(Physics.Raycast(transform.position, Vector3.up, out RaycastHit hit, 10f)) {
            //print(hit.distance + "  " + transform.localScale.y);

            //if(hit.distance < 4) {
                transform.localScale = new Vector3(transform.localScale.x, .1f, transform.localScale.z);
                Physics.SyncTransforms();
            //}

            doTimer = true;
        }

    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 1f);
    }
}
