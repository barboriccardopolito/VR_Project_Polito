using UnityEngine;
using System.Collections;

public class SpostamentoCamera : MonoBehaviour
{
    [Header("Setup Visuale")]
    public Camera cameraDallAlto; 
    public Camera cameraGiocatore; 
    
    [Header("Schermo Mirino")]
    public Camera cameraMirino;

    [Header("Movimento (Solo Regia)")]
    public float velocitaSpostamento = 3.0f;
    public bool usaLimiti = true;
    public float minX = -10f; 
    public float maxX = 10f;  
    public float minZ = -10f; 
    public float maxZ = 10f;  

    // --- NUOVA SEZIONE: ROTAZIONE PAN E TILT ---
    [Header("Rotazione Camera (Pan & Tilt)")]
    [Tooltip("Inserisci qui l'oggetto che fa da collo/testa alla camera (es. Sterzo)")]
    public Transform testaCamera; 
    public float sensibilitaRotazione = 2f;
    [Tooltip("Gradi massimi di rotazione a Destra e Sinistra")]
    public float limiteOrizzontale = 45f; 
    [Tooltip("Gradi massimi di rotazione in Su e Giù")]
    public float limiteVerticale = 25f;   
    
    private float rotPan = 0f;
    private float rotTilt = 0f;
    private Quaternion rotInizialeTesta;
    // -------------------------------------------

    [Header("Riferimenti Player")]
    public GameObject giocatore; 
    public string[] nomiScriptDaDisabilitare; 

    [Header("Modelli Lenti (Opzionali)")]
    public GameObject modelloGrandangolo;
    public GameObject modelloCinematografica;
    public GameObject modelloStandard;
    
    [Header("Audio")]
    public AudioClip suonoMontaggioLente;

    [HideInInspector] public bool lenteMontata = false;
    [HideInInspector] public bool schermoControllato = false;

    private Evidenziatore evidenziatore;
    private Collider mioCollider;
    private AudioSource audioSource;

    private bool inModalitaSpostamento = false;
    private bool inControlloSchermo = false; 
    private bool possoUscire = false; 

    void Start()
    {
        if (cameraDallAlto != null) cameraDallAlto.gameObject.SetActive(false);
        if (cameraGiocatore == null) cameraGiocatore = Camera.main;

        evidenziatore = GetComponent<Evidenziatore>();
        if (evidenziatore == null) evidenziatore = GetComponentInChildren<Evidenziatore>();

        mioCollider = GetComponent<Collider>();
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f;

        // Salviamo la rotazione iniziale del collo della macchina
        if (testaCamera != null) rotInizialeTesta = testaCamera.localRotation;

        NascondiTutteLeLenti();
    }

    public void Interagisci()
    {
        if (GameManager.instance == null) return;

        if (GameManager.instance.taskAttuale == GameManager.Reparto.Fotografia)
        {
            if (!lenteMontata) PiazzaLente();
            else if (lenteMontata && !schermoControllato)
            {
                if (inControlloSchermo) return;
                EntraControlloSchermo();
            }
            else Debug.Log("Questa telecamera è già a posto!");
        }
        else if (GameManager.instance.taskAttuale == GameManager.Reparto.Regia)
        {
            if (inModalitaSpostamento) return;
            EntraInModalitaSpostamento();
        }
    }

    void PiazzaLente()
    {
        InventarioGiocatore inventario = FindFirstObjectByType<InventarioGiocatore>();

        if (inventario != null && inventario.haUnOggetto && inventario.categoriaInMano == OggettoRaccolta.TipoOggetto.Lente)
        {
            string nomeLente = inventario.oggettoInMano;
            GameManager.instance.lenteSceltaFinale = nomeLente;

            GameObject lenteDaAnimare = MostraModelloLente(nomeLente);

            MontaggioLenteCinematica cinematica = GetComponent<MontaggioLenteCinematica>();
            if (cinematica != null && lenteDaAnimare != null)
            {
                cinematica.AvviaCinematicaMontaggio(lenteDaAnimare);
            }
            else
            {
                if (suonoMontaggioLente != null) audioSource.PlayOneShot(suonoMontaggioLente);
            }

            lenteMontata = true;
            Debug.Log($"<color=yellow>Lente montata su {gameObject.name}. Ora controlla lo schermo!</color>");
        }
        else
        {
            Debug.Log("Non hai una lente in mano da montare.");
        }
    }

    void EntraControlloSchermo()
    {
        inControlloSchermo = true;
        possoUscire = false;
        
        if (mioCollider != null) mioCollider.enabled = false;
        BloccaGiocatore(true);

        if (cameraGiocatore != null) cameraGiocatore.enabled = false;
        if (cameraDallAlto != null) cameraDallAlto.gameObject.SetActive(true);

        StartCoroutine(TimerSbloccoUscita());
    }

