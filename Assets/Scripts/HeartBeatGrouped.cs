using UnityEngine;

/**
 * HeartBeat_Grouped (Natural+)
 * - Wrapper guida: accorciamento Y, espansione X/Z quasi-volumetrica, torsione LV-dominante.
 * - Untwist aggressivo in IVR, suction in Early Filling (E-wave), piccolo A-kick.
 * - Filtro 2° ordine critico per scale/pos/rot; micro-HRV; modulazione respiratoria.
 * - Nessuna riparentizzazione delle mesh.
 */
public class HeartBeat_Grouped : MonoBehaviour
{
    [Header("Assign (drag from Hierarchy)")]
    public Transform rightAtrium, leftAtrium, rightVentricle, leftVentricle, groupedRoot;

    [Header("What to animate")]
    public bool animateAtria = true;
    public bool animateVentricles = true;

    [Header("Heart Rate")]
    [Range(30, 180)] public float bpmBase = 70f;
    [Range(0f, 6f)] public float bpmJitter = 2f;
    [Range(0.1f, 3f)] public float jitterSpeed = 0.8f;

    [Header("Respiration (optional)")]
    public bool respiratoryModulation = true;
    [Range(0f, 0.15f)] public float respAmp = 0.05f;
    [Range(4f, 20f)] public float respRate = 10f;
    [Range(0f, 0.4f)] public float respTwistBoost = 0.12f;

    [Header("Atria")]
    [Range(0.90f, 1.00f)] public float atriaScaleYMin = 0.985f;
    [Range(0.98f, 1.00f)] public float atriaScaleXZMin = 0.995f;
    [Range(0f, 0.01f)] public float atriaDown = 0.0015f;
    [Range(0f, 2f)] public float atriaTwistDeg = 0.5f;
    [Tooltip("Ritardo elettrico LA vs RA (sec)")][Range(0f, 0.05f)] public float laDelaySec = 0.012f;
    [Range(0.5f, 1.5f)] public float raGain = 1.12f, laGain = 1.00f;

    [Header("Ventricles / Group driver")]
    [Range(0.70f, 1.00f)] public float ventScaleYMin = 0.82f;
    [Range(1.00f, 1.10f)] public float ventScaleXZMax = 1.05f;
    [Range(0f, 0.02f)] public float ventDown = 0.006f;
    [Range(0f, 20f)] public float ventTwistDeg = 10f;
    [Range(0f, 1f)] public float twistFollow = 0.6f;

    [Header("Group mixing")]
    [Range(0f, 5f)] public float groupTiltDeg = 2f;
    [Range(0f, 1f)] public float tiltFollow = 0.6f;

    [Header("Timing (fractions of cycle)")]
    [Range(0f, 1f)] public float tAtrialLead = 0.05f;
    [Range(0f, 0.3f)] public float tAtrialDur = 0.14f;
    [Range(0f, 1f)] public float tIVC_Start = 0.12f, tIVC_End = 0.18f;
    [Range(0f, 1f)] public float tRapidEj_End = 0.45f, tReducedEj_End = 0.65f;
    [Range(0f, 1f)] public float tIVR_End = 0.75f;
    [Range(0f, 1f)] public float tEarlyFill_End = 0.88f;

    [Header("Natural Response (2nd order filter)")]
    [Range(2f, 20f)] public float wnParts = 9f;
    [Range(0.6f, 1.2f)] public float zetaParts = 1.0f;
    [Range(2f, 20f)] public float wnGroup = 11f;
    [Range(0.6f, 1.2f)] public float zetaGroup = 1.0f;

    [Header("Volume quasi-conservato")]
    [Range(0.0f, 1.0f)] public float volKeep = 0.85f;

    [Header("Hysteresis & Suction")]
    [Range(0.0f, 1.0f)] public float twistHysteresis = 0.55f;
    [Range(0.0f, 1.0f)] public float suctionKick = 0.25f;

    [Header("Micro-noise fisiologico")]
    [Range(0f, 0.003f)] public float noiseAmpPos = 0.0012f;
    [Range(0f, 0.8f)] public float noiseAmpTwist = 0.18f;
    [Range(0.5f, 3f)] public float noiseFreq = 1.2f;

    [Header("Anisotropia espansione (XZ)")] // NEW: più rigonfiamento laterale che anteriore
    [Range(0.8f, 1.2f)] public float xzAniso = 1.06f; // X più ampio di Z

