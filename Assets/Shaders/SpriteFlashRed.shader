// 2D Sprite 受击闪红Shader，兼容Unity内置管线/URP，仅对不透明像素生效
Shader "Custom/SpriteFlashRed"
{
   Properties
{
    [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
    _FlashColor ("Flash Color", Color) = (1,0,0,1)
    _FlashIntensity ("Flash Intensity", Range(0,1)) = 0
    _Color ("Tint", Color) = (1,1,1,1)
    [MaterialToggle] PixelSnap ("Pixel Snap", Float) = 0
}
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
            };

            fixed4 _Color;
            fixed4 _FlashColor;
            float _FlashIntensity;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif
                return OUT;
            }

            sampler2D _MainTex;

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                // 核心逻辑：仅对不透明像素做闪红插值，透明区域完全不受影响
                c.rgb = lerp(c.rgb, _FlashColor.rgb, _FlashIntensity * c.a);
                c.rgb *= c.a;
                return c;
            }
        ENDCG
        }
    }
}