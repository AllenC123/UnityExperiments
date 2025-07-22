using UnityEngine;
using UnityEngine.Rendering; // ShaderPropertyType


[ExecuteInEditMode]
class ShaderControllerImgui: MonoBehaviour
{
    new Renderer renderer;
    private Shader shader;
    private int propCount;
    
    // TODO: reset button
    
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
    }
    
    void OnGUI()
    {
        GUILayout.Label(string.Format("ShaderControls: {0}", shader.name));
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
    }
}