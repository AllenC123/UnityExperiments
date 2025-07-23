using UnityEngine;

// cannot save values properly with 'ExecuteInEditMode' enabled.
// style-name is always reverted to 'horizontalSlider' when entering Play-mode, but other values apply correctly.

// [ExecuteInEditMode]
class SliderStyleEditor: MonoBehaviour
{
    /* [System.NonSerialized] */ public  GUIStyle sliderStyle;
    /* [System.NonSerialized] */ private GUIStyle defaultStyle;
    
    public float sliderValue = 0.0f;
    public float sliderMin = -10.0f;
    public float sliderMax =  10.0f;
    
    void ResetSliderStyle() {
        if (defaultStyle != null) {
            sliderStyle = new GUIStyle(defaultStyle);
            sliderStyle.name = sliderStyle.name.Replace("(default)", "(custom)");
        }
    }
    
    //void OnDisable() { sliderStyle = null; defaultStyle = null; }
    
    static float Slider(string slidername, float value, float lowerlimit, float upperlimit)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(slidername);
        float sliderVal = GUILayout.HorizontalSlider(value, lowerlimit, upperlimit);
        GUILayout.Label(System.Math.Round(sliderVal, 2).ToString().PadRight(4)); // glitchy behavior occurs when the number of digits displayed (after the decimal place) change
        GUILayout.EndHorizontal();
        return sliderVal;
    }
    
    
    void OnGUI()
    {
        // initializing here because Imgui functions can only be called inside OnGUI
        if (defaultStyle == null) {
            defaultStyle = new GUIStyle (GUI.skin.horizontalSlider);
            defaultStyle.normal.background = Texture2D.whiteTexture;
            defaultStyle.name = "horizontalSlider_(default)";
        }
        if (sliderStyle == null) { ResetSliderStyle(); }
        
        // renaming style is apparently impossible?? (always 'horizontalSlider')
        // defaultStyle.name = "defaultStyle"; sliderStyle.name = "customStyle";
        //print(string.Format("{0} {1} {2}", defaultStyle.name, sliderStyle.name, GUI.skin.horizontalSlider.name));
        
        GUI.skin.horizontalSlider = sliderStyle;
        // GUI.skin.horizontalSlider.name = sliderStyle.name;
        
        sliderValue = Slider("testSlider", sliderValue, sliderMin, sliderMax);
        sliderValue = Slider("testSlider2", sliderValue, sliderMin, sliderMax);
        
        GUI.skin.button.hover.textColor = Color.red;
        if (GUILayout.Button("Reset")) { ResetSliderStyle(); sliderValue = 0; }
    }
}