    [Header("RV vs LV (torsione/balance)")] // NEW: LV domina torsione CCW, RV smorza
    [Range(0f, 1f)] public float rvOpposesTwist = 0.25f;

    // internals
    struct Pose { public Vector3 pos, scale; public Quaternion rot; }
    Pose ra0, la0, rv0, lv0, group0;
    float phase, bpmRuntime, lastBeatSeed; // NEW: seme per microvariazioni per battito
    SecondOrder1D1 fY_parts, fXZ_parts, fTw_parts, fDown_parts;
    SecondOrder1D1 fY_group, fXZ_group, fTw_group, fDown_group;

    void Awake()
    {
        if (groupedRoot) group0 = ReadPose(groupedRoot);
        if (rightAtrium) ra0 = ReadPose(rightAtrium);
        if (leftAtrium) la0 = ReadPose(leftAtrium);
        if (rightVentricle) rv0 = ReadPose(rightVentricle);
        if (leftVentricle) lv0 = ReadPose(leftVentricle);
        bpmRuntime = bpmBase;
        lastBeatSeed = Random.value; // NEW
    }

    void Start()
    {
        fY_parts = new SecondOrder1D1(wnParts, zetaParts, 1f);
        fXZ_parts = new SecondOrder1D1(wnParts, zetaParts, 1f);
        fTw_parts = new SecondOrder1D1(wnParts, zetaParts, 1f);
        fDown_parts = new SecondOrder1D1(wnParts, zetaParts, 1f);

        fY_group = new SecondOrder1D1(wnGroup, zetaGroup, 1f);
        fXZ_group = new SecondOrder1D1(wnGroup, zetaGroup, 1f);
        fTw_group = new SecondOrder1D1(wnGroup, zetaGroup, 1f);
        fDown_group = new SecondOrder1D1(wnGroup, zetaGroup, 1f);
    }

