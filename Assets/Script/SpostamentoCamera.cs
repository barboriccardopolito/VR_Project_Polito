using UnityEngine;
using System.Collections;

public class SpostamentoCamera : MonoBehaviour
{
    [Header("Setup Visuale")]
    public Camera cameraDallAlto; 
    public Camera cameraGiocatore; 
    
    [Header("Transizione Visuale")]
    [Tooltip("Velocità del volo della telecamera")]
    public float velocitaTransizione = 2.5f;
    private bool inTransizione = false;

    [Header("Schermo Mirino")]
    public Camera cameraMirino;
    [Tooltip("Frequenza aggiornamento schermo (es. 60)")]
    public float fpsSchermo = 60f;
    private WaitForSeconds attesaFrameMirino;

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

    [Header("Modelli Lenti")]
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

        if (cameraMirino != null)
        {
            cameraMirino.enabled = false; 
            attesaFrameMirino = new WaitForSeconds(1f / fpsSchermo);
            StartCoroutine(AggiornaSchermoMirino());
        }
    }

    IEnumerator AggiornaSchermoMirino()
    {
        while (true)
        {
            if (cameraMirino != null) cameraMirino.Render();
            yield return attesaFrameMirino;
        }
    }

    // --- FUNZIONE PER NASCONDERE RADIO E OGGETTI IN MANO ---
    void ImpostaVisibilitaOggetti(bool visibile)
    {
        // 1. Nascondi Inventario
        InventarioGiocatore inv = Object.FindFirstObjectByType<InventarioGiocatore>();
        if (inv != null)
        {
            Renderer[] rends = inv.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in rends) r.enabled = visibile;
        }

        if (giocatore != null)
        {
            Renderer[] tuttiRends = giocatore.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in tuttiRends)
            {
                if (r.gameObject.name.Contains("Radio") || r.gameObject.name.Contains("Walkie"))
                {
                    r.enabled = visibile;
                }
            }
        }
    }

    public void Interagisci()
    {
        if (inTransizione || GameManager.instance == null) return;

        InventarioGiocatore inventario = Object.FindFirstObjectByType<InventarioGiocatore>();
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
                if (!inControlloSchermo) StartCoroutine(TransizioneEntrata(false));
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
                if (!inModalitaSpostamento) StartCoroutine(TransizioneEntrata(true));
            }
        }
    }

    void PiazzaLente()
    {
        InventarioGiocatore inventario = Object.FindFirstObjectByType<InventarioGiocatore>();

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
                StartCoroutine(NascondiOggettiTemporaneo());
            }
            else
            {
                if (suonoMontaggioLente != null) audioSource.PlayOneShot(suonoMontaggioLente);
            }

            lenteMontata = true;
        }
    }

    IEnumerator NascondiOggettiTemporaneo()
    {
        ImpostaVisibilitaOggetti(false);
        yield return new WaitForSeconds(3.5f);
        ImpostaVisibilitaOggetti(true);
    }

    IEnumerator TransizioneEntrata(bool modalitaSpostamento)
    {
        inTransizione = true;
        if (mioCollider != null) mioCollider.enabled = false;
        BloccaGiocatore(true);
        
        ImpostaVisibilitaOggetti(false);

        Vector3 targetLocalPos = cameraDallAlto.transform.localPosition;
        Quaternion targetLocalRot = cameraDallAlto.transform.localRotation;
        float targetFov = cameraDallAlto.fieldOfView;

        cameraDallAlto.transform.position = cameraGiocatore.transform.position;
        cameraDallAlto.transform.rotation = cameraGiocatore.transform.rotation;
        float startFov = cameraGiocatore.fieldOfView;
        cameraDallAlto.fieldOfView = startFov;

        Vector3 startLocalPos = cameraDallAlto.transform.localPosition;
        Quaternion startLocalRot = cameraDallAlto.transform.localRotation;

        cameraGiocatore.enabled = false;
        cameraDallAlto.gameObject.SetActive(true);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * velocitaTransizione;
            float smooth = Mathf.SmoothStep(0f, 1f, t);
            
            cameraDallAlto.transform.localPosition = Vector3.Lerp(startLocalPos, targetLocalPos, smooth);
            cameraDallAlto.transform.localRotation = Quaternion.Lerp(startLocalRot, targetLocalRot, smooth);
            cameraDallAlto.fieldOfView = Mathf.Lerp(startFov, targetFov, smooth);
            
            yield return null;
        }

        cameraDallAlto.transform.localPosition = targetLocalPos;
        cameraDallAlto.transform.localRotation = targetLocalRot;
        cameraDallAlto.fieldOfView = targetFov;

        if (modalitaSpostamento) inModalitaSpostamento = true;
        else inControlloSchermo = true;

        possoUscire = true;
        inTransizione = false;
    }

    IEnumerator TransizioneUscita(bool modalitaSpostamento)
    {
        inTransizione = true;
        if (modalitaSpostamento) inModalitaSpostamento = false;
        else inControlloSchermo = false;
        
        possoUscire = false;

        Vector3 startLocalPos = cameraDallAlto.transform.localPosition;
        Quaternion startLocalRot = cameraDallAlto.transform.localRotation;
        float startFov = cameraDallAlto.fieldOfView;

        Vector3 startWorldPos = cameraDallAlto.transform.position;
        Quaternion startWorldRot = cameraDallAlto.transform.rotation;
        
        float t = 0f;
        while(t < 1f)
        {
            t += Time.deltaTime * velocitaTransizione;
            float smooth = Mathf.SmoothStep(0f, 1f, t);
            
            cameraDallAlto.transform.position = Vector3.Lerp(startWorldPos, cameraGiocatore.transform.position, smooth);
            cameraDallAlto.transform.rotation = Quaternion.Lerp(startWorldRot, cameraGiocatore.transform.rotation, smooth);
            cameraDallAlto.fieldOfView = Mathf.Lerp(startFov, cameraGiocatore.fieldOfView, smooth);
            
            yield return null;
        }

        cameraDallAlto.gameObject.SetActive(false);
        cameraGiocatore.enabled = true;

        cameraDallAlto.transform.localPosition = startLocalPos;
        cameraDallAlto.transform.localRotation = startLocalRot;
        cameraDallAlto.fieldOfView = startFov;

        ImpostaVisibilitaOggetti(true);

        if (mioCollider != null) mioCollider.enabled = true;

        if (!modalitaSpostamento) 
        {
            schermoControllato = true;
            VerificaCompletamentoFotografia();
        }
        else
        {
            if (GameManager.instance != null) GameManager.instance.cameraPosizionata = true;
        }

        inTransizione = false;
        StartCoroutine(SbloccoComandiRitardato());
    }

    void VerificaCompletamentoFotografia()
    {
        SpostamentoCamera[] tutteLeCamere = Object.FindObjectsByType<SpostamentoCamera>(FindObjectsSortMode.None);
        bool tutteFatte = true;

        foreach (SpostamentoCamera cam in tutteLeCamere)
        {
            if (!cam.schermoControllato) { tutteFatte = false; break; }
        }

        if (tutteFatte)
        {
            InventarioGiocatore inv = Object.FindFirstObjectByType<InventarioGiocatore>();
            if (inv != null) inv.RimuoviOggetto();

            if (GameManager.instance != null) GameManager.instance.CompletaTask(GameManager.Reparto.Fotografia);
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
        if (lenteMontata && !string.IsNullOrEmpty(lenteMontataQui) && GameManager.instance != null)
        {
            GameManager.instance.RestituisciOggettoAlTavolo(lenteMontataQui);
        }

        NascondiTutteLeLenti();
        lenteMontata = false;
        schermoControllato = false;
        lenteMontataQui = "";
        
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
            if (Input.GetKeyDown(KeyCode.E) && possoUscire && !inTransizione) StartCoroutine(TransizioneUscita(true));
        }
        else if (inControlloSchermo)
        {
            if (Input.GetKeyDown(KeyCode.E) && possoUscire && !inTransizione) StartCoroutine(TransizioneUscita(false));
        }
    }

    private bool ControllaIntroRegista()
    {
        InteragibileNPC[] tuttiNPC = Object.FindObjectsByType<InteragibileNPC>(FindObjectsSortMode.None);
        foreach (InteragibileNPC npc in tuttiNPC)
        {
            if (npc.tipoReparto == GameManager.Reparto.Regia)
            {
                NPC_Staff staff = npc.GetComponent<NPC_Staff>();
                if (staff != null) return staff.haGiaParlato;
            }
        }
        return false;
    }

    void GestisciEvidenziatore()
    {
        if (evidenziatore == null || GameManager.instance == null) return;

        if (inModalitaSpostamento || inControlloSchermo || inTransizione)
        {
            evidenziatore.Spegni();
            return;
        }

        bool faseFotografia = (GameManager.instance.taskAttuale == GameManager.Reparto.Fotografia);
        bool faseRevisione = (GameManager.instance.taskAttuale == GameManager.Reparto.Regia);
        
        InventarioGiocatore inv = Object.FindFirstObjectByType<InventarioGiocatore>();
        bool hoLenteInMano = (inv != null && inv.haUnOggetto && inv.categoriaInMano == OggettoRaccolta.TipoOggetto.Lente);
        bool hoManiVuote = (inv == null || !inv.haUnOggetto);
        string nomeLenteInMano = hoLenteInMano ? inv.oggettoInMano : "";

        if (faseFotografia)
        {
            if (!lenteMontata && hoLenteInMano) 
                evidenziatore.Accendi(); 
            else 
                evidenziatore.Spegni(); 
        }
        else if (faseRevisione)
        {
            if (!ControllaIntroRegista()) 
            {
                evidenziatore.Spegni();
            }
            else 
            {
                if (hoManiVuote) 
                    evidenziatore.Accendi();
                else if (hoLenteInMano && (!lenteMontata || nomeLenteInMano != lenteMontataQui)) 
                    evidenziatore.Accendi();
                else 
                    evidenziatore.Spegni();
            }
        }
        else 
        {
            evidenziatore.Spegni();
        }
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
}