# 🫀 Medical Training VR: Visualizzazione 3D del Cuore

![Unity](https://img.shields.io/badge/Made%20with-Unity-black?style=flat&logo=unity)
![Blender](https://img.shields.io/badge/Modeling-Blender-orange?style=flat&logo=blender)
![VR](https://img.shields.io/badge/Platform-Meta%20Quest-blue?style=flat&logo=meta)

> **Progetto del corso di Computer Graphics** > **Università Politecnica delle Marche** - Ingegneria Informatica e dell'Automazione  
> [cite_start]*Anno Accademico 2024/2025* [cite: 247, 254]

## 📖 Descrizione

Questo progetto è un'applicazione in **Realtà Virtuale (VR)** dedicata alla formazione medica, focalizzata sullo studio dell'anatomia cardiaca. [cite_start]L'obiettivo è superare i limiti della didattica tradizionale offrendo un ambiente immersivo dove studenti e docenti possono interagire con un modello 3D realistico del cuore, osservarne i processi fisiologici e manipolarne le componenti anatomiche [cite: 260-263].

## ✨ Funzionalità Principali

* [cite_start]**Manipolazione Immersiva:** Interazione diretta con il modello 3D (rotazione, traslazione, scalatura) per osservare l'organo da ogni angolazione[cite: 264].
* [cite_start]**Analisi Anatomica:** Possibilità di scomporre il cuore, nascondere, isolare o evidenziare specifiche parti (atri, ventricoli, valvole, arterie, vene)[cite: 265, 321].
* [cite_start]**Simulazione Dinamica Procedurale:** Animazione del ciclo cardiaco (sistole/diastole) generata via codice in tempo reale, simulando contrazione e torsione ventricolare senza l'uso di keyframe statici[cite: 267, 333].
* [cite_start]**Emodinamica Visiva:** Visualizzazione del flusso sanguigno tramite shader personalizzati che reagiscono alla frequenza del battito[cite: 353].
* [cite_start]**Interfaccia Didattica:** UI curva nello spazio (World Space) con descrizioni mediche dettagliate per ogni componente selezionato[cite: 409, 416].

## 🛠️ Tecnologie e Strumenti

* [cite_start]**Engine:** Unity (con Meta XR All-in-one SDK)[cite: 269].
* [cite_start]**Modellazione 3D:** Blender (per partizionamento mesh, cutting planes e organizzazione gerarchica)[cite: 269, 282].
* [cite_start]**Shaders:** HLSL/Cg (per la simulazione del flusso nelle vene)[cite: 360].
* [cite_start]**Hardware Target:** Visori VR compatibili con XR Origin (testato con controller Action-based)[cite: 395].

## 🧠 Dettagli Implementativi

### 1. Modellazione e Gerarchia
Il modello 3D originale è stato partizionato manualmente in Blender per separare le aree anatomiche (caping delle mesh). La struttura gerarchica su Unity è divisa in:
* **RootGrabHeart:** Gestisce la fisica e l'interazione globale.
* [cite_start]**Visuals:** Contiene i gruppi logici (Superficie esterna, Arterie, Vene, Valvole) per la visualizzazione selettiva[cite: 319, 323].

### 2. Animazione Procedurale ("Il Pacemaker Software")
A differenza delle animazioni standard, il battito è governato dallo script `HeartBeat_Grouped`.
* **Logica:** Calcola la deformazione basandosi su fasi normalizzate (contrazione isovolumetrica, eiezione, ecc.).
* **Torsione:** Implementa il *twisting* sistolico e l'*untwisting* diastolico con isteresi meccanica.
* [cite_start]**Sincronizzazione:** Uno script `FollowWithOffset` previene il "mesh tearing" tra le parti (es. arterie che seguono i ventricoli) aggiornando le posizioni nel `LateUpdate`[cite: 333, 341, 350].

### 3. Simulazione Flusso Sanguigno (Shader)
Lo shader `HeartVeinCode` gestisce l'aspetto delle vene:
* **UV Scrolling Dinamico:** La velocità della texture del flusso varia in base all'intensità del battito (più veloce in sistole).
* [cite_start]**Pulsazione:** L'emissione luminosa aumenta in sincronia con la spinta cardiaca tramite lo script `HeartSyncPhysical` [cite: 358-363].

### 4. Gestione UI e Interazione
* **Pattern Singleton:** La classe `HeartUIManager` gestisce la logica centrale.
* **HeartParts:** Ogni componente anatomico si "autoregistra" all'avvio e contiene le proprie informazioni mediche.
* [cite_start]**Rendering:** L'evidenziazione (Highlight) usa `MaterialPropertyBlock` per ottimizzare le performance senza istanziare nuovi materiali[cite: 426, 430].

## 🎮 Guida all'Uso

1.  **Avvio:** Indossare il visore e impugnare i controller.
2.  **Movimento:**
    * Afferrare il cuore con il tasto di presa (Grip) per spostarlo o ruotarlo.
    * [cite_start]Usare i controller per scalare (Zoom in/out) il modello [cite: 449-450].
3.  **Interfaccia Utente:**
    * Puntare il raggio laser sul menu laterale.
    * Selezionare una parte dalla lista (es. "Valvola Mitrale") per leggere la descrizione.
4.  **Azioni Rapide:**
    * **Isola:** Mostra solo la parte selezionata.
    * **Nascondi:** Rimuove la parte per vedere l'interno.
    * **Evidenzia:** Illumina la parte selezionata.
    * [cite_start]**Reset:** Il bottone rosso resetta tutte le visualizzazioni [cite: 474-481].

## 👥 Autori

* **Alessandro Pieragostini**
* **Loris Attili**
* **Sara Beccerica**

---
*Progetto sviluppato per l'esame di Computer Graphics, Facoltà di Ingegneria, Università Politecnica delle Marche.*