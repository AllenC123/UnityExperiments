Shader "Unlit/Shader2"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _Density("Density", Range(2,50)) = 30
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc"
            
            struct v2f
            {
                float2 uv: TEXCOORD0;
                float4 vertex: SV_POSITION;
            };
            
            float _Density;
            fixed4 _Color;
            
            // vertex shader
            v2f Vert(float4 pos: POSITION, float2 uv: TEXCOORD0)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(pos);
                o.uv = uv * _Density; // multiplying texture-coordinates to create tiling
                return o;
            }
            
            // fragment shader
            fixed4 Frag(v2f i) : SV_Target
            {
                float2 c = i.uv;
                c = floor(c) / 2;
                float checker = frac(c.x + c.y) * 2;
                return checker * _Color;
            }
            ENDHLSL
        }
    }
}
