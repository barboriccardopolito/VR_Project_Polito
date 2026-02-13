using UnityEngine;
using System.Collections;

public class SpostamentoCamera : MonoBehaviour
{
    [Header("Setup Visuale")]
    public Camera cameraDallAlto; 
    public Camera cameraGiocatore; 

    [Header("Movimento (Solo Regia)")]
    public float velocitaSpostamento = 3.0f;
    public bool usaLimiti = true;
    public float minX = -10f; 
    public float maxX = 10f;  
    public float minZ = -10f; 
    public float maxZ = 10f;  

    [Header("Riferimenti Player")]
    public GameObject giocatore; 
    public string[] nomiScriptDaDisabilitare; 

    [Header("Modelli Lenti (Opzionali)")]
    public GameObject modelloGrandangolo;
    public GameObject modelloCinematografica;
    public GameObject modelloStandard;
    
    [Header("Audio")]
    public AudioClip suonoMontaggioLente;

    private Evidenziatore evidenziatore;
    private Collider mioCollider;
    private AudioSource audioSource;

    private bool inModalitaSpostamento = false;
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

        NascondiTutteLeLenti();
    }

    public void Interagisci()
    {
        if (GameManager.instance == null) return;

        if (GameManager.instance.taskAttuale == GameManager.Reparto.Fotografia)
        {
            PiazzaLente();
        }
        else if (GameManager.instance.taskAttuale == GameManager.Reparto.Regia)
        {
            if (inModalitaSpostamento) return;
            EntraInModalitaSpostamento();
        }
        else
        {
            Debug.Log("La telecamera non ti serve in questo momento.");
        }
    }

    void PiazzaLente()
    {
        InventarioGiocatore inventario = FindFirstObjectByType<InventarioGiocatore>();

        if (inventario != null && inventario.haUnOggetto && inventario.categoriaInMano == OggettoRaccolta.TipoOggetto.Lente)
        {
            string nomeLente = inventario.oggettoInMano;
            
            GameManager.instance.lenteSceltaFinale = nomeLente;

            // --- LA MAGIA MULTI-CAMERA ---
            // Cerchiamo TUTTE le telecamere presenti sulla scena
            SpostamentoCamera[] tutteLeCamere = FindObjectsByType<SpostamentoCamera>(FindObjectsSortMode.None);
            
            // Diciamo a ciascuna telecamera di accendere il modello della lente scelta
            foreach (SpostamentoCamera cam in tutteLeCamere)
            {
                cam.MostraModelloLente(nomeLente);
            }

            if (suonoMontaggioLente != null) audioSource.PlayOneShot(suonoMontaggioLente);

            inventario.RimuoviOggetto();

            GameManager.instance.CompletaTask(GameManager.Reparto.Fotografia);
            
            Debug.Log($"<color=green>Lente {nomeLente} montata su TUTTE le telecamere con successo!</color>");
        }
        else
        {
            Debug.Log("Non hai una lente in mano da montare.");
        }
    }

    // --- NUOVA FUNZIONE PUBBLICA ---
    // Serve per farsi chiamare dalle altre telecamere per accendere il modello giusto
    public void MostraModelloLente(string nomeLente)
    {
        NascondiTutteLeLenti();
        if (nomeLente.Contains("Grandangolo") && modelloGrandangolo) modelloGrandangolo.SetActive(true);
        else if (nomeLente.Contains("Cinematografica") && modelloCinematografica) modelloCinematografica.SetActive(true);
        else if (modelloStandard) modelloStandard.SetActive(true);
    }

    public void ResettaVisualeLenti()
    {
        NascondiTutteLeLenti();
        if (GameManager.instance != null) GameManager.instance.lenteSceltaFinale = "";
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

            if (Input.GetKeyDown(KeyCode.E) && possoUscire)
            {
                EsciDaModalitaSpostamento();
            }
        }
    }

    void GestisciEvidenziatore()
    {
        if (evidenziatore != null && GameManager.instance != null)
        {
            if (inModalitaSpostamento)
            {
                evidenziatore.Spegni();
                return;
            }

            bool faseFotografia = (GameManager.instance.taskAttuale == GameManager.Reparto.Fotografia);
            bool faseRevisione = (GameManager.instance.taskAttuale == GameManager.Reparto.Regia);
            
            InventarioGiocatore inventario = FindFirstObjectByType<InventarioGiocatore>();
            bool hoLenteInMano = (inventario != null && inventario.haUnOggetto && inventario.categoriaInMano == OggettoRaccolta.TipoOggetto.Lente);

            if ((faseFotografia && hoLenteInMano) || faseRevisione)
            {
                evidenziatore.Accendi();
            }
            else
            {
                evidenziatore.Spegni();
            }
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

        Debug.Log("[Camera] Spostamento ATTIVO. Usa WASD/Frecce. (Premi E per uscire)");
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

        if (GameManager.instance != null)
        {
            GameManager.instance.cameraPosizionata = true;
            Debug.Log("<color=green>[Camera] Posizione Salvata!</color>");
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

    void OnDrawGizmosSelected()
    {
        if (usaLimiti)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f); 
            float centroX = (minX + maxX) / 2;
            float centroZ = (minZ + maxZ) / 2;
            float larghezza = maxX - minX;
            float profondita = maxZ - minZ;

            Vector3 centro = new Vector3(centroX, transform.position.y, centroZ);
            Vector3 dimensione = new Vector3(larghezza, 1f, profondita);

            Gizmos.DrawCube(centro, dimensione);
            Gizmos.DrawWireCube(centro, dimensione);
        }
    }
}