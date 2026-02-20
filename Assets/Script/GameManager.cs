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
    public string LuceScelta;
    public string micScelto;
    public bool LucePosizionataCorrettamente = false; 

    [Header("Puzzle Ambientale")]
    public bool rumoreCaffeAttivo = true;
    public AudioSource audioMacchinetta; 

    [Header("Sottotask Audio")]
    public string micDaInstallare = ""; 
    public bool supportoPiazzato = false; 
    public int attoriDaMicrofonare = 2;
    public int attoriMicrofonatiAttuali = 0;

    [Header("Effetti Post-Processing (Wow Factor)")]
    public GameObject volumeGrandangolo;
    public GameObject volumeCinematografica;
    public GameObject volumeDistorta;

    [Header("Parametri Lenti HDRP")]
    public float fovStandard = 60f;
    public float fovGrandangolo = 90f;
    public float fovCinematic = 40f;
    public float fovDistorta = 110f;
    public Volume globalVolume; 
    private LensDistortion distortion;
    public bool cameraPosizionata = false;

    [Header("Parametri Audio Avanzati")]
    public AudioSource[] sorgentiAttori;
    
    [Header("Registro Oggetti Scena")]
    public GameObject[] tuttiGliOggettiRaccoglibili; 
    public GameObject[] supportiLuciFisici; 
    public GameObject[] supportiMicrofoniFisici;

    private class PosizioneOggetto
    {
        public Vector3 posizione;
        public Quaternion rotazione;
    }
    
    private Dictionary<string, PosizioneOggetto> registroPosizioni = new Dictionary<string, PosizioneOggetto>();

    [Header("Gestione Attori")]
    public GameObject gruppoAttoriSala; 
    public GameObject gruppoAttoriSet; 

    void Awake() 
    { 
        if (instance == null) instance = this; 
        else Destroy(gameObject);

        SetupFiltriAudio();
    }

    void Start() 
    {
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

        if (globalVolume != null && globalVolume.profile.TryGet(out distortion))
            Debug.Log("Effetti Lente HDRP pronti.");

        if (gestoreSchermi != null) gestoreSchermi.CambiaStato(true); 

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

    public void MandaAttoriInScena()
    {
        if (gruppoAttoriSala != null) gruppoAttoriSala.SetActive(false);
        if (gruppoAttoriSet != null) gruppoAttoriSet.SetActive(true);
        Debug.Log("[GameManager]: Attori chiamati sul set!");
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
        if (taskAttuale == Reparto.Regia && repartoInteragito != Reparto.Regia)
        {
            Debug.Log($"<color=cyan>[Revisione]: Modifiche a {repartoInteragito}. Torna dal Regista.</color>");
            if (repartoInteragito == Reparto.Fotografia) ResetEffettoLente();
            return; 
        }

        if (repartoInteragito == taskAttuale)
        {
            if (taskAttuale == Reparto.Fotografia) ResetEffettoLente();

            Reparto vecchiaTask = taskAttuale;
            taskAttuale++;

            RadioSistema radio = FindFirstObjectByType<RadioSistema>();
            if (radio != null)
            {
                radio.GestisciCambioTask(vecchiaTask, taskAttuale);
            }

            Debug.Log($"<color=orange>--- BIP! Nuova comunicazione Radio: Task Aggiornata a {taskAttuale} ---</color>");

            if (gestoreSchermi != null)
            {
                gestoreSchermi.CambiaStato(false); 
                if (taskAttuale != Reparto.Finito) Invoke("AttivaAllertaSchermi", 4.0f);
            }
        }
    }
    
    void AttivaAllertaSchermi() { if (gestoreSchermi != null) gestoreSchermi.CambiaStato(true); }

    public void RestituisciOggettoAlTavolo(string nomeOggetto)
    {
        if (string.IsNullOrEmpty(nomeOggetto)) return;
        
        foreach (GameObject obj in tuttiGliOggettiRaccoglibili)
        {
            if (obj != null && obj.name == nomeOggetto)
            {
                obj.SetActive(true);
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
                foreach (Transform figlio in supporto.transform)
                {
                    string nome = figlio.name.ToLower();
                    if ((nome.Contains("fresnel") || nome.Contains("softbox") || nome.Contains("artistica")) && !nome.Contains("anello"))
                    {
                        figlio.gameObject.SetActive(false);
                    }
                }

                var scriptSupporto = supporto.GetComponent<SupportoLuce>(); 
                if (scriptSupporto != null)
                {
                    scriptSupporto.ResettaSupporto(); 
                }
            }
        }
        Debug.Log("[Luci] Supporti puliti visivamente e logicamente.");
    }

    public void ResettaVisualeSupportiMicrofoni()
    {
        if (supportiMicrofoniFisici == null) return;

        foreach (GameObject supporto in supportiMicrofoniFisici)
        {
            if (supporto != null)
            {
                var scriptMic = supporto.GetComponent<SupportoMicrofono>();
                if (scriptMic != null)
                {
                    scriptMic.ResettaSupporto();
                }
            }
        }
        Debug.Log("[Audio] Tutti i supporti microfono sono stati puliti.");
    }

    public void ScegliLuce(string nomeLuce) 
    {
        LuceScelta = nomeLuce;
        LucePosizionataCorrettamente = false; 
    }

public void ApplicaEffettoLente(string nomeLente) 
    {
        if (volumeGrandangolo) volumeGrandangolo.SetActive(false);
        if (volumeCinematografica) volumeCinematografica.SetActive(false);
        if (volumeDistorta) volumeDistorta.SetActive(false);

        switch (nomeLente) 
        {
            case "Grandangolo": 
                if (volumeGrandangolo) volumeGrandangolo.SetActive(true);
                break;
            case "Distorta": 
                if (volumeDistorta) volumeDistorta.SetActive(true);
                break;
            case "Cinematografica": 
                if (volumeCinematografica) volumeCinematografica.SetActive(true);
                break;
        }
    }

    public void ResetEffettoLente() 
    {
        if (volumeGrandangolo) volumeGrandangolo.SetActive(false);
        if (volumeCinematografica) volumeCinematografica.SetActive(false);
        if (volumeDistorta) volumeDistorta.SetActive(false);
    }

    public void ApplicaEffettoMicrofono(string nomeMic) 
    {
        if (sorgentiAttori == null) return;
        
        ResetEffettoAudio();
        Debug.Log($"Applico profilo audio MULTIPLO per: {nomeMic}");

        foreach (AudioSource sorgente in sorgentiAttori)
        {
            if (sorgente == null) continue;

            var highPass = AssicuraComponente<AudioHighPassFilter>(sorgente.gameObject);
            var lowPass = AssicuraComponente<AudioLowPassFilter>(sorgente.gameObject);
            var reverb = AssicuraComponente<AudioReverbFilter>(sorgente.gameObject);

            switch (nomeMic) 
            {
                case "Boom": 
                    highPass.enabled = true; 
                    highPass.cutoffFrequency = 450f;
                    
                    sorgente.spatialBlend = 0.65f;
                    
                    reverb.enabled = true;
                    reverb.reverbPreset = AudioReverbPreset.Room;
                    reverb.room = -1000f;
                    
                    ImpostaVolumeMacchinetta(0.2f);
                    break;

                case "Lavalier": 
                    highPass.enabled = false;
                    lowPass.enabled = false;
                    reverb.enabled = false;
                    
                    sorgente.spatialBlend = 0.2f; 
                    sorgente.volume = 1.0f; 

                    ImpostaVolumeMacchinetta(0.05f);
                    break;

                case "Ambisonic": 
                    highPass.enabled = false; 
                    lowPass.enabled = false;
                    
                    sorgente.spatialBlend = 1.0f; 
                    
                    reverb.enabled = true;
                    reverb.reverbPreset = AudioReverbPreset.Hallway; 
                    reverb.room = -400f; 
                    
                    ImpostaVolumeMacchinetta(1.0f); 
                    break;
            }
        }
    }

    public void ResetEffettoAudio() 
    {
        if (sorgentiAttori == null) return;

        foreach (AudioSource sorgente in sorgentiAttori)
        {
            if (sorgente == null) continue;

            var lp = sorgente.GetComponent<AudioLowPassFilter>();
            if (lp) lp.enabled = false;

            var hp = sorgente.GetComponent<AudioHighPassFilter>();
            if (hp) hp.enabled = false;

            var rv = sorgente.GetComponent<AudioReverbFilter>();
            if (rv) rv.enabled = false;
            
            sorgente.spatialBlend = 1f;
            sorgente.volume = 1f;
        }
        
        ImpostaVolumeMacchinetta(0.08f); 
    }
    
    T AssicuraComponente<T>(GameObject obj) where T : Component
    {
        T comp = obj.GetComponent<T>();
        if (comp == null) comp = obj.AddComponent<T>();
        return comp;
    }

    void ImpostaVolumeMacchinetta(float volumeTarget)
    {
        if (audioMacchinetta != null)
        {
            if (rumoreCaffeAttivo) audioMacchinetta.volume = volumeTarget;
            else audioMacchinetta.volume = 0f;
        }
    }

    void SetupFiltriAudio()
    {
        if (sorgentiAttori != null) 
        {
            foreach (AudioSource s in sorgentiAttori)
            {
                if (s != null)
                {
                    AssicuraComponente<AudioLowPassFilter>(s.gameObject);
                    AssicuraComponente<AudioHighPassFilter>(s.gameObject);
                    AssicuraComponente<AudioReverbFilter>(s.gameObject);
                }
            }
            ResetEffettoAudio();
        }
    }
}