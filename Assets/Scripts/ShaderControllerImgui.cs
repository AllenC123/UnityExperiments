using UnityEngine;
using UnityEngine.Rendering; // ShaderPropertyType


[ExecuteInEditMode]
class ShaderControllerImgui: MonoBehaviour
{
    new Renderer renderer;
    private Shader shader;
    private int propCount;
    
    private bool isActive = true;
    
    // hack to reset shader parameters (Unity rewrites material files whenever the value of a shader parameter changes)
    private Material originalMaterial;
    void OnDisable() { if (originalMaterial != null) renderer.sharedMaterial = originalMaterial; }
    void Reset() {
        if (renderer.sharedMaterial != originalMaterial) // not allowed to call CopyProperties when src/dest refer to the same object
            renderer.sharedMaterial.CopyPropertiesFromMaterial(originalMaterial);
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
        GUILayout.Label(System.Math.Round(sliderVal, 2).ToString().PadRight(6)); // glitchy behavior occurs when the number of digits displayed (after the decimal place) change
        // don't forget to account for the decimal place and negative sign!
        GUILayout.EndHorizontal();
        return sliderVal;
    }
    
    
    void OnGUI()
    {
        GUI.skin.button.normal.textColor = (isActive? Color.white : Color.grey);
        GUI.skin.button.hover.textColor  = (isActive? Color.white : Color.grey);
        if (GUILayout.Button(string.Format("ShaderControls: {0}", shader.name))) isActive = !isActive;
        if (!isActive) return;
        
        // these values persist forever once specified; commenting them out won't revert the changes
        GUIStyle sliderStyle = GUI.skin.horizontalSlider;
        sliderStyle.normal.background = Texture2D.whiteTexture;
        sliderStyle.alignment = TextAnchor.LowerLeft;
        sliderStyle.stretchWidth = false;
        sliderStyle.stretchHeight = true;
        sliderStyle.fixedWidth = 100;
        sliderStyle.fixedHeight = 12;
        
        sliderStyle.margin.left = 10;
        sliderStyle.margin.right= 10;
        sliderStyle.padding.top    = 0;
        sliderStyle.padding.bottom = 0;
        
        //sliderStyle.border.top = 0;
        //sliderStyle.border.bottom = 0;
        //sliderStyle.margin.top = 5;
        //sliderStyle.margin.bottom = 5;
        
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
                    /* renderer.sharedMaterial.SetInteger( propName,
                        (GUILayout.Toggle((renderer.sharedMaterial.GetInteger(propName) == 1), propName)? 1:0)
                    ); */
                    
                    // Shader doesn't actually update unless you use 'Enable/DisableKeyword'; 'SetInteger' only updates toggle-state
                    if (GUILayout.Toggle((renderer.sharedMaterial.GetInteger(propName) == 1), propName)) {
                        renderer.sharedMaterial.SetInteger(propName, 1);
                        //renderer.sharedMaterial.EnableKeyword(propName); //prop-name doesn't work
                        renderer.sharedMaterial.EnableKeyword( string.Format("{0}_ON",  propName));
                        renderer.sharedMaterial.DisableKeyword(string.Format("{0}_OFF", propName));
                    } else {
                        renderer.sharedMaterial.SetInteger(propName, 0);
                        renderer.sharedMaterial.EnableKeyword( string.Format("{0}_OFF", propName));
                        renderer.sharedMaterial.DisableKeyword(string.Format("{0}_ON",  propName));
                    }
                    // Unity is a mess
                } break;
                
                default: break;
            }
        }
        
        GUI.skin.button.hover.textColor = Color.red;
        if (GUILayout.Button("Reset")) Reset();
    }
}