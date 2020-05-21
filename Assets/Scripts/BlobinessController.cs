using UnityEngine;



public class BlobinessController : MonoBehaviour {

    BlobData[] blobData = new BlobData[4];
    int activeBlobDataIndex;

    int[] wavelengthPropertyIDs = new int[4];
    int[] rangePropertyIDs = new int[4];
    int[] positionPropertyIDs = new int[4];
    int[] amplitudePropertyIDs = new int[4];
    int[] timerPropertyIDs = new int[4];
    int[] progressPropertyIDs = new int[4];

    [SerializeField] AnimationCurve amplitudeAnim;
    [SerializeField] AnimationCurve rangeAnim;

    Material m;


    [Header("DEBUG")]
    public float wavelength = 5f;
    public float range = 1.5f;
    public float amplitude = .15f;
    public float duration = 1.5f;
    public Vector3 position = Vector3.zero;
    public float speed = 1f;

    void Start() {
        m = GetComponent<Renderer>().sharedMaterial;

        
        wavelengthPropertyIDs[0] = Shader.PropertyToID("_Wavelength0");
        wavelengthPropertyIDs[1] = Shader.PropertyToID("_Wavelength1");
        wavelengthPropertyIDs[2] = Shader.PropertyToID("_Wavelength2");
        wavelengthPropertyIDs[3] = Shader.PropertyToID("_Wavelength3");
        rangePropertyIDs[0] = Shader.PropertyToID("_Range0");
        rangePropertyIDs[1] = Shader.PropertyToID("_Range1");
        rangePropertyIDs[2] = Shader.PropertyToID("_Range2");
        rangePropertyIDs[3] = Shader.PropertyToID("_Range3");
        positionPropertyIDs[0] = Shader.PropertyToID("_Position0");
        positionPropertyIDs[1] = Shader.PropertyToID("_Position1");
        positionPropertyIDs[2] = Shader.PropertyToID("_Position2");
        positionPropertyIDs[3] = Shader.PropertyToID("_Position3");
        amplitudePropertyIDs[0] = Shader.PropertyToID("_Amplitude0");
        amplitudePropertyIDs[1] = Shader.PropertyToID("_Amplitude1");
        amplitudePropertyIDs[2] = Shader.PropertyToID("_Amplitude2");
        amplitudePropertyIDs[3] = Shader.PropertyToID("_Amplitude3");
        timerPropertyIDs[0] = Shader.PropertyToID("_Timer0");
        timerPropertyIDs[1] = Shader.PropertyToID("_Timer1");
        timerPropertyIDs[2] = Shader.PropertyToID("_Timer2");
        timerPropertyIDs[3] = Shader.PropertyToID("_Timer3");
        progressPropertyIDs[0] = Shader.PropertyToID("_Progress0");
        progressPropertyIDs[1] = Shader.PropertyToID("_Progress1");
        progressPropertyIDs[2] = Shader.PropertyToID("_Progress2");
        progressPropertyIDs[3] = Shader.PropertyToID("_Progress3");
        //i don't like string concatenation ok
        
    }

    void Update() {

        if(Input.GetKeyDown(KeyCode.U)) {
            RegisterBlobData(position, range, duration, wavelength, amplitude, speed);
        }

        int activeBlobDataAtStartOfFrame = activeBlobDataIndex;//this serves to prevent the apocalypse if we change activeBlobDataIndex while iterating activeBlobDataIndex times in the loop

        for (int i = 0; i < activeBlobDataAtStartOfFrame; i++) {
            blobData[i].timer += Time.deltaTime;
            blobData[i].timer = Mathf.Min(blobData[i].duration, blobData[i].timer);
            blobData[i].progress = blobData[i].timer / blobData[i].duration;

            blobData[i].currentRange = blobData[i].maxRange;//TODO find out relationship between range and wave propagation

            //set all the material properties
            m.SetFloat(wavelengthPropertyIDs[i], blobData[i].wavelength);
            m.SetFloat(rangePropertyIDs[i], blobData[i].currentRange);
            m.SetFloat(amplitudePropertyIDs[i], blobData[i].amplitude * amplitudeAnim.Evaluate(blobData[i].progress));
            m.SetFloat(timerPropertyIDs[i], blobData[i].timer * blobData[i].speed);//we had speed here rather than when += deltatime because otherwise duration is extended as well
            //print(blobData[i].timer * blobData[i].speed + 3.141593f*.5f);
            print(blobData[i].speed);
            m.SetFloat(progressPropertyIDs[i], blobData[i].progress);
            m.SetVector(positionPropertyIDs[i], (Vector4)blobData[i].position);

            if(blobData[i].duration == blobData[i].timer) {
                RemoveBlobData(i);
            }
        }


        //print(activeBlobDataIndex);

    }

    public void RegisterBlobData (Vector3 position, float range, float duration, float wavelength, float maxAmplitude, float speed) {
        BlobData newData = new BlobData();
        newData.position = position;
        newData.maxRange = range;
        newData.duration = duration;
        newData.wavelength = wavelength;
        newData.amplitude = maxAmplitude;
        newData.speed = speed;

        if(activeBlobDataIndex >= blobData.Length) {//if the array is already full with active blobsies, we replace the one at index 0

            for (int i = 0; i < blobData.Length - 1; i++) {//we shift the existing blobdata
                blobData[i] = blobData[i+1];
            }
            blobData[blobData.Length - 1] = newData;//and add the new data at the bottom of the array

        } else {
            blobData[activeBlobDataIndex] = newData;
            activeBlobDataIndex++;
        }
    }

    void RemoveBlobData (int index) {
        for (int i = 0; i < activeBlobDataIndex - 1; i++) {//we iterate over activeBlobDataIndex - 1 because we're about to remove one slot in the array
            if(i < index) continue;
            
            if(i > index) blobData[i] = blobData[i+1];
            
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
