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

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        foreach (Camera cam in camereSet)
        {
            cam.gameObject.SetActive(false);
            cam.targetTexture = null; 
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
            foreach (var cam in camereSet) { cam.targetTexture = null; cam.gameObject.SetActive(false); }

            camereSet[indiceCam].targetTexture = textureMonitor;
            camereSet[indiceCam].gameObject.SetActive(true);
            
            ApplicaEffettiScelti(camereSet[indiceCam]);

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
        
        if (gestoreRecitazione != null) gestoreRecitazione.AvviaCiakUnico();

        if (mainCameraPlayer) mainCameraPlayer.enabled = false; 

        for (int i = 0; i < camereSet.Length; i++)
        {
            foreach (var cam in camereSet) { cam.targetTexture = null; cam.gameObject.SetActive(false); }

            camereSet[i].targetTexture = null; 
            camereSet[i].gameObject.SetActive(true);
            
            ApplicaEffettiScelti(camereSet[i]);

            yield return new WaitForSeconds(4f); 
        }

        Debug.Log("<color=green>--- STOP! ---</color>");
        
        if (gestoreRecitazione != null) gestoreRecitazione.FermaTutto();

        foreach (var cam in camereSet) cam.gameObject.SetActive(false);
        
        if (mainCameraPlayer) mainCameraPlayer.enabled = true;

        if (GameManager.instance != null) 
            GameManager.instance.CompletaTask(GameManager.Reparto.Regia);

        Debug.Log("Avvio titoli di coda immediati.");
        
        if (gestoreFinale != null)
        {
            gestoreFinale.AvviaTitoliDiCoda();
        }
        else
        {
            Debug.LogError("ERRORE: Non hai collegato il 'GestoreFinale' nell'Inspector!");
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