using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.HighDefinition;

public class RegiaManager : MonoBehaviour
{
    public static RegiaManager instance;

    [Header("Setup Player")]
    public Camera mainCameraPlayer; 
    public GameObject monitorSchermo; 
    public RenderTexture textureMonitor; 

    [Header("Setup Cinema (NUOVO)")]
    public GameObject schermoCinema;     // Trascina qui il Quad grande
    public RenderTexture textureCinema;  // Trascina qui la Texture 1920x1080

    [Header("Camere Cinematografiche")]
    public Camera[] camereSet; 

    [Header("Stato")]
    public bool previewInCorso = false;
    public bool registrazioneInCorso = false;
    public bool replayAttivo = false; // NUOVO

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        // Spegniamo tutto all'inizio
        foreach (Camera cam in camereSet)
        {
            cam.gameObject.SetActive(false);
            cam.targetTexture = null; 
        }
        if (schermoCinema) schermoCinema.SetActive(false); // Cinema spento
    }

    // ... (La funzione AttivaPreview rimane uguale) ...
    public void AttivaPreview()
    {
        if (camereSet == null || camereSet.Length == 0) return;
        if (previewInCorso) return;
        
        previewInCorso = true;
        monitorSchermo.SetActive(true);
        StartCoroutine(CicloPreviewMonitor());
    }

    IEnumerator CicloPreviewMonitor()
    {
        int indiceCam = 0;
        while (!registrazioneInCorso)
        {
            foreach (var cam in camereSet) { cam.targetTexture = null; cam.gameObject.SetActive(false); }

            camereSet[indiceCam].targetTexture = textureMonitor;
            camereSet[indiceCam].gameObject.SetActive(true);
            ApplicaEffettiScelti(camereSet[indiceCam]);

            yield return new WaitForSeconds(2f);
            indiceCam++;
            if (indiceCam >= camereSet.Length) indiceCam = 0;
        }
    }

    // --- FASE 2: REGISTRAZIONE (Modificata per avviare il Replay alla fine) ---
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
        Debug.Log("<color=red>--- REC ---</color>");
        
        mainCameraPlayer.enabled = false; // Player cieco, vede attraverso le cam

        for (int i = 0; i < camereSet.Length; i++)
        {
            foreach (var cam in camereSet) { cam.targetTexture = null; cam.gameObject.SetActive(false); }

            // Target NULL = A tutto schermo (occhi del giocatore)
            camereSet[i].targetTexture = null; 
            camereSet[i].gameObject.SetActive(true);
            ApplicaEffettiScelti(camereSet[i]);

            yield return new WaitForSeconds(4f); 
        }

        Debug.Log("<color=green>--- STOP! ---</color>");
        
        // Riattiva il player
        foreach (var cam in camereSet) cam.gameObject.SetActive(false);
        mainCameraPlayer.enabled = true;
        
        GameManager.instance.CompletaTask(GameManager.Reparto.Regia);

        // --- NUOVO: AVVIA IL REPLAY SUL GRANDE SCHERMO ---
        AvviaReplayCinema();
    }

    // --- FASE 3: REPLAY (NUOVO) ---
    public void AvviaReplayCinema()
    {
        replayAttivo = true;
        monitorSchermo.SetActive(false); // Spegni il monitor piccolo
        schermoCinema.SetActive(true);   // Accendi il cinema

        StartCoroutine(CicloReplayCinema());
    }

    IEnumerator CicloReplayCinema()
    {
        Debug.Log("Inizio proiezione su grande schermo...");
        int indice = 0;

        // Loop infinito (o puoi farlo girare una volta sola)
        while (true)
        {
            // Spegni tutte
            foreach (var cam in camereSet) { cam.targetTexture = null; cam.gameObject.SetActive(false); }

            // Accendi camera corrente MA mandala sulla Texture del Cinema
            camereSet[indice].targetTexture = textureCinema; 
            camereSet[indice].gameObject.SetActive(true);
            
            ApplicaEffettiScelti(camereSet[indice]);

            // Aspetta 4 secondi (stesso tempo della rec)
            yield return new WaitForSeconds(4f);

            indice++;
            if (indice >= camereSet.Length) indice = 0;
        }
    }

    void ApplicaEffettiScelti(Camera camDestinazione)
    {
        if (GameManager.instance.fovStandard > 0) 
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