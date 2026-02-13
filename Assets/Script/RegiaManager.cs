using UnityEngine;
using System.Collections;

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

    [Header("Stato del Sistema")]
    public bool previewInCorso = false;
    public bool registrazioneInCorso = false;

    // --- NOVITÀ: Array per salvare le texture originali dei mirini ---
    private RenderTexture[] textureMiriniOriginali;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        // Inizializza l'array
        textureMiriniOriginali = new RenderTexture[camereSet.Length];

        // Salviamo i mirini e LASCIAMO ACCESE le camere
        for (int i = 0; i < camereSet.Length; i++)
        {
            if (camereSet[i] != null)
            {
                // Salviamo la RenderTexture che hai messo nell'Inspector (es. Mirino_RT_1)
                textureMiriniOriginali[i] = camereSet[i].targetTexture;
                
                // IMPORTANTE: Le lasciamo accese così gli schermetti funzionano da subito!
                camereSet[i].gameObject.SetActive(true);
                camereSet[i].enabled = true;
            }
        }
    }

    // --- FASE 1: PREVIEW (Monitor Piccolo) ---
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
            // 1. Ripristina i mirini locali su tutte le camere
            for (int i = 0; i < camereSet.Length; i++) 
            { 
                if (camereSet[i] != null) 
                    camereSet[i].targetTexture = textureMiriniOriginali[i]; 
            }

            // 2. La camera corrente manda il suo segnale al Monitor Grande
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

    // --- FASE 2: CIAK FINALE ---
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
        
        if (gestoreRecitazione != null) gestoreRecitazione.AvviaCiakUnico();
        if (mainCameraPlayer) mainCameraPlayer.enabled = false; 

        // CICLO DI REGISTRAZIONE
        for (int i = 0; i < camereSet.Length; i++)
        {
            // A. Assicuriamoci che tutte le altre camere abbiano il loro mirino locale
            for (int j = 0; j < camereSet.Length; j++) 
            { 
                if(camereSet[j] != null) camereSet[j].targetTexture = textureMiriniOriginali[j]; 
            }

            if (camereSet[i] == null) continue;

            // B. LA CAMERA CORRENTE VA A TUTTO SCHERMO
            Camera camAttuale = camereSet[i];
            
            // Stacchiamo la texture, così l'immagine va sul tuo monitor vero!
            camAttuale.targetTexture = null; 
            camAttuale.enabled = true; 

            // Se hai ancora lo script per abbassare gli FPS, disattiviamolo durante il film finale
            OttimizzaCamera optScript = camAttuale.GetComponent<OttimizzaCamera>();
            if (optScript != null) optScript.enabled = false;
            
            if (GameManager.instance != null) ApplicaEffettiScelti(camAttuale);

            yield return new WaitForSeconds(4f); 
        }

        Debug.Log("<color=green>--- STOP! ---</color>");
        
        if (gestoreRecitazione != null) gestoreRecitazione.FermaTutto();
        if (mainCameraPlayer) mainCameraPlayer.enabled = true;

        // RIORDINO FINALE: Rimettiamo le texture ai mirini e riattiviamo l'ottimizzazione
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
        if (GameManager.instance != null && GameManager.instance.fovStandard > 0) 
        {
            switch (GameManager.instance.lenteSceltaFinale)
            {
                case "Grandangolo": camDestinazione.fieldOfView = GameManager.instance.fovGrandangolo; break;
                case "Cinematografica": camDestinazione.fieldOfView = GameManager.instance.fovCinematic; break;
                default: camDestinazione.fieldOfView = GameManager.instance.fovStandard; break;
            }
        }
    }
}