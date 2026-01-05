using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CurvedListItem : MonoBehaviour
{
    public HeartPart linkedPart; // trascina parte del cuore qui nell'Inspector
    private Toggle _toggle;

    void Awake()
    {
        HeartUIManager.Instance?.RegisterPart(linkedPart); // registra la parte nel manager
        _toggle = GetComponent<Toggle>(); // se il prefab usa Toggle standard
    }

    // Metodo chiamato da Toggle.onValueChanged (bool)
    public void OnItemToggled(bool isOn)
    {
        if (isOn)
        {
            Debug.Log(name + "Selezionato da bottone");
            HeartUIManager.Instance?.SelectHeartPart(linkedPart);
        }
            
    }

    // Metodo alternativo per eventi senza parametro (OnClick)
    public void OnItemSelected()
    {
        if (_toggle != null) _toggle.isOn = true;
        HeartUIManager.Instance?.SelectHeartPart(linkedPart);
    }
}