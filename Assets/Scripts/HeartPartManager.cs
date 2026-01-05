using System.Collections.Generic;
using UnityEngine;
using TMPro; // NECESSARIO PER IL TESTO

public class HeartUIManager : MonoBehaviour
{
    public static HeartUIManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject descriptionPanel;     // Il pannello di sfondo
    public TextMeshProUGUI descriptionText; // Il componente testo

    [Header("Logic")]
    public List<HeartPart> allParts = new List<HeartPart>();
    public HeartPart currentSelectedPart;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Nascondi il pannello all'avvio
        if (descriptionPanel != null) descriptionPanel.SetActive(false);
    }

    public void RegisterPart(HeartPart p)
    {
        if (!allParts.Contains(p)) allParts.Add(p);
    }

    public void UnregisterPart(HeartPart p)
    {
        if (allParts.Contains(p)) allParts.Remove(p);
    }

    public void SelectHeartPart(HeartPart part)
    {
        currentSelectedPart = part;
        Debug.Log("Selezionato: " + (part ? part.name : "null"));

        if (currentSelectedPart != null)
        {
            // MODIFICA QUI:
            // Appena selezioniamo una parte, mostriamo SUBITO la descrizione
            ShowDescriptionUI(currentSelectedPart.description);
        }
        else
        {
            // Se deselezioniamo (passando null), nascondiamo il pannello
            HideDescriptionUI();
        }
    }

    public void HideSelected()
    {
        if (currentSelectedPart != null)
        {
            currentSelectedPart.ToggleVisibility();
            
        }
    }

    public void IsolateSelected()
    {
        if (currentSelectedPart == null) return;

        foreach (var p in allParts)
            if (p == currentSelectedPart) p.Isolate(); else p.Hide();

        
    }

    public void HighlightSelected()
    {
        if (currentSelectedPart != null)
        {
            currentSelectedPart.ToggleHighlight();

        }
    }

    public void ResetFilters()
    {
        foreach (var p in allParts)
        {
            p.Show();
            p.ResetVisuals();
        }
        // Nascondi descrizione quando resetti tutto
        HideDescriptionUI();
    }

    // --- HELPER UI ---

    private void ShowDescriptionUI(string text)
    {
        if (descriptionPanel != null)
        {
            descriptionPanel.SetActive(true);
            if (descriptionText != null)
                descriptionText.text = text;
        }
    }

    private void HideDescriptionUI()
    {
        if (descriptionPanel != null)
            descriptionPanel.SetActive(false);
    }

    private void UpdateDescriptionUI(string text)
    {
        if (descriptionText != null)
            descriptionText.text = text;
    }
}