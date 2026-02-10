# 🫀 Medical Training VR: Visualizzazione 3D del Cuore

![Unity](https://img.shields.io/badge/Made%20with-Unity-black?style=flat&logo=unity)
![Blender](https://img.shields.io/badge/Modeling-Blender-orange?style=flat&logo=blender)
![VR](https://img.shields.io/badge/Platform-Meta%20Quest-blue?style=flat&logo=meta)

> **Progetto del corso di Computer Graphics** > **Università Politecnica delle Marche** - Ingegneria Informatica e dell'Automazione  
> *Anno Accademico 2024/2025*

## 📖 Descrizione

Questo progetto è un'applicazione in **Realtà Virtuale (VR)** dedicata alla formazione medica, focalizzata sullo studio dell'anatomia cardiaca. L'obiettivo è superare i limiti della didattica tradizionale offrendo un ambiente immersivo dove studenti e docenti possono interagire con un modello 3D realistico del cuore, osservarne i processi fisiologici e manipolarne le componenti anatomiche.

## ✨ Funzionalità Principali

* **Manipolazione Immersiva:** Interazione diretta con il modello 3D (rotazione, traslazione, scalatura) per osservare l'organo da ogni angolazione.
* **Analisi Anatomica:** Possibilità di scomporre il cuore, nascondere, isolare o evidenziare specifiche parti (atri, ventricoli, valvole, arterie, vene).
* **Simulazione Dinamica Procedurale:** Animazione del ciclo cardiaco (sistole/diastole) generata via codice in tempo reale, simulando contrazione e torsione ventricolare senza l'uso di keyframe statici.
* **Emodinamica Visiva:** Visualizzazione del flusso sanguigno tramite shader personalizzati che reagiscono alla frequenza del battito.
* **Interfaccia Didattica:** UI curva nello spazio (World Space) con descrizioni mediche dettagliate per ogni componente selezionato.

## 🛠️ Tecnologie e Strumenti

* **Engine:** Unity (con Meta XR All-in-one SDK).
* **Modellazione 3D:** Blender (per partizionamento mesh, cutting planes e organizzazione gerarchica).
* **Shaders:** HLSL/Cg (per la simulazione del flusso nelle vene).
* **Hardware Target:** Visori VR compatibili con XR Origin (testato con controller Action-based).

## 🧠 Dettagli Implementativi

### 1. Modellazione e Gerarchia
Il modello 3D originale è stato partizionato manualmente in Blender per separare le aree anatomiche (caping delle mesh). La struttura gerarchica su Unity è divisa in:
* **RootGrabHeart:** Gestisce la fisica e l'interazione globale.
* **Visuals:** Contiene i gruppi logici (Superficie esterna, Arterie, Vene, Valvole) per la visualizzazione selettiva.

### 2. Animazione Procedurale ("Il Pacemaker Software")
A differenza delle animazioni standard, il battito è governato dallo script `HeartBeat_Grouped`.
* **Logica:** Calcola la deformazione basandosi su fasi normalizzate (contrazione isovolumetrica, eiezione, ecc.).
* **Torsione:** Implementa il *twisting* sistolico e l'*untwisting* diastolico con isteresi meccanica.
* **Sincronizzazione:** Uno script `FollowWithOffset` previene il "mesh tearing" tra le parti (es. arterie che seguono i ventricoli) aggiornando le posizioni nel `LateUpdate`.

### 3. Simulazione Flusso Sanguigno (Shader)
Lo shader `HeartVeinCode` gestisce l'aspetto delle vene:
* **UV Scrolling Dinamico:** La velocità della texture del flusso varia in base all'intensità del battito (più veloce in sistole).
* **Pulsazione:** L'emissione luminosa aumenta in sincronia con la spinta cardiaca tramite lo script `HeartSyncPhysical`.

### 4. Gestione UI e Interazione
* **Pattern Singleton:** La classe `HeartUIManager` gestisce la logica centrale.
* **HeartParts:** Ogni componente anatomico si "autoregistra" all'avvio e contiene le proprie informazioni mediche.
* **Rendering:** L'evidenziazione (Highlight) usa `MaterialPropertyBlock` per ottimizzare le performance senza istanziare nuovi materiali.

## 🎮 Guida all'Uso

1.  **Avvio:** Indossare il visore e impugnare i controller.
2.  **Movimento:**
    * Afferrare il cuore con il tasto di presa (Grip) per spostarlo o ruotarlo.
    * Usare i controller per scalare (Zoom in/out) il modello.
3.  **Interfaccia Utente:**
    * Puntare il raggio laser sul menu laterale.
    * Selezionare una parte dalla lista (es. "Valvola Mitrale") per leggere la descrizione.
4.  **Azioni Rapide:**
    * **Isola:** Mostra solo la parte selezionata.
    * **Nascondi:** Rimuove la parte per vedere l'interno.
    * **Evidenzia:** Illumina la parte selezionata.
    * **Reset:** Il bottone rosso resetta tutte le visualizzazioni.

## 👥 Autori

* **Alessandro Pieragostini**
* **Loris Attili**
* **Sara Beccerica**

---
*Progetto sviluppato per l'esame di Computer Graphics, Facoltà di Ingegneria, Università Politecnica delle Marche.*
