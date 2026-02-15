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

    [Header("Rotazione Camera (Pan & Tilt)")]
    public Transform testaCamera; 
    public float sensibilitaRotazione = 2f;
    public float limiteOrizzontale = 45f; 
    public float limiteVerticale = 25f;   
    
    private float rotPan = 0f;
    private float rotTilt = 0f;
    private Quaternion rotInizialeTesta;
    
    private string lenteMontataQui = "";

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

        if (testaCamera != null) rotInizialeTesta = testaCamera.localRotation;

        NascondiTutteLeLenti();
    }

    public void Interagisci()
    {
        if (GameManager.instance == null) return;

        InventarioGiocatore inventario = FindFirstObjectByType<InventarioGiocatore>();
        bool hoLenteInMano = (inventario != null && inventario.haUnOggetto && inventario.categoriaInMano == OggettoRaccolta.TipoOggetto.Lente);
        string nomeLenteInMano = hoLenteInMano ? inventario.oggettoInMano : "";

        if (GameManager.instance.taskAttuale == GameManager.Reparto.Fotografia)
        {
            if (!lenteMontata) 
            {
                PiazzaLente();
            }
            else if (lenteMontata && !schermoControllato)
            {
                if (!inControlloSchermo) EntraControlloSchermo();
            }
            else if (lenteMontata && schermoControllato)
            {
                if (hoLenteInMano && nomeLenteInMano != lenteMontataQui)
                {
                    if (lenteMontataQui != "") GameManager.instance.RestituisciOggettoAlTavolo(lenteMontataQui);
                    ResettaVisualeLenti(); 
                    PiazzaLente();
                }
            }
        }
        else if (GameManager.instance.taskAttuale == GameManager.Reparto.Regia)
        {
            if (hoLenteInMano && nomeLenteInMano != lenteMontataQui) 
            {
                if (lenteMontataQui != "") GameManager.instance.RestituisciOggettoAlTavolo(lenteMontataQui);
                ResettaVisualeLenti(); 
                PiazzaLente();         
            }
            else 
            {
                if (!inModalitaSpostamento) EntraInModalitaSpostamento();
            }
        }
    }

    void PiazzaLente()
    {
        InventarioGiocatore inventario = FindFirstObjectByType<InventarioGiocatore>();

        if (inventario != null && inventario.haUnOggetto && inventario.categoriaInMano == OggettoRaccolta.TipoOggetto.Lente)
        {
            string nomeLente = inventario.oggettoInMano;
            lenteMontataQui = nomeLente; 
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
            inventario.RimuoviOggetto();

            Debug.Log($"<color=yellow>Lente montata su {gameObject.name}. Ora controlla lo schermo!</color>");
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

        if (mioCollider != null) mioCollider.enabled = true;

        schermoControllato = true;
        VerificaCompletamentoFotografia();

        StartCoroutine(SbloccoComandiRitardato());
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
            GestisciRotazione(); 
            if (Input.GetKeyDown(KeyCode.E) && possoUscire) EsciDaModalitaSpostamento();
        }
        else if (inControlloSchermo)
        {
            if (Input.GetKeyDown(KeyCode.E) && possoUscire) EsciControlloSchermo();
        }
    }

    // --- NUOVA LOGICA EVIDENZIATORE ---
    void GestisciEvidenziatore()
    {
        if (evidenziatore == null || GameManager.instance == null) return;

        if (inModalitaSpostamento || inControlloSchermo)
        {
            evidenziatore.Spegni();
            return;
        }

        bool faseFotografia = (GameManager.instance.taskAttuale == GameManager.Reparto.Fotografia);
        bool faseRevisione = (GameManager.instance.taskAttuale == GameManager.Reparto.Regia);
        
        InventarioGiocatore inv = FindFirstObjectByType<InventarioGiocatore>();
        bool hoLenteInMano = (inv != null && inv.haUnOggetto && inv.categoriaInMano == OggettoRaccolta.TipoOggetto.Lente);
        bool hoManiVuote = (inv == null || !inv.haUnOggetto);
        string nomeLenteInMano = hoLenteInMano ? inv.oggettoInMano : "";

        if (faseFotografia)
        {
            if (!lenteMontata && hoLenteInMano) 
                evidenziatore.Accendi(); 
            else if (lenteMontata && !schermoControllato) 
                evidenziatore.Accendi(); 
            else if (lenteMontata && schermoControllato && hoLenteInMano && nomeLenteInMano != lenteMontataQui) 
                evidenziatore.Accendi();
            else 
                evidenziatore.Spegni(); 
        }
        else if (faseRevisione)
        {
            if (hoManiVuote) 
                evidenziatore.Accendi();
            else if (hoLenteInMano && nomeLenteInMano != lenteMontataQui) 
                evidenziatore.Accendi();
            else 
                evidenziatore.Spegni();
        }
        else 
        {
            evidenziatore.Spegni();
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

        if (mioCollider != null) mioCollider.enabled = true;

        if (GameManager.instance != null) GameManager.instance.cameraPosizionata = true;

        StartCoroutine(SbloccoComandiRitardato());
    }

    void GestisciMovimento()
    {
        float x = Input.GetAxis("Horizontal"); 
        float z = Input.GetAxis("Vertical");   

        Vector3 camRight = cameraDallAlto.transform.right;
        Vector3 camForward = cameraDallAlto.transform.up; 

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

    void GestisciRotazione()
    {
        if (testaCamera == null) return;

        float mouseX = Input.GetAxis("Mouse X") * sensibilitaRotazione;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilitaRotazione;

        rotPan += mouseX;
        rotTilt -= mouseY; 

        rotPan = Mathf.Clamp(rotPan, -limiteOrizzontale, limiteOrizzontale);
        rotTilt = Mathf.Clamp(rotTilt, -limiteVerticale, limiteVerticale);

        testaCamera.localRotation = rotInizialeTesta * Quaternion.Euler(rotTilt, 0, rotPan);
    }

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

    IEnumerator SbloccoComandiRitardato()
    {
        yield return new WaitUntil(() => !Input.GetKey(KeyCode.E));
        yield return new WaitForSeconds(0.1f);
        BloccaGiocatore(false);
    }

    void OnDrawGizmosSelected()
    {
        if (usaLimiti)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.8f);

            float centroX = (minX + maxX) / 2f;
            float centroZ = (minZ + maxZ) / 2f;

            float larghezzaX = Mathf.Abs(maxX - minX);
            float lunghezzaZ = Mathf.Abs(maxZ - minZ);

            Vector3 centroGizmo = new Vector3(centroX, transform.position.y, centroZ);
            Vector3 dimensioneGizmo = new Vector3(larghezzaX, 0.05f, lunghezzaZ); 

            Gizmos.DrawWireCube(centroGizmo, dimensioneGizmo);

            Gizmos.color = new Color(0f, 1f, 1f, 0.1f);
            Gizmos.DrawCube(centroGizmo, dimensioneGizmo);
        }
    }
}