using UnityEngine;

public class HeartSyncPhysical : MonoBehaviour
{
    [Header("Collegamenti")]
    public Renderer heartRenderer; // Trascina qui l'oggetto che ha il materiale delle vene
    public Transform beatSource;   // Trascina qui l'oggetto che si ingrandisce/rimpicciolisce (il Leader o se stesso)

    [Header("Calibrazione Battito")]
    [Tooltip("La scala del cuore quando è al minimo (contrazione massima)")]
    public float minScale = 0.8f;

    [Tooltip("La scala del cuore quando è al massimo (rilassamento massimo)")]
    public float maxScale = 1.2f;

    [Header("Reazione Luce")]
    [Tooltip("Se vero: Più è PICCOLO il cuore, più luce fa (Realistico: il sangue viene spremuto).")]
    public bool lightOnContraction = true;
    public float maxIntensity = 5.0f; // Quanta luce fa al picco

    // Nomi interni dello shader
    private int intensityID;

    void Start()
    {
        // Ottimizzazione: prendiamo l'ID della proprietà una volta sola
        intensityID = Shader.PropertyToID("_PulseIntensity");

        if (heartRenderer == null) heartRenderer = GetComponent<Renderer>();
        if (beatSource == null) beatSource = transform;
    }

    void Update()
    {
        if (beatSource == null || heartRenderer == null) return;

        // 1. Leggiamo la grandezza attuale del cuore (usiamo la media di X, Y, Z o solo X)
        float currentScale = beatSource.localScale.x;

        // 2. Calcoliamo una percentuale (da 0 a 1) basata su dove ci troviamo tra Min e Max
        // Mathf.InverseLerp restituisce 0 se siamo a minScale, 1 se siamo a maxScale
        float t = Mathf.InverseLerp(minScale, maxScale, currentScale);

        // 3. Decidiamo l'intensità della luce
        float finalLight = 0f;

        if (lightOnContraction)
        {
            // Se il cuore è PICCOLO (t vicino a 0), luce FORTE (1)
            // Usiamo (1 - t) per invertire
            finalLight = (1f - t) * maxIntensity;
        }
        else
        {
            // Se il cuore è GRANDE (t vicino a 1), luce FORTE
            finalLight = t * maxIntensity;
        }

        // 4. Mandiamo il valore allo shader
        // Usiamo PropertyBlock per performance migliore, ma qui va bene anche direct access per semplicità
        heartRenderer.material.SetFloat(intensityID, finalLight);
    }
}