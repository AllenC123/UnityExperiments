using UnityEngine;


// RequireComponent attribute automatically adds required components as dependencies
[ExecuteInEditMode]
[RequireComponent(typeof(CameraController))]
public class CameraControlsImgui: MonoBehaviour
{
    private bool isActive = true;
    private CameraController controller;
    public Vector2 menuArea = new Vector2(350, 200);
    public Font font; public int fontsize = 12;
    
    private float[] sliderDefaults;
    
    void Start()
    {
        controller = GetComponent<CameraController>();
        sliderDefaults = new float[4]{
            controller.angle,
            controller.elevation,
            controller.orbit_radius,
            controller.auto_orbit_speed,
        };
    }
    
    
    static float Slider(string slidername, float value, float lowerlimit, float upperlimit)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(slidername.PadRight(12)); // 12 == length of longest slider-label
        float sliderVal = GUILayout.HorizontalSlider(value, lowerlimit, upperlimit);
        // glitchy behavior occurs when the number of digits displayed (after the decimal place) change
        GUILayout.Label(System.Math.Round(sliderVal, 2).ToString().PadRight(7)); // angle can have 3 digits, +2 decimal-places, +1 for negative-sign, +1 for decimal-point
        GUILayout.EndHorizontal();
        return sliderVal;
    }
    
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(Screen.width-menuArea.x, 0, menuArea.x, menuArea.y));
        GUI.skin.button.normal.textColor = (isActive? Color.white : Color.grey);
        GUI.skin.button.hover.textColor  = (isActive? Color.white : Color.grey);
        if (GUILayout.Button("CameraControls")) isActive = !isActive;
        if (!isActive) { GUILayout.EndArea(); return; }
        
        GUIStyle labelStyle = GUI.skin.label;
        GUIStyle sliderStyle = GUI.skin.horizontalSlider;
        sliderStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.alignment = TextAnchor.MiddleLeft;
        labelStyle.stretchWidth = false;
        //labelStyle.clipping = TextClipping.Clip;
        
        if (font == null) {
            font = Resources.Load<Font>("Fonts/CascadiaMono");
        } // note: font must be located under 'Resources' folder to be loaded by 'Resources.Load'
        else { labelStyle.font = font; labelStyle.fontSize = fontsize; }
        
        sliderStyle.wordWrap = false;
        sliderStyle.fixedWidth = menuArea.x * 0.5f;
        sliderStyle.margin.left = 10;
        sliderStyle.margin.right= 10;
        
        controller.angle = Slider("angle", controller.angle, -360.0f, 360f);
        controller.elevation = Slider("elevation", controller.elevation, -100.0f, 100.0f);
        controller.orbit_radius = Slider("orbit radius", controller.orbit_radius, 0.001f, 100.0f);
        float orbitSpeedNew = Slider("auto-orbit", controller.auto_orbit_speed, -10.0f, 10.0f);
        if ((orbitSpeedNew != 0) && (controller.auto_orbit_speed != orbitSpeedNew)) controller.autoOrbit = true;
        controller.auto_orbit_speed = orbitSpeedNew;
        
        controller.autoOrbit = GUILayout.Toggle(controller.autoOrbit, "enable auto-orbiting");
        if (GUILayout.Button("Reset")) Reset();
        GUILayout.EndArea();
    }
    
    void Reset()
    {
        if (sliderDefaults == null) { return; }
        controller.objSelectionMode = false;
        controller.trackObject = null;
        controller.Reset();
        
                   controller.angle = sliderDefaults[0];
               controller.elevation = sliderDefaults[1];
            controller.orbit_radius = sliderDefaults[2];
        controller.auto_orbit_speed = sliderDefaults[3];
    }
}