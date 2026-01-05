using UnityEngine;

public class HeartPart : MonoBehaviour
{
    [Header("Data")]
    [TextArea(5, 20)] // Crea un box grande nell'Inspector per incollare il testo
    public string description;

    [Header("Highlight (optional)")]
    public Color highlightColor = Color.yellow;

    private Renderer[] _renderers;
    private MaterialPropertyBlock _mpb;
    public bool IsHighlighted { get; private set; } = false; // Pubblico per leggerlo dal Manager

    void Awake()
    {
        if (_renderers == null)
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
            _mpb = new MaterialPropertyBlock();
        }
    }

    void Start()
    {
        if (HeartUIManager.Instance != null)
            HeartUIManager.Instance.RegisterPart(this);
    }

    public void Show() => gameObject.SetActive(true);

    public void Hide() => gameObject.SetActive(false);

    public void ToggleVisibility() => gameObject.SetActive(!gameObject.activeSelf);

    public void Isolate()
    {
        gameObject.SetActive(true);
        // Quando isoliamo, ci assicuriamo che visivamente sia pulito o evidenziato a scelta
        // Qui lo lasciamo al naturale, ma acceso.
    }

    public void ToggleHighlight()
    {
        if (IsHighlighted)
        {
            ResetVisuals();
        }
        else
        {
            ApplyHighlight();
        }
        // Lo stato viene invertito nelle funzioni Reset/Apply o qui sotto
        // Per sicurezza lo gestiamo qui:
        // (Nota: ApplyHighlight e ResetVisuals gestiranno la grafica, qui gestiamo il booleano logico)
    }

    private void ApplyHighlight()
    {
        foreach (var r in _renderers)
        {
            r.GetPropertyBlock(_mpb);
            Color boosted = highlightColor * 10.0f;
            boosted.a = 1f;
            _mpb.SetColor("_Color", boosted);
            r.SetPropertyBlock(_mpb);
        }
        IsHighlighted = true;
    }

    public void ResetVisuals()
    {
        foreach (var r in _renderers)
        {
            r.GetPropertyBlock(_mpb);
            _mpb.Clear();
            r.SetPropertyBlock(_mpb);
        }
        IsHighlighted = false;
    }
}