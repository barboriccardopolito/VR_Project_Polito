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

    // --- NUOVI SLOT PER GLI EFFETTI VISIVI ---
    [Header("Effetti Post-Processing (Wow Factor)")]
    public GameObject volumeGrandangolo;
    public GameObject volumeCinematografica;
    public GameObject volumeDistorta;

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
        
        if (gestoreRecitazione != null) gestoreRecitazione.AvviaCiakUnico();
        if (mainCameraPlayer) mainCameraPlayer.enabled = false; 

        for (int i = 0; i < camereSet.Length; i++)
        {
            for (int j = 0; j < camereSet.Length; j++) 
            { 
                if(camereSet[j] != null) camereSet[j].targetTexture = textureMiriniOriginali[j]; 
            }

            if (camereSet[i] == null) continue;

            Camera camAttuale = camereSet[i];
            
            camAttuale.targetTexture = null; 
            camAttuale.enabled = true; 

            OttimizzaCamera optScript = camAttuale.GetComponent<OttimizzaCamera>();
            if (optScript != null) optScript.enabled = false;
            
            if (GameManager.instance != null) ApplicaEffettiScelti(camAttuale);

            yield return new WaitForSeconds(4f); 
        }

        Debug.Log("<color=green>--- STOP! ---</color>");
        
        if (gestoreRecitazione != null) gestoreRecitazione.FermaTutto();
        if (mainCameraPlayer) mainCameraPlayer.enabled = true;

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

    // --- FUNZIONE AGGIORNATA PER I VOLUMI ---
    void ApplicaEffettiScelti(Camera camDestinazione)
    {
        // 1. Spegniamo tutti gli effetti speciali di default
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