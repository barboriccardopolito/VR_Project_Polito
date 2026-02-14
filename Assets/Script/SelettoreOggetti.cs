using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Collections;

public class SelettoreOggetti : MonoBehaviour
{
    [Header("Setup Visuale")]
    public Camera cameraDallAlto;
    public Camera cameraGiocatore;
    public Transform puntoIspezione; 

    [Header("Riferimenti Player")]
    public GameObject giocatore;
    public string[] nomiScriptDaDisabilitare;
    private InterazioneGiocatore scriptInterazione;
    private CharacterController controllerGiocatore;

    [Header("Impostazioni Task")]
    public GameManager.Reparto taskRichiesta;
    
    [Header("Oggetti Selezionabili (I Pivot)")]
    public OggettoRaccolta[] oggetti;

    [Header("Impostazioni Ispezione 3D")]
    public float distanzaSfondo = 3.0f;    
    public float sensibilitaMouse = 8f;    
    public float velocitaAnimazione = 12f; 
    [Range(0f, 1f)] public float opacitaSfondo = 0.85f;

    [Header("UI Scheda Tecnica")]
    public GameObject pannelloSchedaUI;
    public TextMeshProUGUI testoTitolo;
    public TextMeshProUGUI testoDescrizione;

    // --- NUOVE VARIABILI PER LA MACCHINA DA SCRIVERE ---
    [Header("Effetto Macchina Da Scrivere")]
    public float velocitaScrittura = 0.03f; // Tempo tra una lettera e l'altra
    public AudioClip suonoBattitura;        // Il suono del singolo tasto
    private AudioSource audioScrittura;     // Chi riproduce il suono
    private Coroutine coroutineScrittura;   // Riferimento all'animazione in corso

    private bool inSelezione = false;
    private int indiceAttuale = 0;
    private bool possoUscire = false;

    private Vector3[] posOriginali;
    private Quaternion[] rotOriginali;
    private Vector2 rotazioneOggettoCorrente;

    private Canvas sfondoCanvas;
    private Image immagineSfondo;
    private float targetAlphaSfondo = 0f;

    void Start()
    {
        if (cameraDallAlto != null) 
        {
            cameraDallAlto.gameObject.SetActive(false);
            AudioListener al = cameraDallAlto.GetComponent<AudioListener>();
            if (al != null) al.enabled = false;
        }
        if (cameraGiocatore == null) cameraGiocatore = Camera.main;
        
        if (giocatore != null)
        {
            scriptInterazione = giocatore.GetComponent<InterazioneGiocatore>();
            controllerGiocatore = giocatore.GetComponent<CharacterController>();
        }

        if (pannelloSchedaUI != null) pannelloSchedaUI.SetActive(false);

        // Prepariamo il componente audio per la battitura dei tasti
        audioScrittura = gameObject.AddComponent<AudioSource>();
        audioScrittura.playOnAwake = false;
        audioScrittura.spatialBlend = 0f; // Audio 2D così si sente chiaro nell'UI

        posOriginali = new Vector3[oggetti.Length];
        rotOriginali = new Quaternion[oggetti.Length];
        
        for(int i = 0; i < oggetti.Length; i++)
        {
            if(oggetti[i] != null)
            {
                posOriginali[i] = oggetti[i].transform.localPosition;
                rotOriginali[i] = oggetti[i].transform.localRotation;
            }
        }
        CreaSfondoScuro();
    }

    void CreaSfondoScuro()
    {
        GameObject canvasObj = new GameObject("SfondoIspezione");
        canvasObj.transform.SetParent(cameraDallAlto.transform);
        sfondoCanvas = canvasObj.AddComponent<Canvas>();
        sfondoCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        sfondoCanvas.worldCamera = cameraDallAlto;
        sfondoCanvas.planeDistance = distanzaSfondo; 
        
        GameObject panelObj = new GameObject("PannelloNero");
        panelObj.transform.SetParent(canvasObj.transform, false);
        immagineSfondo = panelObj.AddComponent<Image>();
        immagineSfondo.color = new Color(0.1f, 0.1f, 0.1f, 0f); 
        
        RectTransform rt = immagineSfondo.rectTransform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;
        canvasObj.SetActive(false);
    }

