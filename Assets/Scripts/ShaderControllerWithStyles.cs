using UnityEngine;
using UnityEngine.Rendering; // ShaderPropertyType


[ExecuteInEditMode]
class ShaderControllerWithStyles: MonoBehaviour
{
    new Renderer renderer;
    private Shader shader;
    private int propCount;
    
    private bool isActive = true;
    public  GUIStyle sliderStyle;
    private GUIStyle defaultStyle;
    
    void ResetSliderStyle() {
        if (defaultStyle != null) {
            sliderStyle = new GUIStyle(defaultStyle);
            sliderStyle.name.Replace("(default)", "(custom)");
        } else sliderStyle = null;
    }
    
    // hack to reset shader parameters (Unity rewrites material files whenever the value of a shader parameter changes)
    private Material originalMaterial;
    void OnDisable() {
        if (originalMaterial != null) renderer.sharedMaterial = originalMaterial;
        sliderStyle = null; defaultStyle = null;
    }
    
    void Reset() {
        if (renderer.sharedMaterial != originalMaterial) // not allowed to call CopyProperties when src/dest refer to the same object
            renderer.sharedMaterial.CopyPropertiesFromMaterial(originalMaterial);
        ResetSliderStyle();
        return;
    }
    
    
    void Start()
    {
        print(string.Format("Initializing ShaderController [{0}]", gameObject.name));
        renderer = GetComponent<Renderer>();
        shader = renderer.sharedMaterial.shader;
        propCount = shader.GetPropertyCount();
        print(string.Format("shader '{0}': {1} properties", shader.name, propCount));
        
        for (int I=0; I<propCount; ++I) {
            ShaderPropertyType propType = shader.GetPropertyType(I);
            print(string.Format("{1} (#{0}): <{2}>", I, shader.GetPropertyName(I), propType));
        }
        
        originalMaterial = new Material(renderer.sharedMaterial);
    }
    
    
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
        GUI.skin.button.normal.textColor = (isActive? Color.white : Color.grey);
        GUI.skin.button.hover.textColor  = (isActive? Color.white : Color.grey);
        if (GUILayout.Button(string.Format("ShaderControls: {0}", shader.name))) isActive = !isActive;
        if (!isActive) return;
        
        // initializing here because Imgui functions can only be called inside OnGUI
        if (defaultStyle == null) { 
            defaultStyle = new GUIStyle(GUI.skin.horizontalSlider);
            defaultStyle.normal.background = Texture2D.grayTexture;
            defaultStyle.name = string.Format("{0}_(default)", defaultStyle.name);
        }
        if (sliderStyle == null) { ResetSliderStyle(); }
        GUI.skin.horizontalSlider = sliderStyle;
        
        for (int I=0; I < propCount; ++I)
        {
            string propName = shader.GetPropertyName(I);
            switch(shader.GetPropertyType(I))
            {
                case ShaderPropertyType.Range: {
                    Vector2 propRange = shader.GetPropertyRangeLimits(I);
                    renderer.sharedMaterial.SetFloat( propName,
                        Slider( propName,
                            renderer.sharedMaterial.GetFloat(propName),
                            propRange[0], propRange[1]
                        )
                    );
                } break;
                
                // assume boolean; actual numeric parameters will probably be float
                case ShaderPropertyType.Int: {
                    // C# doesn't have Int/Boolean conversions
                    renderer.sharedMaterial.SetInteger( propName,
                        (GUILayout.Toggle((renderer.sharedMaterial.GetInteger(propName) == 1), propName)? 1:0)
                    );
                } break;
                
                default: break;
            }
        }
        
        GUI.skin.button.hover.textColor = Color.red;
        if (GUILayout.Button("Reset")) Reset();
    }
}