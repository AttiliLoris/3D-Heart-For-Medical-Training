using UnityEngine;

public class HeartBeatController : MonoBehaviour
{
    [Header("Impostazioni")]
    public Material veinMaterial; // Qui trascinerai il materiale delle vene
    public float bpm = 60f;       // Battiti per minuto
    public float intensity = 5f;  // Quanto forte si illumina la luce

    [Header("Curva del Battito")]
    // Questo disegna il "ritmo" (es. Tum-Tum... pausa)
    public AnimationCurve beatCurve = new AnimationCurve(
        new Keyframe(0, 0),    // Inizio spento
        new Keyframe(0.1f, 1), // Picco rapido (Tum!)
        new Keyframe(0.3f, 0), // Spegnimento
        new Keyframe(0.4f, 0.5f), // Secondo picco più basso (tum)
        new Keyframe(0.6f, 0)  // Pausa
    );

    private float timer;

    void Update()
    {
        // 1. Calcola il tempo di un singolo battito
        float beatDuration = 60f / bpm;

        // 2. Aggiorna il timer
        timer += Time.deltaTime;
        if (timer >= beatDuration) timer -= beatDuration; // Reset del ciclo

        // 3. Leggi il valore dalla curva in questo momento preciso
        float currentStrength = beatCurve.Evaluate(timer / beatDuration);

        // 4. Manda il valore al Materiale ("_PulseIntensity" è il nome nello Shader)
        if (veinMaterial != null)
        {
            veinMaterial.SetFloat("_PulseIntensity", currentStrength * intensity);
        }
    }
}