    public bool PuoiInteragire() { return GameManager.instance != null && GameManager.instance.taskAttuale == taskRichiesta; }

    public void EntraInSelezione()
    {
        if (inSelezione) return;
        inSelezione = true; 
        possoUscire = false;

        indiceAttuale = 0;
        for (int i = 0; i < oggetti.Length; i++) { if (oggetti[i].gameObject.activeInHierarchy) { indiceAttuale = i; break; } }

        BloccaGiocatore(true);
        if (cameraGiocatore != null) cameraGiocatore.enabled = false;
        if (cameraDallAlto != null) cameraDallAlto.gameObject.SetActive(true);
        if (scriptInterazione != null) scriptInterazione.enabled = false; 

        rotazioneOggettoCorrente = Vector2.zero;
        sfondoCanvas.gameObject.SetActive(true);
        targetAlphaSfondo = opacitaSfondo;

        if (pannelloSchedaUI != null) pannelloSchedaUI.SetActive(true);
        AggiornaSchedaTecnica();

        StartCoroutine(TimerSblocco());
    }

    IEnumerator TimerSblocco() { yield return new WaitForSeconds(0.5f); possoUscire = true; }

    void Update()
    {
        GestisciAnimazione();

        if (!inSelezione) return;

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) CambiaSelezione(1);
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) CambiaSelezione(-1);

        float mouseX = Input.GetAxis("Mouse X") * sensibilitaMouse;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilitaMouse;
        rotazioneOggettoCorrente.x += mouseY; 
        rotazioneOggettoCorrente.y -= mouseX; 

        if (Input.GetKeyDown(KeyCode.E) && possoUscire) 
        {
            ScegliOggetto();
        }
    }

    void GestisciAnimazione()
    {
        if (immagineSfondo != null)
        {
            Color c = immagineSfondo.color;
            c.a = Mathf.Lerp(c.a, targetAlphaSfondo, Time.deltaTime * velocitaAnimazione);
            immagineSfondo.color = c;
            if (!inSelezione && c.a < 0.01f) sfondoCanvas.gameObject.SetActive(false);
        }

        for (int i = 0; i < oggetti.Length; i++)
        {
            if (oggetti[i] == null || !oggetti[i].gameObject.activeInHierarchy) continue;

            if (inSelezione && i == indiceAttuale)
            {
                Vector3 targetPos = puntoIspezione != null ? puntoIspezione.position : cameraDallAlto.transform.position + (cameraDallAlto.transform.forward * 0.5f);
                oggetti[i].transform.position = Vector3.Lerp(oggetti[i].transform.position, targetPos, Time.deltaTime * velocitaAnimazione);
                
                Quaternion rotazioneSchermo = cameraDallAlto.transform.rotation;
                Quaternion offsetMouse = Quaternion.Euler(rotazioneOggettoCorrente.x, rotazioneOggettoCorrente.y, 0);
                oggetti[i].transform.rotation = Quaternion.Lerp(oggetti[i].transform.rotation, rotazioneSchermo * offsetMouse, Time.deltaTime * velocitaAnimazione);
            }
            else
            {
                oggetti[i].transform.localPosition = Vector3.Lerp(oggetti[i].transform.localPosition, posOriginali[i], Time.deltaTime * velocitaAnimazione);
                oggetti[i].transform.localRotation = Quaternion.Lerp(oggetti[i].transform.localRotation, rotOriginali[i], Time.deltaTime * velocitaAnimazione);
            }
        }
    }

    void CambiaSelezione(int dir)
    {
        int tentativi = 0;
        do {
            indiceAttuale += dir;
            if (indiceAttuale >= oggetti.Length) indiceAttuale = 0;
            else if (indiceAttuale < 0) indiceAttuale = oggetti.Length - 1;
            tentativi++;
        } while (!oggetti[indiceAttuale].gameObject.activeInHierarchy && tentativi < oggetti.Length);
        
        rotazioneOggettoCorrente = Vector2.zero;
        
        AggiornaSchedaTecnica();
    }

    void ScegliOggetto()
    {
        inSelezione = false; 
        possoUscire = false; 
        targetAlphaSfondo = 0f; 

        if (pannelloSchedaUI != null) pannelloSchedaUI.SetActive(false);
        
        // Fermiamo la scrittura se stiamo uscendo dall'ispezione
        if (coroutineScrittura != null) StopCoroutine(coroutineScrittura);

        if (cameraDallAlto != null) cameraDallAlto.gameObject.SetActive(false);
        if (cameraGiocatore != null) cameraGiocatore.enabled = true;
        if (scriptInterazione != null) scriptInterazione.enabled = true;
        
        BloccaGiocatore(false);

        if (oggetti[indiceAttuale].gameObject.activeInHierarchy)
        {
            oggetti[indiceAttuale].EseguiRaccolta();
            oggetti[indiceAttuale].transform.localPosition = posOriginali[indiceAttuale];
            oggetti[indiceAttuale].transform.localRotation = rotOriginali[indiceAttuale];
        }
    }

    // --- LOGICA MODIFICATA: Avvia la Coroutine invece di scrivere di botto ---
    void AggiornaSchedaTecnica()
    {
        if (pannelloSchedaUI == null) return;

        if (oggetti.Length > 0 && oggetti[indiceAttuale] != null)
        {
            // Se c'era già una scrittura in corso (es. l'utente ha premuto A/D velocemente), fermala
            if (coroutineScrittura != null) StopCoroutine(coroutineScrittura);
            
            // Fai partire la nuova animazione
            string titolo = oggetti[indiceAttuale].nomeTecnico;
            string descrizione = oggetti[indiceAttuale].descrizioneTecnica;
            coroutineScrittura = StartCoroutine(EffettoTestoAnimato(titolo, descrizione));
        }
    }

    // --- COROUTINE DELLA MACCHINA DA SCRIVERE ---
    IEnumerator EffettoTestoAnimato(string titoloCompleto, string descCompleta)
    {
        // Svuota i testi all'inizio
        if (testoTitolo != null) testoTitolo.text = "";
        if (testoDescrizione != null) testoDescrizione.text = "";

        // 1. Scrive il Titolo
        if (testoTitolo != null)
        {
            foreach (char lettera in titoloCompleto)
            {
                testoTitolo.text += lettera;
                SuonaTasto(lettera);
                yield return new WaitForSeconds(velocitaScrittura);
            }
        }

        // Piccola pausa scenica tra titolo e descrizione
        yield return new WaitForSeconds(0.2f);

        // 2. Scrive la Descrizione (leggermente più veloce del titolo)
        if (testoDescrizione != null)
        {
            foreach (char lettera in descCompleta)
            {
                testoDescrizione.text += lettera;
                SuonaTasto(lettera);
                yield return new WaitForSeconds(velocitaScrittura * 0.6f); 
            }
        }
    }

    void SuonaTasto(char lettera)
    {
        if (suonoBattitura != null && audioScrittura != null)
        {
            // Evita di fare rumore sugli spazi vuoti, rende l'audio più "organico"
            if (lettera != ' ')
            {
                // Varia leggermente il "tono" (pitch) per non farlo sembrare un mitra robotico
                audioScrittura.pitch = Random.Range(0.9f, 1.1f);
                audioScrittura.PlayOneShot(suonoBattitura, 0.4f); // 0.4f è il volume
            }
        }
    }

    void BloccaGiocatore(bool blocca)
    {
        if (giocatore == null) return;

        if (nomiScriptDaDisabilitare != null)
        {
            foreach (string nomeScript in nomiScriptDaDisabilitare)
            {
                MonoBehaviour s = giocatore.GetComponent(nomeScript) as MonoBehaviour;
                if (s != null) s.enabled = !blocca;
            }
        }

        if (controllerGiocatore != null) controllerGiocatore.enabled = !blocca;
        
        if (blocca) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
        else { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
    }
}