    void EsciControlloSchermo()
    {
        inControlloSchermo = false;
        possoUscire = false;

        if (cameraDallAlto != null) cameraDallAlto.gameObject.SetActive(false);
        if (cameraGiocatore != null) cameraGiocatore.enabled = true;

        BloccaGiocatore(false);
        if (mioCollider != null) mioCollider.enabled = true;

        schermoControllato = true;
        VerificaCompletamentoFotografia();
    }

    void VerificaCompletamentoFotografia()
    {
        SpostamentoCamera[] tutteLeCamere = FindObjectsByType<SpostamentoCamera>(FindObjectsSortMode.None);
        bool tutteFatte = true;

        foreach (SpostamentoCamera cam in tutteLeCamere)
        {
            if (!cam.schermoControllato) { tutteFatte = false; break; }
        }

        if (tutteFatte)
        {
            InventarioGiocatore inv = FindFirstObjectByType<InventarioGiocatore>();
            if (inv != null) inv.RimuoviOggetto();

            if (GameManager.instance != null) GameManager.instance.CompletaTask(GameManager.Reparto.Fotografia);
            Debug.Log("<color=green>Tutte le camere sono pronte! Task Fotografia COMPLETATA!</color>");
        }
    }

    public GameObject MostraModelloLente(string nomeLente)
    {
        NascondiTutteLeLenti();
        float nuovoFov = 60f; 
        GameObject lenteAttivata = null; 

        if (GameManager.instance != null)
        {
            GameManager.instance.ApplicaEffettoLente(nomeLente);

            if (nomeLente.Contains("Grandangolo")) 
            {
                if (modelloGrandangolo) { modelloGrandangolo.SetActive(true); lenteAttivata = modelloGrandangolo; }
                nuovoFov = GameManager.instance.fovGrandangolo; 
            }
            else if (nomeLente.Contains("Cinematografica")) 
            {
                if (modelloCinematografica) { modelloCinematografica.SetActive(true); lenteAttivata = modelloCinematografica; }
                nuovoFov = GameManager.instance.fovCinematic;
            }
            else 
            {
                if (modelloStandard) { modelloStandard.SetActive(true); lenteAttivata = modelloStandard; }
                nuovoFov = GameManager.instance.fovStandard;
            }
        }

        if (cameraDallAlto != null) cameraDallAlto.fieldOfView = nuovoFov;
        if (cameraMirino != null) cameraMirino.fieldOfView = nuovoFov;

        return lenteAttivata; 
    }

    public void ResettaVisualeLenti()
    {
        NascondiTutteLeLenti();
        lenteMontata = false;
        schermoControllato = false;

        if (GameManager.instance != null) GameManager.instance.lenteSceltaFinale = "";
        
        if (GameManager.instance != null)
        {
            if (cameraDallAlto != null) cameraDallAlto.fieldOfView = GameManager.instance.fovStandard;
            if (cameraMirino != null) cameraMirino.fieldOfView = GameManager.instance.fovStandard;
        }
    }

    void NascondiTutteLeLenti()
    {
        if (modelloGrandangolo) modelloGrandangolo.SetActive(false);
        if (modelloCinematografica) modelloCinematografica.SetActive(false);
        if (modelloStandard) modelloStandard.SetActive(false);
    }

    void Update()
    {
        GestisciEvidenziatore();

        if (inModalitaSpostamento)
        {
            GestisciMovimento(); 
            GestisciRotazione(); // <--- CHIAMATA ALLA ROTAZIONE
            if (Input.GetKeyDown(KeyCode.E) && possoUscire) EsciDaModalitaSpostamento();
        }
        else if (inControlloSchermo)
        {
            if (Input.GetKeyDown(KeyCode.E) && possoUscire) EsciControlloSchermo();
        }
    }

    void GestisciEvidenziatore()
    {
        if (evidenziatore != null && GameManager.instance != null)
        {
            if (inModalitaSpostamento || inControlloSchermo)
            {
                evidenziatore.Spegni();
                return;
            }

            bool faseFotografia = (GameManager.instance.taskAttuale == GameManager.Reparto.Fotografia);
            bool faseRevisione = (GameManager.instance.taskAttuale == GameManager.Reparto.Regia);
            
            InventarioGiocatore inventario = FindFirstObjectByType<InventarioGiocatore>();
            bool hoLenteInMano = (inventario != null && inventario.haUnOggetto && inventario.categoriaInMano == OggettoRaccolta.TipoOggetto.Lente);

            if (faseFotografia)
            {
                if (!lenteMontata && hoLenteInMano) evidenziatore.Accendi(); 
                else if (lenteMontata && !schermoControllato) evidenziatore.Accendi(); 
                else evidenziatore.Spegni(); 
            }
            else if (faseRevisione)
            {
                evidenziatore.Accendi(); 
            }
            else evidenziatore.Spegni();
        }
    }

