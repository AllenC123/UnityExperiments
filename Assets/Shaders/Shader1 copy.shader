Shader "Unlit/Shader1"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
        _ColorR("R", Range(0,1)) = 0
        _ColorG("G", Range(0,1)) = 0
        _ColorB("B", Range(0,1)) = 0
        _ColorA("A", Range(0,1)) = 1
        _Color("Color", Color) = (0,0,0,1)
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            fixed4 _Color;
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
                _ColorR = _Color[0];
                _ColorG = _Color[1];
                _ColorB = _Color[2];
                _ColorA = _Color[3];
                return fixed4(_ColorR, _ColorG, _ColorB, _ColorA);
            }
            ENDHLSL
        }
    }
}
