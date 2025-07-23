using UnityEngine;
using UnityEngine.Rendering; // ShaderPropertyType


[ExecuteInEditMode]
class ShaderControllerImgui: MonoBehaviour
{
    new Renderer renderer;
    private Shader shader;
    private int propCount;
    
    private bool isActive = true;
    
    private Material originalMaterial;
    // hack to reset shader parameters (Unity rewrites material files whenever the value of a shader parameter changes)
    void OnDisable() { if (originalMaterial == null) return; renderer.sharedMaterial = originalMaterial; }
    void Reset() { renderer.sharedMaterial.CopyPropertiesFromMaterial(originalMaterial); }
    
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
    
    void OnGUI()
    {
        GUI.skin.button.normal.textColor = (isActive? Color.white : Color.grey);
        GUI.skin.button.hover.textColor  = (isActive? Color.white : Color.grey);
        if (GUILayout.Button(string.Format("ShaderControls: {0}", shader.name))) isActive = !isActive;
        if (!isActive) return;
        
        GUIStyle sliderStyle = GUI.skin.horizontalSlider;
        sliderStyle.normal.background = Texture2D.whiteTexture;
        
        for (int I=0; I < propCount; ++I)
        {
            string propName = shader.GetPropertyName(I);
            switch(shader.GetPropertyType(I))
            {
                case ShaderPropertyType.Range: {
                    Vector2 propRange = shader.GetPropertyRangeLimits(I);
                    GUILayout.Label(propName);
                    
                    renderer.sharedMaterial.SetFloat( propName,
                        GUILayout.HorizontalSlider(
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