    void Update()
    {
        float dt = Mathf.Max(Time.deltaTime, 1e-4f);

        // ---- HRV jitter (Perlin) ----
        float j = (Mathf.PerlinNoise(0.123f, Time.time * jitterSpeed) - 0.5f) * 2f;
        float bpm = bpmBase + j * bpmJitter;

        // ---- Respiration modulation ----
        float resp = 0f;
        if (respiratoryModulation)
        {
            float hz = Mathf.Max(0.02f, respRate / 60f);
            resp = Mathf.Sin(2f * Mathf.PI * hz * Time.time);
            bpm *= (1f + respAmp * 0.12f * resp);
        }
        bpmRuntime = bpm;

        // ---- Phase advance ----
        float T = 60f / Mathf.Max(30f, bpm);
        float prevPhase = phase;
        phase += dt / T;
        phase -= Mathf.Floor(phase);

        // NEW: nuovo seme per micro-asimmetrie ad ogni R-peak (wrapping della fase)
        if (phase < prevPhase) lastBeatSeed = Mathf.PerlinNoise(Time.time * 0.37f, 0.91f);

        // ---- Build phases ----
        float atr = WindowedPulse(phase, tAtrialLead, tAtrialDur, 1.6f);
        float laDelayFrac = Mathf.Clamp01(laDelaySec / Mathf.Max(1e-4f, T));
        float atl = WindowedPulse(NormPhase(phase - laDelayFrac), tAtrialLead, tAtrialDur, 1.6f);

        float ivc = Segment(phase, tIVC_Start, tIVC_End);
        float ej1 = Segment(phase, tIVC_End, tRapidEj_End);
        float ej2 = Segment(phase, tRapidEj_End, tReducedEj_End);
        float ivr = Segment(phase, tReducedEj_End, tIVR_End);
        float efill = Segment(phase, tIVR_End, tEarlyFill_End);
        float dias = 1f - Mathf.Clamp01(ivc + ej1 + ej2 + ivr + efill);

        // ---- Ventricular composite pulse (più fisiologico) ----
        float vPulse = VentPulse(ivc, ej1, ej2, ivr, efill, dias);

        // ---- Targets GROUP ----
        float yTarget = Mathf.Lerp(1f, ventScaleYMin, vPulse);
        float xzTarget = Mathf.Lerp(1f, ventScaleXZMax, vPulse);

        // NEW: anisotropia X/Z (leggera)
        float xz_x = Mathf.Lerp(1f, ventScaleXZMax * xzAniso, vPulse);
        float xz_z = Mathf.Lerp(1f, ventScaleXZMax / xzAniso, vPulse);

        // Torsione con isteresi e LV dominance
        float twist01 = 0f;
        twist01 += SmoothRise(ivc) * 0.35f;
        twist01 += SmoothRise(ej1) * 0.85f;   // leggermente più forte
        twist01 += SmoothFall(ej2) * 0.30f;
        twist01 -= SmoothRise(ivr) * 1.35f;   // untwist più rapido
        twist01 -= SmoothRise(efill) * 0.25f;

        // NEW: RV “oppone” parte del twist (ammorbidisce il picco)
        twist01 = Mathf.Clamp01(twist01 * (1f - rvOpposesTwist * 0.35f));

        float twTargetDeg = twist01 * ventTwistDeg * twistFollow;

        // Discesa apice (AV-ring descent)
        float downTarget = ventDown * vPulse;

        // Volume quasi-conservato
        if (volKeep > 0f)
        {
            float xzFromVol = Mathf.Pow(1f / Mathf.Max(1e-4f, yTarget), 0.5f);
            xz_x = Mathf.Lerp(xz_x, xzFromVol, volKeep);
            xz_z = Mathf.Lerp(xz_z, xzFromVol, volKeep);
        }

        // Hysteresis sul twist + suction
        twTargetDeg -= ivr * twistHysteresis * twTargetDeg;
        downTarget -= efill * suctionKick * downTarget;

        // Respirazione
        if (respiratoryModulation)
        {
            float resp01 = (resp * 0.5f) + 0.5f;
            float mod = 1f + respAmp * (resp01 - 0.5f) * 2f;
            yTarget = Mathf.Clamp(yTarget * (1f - respAmp * 0.25f * resp), 0.70f, 1.05f);
            xz_x = Mathf.Clamp(xz_x * mod, 0.90f, 1.15f);
            xz_z = Mathf.Clamp(xz_z * mod, 0.90f, 1.15f);
            twTargetDeg += respTwistBoost * resp;
        }

        // Filtri 2° ordine + micro-noise
        float yF = fY_group.Step(dt, yTarget);
        float xF = fXZ_group.Step(dt, xz_x);
        float zF = fXZ_group.Step(dt, xz_z);           // riuso: stesso filtro per semplicità
        float twF = fTw_group.Step(dt, twTargetDeg);
        float dF = fDown_group.Step(dt, downTarget);

        float tNow = Time.time;
        float nPos = (Mathf.PerlinNoise(11.3f, tNow * noiseFreq + lastBeatSeed) - 0.5f) * 2f * noiseAmpPos;
        float nTw = (Mathf.PerlinNoise(7.9f, tNow * noiseFreq + lastBeatSeed) - 0.5f) * 2f * noiseAmpTwist;

        if (groupedRoot)
        {
            groupedRoot.localScale = new Vector3(group0.scale.x * xF, group0.scale.y * yF, group0.scale.z * zF);
            groupedRoot.localPosition = group0.pos + new Vector3(0f, -dF + nPos, 0f);
            groupedRoot.localRotation = group0.rot * Quaternion.Euler(0f, 0f, twF + nTw);

            float tilt = groupTiltDeg * (twist01 - 0.5f) * 2f * tiltFollow;
            groupedRoot.localRotation *= Quaternion.Euler(tilt, 0f, 0f);
        }

        // --- Atria ---
        if (animateAtria)
        {
            float raPulse = atr * raGain;
            float laPulse = atl * laGain;
            if (rightAtrium) ApplyPart(rightAtrium, ra0, raPulse, atriaScaleYMin, atriaScaleXZMin, atriaDown, atriaTwistDeg, dt);
            if (leftAtrium) ApplyPart(leftAtrium, la0, laPulse, atriaScaleYMin, atriaScaleXZMin, atriaDown, atriaTwistDeg, dt);
        }

        // --- Ventricoli (ritocchi locali, piccoli) ---
        if (animateVentricles)
        {
            float pv = Mathf.Clamp01(vPulse * 0.35f);
            if (rightVentricle)
                ApplyPart(rightVentricle, rv0, pv,
                    Mathf.Lerp(1f, ventScaleYMin, 0.25f),
                    Mathf.Lerp(1f, ventScaleXZMax * (1f - rvOpposesTwist * 0.15f), 0.25f),
                    ventDown * 0.25f, ventTwistDeg * 0.25f, dt);
            if (leftVentricle)
                ApplyPart(leftVentricle, lv0, pv,
                    Mathf.Lerp(1f, ventScaleYMin, 0.25f),
                    Mathf.Lerp(1f, ventScaleXZMax, 0.25f),
                    ventDown * 0.25f, ventTwistDeg * 0.25f, dt);
        }
    }

