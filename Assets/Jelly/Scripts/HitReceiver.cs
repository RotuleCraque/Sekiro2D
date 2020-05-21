using UnityEngine;

public class HitReceiver : MonoBehaviour {

    [SerializeField] float primaryImpactDuration = .86f;
    [SerializeField] AnimationCurve primaryImpactAmplitudeFadeCurve;
    [SerializeField] AnimationCurve primaryImpactRangeFadeCurve;
    [SerializeField] AnimationCurve primaryImpactSpeedFadeCurve;
    [SerializeField] AnimationCurve primaryImpactWavelengthFadeCurve;

    [SerializeField] AnimationCurve primaryImpactSubsurfaceRangeFadeCurve;
    [SerializeField] float primaryImpactSubsurfaceDuration = 1f;

    [SerializeField] float secondaryImpactDuration = 1.21f;
    [SerializeField] AnimationCurve secondaryImpactAmplitudeFadeCurve;
    [SerializeField] AnimationCurve secondaryImpactRangeFadeCurve;
    [SerializeField] AnimationCurve secondaryImpactSpeedFadeCurve;
    [SerializeField] AnimationCurve secondaryImpactWavelengthFadeCurve;

    float impactOneTimer;
    bool isOneImpacted;

    float impactTwoTimer;
    bool isTwoImpacted;

    Material mat;
    HitInfo primaryHitInfo1;
    HitInfo primaryHitInfo2;

    bool canGoAllFoggyAndStuff;
    float foggyTimer;
    [SerializeField] float transitionToFogDuration = 3.3f;
    [SerializeField] AnimationCurve fogCurve;

    bool canRotate = true;
    [SerializeField] float rotationSpeed;

    void Start() {
        mat = GetComponent<Renderer>().material;
        mat.SetVector("_ImpactPoint1", Vector4.zero);
        mat.SetFloat("_Amplitude1", 0f);
        mat.SetFloat("_Wavelength1", 1f);
        mat.SetFloat("_Range1", 0f);
        mat.SetFloat("_Speed1", 0f);
        mat.SetFloat("_SecondaryAmplitude1", 0f);
        mat.SetFloat("_SecondaryWavelength1", 1f);
        mat.SetFloat("_SecondaryRange1", 0f);
        mat.SetFloat("_SecondarySpeed1", 0f);


        mat.SetVector("_ImpactPoint2", Vector4.zero);
        mat.SetFloat("_Amplitude2", 0f);
        mat.SetFloat("_Wavelength2", 1f);
        mat.SetFloat("_Range2", 0f);
        mat.SetFloat("_Speed2", 0f);
        mat.SetFloat("_SecondaryAmplitude2", 0f);
        mat.SetFloat("_SecondaryWavelength2", 1f);
        mat.SetFloat("_SecondaryRange2", 0f);
        mat.SetFloat("_SecondarySpeed2", 0f);
    }


