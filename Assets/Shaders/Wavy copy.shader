Shader "WavyCopy"
{
    Properties
    {
        _magnitudeX("magnitude X", Range(-10,10)) = 0
        _magnitudeY("magnitude Y", Range(-10,10)) = 0
    }
    
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc"
            
            float _magnitudeX;
            float _magnitudeY;
            
            struct v2f {
                float4 vertex: SV_POSITION;
                fixed4 color: COLOR;
                fixed2 uv: TEXCOORD0;
            };
            
            v2f Vert(
                float4 vertex: POSITION,
                float2 uv0: TEXCOORD0,
                float2 uv1: TEXCOORD1,
                float2 uv2: TEXCOORD2,
                float2 uv3: TEXCOORD3 
            ) {
                v2f output;
                output.uv = uv0;
                output.vertex = UnityObjectToClipPos(vertex);
                output.vertex.x = output.vertex.x + cos(vertex.z*_magnitudeX) + _magnitudeX * sin(2*3.14*_CosTime[1]+(output.vertex.z*0.25));
                output.vertex.y = output.vertex.y + sin(vertex.x*_magnitudeY) + _magnitudeY * sin(2*3.14*_SinTime[0]+(output.vertex.x*0.25));
                //output.color.xyzw = fixed4(1-uv0.x, uv0.y, uv0.x, 1);
                output.color.xyzw = fixed4(output.vertex.z, vertex.z, 0, 1);
                return output;
            }
            
            fixed4 Frag(v2f input) : SV_Target
            {
                return input.color;
            }
            ENDHLSL
        }
    }
}
