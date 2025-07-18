Shader "Unlit/Shader1"
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
                return fixed4(_ColorR, _ColorG, _ColorB, _ColorA);
            }
            ENDHLSL
        }
    }
}
