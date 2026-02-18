using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Fondamentale per usare le List<>

public class RegiaManager : MonoBehaviour
{
    public static RegiaManager instance;

    [Header("Collegamenti Esterni")]
    public GestoreFinale gestoreFinale; 
    public GestoreRecitazione gestoreRecitazione; 

    [Header("Setup Player")]
    public Camera mainCameraPlayer;   
    public GameObject monitorSchermo; 
    public RenderTexture textureMonitor; 

    [Header("Camere del Set")]
    public Camera[] camereSet; 

    [Header("Effetti Post-Processing (Wow Factor)")]
    public GameObject volumeGrandangolo;
    public GameObject volumeCinematografica;
    public GameObject volumeDistorta;

    // --- NUOVA SEZIONE: IL GRANDE CIAK ---
    [Header("Sequenza Finale (Luci e Audio)")]
    [Tooltip("Trascina qui tutti i GameObjects dei faretti sul soffitto (così spegniamo sia la luce che il materiale emissivo)")]
    public GameObject[] luciGeneraliCapannone; 
    public AudioSource audioSourceRegia;
    public AudioClip suonoBlackout;
    public AudioClip suonoAccensioneFaro;
    public AudioClip suonoCiak;

    [Header("Stato del Sistema")]
    public bool previewInCorso = false;
    public bool registrazioneInCorso = false;

    private RenderTexture[] textureMiriniOriginali;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        textureMiriniOriginali = new RenderTexture[camereSet.Length];

        // Creiamo un AudioSource se ti dimentichi di assegnarlo
        if (audioSourceRegia == null) audioSourceRegia = gameObject.AddComponent<AudioSource>();

