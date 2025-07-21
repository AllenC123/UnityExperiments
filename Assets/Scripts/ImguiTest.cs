using UnityEngine;


[ExecuteInEditMode]
public class ImguiTest: MonoBehaviour
{
    private int _clickCount = 0;
    private int _clickCount2 = 0;
    private bool isChecked = false;
    private float sliderVal = 0.0f;
    
    void OnGUI()
    {
        GUIStyle customStyle = GUI.skin.button;
        customStyle.normal.textColor = Color.teal;
        customStyle.active.textColor = Color.gold;
        
        GUI.BeginGroup(new Rect(0, 200, 1000, 1000));
        
        if (GUILayout.Button((_clickCount > 0)? string.Format("Button ({0})", _clickCount) : "Button", customStyle)) { ++_clickCount; }
        if (GUILayout.RepeatButton((_clickCount2 > 0)? string.Format("RepeatButton ({0})", _clickCount2) : "RepeatButton", customStyle)) { ++_clickCount2; }
        if (GUILayout.Button("Reset")) { _clickCount = 0; _clickCount2 = 0; isChecked = false; }
        isChecked = GUILayout.Toggle(isChecked, "Toggle");
        
        // getting slider label on same line is ridiculous
        GUILayout.BeginArea(new Rect(0, 120, 360, 16));
        GUILayout.BeginHorizontal();
        sliderVal = GUILayout.HorizontalSlider(sliderVal, -360, 360);
        GUILayout.Label(Mathf.Round(sliderVal).ToString()); // glitchy behavior occurs when the number of digits displayed (after the decimal place) change
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
        
        GUI.EndGroup();
    }
}
