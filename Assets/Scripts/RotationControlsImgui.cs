using UnityEngine;


//[ExecuteInEditMode]
public class RotationControlsImgui: MonoBehaviour
{
    private Quaternion initialRotation;
    
    private float rotationX = 0.0f;
    private float rotationY = 0.0f;
    private float rotationZ = 0.0f;
    
    // interpret sliders as absolute rotation - otherwise, interpret as deltas
    private bool setRotation = true;
    
    private bool isActive = true;
    [Range(0, 1080)] public float menuY = 200;
    [Range(0, 360)] public float menuWidth = 180;
    
    void Start() { initialRotation = transform.rotation; }
    
    void ResetRotation() {
        transform.rotation = initialRotation;
        if (setRotation) {
            rotationX = initialRotation[0];
            rotationY = initialRotation[1];
            rotationZ = initialRotation[2];
        }
    }
    
    void ResetSliders() {
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
    
    void Update()
    {
        if (setRotation) { transform.rotation = new Quaternion(rotationX, rotationY, rotationZ, 1); }
        else { transform.rotation *= new Quaternion(
                rotationX * Time.smoothDeltaTime,
                rotationY * Time.smoothDeltaTime,
                rotationZ * Time.smoothDeltaTime,
                1
            );
        }
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(0, menuY, menuWidth, Screen.height-menuY));
        
        // the length of the string somehow determines the length of all sliders. Unity is a mess
        if (GUILayout.Button("Rotation Controls")) isActive = !isActive;
        if (!isActive) { GUILayout.EndArea(); return; }
        
        GUIStyle sliderStyle = GUI.skin.horizontalSlider;
        sliderStyle.normal.background = Texture2D.whiteTexture;
        sliderStyle.fixedWidth = menuWidth;
        sliderStyle.stretchWidth = false;
        
        rotationX = GUILayout.HorizontalSlider(rotationX, -1, 1);
        rotationY = GUILayout.HorizontalSlider(rotationY, -1, 1);
        rotationZ = GUILayout.HorizontalSlider(rotationZ, -1, 1);
        if (GUILayout.Toggle(setRotation, "setRotation") != setRotation) { setRotation = !setRotation; ResetSliders(); }
        if (GUILayout.Button("Reset")) { if(setRotation) ResetRotation(); else ResetSliders(); }
        
        GUILayout.EndArea();
    }
}