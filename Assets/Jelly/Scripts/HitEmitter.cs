using UnityEngine;

public class HitEmitter : MonoBehaviour {

    Camera cam;
    public LayerMask hitMask;
    RaycastHit hit1;

    HitInfo _hitInfo1;
    HitInfo _hitInfo2;
    HitInfo _hitInfo3;

    void Start() {
        cam = this.GetComponent<Camera>();
    }

    void Update() {

        if (Input.GetMouseButtonDown(0)) {

            Ray _ray = cam.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(_ray, out hit1, float.MaxValue, hitMask)) {
                _hitInfo1.isHit1 = true;
                _hitInfo1.impactPosition = hit1.point;
                _hitInfo1.impactNormal = hit1.normal;

                _hitInfo1.initialPrimaryRange = 1.11f;
                //_hitInfo1.initialPrimaryRange = 0f;
                _hitInfo1.initialPrimaryAmplitude = .09f;
               // _hitInfo1.initialPrimaryAmplitude = .0f;
                _hitInfo1.initialPrimaryWavelength = .65f;
                _hitInfo1.initialPrimarySpeed = 2.35f;

                _hitInfo1.initialSecondaryRange = 6f;
                _hitInfo1.initialSecondaryAmplitude = .12f;
                _hitInfo1.initialSecondaryWavelength = 2.1f;
                _hitInfo1.initialSecondarySpeed = 5.25f;



                _hitInfo1.hitDirProjectedOnPlane = hit1.point - cam.transform.position;

                hit1.collider.GetComponent<HitReceiver>().HitReceived(_hitInfo1);
            }
        }

        if (Input.GetMouseButtonDown(1)) {

            Ray _ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(_ray, out hit1, float.MaxValue, hitMask)) {
                _hitInfo1.isHit1 = false;
                _hitInfo1.impactPosition = hit1.point;
                _hitInfo1.impactNormal = hit1.normal;

                _hitInfo1.initialPrimaryRange = 1.11f;
                //_hitInfo1.initialPrimaryRange = 0f;
                _hitInfo1.initialPrimaryAmplitude = .09f;
                // _hitInfo1.initialPrimaryAmplitude = .0f;
                _hitInfo1.initialPrimaryWavelength = .65f;
                _hitInfo1.initialPrimarySpeed = 2.35f;

                _hitInfo1.initialSecondaryRange = 6f;
                _hitInfo1.initialSecondaryAmplitude = .12f;
                _hitInfo1.initialSecondaryWavelength = 2.1f;
                _hitInfo1.initialSecondarySpeed = 5.25f;



                _hitInfo1.hitDirProjectedOnPlane = hit1.point - cam.transform.position;

                hit1.collider.GetComponent<HitReceiver>().HitReceived(_hitInfo1);
            }
        }

    }

    
}

[System.Serializable]
public struct HitInfo {
    public Vector3 impactPosition;
    public Vector3 impactNormal;
    public float initialPrimaryRange;
    public float initialPrimaryWavelength;
    public float initialPrimarySpeed;
    public float initialPrimaryAmplitude;

    public float initialSecondaryRange;
    public float initialSecondaryWavelength;
    public float initialSecondarySpeed;
    public float initialSecondaryAmplitude;

    public bool isHit1;

    public Vector3 hitDirProjectedOnPlane;
};
