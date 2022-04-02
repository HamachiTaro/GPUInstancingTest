Shader "Unlit/DrawMeshInstancedTest"
{
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            struct my_struct
            {
                float4 color;
                float3 position;
                float3 scale;
            };

            RWStructuredBuffer<my_struct> _Buffer;
            
            v2f vert(appdata v, uint instanceId : SV_InstanceID)
            {
                v2f o;
                
                my_struct data = _Buffer[instanceId];
                // モデル変換（ローカル座標からワールド座標に変換）
                float3 pos = (v.vertex * data.scale) + data.position;
                // ビュー変換、プロジェクション変換のみ行えばよいので、UNITY_MATRIX_"M"VPではなくUNITY_MATRIX_VPをかける
                o.vertex = mul(UNITY_MATRIX_VP, float4(pos, 1.0));
                o.color = data.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}