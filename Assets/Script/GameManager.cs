using UnityEngine;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public enum Reparto { Produzione, Fotografia, Luci, Fonico, Regia, Attori, Finito }
    public Reparto taskAttuale = Reparto.Produzione;

    [Header("UI Obiettivi")]
    public TextMeshProUGUI visualizzatoreObiettivo;

    [Header("Collegamenti Grafici")]
    public GestoreSchermi gestoreSchermi;

    [Header("Salvataggio Scelte")]
    public string lenteSceltaFinale;
    public string luceScelta;
    public string micScelto;

    [Header("Puzzle Ambientale")]
    public bool rumoreCaffeAttivo = true;
    public AudioSource audioMacchinetta;

    [Header("Sottotask Audio")]
    public string micDaInstallare = ""; 
    public bool supportoPiazzato = false; 
    public int attoriDaMicrofonare = 2;
    public int attoriMicrofonatiAttuali = 0;

    [Header("Parametri Lenti HDRP")]
    public float fovStandard = 60f;
    public float fovGrandangolo = 90f;
    public float fovCinematic = 40f;
    public Volume globalVolume; 
    private LensDistortion distortion;
    public bool cameraPosizionata = false;

    [Header("Parametri Audio")]
    public AudioSource sorgenteAttori; 
    private AudioLowPassFilter lowPass;
    private AudioHighPassFilter highPass;

    [Header("Stato Task Luci")]
    public string LuceScelta = ""; 
    public bool LucePosizionataCorrettamente = false; 

    // --- SISTEMA DI RESTITUZIONE E PULIZIA ---
    [Header("Registro Oggetti Scena")]
    public GameObject[] tuttiGliOggettiRaccoglibili; 
    public GameObject[] supportiLuciFisici; // I treppiedi delle luci nella scena

    private class PosizioneOggetto
    {
        public Vector3 posizione;
        public Quaternion rotazione;
    }
    
    private Dictionary<string, PosizioneOggetto> registroPosizioni = new Dictionary<string, PosizioneOggetto>();

    [Header("Gestione Attori")]
    public GameObject gruppoAttoriSala; // Trascina qui il padre degli attori in sala
    public GameObject gruppoAttoriSet;  // Trascina qui il padre degli attori sul set

    void Awake() 
    { 
        if (instance == null) instance = this; 
        else Destroy(gameObject);

        SetupFiltriAudio();
    }

    void Start() 
    {
        // 1. Setup Dizionario Posizioni
        foreach (GameObject obj in tuttiGliOggettiRaccoglibili)
        {
            if (obj != null)
            {
                PosizioneOggetto pos = new PosizioneOggetto();
                pos.posizione = obj.transform.position;
                pos.rotazione = obj.transform.rotation;
                if (!registroPosizioni.ContainsKey(obj.name))
                    registroPosizioni.Add(obj.name, pos);
            }
        }

        // 2. Setup HDRP
        if (globalVolume != null && globalVolume.profile.TryGet(out distortion))
            Debug.Log("Effetti Lente HDRP pronti.");

        // 3. Setup Schermi UI
        if (gestoreSchermi != null) gestoreSchermi.CambiaStato(true); 

        // 4. STATO INIZIALE ATTORI
        if (gruppoAttoriSala != null) gruppoAttoriSala.SetActive(true);
        if (gruppoAttoriSet != null) gruppoAttoriSet.SetActive(false);
    }

    void Update()
    {
        if (visualizzatoreObiettivo != null)
        {
            string testoObiettivo = (taskAttuale != Reparto.Finito) ? "Obiettivo: Vai da " + taskAttuale : "Giornata finita!";
            if (visualizzatoreObiettivo.text != testoObiettivo)
                visualizzatoreObiettivo.text = testoObiettivo;
        }
    }

    // --- NUOVA FUNZIONE PER ATTORI ---
    public void MandaAttoriInScena()
    {
        if (gruppoAttoriSala != null) gruppoAttoriSala.SetActive(false); // Nascondi sala
        if (gruppoAttoriSet != null) gruppoAttoriSet.SetActive(true);    // Mostra set
        
        Debug.Log("[GameManager]: Attori chiamati sul set!");
    }

    // --- LOGICA RADIO ---
    public string OttieniSuggerimentoRadio()
    {
        switch (taskAttuale)
        {
            case Reparto.Produzione: return "Vai in Produzione per recuperare la radio.";
            case Reparto.Fotografia: return "Prendi una lente e portala al Direttore della Fotografia.";
            case Reparto.Luci: return "L'addetto all'illuminazione del set ti aspetta!.";
            case Reparto.Fonico: 
                if (micDaInstallare == "Lavalier") return $"Devi ancora microfonare {attoriDaMicrofonare - attoriMicrofonatiAttuali} attori.";
                if (micDaInstallare == "Boom") return "Posiziona l'asta del Boom davanti agli attori.";
                if (micDaInstallare == "Ambisonic") return "Posiziona il treppiede Ambisonic al centro del set.";
                return "Porta il microfono scelto al fonico.";
            case Reparto.Regia: return "Il Regista ti aspetta.";
            case Reparto.Attori: return "Tutto pronto! Chiama gli attori.";
            default: return "Fine giornata.";
        }
    }

    public void CompletaTask(Reparto repartoInteragito)
    {
        // CASO 1: REVISIONE
        if (taskAttuale == Reparto.Regia && repartoInteragito != Reparto.Regia)
        {
            Debug.Log($"<color=cyan>[Revisione]: Modifiche a {repartoInteragito}. Torna dal Regista.</color>");
            if (repartoInteragito == Reparto.Fotografia) ResetEffettoLente();
            return; 
        }

        // CASO 2: FLUSSO NORMALE
        if (repartoInteragito == taskAttuale)
        {
            if (taskAttuale == Reparto.Fotografia) ResetEffettoLente();

            // --- AUDIO AUTOMATICO (MODIFICATO PER PRE/POST TASK) ---
            
            // 1. Memorizza la task vecchia (quella che stiamo chiudendo)
            Reparto vecchiaTask = taskAttuale;

            // 2. Passa alla task nuova
            taskAttuale++;

            // 3. Chiama la Radio passando ENTRAMBE le fasi per gestire la transizione audio
            RadioSistema radio = FindFirstObjectByType<RadioSistema>();
            if (radio != null)
            {
                // Usa la NUOVA funzione creata nello step precedente
                radio.GestisciCambioTask(vecchiaTask, taskAttuale);
            }
            // -------------------------------------------------------

            Debug.Log($"<color=orange>--- BIP! Nuova comunicazione Radio: Task Aggiornata a {taskAttuale} ---</color>");

            if (gestoreSchermi != null)
            {
                gestoreSchermi.CambiaStato(false); 
                if (taskAttuale != Reparto.Finito) Invoke("AttivaAllertaSchermi", 4.0f);
            }
        }
    }
    
    void AttivaAllertaSchermi() { if (gestoreSchermi != null) gestoreSchermi.CambiaStato(true); }

    // --- GESTIONE OGGETTI ---
    public void RestituisciOggettoAlTavolo(string nomeOggetto)
    {
        if (string.IsNullOrEmpty(nomeOggetto)) return;
        
        foreach (GameObject obj in tuttiGliOggettiRaccoglibili)
        {
            if (obj != null && obj.name == nomeOggetto)
            {
                obj.SetActive(true); // Riaccende l'oggetto
                if (registroPosizioni.ContainsKey(nomeOggetto))
                {
                    obj.transform.position = registroPosizioni[nomeOggetto].posizione;
                    obj.transform.rotation = registroPosizioni[nomeOggetto].rotazione;
                }
                Debug.Log($"<color=cyan>[Inventario]</color> Restituito {nomeOggetto} al tavolo.");
                return;
            }
        }
    }

    public void ResettaVisualeSupportiLuci()
    {
        if (supportiLuciFisici == null) return;

        foreach (GameObject supporto in supportiLuciFisici)
        {
            if (supporto != null)
            {
                // 1. RESET VISIVO
                foreach (Transform figlio in supporto.transform)
                {
                    string nome = figlio.name.ToLower();
                    if ((nome.Contains("fresnel") || nome.Contains("softbox") || nome.Contains("artistica")) && !nome.Contains("anello"))
                    {
                        figlio.gameObject.SetActive(false);
                    }
                }

                // 2. RESET LOGICO
                var scriptSupporto = supporto.GetComponent<SupportoLuce>(); 
                if (scriptSupporto != null)
                {
                    scriptSupporto.ResettaSupporto(); 
                }
            }
        }
        Debug.Log("[Luci] Supporti puliti visivamente e logicamente.");
    }

    public void ScegliLuce(string nomeLuce) 
    {
        LuceScelta = nomeLuce;
        LucePosizionataCorrettamente = false; 
    }

    // --- EFFETTI AUDIO/VIDEO ---
    public void ApplicaEffettoLente(string nomeLente) 
    {
        Camera cam = Camera.main;
        if (cam == null || distortion == null) return;
        switch (nomeLente) 
        {
            case "Grandangolo": cam.fieldOfView = fovGrandangolo; distortion.intensity.value = -0.3f; break;
            case "Distorta": cam.fieldOfView = fovStandard; distortion.intensity.value = 0.6f; break;
            case "Cinematografica": cam.fieldOfView = fovCinematic; distortion.intensity.value = 0f; break;
            default: cam.fieldOfView = fovStandard; distortion.intensity.value = 0f; break;
        }
    }

    public void ResetEffettoLente() {
        if (Camera.main != null) Camera.main.fieldOfView = fovStandard;
        if (distortion != null) distortion.intensity.value = 0f;
    }

    public void ApplicaEffettoMicrofono(string nomeMic) 
    {
        if (sorgenteAttori == null) return;
        if (lowPass == null || highPass == null) SetupFiltriAudio();
        float volumeRumoreFondo = 0f;

        switch (nomeMic) 
        {
            case "Boom": 
                if (lowPass) lowPass.enabled = false; 
                if (highPass) highPass.enabled = false; 
                sorgenteAttori.spatialBlend = 0.5f; 
                volumeRumoreFondo = 0.1f; 
                break;
            case "Lavalier": 
                if (lowPass) { lowPass.enabled = true; lowPass.cutoffFrequency = 4000f; }
                if (highPass) { highPass.enabled = true; highPass.cutoffFrequency = 300f; }
                sorgenteAttori.spatialBlend = 0.2f; 
                volumeRumoreFondo = 0.05f; 
                break;
            case "Ambisonic": 
                if (lowPass) lowPass.enabled = false; 
                if (highPass) { highPass.enabled = true; highPass.cutoffFrequency = 100f; }
                sorgenteAttori.spatialBlend = 1.0f; 
                volumeRumoreFondo = 1.0f; 
                break;
        }

        if (audioMacchinetta != null)
        {
            if (rumoreCaffeAttivo) audioMacchinetta.volume = volumeRumoreFondo;
            else audioMacchinetta.volume = 0f;
        }
    }

    public void ResetEffettoAudio() 
    {
        if (lowPass) lowPass.enabled = false;
        if (highPass) highPass.enabled = false;
        if (sorgenteAttori) sorgenteAttori.spatialBlend = 1f;
        if (audioMacchinetta != null && rumoreCaffeAttivo) audioMacchinetta.volume = 0.5f; 
    }
    
    void SetupFiltriAudio()
    {
         if (sorgenteAttori != null) 
        {
            lowPass = sorgenteAttori.gameObject.GetComponent<AudioLowPassFilter>();
            if (lowPass == null) lowPass = sorgenteAttori.gameObject.AddComponent<AudioLowPassFilter>();
            highPass = sorgenteAttori.gameObject.GetComponent<AudioHighPassFilter>();
            if (highPass == null) highPass = sorgenteAttori.gameObject.AddComponent<AudioHighPassFilter>();
            ResetEffettoAudio();
        }
    }
}