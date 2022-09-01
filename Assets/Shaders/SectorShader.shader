
Shader "Qzw/Cylinder"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("MainTex", 2D) = "white" {}
        _CenterPosX("CenterPosX",float) = 0
        _CenterPosY("CenterPosY",float) = 0
        _CenterPosZ("CenterPosZ",float) = 0
        _Emission("Emission",2D) = "White"{}
        _EmissionColor("EmissionColor",Color)=(1,1,1,1)
        _EmissionPower("EmissionPower",Range(0,2)) = 1
        _Radius("Radius",float) = 0
        _Angle("Angle",Range(0,180)) = 180
        _Fwd("Forward",Color) = (1,1,1,1)
        _Height("Height",float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        Pass{
            Tags {"LightMode"="ForwardBase"}
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Lighting.cginc"
            fixed3 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _CenterPosX;
            float _CenterPosY;
            float _CenterPosZ;
            float _Radius;
            float _Angle;
            fixed4 _Fwd;
            float _Height;
            sampler2D _Emission;
            fixed4 _EmissionColor;
            fixed _EmissionPower;


            struct a2v{
                float4 vertex:POSITION;
                float3 normal:NORMAL;
                float4 texcoord:TEXCOORD0;
            };
            struct v2f{
                float4 pos:SV_POSITION;
                float3 worldPos:TEXCOORD0;
                float3 worldNormal:TEXCOORD1;
                float range:TEXCOORD2;
                float2 uv:TEXCOORD3;
            };
            v2f vert(a2v v){
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.range = distance(v.vertex,float4(0,0,0,1));
                o.worldPos = mul(unity_ObjectToWorld,v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.uv = _MainTex_ST.xy * v.texcoord.xy + _MainTex_ST.zw;
                return o;
            }
            fixed4 frag(v2f i):SV_TARGET{
                clip(_Height-abs(i.worldPos.y-_CenterPosY));
                fixed3 lightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
                float rng = distance(float4(i.worldPos.x,0,i.worldPos.z,0),float4(_CenterPosX,0,_CenterPosZ,0));
                clip(rng-_Radius);
                fixed3 albedo = tex2D(_MainTex,i.uv);
                fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT * albedo;
                fixed3 diffuse = _LightColor0.rgb * albedo * _Color.rgb * saturate(dot(i.worldNormal,lightDir));
                                fixed4 em = tex2D(_Emission,i.uv.xy);
                                fixed3 emission = _EmissionColor * em * _EmissionPower;
                return fixed4 (diffuse+ ambient+emission,0.5);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}