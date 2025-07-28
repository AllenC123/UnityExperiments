Shader "Wavy"
{
    Properties
    {
        _magnitudeX("magnitude X", Range(-10,10)) = 0
        _magnitudeY("magnitude Y", Range(-32,32)) = 0
        _frequencyX("frequency X", Range(0, 1)) = 0.25
        _frequencyY("frequency Y", Range(0, 1)) = 0.25
        [Toggle] _ALTDEPTH("alt-depth", Integer) = 0
        [Toggle] _TEXSWIRL("tex-swirl", Integer) = 0
    }
    
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma shader_feature_local _ALTDEPTH_ON _ALTDEPTH_OFF
            #pragma shader_feature_local _TEXSWIRL_ON _TEXSWIRL_OFF
            
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc"
            
            float _magnitudeX;
            float _magnitudeY;
            float _frequencyX;
            float _frequencyY;
            
            struct v2f {
                float4 pos: SV_POSITION;
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
                //float4 world_pos = mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, vertex));
                output.pos = UnityObjectToClipPos(vertex);
                output.pos.y = output.pos.y + _magnitudeY * sin(2*3.14*_SinTime[1]+(output.pos.x * _frequencyY));
                output.pos.x = output.pos.x + _magnitudeX * sin(2*3.14*_CosTime[0]+(output.pos.y * _frequencyX));
                #if _TEXSWIRL_ON
                output.color.xyzw = fixed4(abs(sin((1-uv0.x)*6.18f)*_SinTime[1]), abs(cos((uv0.y*2-uv0.x)*6.18f)*_SinTime[0])*0.25f, abs(cos((1-uv0.x)*6.18f)*_CosTime[1]), 1); // swirl
                #else
                output.color.xyzw = fixed4(1-uv0.x, uv0.y, uv0.x, 1);
                #endif
                
                // output.color.xyzw = fixed4(abs(1-(uv0.x*2)), 0, abs(cos(uv0.x*6.18)), 1);
                // output.color.xyzw = fixed4(output.pos.z, world_pos.z, 0, 1);
                return output;
            }
            
            // already defined in UnityCG header?
            /* inline float LinearEyeDepth( float z ) {
                return 1.0 / (_ZBufferParams.z * z + _ZBufferParams.w);
            } */
            
            inline float LinearEyeDepthToOutDepth(float z) {
                return (1 - _ZBufferParams.w * z) / (_ZBufferParams.z * z);
            }
            
            #if _ALTDEPTH_ON
            fixed4 Frag(v2f input, out float outDepth: SV_DEPTH) : SV_Target
            #else
            fixed4 Frag(v2f input) : SV_Target
            #endif
            {
                //outDepth = input.pos.z;
                //outDepth = LinearEyeDepthToOutDepth(LinearEyeDepth());
                // outDepth = input.color.b * input.color.r;
                // outDepth = 1.f - (input.color.r + input.color.g + input.color.b);
                #if _ALTDEPTH_ON
                outDepth = (input.color.r*_SinTime[0] + input.color.g*_CosTime[2] + input.color.b*_SinTime[1]);
                #endif
                
                // return fixed4(input.color.r*((_SinTime[0]+1)*0.5), input.color.g*((_CosTime[2]+1)*0.5), input.color.b*((_SinTime[1]+1)*0.5), 1);
                return input.color;
            }
            ENDHLSL
        }
    }
}
