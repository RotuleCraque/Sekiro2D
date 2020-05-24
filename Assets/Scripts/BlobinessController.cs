using UnityEngine;



public class BlobinessController : MonoBehaviour {

    BlobData[] blobData = new BlobData[4];
    Vector4[] impactPositions = new Vector4[4];
    float[] wavelengths = new float[4];
    float[] amplitudes = new float[4];
    float[] ranges = new float[4];
    float[] timers = new float[4];
    int activeBlobDataIndex;

    int[] wavelengthPropertyIDs = new int[4];
    int[] rangePropertyIDs = new int[4];
    int[] positionPropertyIDs = new int[4];
    int[] amplitudePropertyIDs = new int[4];
    int[] timerPropertyIDs = new int[4];
    int[] progressPropertyIDs = new int[4];

    [SerializeField] AnimationCurve amplitudeFadeCurve;

    Material m;

    int wavelengthsPropertyID;
    int rangesPropertyID;
    int impactPositionsPropertyID;
    int amplitudesPropertyID;
    int timersPropertyID;


    [Header("DEBUG")]
    public float wavelength = 5f;
    public float range = 1.5f;
    public float amplitude = .15f;
    public float duration = 1.5f;
    public Vector3 position = Vector3.zero;
    public float speed = 1f;

    void Start() {
        m = GetComponent<Renderer>().sharedMaterial;

        wavelengthsPropertyID = Shader.PropertyToID("_Wavelengths");
        rangesPropertyID = Shader.PropertyToID("_Ranges");
        impactPositionsPropertyID = Shader.PropertyToID("_ImpactPositions");
        amplitudesPropertyID = Shader.PropertyToID("_Amplitudes");
        timersPropertyID = Shader.PropertyToID("_Timers");
        
    }

    void Update() {

        if(Input.GetKeyDown(KeyCode.U)) {
            RegisterBlobData(position, range, duration, wavelength, amplitude, speed);
            //Debug.Break();
        }

        int activeBlobDataAtStartOfFrame = activeBlobDataIndex;//this serves to prevent the apocalypse if we change activeBlobDataIndex while iterating activeBlobDataIndex times in the loop

        for (int i = 0; i < activeBlobDataAtStartOfFrame; i++) {
            blobData[i].timer += Time.deltaTime;
            blobData[i].timer = Mathf.Min(blobData[i].duration, blobData[i].timer);
            blobData[i].progress = blobData[i].timer / blobData[i].duration;

            blobData[i].currentRange = Mathf.Min(blobData[i].timer * blobData[i].speed, blobData[i].maxRange);//the range increases progressively to mimic wave propagation

            //set all the material properties
            /*
            m.SetFloat(wavelengthPropertyIDs[i], blobData[i].wavelength);
            m.SetFloat(rangePropertyIDs[i], blobData[i].currentRange);
            m.SetFloat(amplitudePropertyIDs[i], blobData[i].amplitude * amplitudeAnim.Evaluate(blobData[i].progress));
            m.SetFloat(timerPropertyIDs[i], blobData[i].timer * blobData[i].speed + .5f * blobData[i].wavelength);//we had speed here rather than when += deltatime because otherwise duration is extended as well
            m.SetFloat(progressPropertyIDs[i], blobData[i].progress);
            m.SetVector(positionPropertyIDs[i], (Vector4)blobData[i].position);
            */

            impactPositions[i] = (Vector4)blobData[i].position;
            wavelengths[i] = blobData[i].wavelength;
            ranges[i] = blobData[i].currentRange;
            amplitudes[i] = blobData[i].amplitude * amplitudeFadeCurve.Evaluate(blobData[i].progress);
            timers[i] = blobData[i].timer * blobData[i].speed + .5f * blobData[i].wavelength;

            if(blobData[i].duration == blobData[i].timer) {
                RemoveBlobData(i);
            }
            
        }

        if(activeBlobDataAtStartOfFrame != 0) {
            //print("dddd");
            m.SetVectorArray(impactPositionsPropertyID, impactPositions);
            m.SetFloatArray(wavelengthsPropertyID, wavelengths);
            m.SetFloatArray(rangesPropertyID, ranges);
            m.SetFloatArray(amplitudesPropertyID, amplitudes);
            m.SetFloatArray(timersPropertyID, timers);
        }

        //print(amplitudes[0] + " " + amplitudes[1] + " " + amplitudes[2] +" " + amplitudes[3]);

        //print(activeBlobDataAtStartOfFrame);

    }

    public void RegisterBlobData (Vector3 position, float range, float duration, float wavelength, float maxAmplitude, float speed) {
        BlobData newData = new BlobData();
        newData.position = position;
        newData.maxRange = range;
        newData.duration = duration;
        newData.wavelength = wavelength;
        newData.amplitude = maxAmplitude;
        newData.speed = speed;
        newData.timer = 0f;
        newData.progress = 0f;


        //we always store the newest impact at index 0 and shift all others
        for (int i = 0; i < blobData.Length - 1; i++) {
            blobData[i+1] = blobData[i];//shift all blobdata and overwrite oldest if array is already full
        }
        blobData[0] = newData;

        activeBlobDataIndex = Mathf.Min(blobData.Length - 1, activeBlobDataIndex + 1);//increment number of active blobs

        /*
        if(activeBlobDataIndex >= blobData.Length) {//if the array is already full with active blobsies, we replace the one at index 0

            for (int i = 0; i < blobData.Length - 1; i++) {//we shift the existing blobdata
                blobData[i] = blobData[i+1];
            }
            blobData[blobData.Length - 1] = newData;//and add the new data at the bottom of the array

        } else {
            blobData[activeBlobDataIndex] = newData;
            activeBlobDataIndex++;
        }
        */
    }

    void RemoveBlobData (int index) {


        
        for (int i = 0; i < activeBlobDataIndex - 1; i++) {//we iterate over activeBlobDataIndex - 1 because we're about to remove one slot in the array
            if(i < index) continue;
            
            if(i > index) {
                blobData[i] = blobData[i+1];
                /*
                blobData[i+1].position = Vector3.zero;
                blobData[i+1].maxRange = 0;
                blobData[i+1].duration = 0;
                blobData[i+1].wavelength = 0;
                blobData[i+1].amplitude = 0;
                blobData[i+1].speed = 0;
                blobData[i+1].timer = 0;
                blobData[i+1].progress = 0;
                */

                impactPositions[i+1] = Vector4.zero;
                wavelengths[i+1] = 0;
                ranges[i+1] = 0;
                amplitudes[i+1] = 0;
                timers[i+1] = 0;
            }
            
        }

        activeBlobDataIndex--;
        
    }




    public struct BlobData {
        public float currentRange;
        public float maxRange;
        public float amplitude;
        public float wavelength;
        public Vector3 position;
        public float timer;
        public float duration;
        public float progress;
        public float speed;
    }

}