    // ------ Helpers -------
    void ApplyPart(Transform tr, Pose rest, float p, float yMin, float xzMinOrMax, float dMax, float twDeg, float dt)
    {
        float y = Mathf.Lerp(1f, yMin, p);
        float xz = Mathf.Lerp(1f, xzMinOrMax, p);
        float d = Mathf.Lerp(0f, dMax, p);
        float tw = Mathf.LerpUnclamped(0f, twDeg, p);

        if (volKeep > 0f)
        {
            float xzFromVol = Mathf.Pow(1f / Mathf.Max(1e-4f, y), 0.5f);
            xz = Mathf.Lerp(xz, xzFromVol, volKeep);
        }

        float yF = fY_parts.Step(dt, y);
        float xzF = fXZ_parts.Step(dt, xz);
        float twF = fTw_parts.Step(dt, tw);
        float dF = fDown_parts.Step(dt, d);

        tr.localScale = new Vector3(rest.scale.x * xzF, rest.scale.y * yF, rest.scale.z * xzF);
        tr.localPosition = rest.pos + new Vector3(0f, -dF, 0f);
        tr.localRotation = rest.rot * Quaternion.Euler(0f, 0f, twF);
    }

    // Profilo ventricolare più “miofibrillare”: picco netto in eiezione rapida,
    // plateau breve, rilasciamento ripido, rimbalzo diastolico (E-wave) e quiete (diastasi).
    float VentPulse(float ivc, float ej1, float ej2, float ivr, float efill, float dias)
    {
        float p = 0f;
        p += SmoothRise(ivc) * 0.55f;
        p += SmoothRise(ej1) * 1.05f;
        p += SmoothRise(ej2) * 0.45f;
        p -= SmoothRise(ivr) * 1.00f;
        p -= SmoothRise(efill) * 0.25f; // suction riduce subito la “tensione”
        // NEW: leggerissima asimmetria per battito (più naturale)
        p += (lastBeatSeed - 0.5f) * 0.05f * (ej1 + ej2 - ivr);
        return Mathf.Clamp01(p);
    }

    float WindowedPulse(float ph, float lead, float dur, float easingPow = 1.0f)
    {
        float a = SegFrac(ph, lead, lead + dur);
        float s = Mathf.Sin(a * Mathf.PI);
        return Mathf.Pow(Mathf.Clamp01(s), easingPow);
    }

    float SegFrac(float ph, float a, float b)
    {
        ph = NormPhase(ph); a = NormPhase(a); b = NormPhase(b);
        if (a == b) return 0f;
        if (b < a) b += 1f; if (ph < a) ph += 1f;
        if (ph <= b) return Mathf.Clamp01((ph - a) / Mathf.Max(1e-5f, (b - a)));
        return 0f;
    }
    float Segment(float ph, float a, float b) => SegFrac(ph, a, b) > 0f ? 1f : 0f;
    float NormPhase(float ph) { ph -= Mathf.Floor(ph); return ph; }
    float SmoothRise(float x) => Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(x));
    float SmoothFall(float x) => 1f - SmoothRise(x);

    Pose ReadPose(Transform t) => new Pose { pos = t.localPosition, rot = t.localRotation, scale = t.localScale };

    [ContextMenu("Rebind Rest Poses")]
    void Rebind()
    {
        if (rightAtrium) ra0 = ReadPose(rightAtrium);
        if (leftAtrium) la0 = ReadPose(leftAtrium);
        if (rightVentricle) rv0 = ReadPose(rightVentricle);
        if (leftVentricle) lv0 = ReadPose(leftVentricle);
        if (groupedRoot) group0 = ReadPose(groupedRoot);
    }
}

[System.Serializable]
public struct SecondOrder1D1
{
    public float y, v, wn, zeta, k;
    public SecondOrder1D1(float wn, float zeta, float k) { y = 0f; v = 0f; this.wn = Mathf.Max(1e-3f, wn); this.zeta = Mathf.Max(0.1f, zeta); this.k = k; }
    public float Step(float dt, float x)
    {
        float a = 2f * zeta * wn, b = wn * wn;
        float ydd = (b * (k * x - y)) - (a * v);
        v += ydd * dt; y += v * dt; return y;
    }
}
