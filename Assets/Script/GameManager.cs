using UnityEngine;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public enum Reparto { Produzione, Fotografia, Luci, Fonico, Regia, Attori, Finito }
    public Reparto taskAttuale = Reparto.Produzione;

    public TextMeshProUGUI visualizzatoreObiettivo;

    [Header("Salvataggio Scelte")]
    public string lenteSceltaFinale;
    public string luceScelta;
    public string micScelto;

    [Header("Puzzle Ambientale (Macchinetta Caffè)")]
    public bool rumoreCaffeAttivo = true;
    public AudioSource audioMacchinetta;

    [Header("Sottotask Audio (Installazione)")]
    public string micDaInstallare = ""; 
    public bool supportoPiazzato = false; 
    public int attoriDaMicrofonare = 3;
    public int attoriMicrofonatiAttuali = 0;

    [Header("Parametri Lenti HDRP")]
    public float fovStandard = 60f;
    public float fovGrandangolo = 90f;
    public float fovCinematic = 40f;
    public Volume globalVolume; 
    private LensDistortion distortion;

    [Header("Parametri Audio (Microfoni Attori)")]
    public AudioSource sorgenteAttori; 
    private AudioLowPassFilter lowPass;
    private AudioHighPassFilter highPass;

    [Header("Stato Task Luci")]
    public string LuceScelta = ""; // Diventa "Fresnel", "Softbox", etc. quando clicchi sulla UI
    public bool LucePosizionataCorrettamente = false; // Diventa TRUE solo dopo averla messa sul supporto

    // Funzione chiamata dai pulsanti della UI (Scelta Luce)
    public void ScegliLuce(string nomeLuce)
    {
        LuceScelta = nomeLuce;
        LucePosizionataCorrettamente = false; // Reset nel caso cambi idea
        Debug.Log("Hai preso la luce: " + nomeLuce + ". Ora vai a montarla!");
    }

    void Awake() 
    { 
        if (instance == null) instance = this; 
        SetupFiltriAudio();
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

    void Start() 
    {
        if (globalVolume != null && globalVolume.profile.TryGet(out distortion))
            Debug.Log("Effetti Lente HDRP pronti.");
    }

    void Update()
    {
        if (visualizzatoreObiettivo != null)
        {
            visualizzatoreObiettivo.text = (taskAttuale != Reparto.Finito) ? "Obiettivo: Vai da " + taskAttuale : "Giornata finita!";
        }
    }

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
        if (repartoInteragito == taskAttuale)
        {
            if (taskAttuale == Reparto.Fotografia) ResetEffettoLente();
            taskAttuale++;
            Debug.Log("<color=orange>--- BIP! Nuova comunicazione Radio (R) ---</color>");
        }
    }

    // --- SEZIONE VISIVA (LENTI) ---
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

    // --- SEZIONE AUDIO (MICROFONI & AMBIENTE) ---
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
                
                // Il Boom è direzionale, abbatte molto il rumore di fondo
                volumeRumoreFondo = 0.1f; 
                break;

            case "Lavalier": 
                if (lowPass) { lowPass.enabled = true; lowPass.cutoffFrequency = 4000f; }
                if (highPass) { highPass.enabled = true; highPass.cutoffFrequency = 300f; }
                sorgenteAttori.spatialBlend = 0.2f; 
                
                // Il Lavalier è vicinissimo alla bocca, sente pochissimo il fondo
                volumeRumoreFondo = 0.05f; 
                break;

            case "Ambisonic": 
                if (lowPass) lowPass.enabled = false; 
                if (highPass) { highPass.enabled = true; highPass.cutoffFrequency = 100f; }
                sorgenteAttori.spatialBlend = 1.0f; 
                
                // L'Ambisonic è omnidirezionale, cattura TUTTO il rumore
                volumeRumoreFondo = 1.0f; 
                break;
        }

        // --- GESTIONE VOLUME MACCHINETTA CAFFÈ ---
        if (audioMacchinetta != null)
        {
            if (rumoreCaffeAttivo)
                audioMacchinetta.volume = volumeRumoreFondo;
            else
                audioMacchinetta.volume = 0f;
        }

        Debug.Log($"Effetto Audio Applicato: {nomeMic} (Rumore Fondo: {volumeRumoreFondo * 100}%)");
    }

    public void ResetEffettoAudio() 
    {
        if (lowPass) lowPass.enabled = false;
        if (highPass) highPass.enabled = false;
        if (sorgenteAttori) sorgenteAttori.spatialBlend = 1f;

        if (audioMacchinetta != null && rumoreCaffeAttivo) 
            audioMacchinetta.volume = 0.5f; 
    }
}