    void EntraInModalitaSpostamento()
    {
        inModalitaSpostamento = true;
        possoUscire = false;
        
        if (mioCollider != null) mioCollider.enabled = false;
        BloccaGiocatore(true);

        if (cameraGiocatore != null) cameraGiocatore.enabled = false;
        if (cameraDallAlto != null) cameraDallAlto.gameObject.SetActive(true);

        StartCoroutine(TimerSbloccoUscita());
    }

    IEnumerator TimerSbloccoUscita()
    {
        yield return new WaitForSeconds(0.5f);
        possoUscire = true;
    }

    void EsciDaModalitaSpostamento()
    {
        inModalitaSpostamento = false;
        possoUscire = false;

        if (cameraDallAlto != null) cameraDallAlto.gameObject.SetActive(false);
        if (cameraGiocatore != null) cameraGiocatore.enabled = true;

        BloccaGiocatore(false);
        if (mioCollider != null) mioCollider.enabled = true;

        if (GameManager.instance != null) GameManager.instance.cameraPosizionata = true;
    }

    void GestisciMovimento()
    {
        float x = Input.GetAxis("Horizontal"); 
        float z = Input.GetAxis("Vertical");   

        Vector3 camRight = cameraDallAlto.transform.right;
        Vector3 camForward = cameraDallAlto.transform.up; // "up" è avanti quando la camera guarda in basso

        camRight.y = 0;
        camForward.y = 0;
        camRight.Normalize();
        camForward.Normalize();

        Vector3 move = (camRight * x + camForward * z) * velocitaSpostamento * Time.deltaTime;
        transform.Translate(move, Space.World);

        if (usaLimiti)
        {
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
            transform.position = pos;
        }
    }

    // --- FUNZIONE ROTAZIONE ADATTATA AGLI ASSI DEL MODELLO ---
    void GestisciRotazione()
    {
        if (testaCamera == null) return;

        // Leggiamo i movimenti del mouse
        float mouseX = Input.GetAxis("Mouse X") * sensibilitaRotazione;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilitaRotazione;

        rotPan += mouseX;
        rotTilt -= mouseY; 

        // Blocchiamo le rotazioni entro i limiti impostati
        rotPan = Mathf.Clamp(rotPan, -limiteOrizzontale, limiteOrizzontale);
        rotTilt = Mathf.Clamp(rotTilt, -limiteVerticale, limiteVerticale);

        // IL TRUCCO È QUI: Abbiamo spostato 'rotPan' dalla Y (il secondo valore) alla Z (il terzo valore)
        testaCamera.localRotation = rotInizialeTesta * Quaternion.Euler(rotTilt, 0, rotPan);
    }
    // --------------------------------------------------------

    void BloccaGiocatore(bool blocca)
    {
        if (giocatore == null) return;

        if (nomiScriptDaDisabilitare != null)
        {
            foreach (string nomeScript in nomiScriptDaDisabilitare)
            {
                MonoBehaviour scriptPlayer = giocatore.GetComponent(nomeScript) as MonoBehaviour;
                if (scriptPlayer != null) scriptPlayer.enabled = !blocca;
                
                if (cameraGiocatore != null)
                {
                    MonoBehaviour scriptCam = cameraGiocatore.GetComponent(nomeScript) as MonoBehaviour;
                    if (scriptCam != null) scriptCam.enabled = !blocca;
                }
            }
        }

        CharacterController cc = giocatore.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = !blocca;
        
        if (blocca) 
        { 
            Cursor.lockState = CursorLockMode.Locked; 
            Cursor.visible = false; 
        }
    }
    // --- FUNZIONE PER DISEGNARE I LIMITI NELLA SCENA ---
    void OnDrawGizmosSelected()
    {
        if (usaLimiti)
        {
            // Colore del recinto (Azzurro semitrasparente)
            Gizmos.color = new Color(0f, 1f, 1f, 0.8f);

            // Calcoliamo il centro del nostro recinto
            float centroX = (minX + maxX) / 2f;
            float centroZ = (minZ + maxZ) / 2f;

            // Calcoliamo quanto è largo e lungo
            float larghezzaX = Mathf.Abs(maxX - minX);
            float lunghezzaZ = Mathf.Abs(maxZ - minZ);

            // Posizioniamo il recinto alla stessa altezza (Y) della base della telecamera
            Vector3 centroGizmo = new Vector3(centroX, transform.position.y, centroZ);
            Vector3 dimensioneGizmo = new Vector3(larghezzaX, 0.05f, lunghezzaZ); // Molto sottile sull'asse Y

            // Disegna il contorno del recinto
            Gizmos.DrawWireCube(centroGizmo, dimensioneGizmo);

            // Disegna l'interno del recinto con un colore molto più trasparente
            Gizmos.color = new Color(0f, 1f, 1f, 0.1f);
            Gizmos.DrawCube(centroGizmo, dimensioneGizmo);
        }
    }
}