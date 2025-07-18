Shader "VertexShaderTest"
{
    Properties
    {
        // creates a dropdown to select the vertex shader
        [KeywordEnum(NORMALMAP, SPLITTONE)] _VSHADER_SELECTION("Vertex Shader", Integer) = 0
        _numSubdivisions("Subdivisions", Range(1,10)) = 1
    }
    
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            
            // this naming pattern in the pragma arguments is mandatory (featureset-name + identifier)
            #pragma shader_feature_local _VSHADER_SELECTION_NORMALMAP _VSHADER_SELECTION_SPLITTONE
            // use 'shader_feature_local' instead of 'multi_compile_local' when branch is based on material-properties only
            // https://docs.unity3d.com/Manual/shader-conditionals-choose-a-type.html
            
            // pragmas cannot be conditional - the file can never contain multiple '#pragma vertex' statements
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc"
            
            int _numSubdivisions;
            
            struct v2f {
                float4 vertex: SV_POSITION;
                fixed4 color: COLOR;
                fixed2 uv: TEXCOORD0;
            };
            
            
            #if _VSHADER_SELECTION_NORMALMAP
            // appdata_base: position, normal and one texture coordinate.
            // https://docs.unity3d.com/Manual/SL-VertexProgramInputs.html
            v2f Vert(appdata_base v) // NormalMap vertex-shader
            {
                v2f output;
                float colorOffset = 0.75 + (0.25 * _SinTime[3]); // unity time is float4
                output.vertex = UnityObjectToClipPos(v.vertex);
                output.color.xyz = (v.normal * colorOffset) + (0.5*colorOffset);
                output.color.w = 1.0;
                return output;
            }
            #endif
            
            #if _VSHADER_SELECTION_SPLITTONE
            v2f Vert( // SplitTone vertex-shader
                float4 vertex: POSITION,
                float2 uv0: TEXCOORD0,
                float2 uv1: TEXCOORD1,
                float2 uv2: TEXCOORD2,
                float2 uv3: TEXCOORD3 
            ) {
                v2f output;
                output.uv = uv0 * _numSubdivisions;
                output.vertex = UnityObjectToClipPos(vertex);
                // output.color.xyzw = float4(uv0.x-uv0.y, 0, uv0.y-uv0.x, 1);
                output.color.xyzw = float4(uv0.x-uv0.y, uv0.x*uv0.y, uv0.y-uv0.x, 1);
                return output;
            }
            #endif
            
            fixed4 Frag(v2f input) : SV_Target
            {
                #if _VSHADER_SELECTION_SPLITTONE
                // return input.color * frac((input.uv.x + input.uv.y)/2 * _numSubdivisions);
                return input.color / _numSubdivisions / frac((input.uv.x * _numSubdivisions - input.uv.y));
                #endif
                
                return input.color;
            }
            ENDHLSL
        }
    }
}