        for (int i = 0; i < camereSet.Length; i++)
        {
            if (camereSet[i] != null)
            {
                textureMiriniOriginali[i] = camereSet[i].targetTexture;
                camereSet[i].gameObject.SetActive(true);
                camereSet[i].enabled = true;
            }
        }
    }

    public void AttivaPreview()
    {
        if (camereSet == null || camereSet.Length == 0) return;
        if (previewInCorso || registrazioneInCorso) return;
        
        previewInCorso = true;
        if (monitorSchermo) monitorSchermo.SetActive(true);
        
        if (gestoreRecitazione != null) gestoreRecitazione.AvviaLoopRecitazione();

        StartCoroutine(CicloPreviewMonitor());
    }

    IEnumerator CicloPreviewMonitor()
    {
        int indiceCam = 0;
        
        while (!registrazioneInCorso)
        {
            for (int i = 0; i < camereSet.Length; i++) 
            { 
                if (camereSet[i] != null) 
                    camereSet[i].targetTexture = textureMiriniOriginali[i]; 
            }

            if (camereSet[indiceCam] != null)
            {
                camereSet[indiceCam].targetTexture = textureMonitor;
                ApplicaEffettiScelti(camereSet[indiceCam]);
            }

            yield return new WaitForSeconds(2f);
            
            indiceCam++;
            if (indiceCam >= camereSet.Length) indiceCam = 0;
        }
    }

    public void AvviaCiak()
    {
        if (registrazioneInCorso) return;

        registrazioneInCorso = true;
        previewInCorso = false; 
        StopAllCoroutines(); 

        StartCoroutine(SequenzaRegistrazione());
    }

    IEnumerator SequenzaRegistrazione()
    {
        Debug.Log("<color=red>--- REC: INIZIO REGISTRAZIONE ---</color>");

        if (camereSet == null || camereSet.Length == 0) yield break;
        
        // RIMOSSO LO SPEGNIMENTO DELLA CAMERA QUI! 
        // Il giocatore resta attivo per vedere il blackout con i suoi occhi.

        // 1. CERCHIAMO LE LUCI PIAZZATE DAL GIOCATORE
        List<Light> luciDelSet = new List<Light>();
        if (GameManager.instance != null && GameManager.instance.supportiLuciFisici != null)
        {
            foreach (GameObject supporto in GameManager.instance.supportiLuciFisici)
            {
                SupportoLuce scriptLuce = supporto.GetComponent<SupportoLuce>();
                if (scriptLuce != null && scriptLuce.luceGiaPosizionata)
                {
                    Light[] luci = supporto.GetComponentsInChildren<Light>(false); 
                    luciDelSet.AddRange(luci);
                }
            }
        }

        // 2. IL BLACKOUT (Tonfo sordo e buio totale vissuto in prima persona!)
        if (audioSourceRegia != null && suonoBlackout != null) audioSourceRegia.PlayOneShot(suonoBlackout);
        
        foreach (GameObject luceCap in luciGeneraliCapannone) { if (luceCap != null) luceCap.SetActive(false); }
        foreach (Light l in luciDelSet) { if (l != null) l.enabled = false; }

        yield return new WaitForSeconds(2.0f);

        // 3. ACCENSIONE PROGRESSIVA DEI FARI (Sempre visto dal giocatore)
        foreach (Light l in luciDelSet)
        {
            if (l != null)
            {
                l.enabled = true;
                if (audioSourceRegia != null && suonoAccensioneFaro != null) audioSourceRegia.PlayOneShot(suonoAccensioneFaro);
                yield return new WaitForSeconds(0.8f); 
            }
        }

        yield return new WaitForSeconds(1.0f);

        // 4. IL CIAK!
        if (audioSourceRegia != null && suonoCiak != null) audioSourceRegia.PlayOneShot(suonoCiak);
        yield return new WaitForSeconds(1.5f); 

        // 5. AZIONE! (Partono gli attori)
        if (gestoreRecitazione != null) gestoreRecitazione.AvviaCiakUnico();

        // -> SPEGNIAMO GLI OCCHI DEL GIOCATORE SOLO ORA CHE PARTONO LE CAMERE! <-
        if (mainCameraPlayer) mainCameraPlayer.enabled = false; 

        // 6. CAMBIO CAMERE DURANTE LA SCENA (Visione da Regista)
        for (int i = 0; i < camereSet.Length; i++)
        {
            for (int j = 0; j < camereSet.Length; j++) 
            { 
                if(camereSet[j] != null) camereSet[j].targetTexture = textureMiriniOriginali[j]; 
            }

            if (camereSet[i] == null) continue;

            Camera camAttuale = camereSet[i];
            
            camAttuale.targetTexture = null; // Manda il flusso allo schermo intero
            camAttuale.enabled = true; 

            OttimizzaCamera optScript = camAttuale.GetComponent<OttimizzaCamera>();
            if (optScript != null) optScript.enabled = false;
            
            if (GameManager.instance != null) ApplicaEffettiScelti(camAttuale);

            yield return new WaitForSeconds(4f); 
        }

        Debug.Log("<color=green>--- STOP! ---</color>");
        
        // 7. FINE SCENA E TITOLI DI CODA
        if (gestoreRecitazione != null) gestoreRecitazione.FermaTutto();
        if (mainCameraPlayer) mainCameraPlayer.enabled = true; // Riaccende gli occhi del giocatore

        for (int i = 0; i < camereSet.Length; i++) 
        {
            if(camereSet[i] != null) 
            {
                camereSet[i].targetTexture = textureMiriniOriginali[i];
                OttimizzaCamera opt = camereSet[i].GetComponent<OttimizzaCamera>();
                if(opt) opt.enabled = true;
            }
        }
        
        if (GameManager.instance != null) GameManager.instance.CompletaTask(GameManager.Reparto.Regia);
        if (gestoreFinale != null) gestoreFinale.AvviaTitoliDiCoda();
    }

    void ApplicaEffettiScelti(Camera camDestinazione)
    {
        if (volumeGrandangolo) volumeGrandangolo.SetActive(false);
        if (volumeCinematografica) volumeCinematografica.SetActive(false);
        if (volumeDistorta) volumeDistorta.SetActive(false);

        if (GameManager.instance != null && GameManager.instance.fovStandard > 0) 
        {
            switch (GameManager.instance.lenteSceltaFinale)
            {
                case "Grandangolo": 
                    camDestinazione.fieldOfView = GameManager.instance.fovGrandangolo; 
                    if (volumeGrandangolo) volumeGrandangolo.SetActive(true);
                    break;
                case "Cinematografica": 
                    camDestinazione.fieldOfView = GameManager.instance.fovCinematic; 
                    if (volumeCinematografica) volumeCinematografica.SetActive(true);
                    break;
                case "Distorta": 
                    camDestinazione.fieldOfView = GameManager.instance.fovDistorta; 
                    if (volumeDistorta) volumeDistorta.SetActive(true);
                    break;
                default: 
                    camDestinazione.fieldOfView = GameManager.instance.fovStandard; 
                    break;
            }
        }
    }
}