    public void HitReceived(HitInfo _hit) {

        if (_hit.isHit1) {

            isOneImpacted = true;
            impactOneTimer = 0f;

            primaryHitInfo1 = _hit;


            _hit.impactPosition = transform.InverseTransformPoint(_hit.impactPosition);
            Vector4 _impactPosition = new Vector4(_hit.impactPosition.x, _hit.impactPosition.y, _hit.impactPosition.z, 0f);

            mat.SetVector("_ImpactPoint1", _impactPosition);
            mat.SetFloat("_Amplitude1", _hit.initialPrimaryAmplitude);
            mat.SetFloat("_Wavelength1", _hit.initialPrimaryWavelength);
            mat.SetFloat("_Range1", _hit.initialPrimaryRange);
            mat.SetFloat("_Speed1", _hit.initialPrimarySpeed);

            mat.SetFloat("_SecondaryAmplitude1", _hit.initialSecondaryAmplitude);
            mat.SetFloat("_SecondaryWavelength1", _hit.initialSecondaryWavelength);
            mat.SetFloat("_SecondaryRange1", _hit.initialSecondaryRange);
            mat.SetFloat("_SecondarySpeed1", _hit.initialSecondarySpeed);



            /*
            Vector3 _hitDirOnPlane = Vector3.ProjectOnPlane(_hit.hitDirProjectedOnPlane, _hit.impactNormal).normalized;
            _hitDirOnPlane = transform.InverseTransformDirection(_hitDirOnPlane);
            Vector4 _hitDirOS = new Vector4(_hitDirOnPlane.x, _hitDirOnPlane.y, _hitDirOnPlane.z, 0f);
            mat.SetVector("_HitDirProj1", _hitDirOS);
            */


            /*
            else {
                _hit.impactPosition = transform.InverseTransformPoint(_hit.impactPosition);
                Vector4 _impactPosition = new Vector4(_hit.impactPosition.x, _hit.impactPosition.y, _hit.impactPosition.z, 0f);
                mat.SetVector("_ImpactPoint2", _impactPosition);
                mat.SetFloat("_Amplitude2", _hit.initialAmplitude);
                mat.SetFloat("_Wavelength2", _hit.initialWavelength);
                mat.SetFloat("_Range2", _hit.initialRange);
                mat.SetFloat("_Speed2", _hit.initialSpeed);


                //Vector3 _hitDirOnPlane = Vector3.ProjectOnPlane(_hit.hitDirProjectedOnPlane, _hit.impactNormal);
                //_hitDirOnPlane = transform.InverseTransformPoint(_hitDirOnPlane).normalized;
                //Vector4 _hitDirOS = new Vector4(_hitDirOnPlane.x, _hitDirOnPlane.y, _hitDirOnPlane.z, 0f);
                //mat.SetVector("_HitDirProj2", _hitDirOS);

            }
            */
        } else {
            
            isTwoImpacted = true;
            impactTwoTimer = 0f;

            primaryHitInfo2 = _hit;


            _hit.impactPosition = transform.InverseTransformPoint(_hit.impactPosition);
            Vector4 _impactPosition = new Vector4(_hit.impactPosition.x, _hit.impactPosition.y, _hit.impactPosition.z, 0f);

            mat.SetVector("_ImpactPoint2", _impactPosition);
            mat.SetFloat("_Amplitude2", _hit.initialPrimaryAmplitude);
            mat.SetFloat("_Wavelength2", _hit.initialPrimaryWavelength);
            mat.SetFloat("_Range2", _hit.initialPrimaryRange);
            mat.SetFloat("_Speed2", _hit.initialPrimarySpeed);

            mat.SetFloat("_SecondaryAmplitude2", _hit.initialSecondaryAmplitude);
            mat.SetFloat("_SecondaryWavelength2", _hit.initialSecondaryWavelength);
            mat.SetFloat("_SecondaryRange2", _hit.initialSecondaryRange);
            mat.SetFloat("_SecondarySpeed2", _hit.initialSecondarySpeed);
            
        }
    }

