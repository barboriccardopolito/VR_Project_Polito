using UnityEngine;
using System.Collections;

public class SpostamentoCamera : MonoBehaviour
{
    [Header("Setup Visuale")]
    public Camera cameraDallAlto; 
    public Camera cameraGiocatore; 

    [Header("Movimento")]
    public float velocitaSpostamento = 3.0f;
    
    [Header("LIMITI STANZA (Confini)")]
    public bool usaLimiti = true;
    public float minX = -10f; 
    public float maxX = 10f;  
    public float minZ = -10f; 
    public float maxZ = 10f;  

    [Header("Riferimenti Player")]
    public GameObject giocatore; 

    [Header("Nomi Esatti degli Script da bloccare")]
    public string[] nomiScriptDaDisabilitare; 

    // Riferimento all'anello luminoso
    private Evidenziatore evidenziatore;
    private Collider mioCollider; // <--- Riferimento al Collider

    private bool inModalitaSpostamento = false;
    private bool possoUscire = false; 

    void Start()
    {
        if (cameraDallAlto != null) cameraDallAlto.gameObject.SetActive(false);
        if (cameraGiocatore == null) cameraGiocatore = Camera.main;

        // Cerca l'evidenziatore
        evidenziatore = GetComponent<Evidenziatore>();
        if (evidenziatore == null) evidenziatore = GetComponentInChildren<Evidenziatore>();

        // Cerca il collider (per disabilitarlo durante lo spostamento)
        mioCollider = GetComponent<Collider>();
    }

    // Chiamata dallo script InterazioneGiocatore
    public void Interagisci()
    {
        if (inModalitaSpostamento) return;
        EntraInModalitaSpostamento();
    }

    void Update()
    {
        // --- LOGICA ANELLO LUMINOSO ---
        GestisciEvidenziatore();

        // --- LOGICA MOVIMENTO ---
        if (inModalitaSpostamento)
        {
            GestisciMovimento();

            // Uscita con tasto E (SOLO SE il cooldown è finito)
            if (Input.GetKeyDown(KeyCode.E) && possoUscire)
            {
                EsciDaModalitaSpostamento();
            }
        }
    }

    void GestisciEvidenziatore()
    {
        if (evidenziatore != null)
        {
            if (inModalitaSpostamento)
            {
                evidenziatore.Spegni();
            }
            else
            {
                // Accendi solo se è il momento giusto (Fotografia o Revisione)
                bool faseFotografia = (GameManager.instance != null && GameManager.instance.taskAttuale == GameManager.Reparto.Fotografia);
                bool faseRevisione = (GameManager.instance != null && GameManager.instance.taskAttuale == GameManager.Reparto.Regia);
                bool hoLaLente = (GameManager.instance != null && !string.IsNullOrEmpty(GameManager.instance.lenteSceltaFinale));

                if ((faseFotografia || faseRevisione) && hoLaLente) evidenziatore.Accendi();
                else evidenziatore.Spegni();
            }
        }
    }

    void EntraInModalitaSpostamento()
    {
        inModalitaSpostamento = true;
        possoUscire = false; // Blocco uscita immediata
        
        // --- DISABILITA COLLIDER (Così il Raycast non la vede più!) ---
        if (mioCollider != null) mioCollider.enabled = false;

        BloccaGiocatore(true);

        if (cameraGiocatore != null) cameraGiocatore.enabled = false;
        if (cameraDallAlto != null) cameraDallAlto.gameObject.SetActive(true);

        Debug.Log("[Camera] Spostamento ATTIVO. Usa WASD/Frecce. (Premi E per uscire)");
        
        // Avvia il timer di sicurezza
        StartCoroutine(TimerSbloccoUscita());
    }

    IEnumerator TimerSbloccoUscita()
    {
        // Aspetta mezzo secondo per evitare il "doppio click" accidentale
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
        
        // --- RIABILITA COLLIDER (Ora puoi interagire di nuovo) ---
        if (mioCollider != null) mioCollider.enabled = true;

        if (GameManager.instance != null)
        {
            GameManager.instance.cameraPosizionata = true;
            Debug.Log("<color=green>[Camera] Posizione Salvata! Torna dall'addetto.</color>");
        }
    }

    void GestisciMovimento()
    {
        float x = Input.GetAxis("Horizontal"); 
        float z = Input.GetAxis("Vertical");   

        // Calcolo direzione basata sulla rotazione della camera dall'alto
        Vector3 camRight = cameraDallAlto.transform.right;
        Vector3 camForward = cameraDallAlto.transform.up; // Per camere top-down spesso è UP

        camRight.y = 0;
        camForward.y = 0;
        camRight.Normalize();
        camForward.Normalize();

        Vector3 move = (camRight * x + camForward * z) * velocitaSpostamento * Time.deltaTime;
        
        // Applica movimento locale ma rispetto al mondo
        transform.Translate(move, Space.World);

        // --- BLOCCO LIMITI ---
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

        // Blocca script movimento
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

        // Blocca CharacterController
        CharacterController cc = giocatore.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = !blocca;
        
        // Gestione cursore
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