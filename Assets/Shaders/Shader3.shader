Shader "Unlit/Shader3"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
        //_Color("Color", Color) = (1,1,1,1)
        _ColorR("R", Range(0,1)) = 0
        _ColorG("G", Range(0,1)) = 0
        _ColorB("B", Range(0,1)) = 0
        _ColorA("A", Range(0,1)) = 1
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            //fixed4 _Color;
            float _ColorR;
            float _ColorG;
            float _ColorB;
            float _ColorA;
            
            // vertex shader
            float4 vert(float4 vertex : POSITION) : SV_POSITION
            {
                // SV_POSITION == ???
                // MVP == current model view projection matrix
                return mul(UNITY_MATRIX_MVP, vertex);
            }
            
            // fragment shader
            fixed4 frag() : SV_Target
            {
                // SV_Target == ???
                //float t = (_SinTime[3] + 1.f) / 2.f;
                // unity gives time as float4; with scales of (1/8, 1/4, 1/2, 1)
                fixed4 time = fixed4(
                    ((_SinTime[3] + 1.f) / 2.f),
                    ((_SinTime[1] + 1.f) / 2.f),
                    ((_SinTime[2] + 1.f) / 2.f),
                    ((_SinTime[0] + 1.f) / 2.f)
                );
                // trailing commas are not allowed
                // you need to be in the 'game' view to see the color-modulation effect
                
                fixed4 adjustedColor = fixed4(
                    _ColorR * time[3],
                    _ColorG * time[1],
                    _ColorB * time[2],
                    _ColorA * time[0]
                );
                
                return adjustedColor;
            }
            ENDHLSL
        }
    }
}