    void Update() {
        if (isOneImpacted) {

            impactOneTimer += Time.deltaTime;
            mat.SetFloat("_TimerImpact1", impactOneTimer);

            if (impactOneTimer <= primaryImpactDuration) {
                float _percent = impactOneTimer / primaryImpactDuration;
                _percent = Mathf.Min(1f, _percent);

                float _primaryAmplitudeAmount = primaryImpactAmplitudeFadeCurve.Evaluate(_percent);
                _primaryAmplitudeAmount = _primaryAmplitudeAmount < .01f ? 0f : _primaryAmplitudeAmount;

                float _primaryRangeAmount = primaryImpactRangeFadeCurve.Evaluate(_percent);
                _primaryRangeAmount = _primaryRangeAmount < .01f ? 0f : _primaryRangeAmount;

                float _primarySpeedAmount = primaryImpactSpeedFadeCurve.Evaluate(_percent);
                _primarySpeedAmount = _primarySpeedAmount < .01f ? 0f : _primarySpeedAmount;

                float _primaryWavelengthAmount = primaryImpactWavelengthFadeCurve.Evaluate(_percent);
                _primaryWavelengthAmount = _primaryWavelengthAmount < .01f ? 0f : _primaryWavelengthAmount;

                mat.SetFloat("_Amplitude1", primaryHitInfo1.initialPrimaryAmplitude * _primaryAmplitudeAmount);
                mat.SetFloat("_Wavelength1", primaryHitInfo1.initialPrimaryWavelength * _primaryWavelengthAmount);
                mat.SetFloat("_Range1", primaryHitInfo1.initialPrimaryRange * _primaryRangeAmount);
                mat.SetFloat("_Speed1", primaryHitInfo1.initialPrimarySpeed * _primarySpeedAmount);



            }

            if (impactOneTimer <= primaryImpactSubsurfaceDuration) {
                float _percent = impactOneTimer / primaryImpactSubsurfaceDuration;
                _percent = Mathf.Min(1f, _percent);

                float _primarySubsurfaceRangeAmount = primaryImpactSubsurfaceRangeFadeCurve.Evaluate(_percent);
                _primarySubsurfaceRangeAmount = _primarySubsurfaceRangeAmount < .01f ? 0f : _primarySubsurfaceRangeAmount;

                mat.SetFloat("_SubsurfaceIntensity1", _primarySubsurfaceRangeAmount);

            }

            if (impactOneTimer <= secondaryImpactDuration) {
                float _percent = impactOneTimer / secondaryImpactDuration;
                _percent = Mathf.Min(1f, _percent);

                float _secondaryAmplitudeAmount = secondaryImpactAmplitudeFadeCurve.Evaluate(_percent);
                _secondaryAmplitudeAmount = _secondaryAmplitudeAmount < .01f ? 0f : _secondaryAmplitudeAmount;

                float _secondaryRangeAmount = secondaryImpactRangeFadeCurve.Evaluate(_percent);
                _secondaryRangeAmount = _secondaryRangeAmount < .01f ? 0f : _secondaryRangeAmount;

                float _secondarySpeedAmount = secondaryImpactSpeedFadeCurve.Evaluate(_percent);
                _secondarySpeedAmount = _secondarySpeedAmount < .01f ? 0f : _secondarySpeedAmount;

                float _secondaryWavelengthAmount = secondaryImpactWavelengthFadeCurve.Evaluate(_percent);

                mat.SetFloat("_SecondaryAmplitude1", primaryHitInfo1.initialSecondaryAmplitude * _secondaryAmplitudeAmount);
                mat.SetFloat("_SecondaryWavelength1", primaryHitInfo1.initialSecondaryWavelength * _secondaryWavelengthAmount);
                mat.SetFloat("_SecondaryRange1", primaryHitInfo1.initialSecondaryRange * _secondaryRangeAmount);
                mat.SetFloat("_SecondarySpeed1", primaryHitInfo1.initialSecondarySpeed * _secondarySpeedAmount);


            }

            if (impactOneTimer >= secondaryImpactDuration) {
                isOneImpacted = false;
                impactOneTimer = 0f;
            }



        }

        if (isTwoImpacted) {
            impactTwoTimer += Time.deltaTime;
            mat.SetFloat("_TimerImpact2", impactTwoTimer);

            if (impactTwoTimer <= primaryImpactDuration) {
                float _percent = impactTwoTimer / primaryImpactDuration;
                _percent = Mathf.Min(1f, _percent);

                float _primaryAmplitudeAmount = primaryImpactAmplitudeFadeCurve.Evaluate(_percent);
                _primaryAmplitudeAmount = _primaryAmplitudeAmount < .01f ? 0f : _primaryAmplitudeAmount;

                float _primaryRangeAmount = primaryImpactRangeFadeCurve.Evaluate(_percent);
                _primaryRangeAmount = _primaryRangeAmount < .01f ? 0f : _primaryRangeAmount;

                float _primarySpeedAmount = primaryImpactSpeedFadeCurve.Evaluate(_percent);
                _primarySpeedAmount = _primarySpeedAmount < .01f ? 0f : _primarySpeedAmount;

                float _primaryWavelengthAmount = primaryImpactWavelengthFadeCurve.Evaluate(_percent);
                _primaryWavelengthAmount = _primaryWavelengthAmount < .01f ? 0f : _primaryWavelengthAmount;

                mat.SetFloat("_Amplitude2", primaryHitInfo2.initialPrimaryAmplitude * _primaryAmplitudeAmount);
                mat.SetFloat("_Wavelength2", primaryHitInfo2.initialPrimaryWavelength * _primaryWavelengthAmount);
                mat.SetFloat("_Range2", primaryHitInfo2.initialPrimaryRange * _primaryRangeAmount);
                mat.SetFloat("_Speed2", primaryHitInfo2.initialPrimarySpeed * _primarySpeedAmount);



            }

            if (impactTwoTimer <= primaryImpactSubsurfaceDuration) {
                float _percent = impactTwoTimer / primaryImpactSubsurfaceDuration;
                _percent = Mathf.Min(1f, _percent);

                float _primarySubsurfaceRangeAmount = primaryImpactSubsurfaceRangeFadeCurve.Evaluate(_percent);
                _primarySubsurfaceRangeAmount = _primarySubsurfaceRangeAmount < .01f ? 0f : _primarySubsurfaceRangeAmount;

                mat.SetFloat("_SubsurfaceIntensity2", _primarySubsurfaceRangeAmount);

            }

            if (impactTwoTimer <= secondaryImpactDuration) {
                float _percent = impactTwoTimer / secondaryImpactDuration;
                _percent = Mathf.Min(1f, _percent);

                float _secondaryAmplitudeAmount = secondaryImpactAmplitudeFadeCurve.Evaluate(_percent);
                _secondaryAmplitudeAmount = _secondaryAmplitudeAmount < .01f ? 0f : _secondaryAmplitudeAmount;

                float _secondaryRangeAmount = secondaryImpactRangeFadeCurve.Evaluate(_percent);
                _secondaryRangeAmount = _secondaryRangeAmount < .01f ? 0f : _secondaryRangeAmount;

                float _secondarySpeedAmount = secondaryImpactSpeedFadeCurve.Evaluate(_percent);
                _secondarySpeedAmount = _secondarySpeedAmount < .01f ? 0f : _secondarySpeedAmount;

                float _secondaryWavelengthAmount = secondaryImpactWavelengthFadeCurve.Evaluate(_percent);

                mat.SetFloat("_SecondaryAmplitude2", primaryHitInfo2.initialSecondaryAmplitude * _secondaryAmplitudeAmount);
                mat.SetFloat("_SecondaryWavelength2", primaryHitInfo2.initialSecondaryWavelength * _secondaryWavelengthAmount);
                mat.SetFloat("_SecondaryRange2", primaryHitInfo2.initialSecondaryRange * _secondaryRangeAmount);
                mat.SetFloat("_SecondarySpeed2", primaryHitInfo2.initialSecondarySpeed * _secondarySpeedAmount);


            }

            if (impactTwoTimer >= secondaryImpactDuration) {
                isTwoImpacted = false;
                impactTwoTimer = 0f;
            }
        }

        if (Input.GetKeyDown(KeyCode.Q)) canGoAllFoggyAndStuff = true;

        if (canGoAllFoggyAndStuff) {
            foggyTimer += Time.deltaTime;
            float _percent = foggyTimer / transitionToFogDuration;
            _percent = Mathf.Min(1f, _percent);

            float fogAmount = fogCurve.Evaluate(_percent);

            mat.SetFloat("_DebugSlider", fogAmount);
        }

        if (Input.GetKeyDown(KeyCode.O)) canRotate = true;

        if (canRotate) {
            Vector3 _rotationVector = new Vector3(0, 0, rotationSpeed * Time.deltaTime);
            transform.Rotate(_rotationVector, Space.Self);
        }
    } 

}
