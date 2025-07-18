using UnityEngine;


// https://docs.unity3d.com/Documentation/ScriptReference/Transform-eulerAngles.html
public class RotationControls: MonoBehaviour
{
    // members must be public to see them in Unity's inspector pane
    [Range(-1.0f, 1.0f)] public float rotationX = 0.0f;
    [Range(-1.0f, 1.0f)] public float rotationY = 0.0f;
    [Range(-1.0f, 1.0f)] public float rotationZ = 0.0f;
    // Range property creates a slider in the Inspector pane - otherwise it's just a text-field
    
    // interpret sliders as absolute rotation - otherwise, interpret as deltas
    public bool setRotation = true;
    
    // reset hacks
    Quaternion initialRotation;
    public bool resetSliders = false;
    public bool resetRotation = false;
    bool currentSliderSetting = false; // triggers slider-reset when 'setRotation' is toggled
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //print("Start");
        
        initialRotation = transform.rotation;
        currentSliderSetting = setRotation;
        //resetSliders = true;
        //Reset();
        
        /* print(string.Format(
            "rotations: [ X: {0} Y: {1} Z: {2} ]",
             rotationX, rotationY, rotationZ
        )); */
    }
    
    // check if reset should be triggered
    bool ResetCheck() {
        if (currentSliderSetting != setRotation) resetSliders = true;
        return (resetSliders || resetRotation);
    }
    
    void Reset()
    {
        if (resetRotation) { 
            transform.rotation = initialRotation;
            if (setRotation) {
                rotationX = initialRotation[0];
                rotationY = initialRotation[1];
                rotationZ = initialRotation[2];
            }
        }
        if (resetSliders) {
            if (setRotation) {
                rotationX = transform.rotation[0];
                rotationY = transform.rotation[1];
                rotationZ = transform.rotation[2];
            } else {
                rotationX = 0.0f;
                rotationY = 0.0f;
                rotationZ = 0.0f;
            }
        }
        currentSliderSetting = setRotation;
        resetRotation = false;
        resetSliders = false;
    }
    
    private void Rotate()
    {
        // prevents the bullshit rotation that happens when switching / resetting sliders in set-mode
        //if (setRotation) { transform.rotation = new Quaternion(rotationX, rotationY, rotationZ, transform.rotation[3]); }
        if (setRotation) { transform.rotation = new Quaternion(rotationX, rotationY, rotationZ, 1); }
        else { transform.rotation *= new Quaternion(
                rotationX * Time.deltaTime,
                rotationY * Time.deltaTime,
                rotationZ * Time.deltaTime,
                1
            );
        }
    }

    // Update is called once per frame
    void Update()
    {
        //print(transform.rotation);
        /* transform.rotation *= Quaternion.AngleAxis(rotationX * Time.deltaTime, Vector3.up);
        transform.rotation *= Quaternion.AngleAxis(rotationY * Time.deltaTime, Vector3.right); */
        if (ResetCheck()) Reset();
        Rotate();
    }
}
