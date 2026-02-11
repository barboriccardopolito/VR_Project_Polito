using UnityEngine;
using System.Collections;

public class RegiaManager : MonoBehaviour
{
    public static RegiaManager instance;

    [Header("Collegamenti Esterni")]
    public GestoreFinale gestoreFinale; // <--- FONDAMENTALE: Trascina qui lo script dei titoli!

    [Header("Setup Player")]
    public Camera mainCameraPlayer;   // La telecamera del giocatore
    public GameObject monitorSchermo; // Il piccolo monitor per la preview
    public RenderTexture textureMonitor; // La texture del monitor piccolo

    [Header("Camere del Set")]
    public Camera[] camereSet; // Le varie telecamere posizionate nella scena

    [Header("Stato del Sistema")]
    public bool previewInCorso = false;
    public bool registrazioneInCorso = false;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        // Spegniamo tutte le camere del set all'inizio
        foreach (Camera cam in camereSet)
        {
            cam.gameObject.SetActive(false);
            cam.targetTexture = null; 
        }
    }

    // --- FASE 1: PREVIEW (Monitor Piccolo) ---
    // Funziona finché non avvii il Ciak
    public void AttivaPreview()
    {
        if (camereSet == null || camereSet.Length == 0) return;
        if (previewInCorso || registrazioneInCorso) return;
        
        previewInCorso = true;
        if (monitorSchermo) monitorSchermo.SetActive(true);
        
        StartCoroutine(CicloPreviewMonitor());
    }

    IEnumerator CicloPreviewMonitor()
    {
        int indiceCam = 0;
        
        while (!registrazioneInCorso)
        {
            // Reset: spegni tutte
            foreach (var cam in camereSet) { cam.targetTexture = null; cam.gameObject.SetActive(false); }

            // Accendi solo quella corrente e mandala sul Monitor Piccolo
            camereSet[indiceCam].targetTexture = textureMonitor;
            camereSet[indiceCam].gameObject.SetActive(true);
            
            ApplicaEffettiScelti(camereSet[indiceCam]);

            yield return new WaitForSeconds(2f);
            
            indiceCam++;
            if (indiceCam >= camereSet.Length) indiceCam = 0;
        }
    }

    // --- FASE 2: CIAK E TITOLI DI CODA ---
    public void AvviaCiak()
    {
        if (registrazioneInCorso) return;

        registrazioneInCorso = true;
        previewInCorso = false; 
        StopAllCoroutines(); // Ferma la preview

        StartCoroutine(SequenzaRegistrazione());
    }

    IEnumerator SequenzaRegistrazione()
    {
        Debug.Log("<color=red>--- REC: INIZIO REGISTRAZIONE ---</color>");
        
        // Disattiviamo il giocatore per vedere a tutto schermo dalle camere
        if (mainCameraPlayer) mainCameraPlayer.enabled = false; 

        // Ciclo di registrazione (vedo le inquadrature una per una)
        for (int i = 0; i < camereSet.Length; i++)
        {
            // Reset camere
            foreach (var cam in camereSet) { cam.targetTexture = null; cam.gameObject.SetActive(false); }

            // Attiva camera corrente a TUTTO SCHERMO
            camereSet[i].targetTexture = null; 
            camereSet[i].gameObject.SetActive(true);
            
            ApplicaEffettiScelti(camereSet[i]);

            // Mostra questa inquadratura per 4 secondi
            yield return new WaitForSeconds(4f); 
        }

        Debug.Log("<color=green>--- STOP! ---</color>");
        
        // Spegni l'ultima camera rimasta accesa
        foreach (var cam in camereSet) cam.gameObject.SetActive(false);
        
        // Segna la task come completata (opzionale, visto che il gioco finisce)
        if (GameManager.instance != null) 
            GameManager.instance.CompletaTask(GameManager.Reparto.Regia);

        // --- QUI PARTE IL FINALE ---
        // Non riattivo il player, vado dritto ai titoli
        Debug.Log("Avvio titoli di coda immediati.");
        
        if (gestoreFinale != null)
        {
            gestoreFinale.AvviaTitoliDiCoda();
        }
        else
        {
            Debug.LogError("ERRORE: Non hai collegato il 'GestoreFinale' nell'Inspector!");
            // Se fallisce, riattiva il player per non bloccare il gioco
            if (mainCameraPlayer) mainCameraPlayer.enabled = true;
        }
    }

    void ApplicaEffettiScelti(Camera camDestinazione)
    {
        if (GameManager.instance != null && GameManager.instance.fovStandard > 0) 
        {
            switch (GameManager.instance.lenteSceltaFinale)
            {
                case "Grandangolo": 
                    camDestinazione.fieldOfView = GameManager.instance.fovGrandangolo; 
                    break;
                case "Cinematografica": 
                    camDestinazione.fieldOfView = GameManager.instance.fovCinematic; 
                    break;
                default: 
                    camDestinazione.fieldOfView = GameManager.instance.fovStandard; 
                    break;
            }
        }
    }
}