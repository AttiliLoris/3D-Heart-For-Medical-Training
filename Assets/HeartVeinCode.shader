Shader "Custom/HeartVeinCode"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (Base Texture)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        
        // --- VEIN SETTINGS ---
        [Header(Vein Settings)]
        _VeinMask ("Vein Mask (Black=Skin, White=Vein)", 2D) = "black" {}
        _FlowTex ("Flow Noise (Light pattern)", 2D) = "white" {}
        _FlowColor ("Flow Color", Color) = (1,0,0,1)
        _FlowSpeed ("Base Flow Speed", Float) = 1.0

        // Valore controllato dallo script (battito)
        _PulseIntensity ("Pulse Intensity (Script)", Float) = 0.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        sampler2D _MainTex;
        sampler2D _VeinMask;
        sampler2D _FlowTex;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_FlowTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        fixed4 _FlowColor;
        float _FlowSpeed;
        float _PulseIntensity;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // BASE SKIN
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;

            // MASK delle vene
            fixed mask = tex2D(_VeinMask, IN.uv_MainTex).r;

            // --- SINCRONIZZAZIONE PULSE + FLOW ---
            // "pulseFlow" determina quanto velocemente scorre il sangue
            // Durante il TUM accelera, nella pausa rallenta.
            float pulseFlow = lerp(0.15, 2.0, _PulseIntensity);

            // Scorrimento UV sincronizzato con il battito
            float2 scrolledUV = IN.uv_FlowTex;
            scrolledUV.y += _Time.y * _FlowSpeed * pulseFlow;

            // Sample del flusso animato
            float flowSample = tex2D(_FlowTex, scrolledUV).r;

            // --- EMISSIONE ---
            // Il sangue si illumina di più quando pulsa
            float emissionStrength = flowSample * mask * (_PulseIntensity * 2.0);

            o.Emission = _FlowColor.rgb * emissionStrength;

            // Parametri standard
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
