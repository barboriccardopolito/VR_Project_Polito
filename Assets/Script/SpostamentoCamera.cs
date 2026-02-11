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
    public float minX = -10f; // Sinistra
    public float maxX = 10f;  // Destra
    public float minZ = -10f; // Dietro
    public float maxZ = 10f;  // Avanti

    [Header("Riferimenti Player")]
    public GameObject giocatore; 

    [Header("Nomi Esatti degli Script da bloccare")]
    public string[] nomiScriptDaDisabilitare; 

    // Riferimento all'anello luminoso
    private Evidenziatore evidenziatore;

    private bool inModalitaSpostamento = false;
    private bool possoUscire = false; 

    void Start()
    {
        if (cameraDallAlto != null) cameraDallAlto.gameObject.SetActive(false);
        if (cameraGiocatore == null) cameraGiocatore = Camera.main;

        // Cerca l'evidenziatore su questo oggetto o nei figli
        evidenziatore = GetComponent<Evidenziatore>();
        if (evidenziatore == null) evidenziatore = GetComponentInChildren<Evidenziatore>();
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

    void EntraInModalitaSpostamento()
    {
        inModalitaSpostamento = true;
        possoUscire = false; 
        
        BloccaGiocatore(true);

        if (cameraGiocatore != null) cameraGiocatore.enabled = false;
        if (cameraDallAlto != null) cameraDallAlto.gameObject.SetActive(true);

        Debug.Log("[Camera] Spostamento ATTIVO. Usa WASD/Frecce. (Premi E per uscire)");
        
        StartCoroutine(AbilitaUscitaRoutine());
    }

    IEnumerator AbilitaUscitaRoutine()
    {
        yield return new WaitForSeconds(1.0f);
        possoUscire = true;
    }

    void EsciDaModalitaSpostamento()
    {
        inModalitaSpostamento = false;
        possoUscire = false;

        if (cameraDallAlto != null) cameraDallAlto.gameObject.SetActive(false);
        if (cameraGiocatore != null) cameraGiocatore.enabled = true;

        BloccaGiocatore(false);

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

        // Calcolo direzione basata sulla rotazione della camera
        Vector3 camRight = cameraDallAlto.transform.right;
        Vector3 camForward = cameraDallAlto.transform.up; // O transform.forward a seconda di come è ruotata la tua cam dall'alto

        camRight.y = 0;
        camForward.y = 0;
        camRight.Normalize();
        camForward.Normalize();

        Vector3 move = (camRight * x + camForward * z) * velocitaSpostamento * Time.deltaTime;
        
        // Applica movimento
        transform.Translate(move, Space.World);

        // --- BLOCCO LIMITI (Nuova parte) ---
        if (usaLimiti)
        {
            Vector3 pos = transform.position;
            
            // Blocca la X e la Z dentro i valori minimi e massimi
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
            
            transform.position = pos;
        }
    }

    void BloccaGiocatore(bool blocca)
    {
        if (giocatore == null) return;

        // Blocca script movimento (MouseLook, PlayerMovement)
        if (nomiScriptDaDisabilitare != null)
        {
            foreach (string nomeScript in nomiScriptDaDisabilitare)
            {
                MonoBehaviour scriptPlayer = giocatore.GetComponent(nomeScript) as MonoBehaviour;
                if (scriptPlayer != null) scriptPlayer.enabled = !blocca;
                
                // Cerca anche nella camera figlia (spesso MouseLook è lì)
                if (cameraGiocatore != null)
                {
                    MonoBehaviour scriptCam = cameraGiocatore.GetComponent(nomeScript) as MonoBehaviour;
                    if (scriptCam != null) scriptCam.enabled = !blocca;
                }
            }
        }

        // Blocca CharacterController (fisica)
        CharacterController cc = giocatore.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = !blocca;
        
        // Nascondi cursore
        if (blocca) 
        { 
            Cursor.lockState = CursorLockMode.Locked; 
            Cursor.visible = false; 
        }
    }

    // --- AIUTO VISIVO (Disegna l'area rossa nell'Editor) ---
    void OnDrawGizmosSelected()
    {
        if (usaLimiti)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f); // Rosso semi-trasparente
            
            float centroX = (minX + maxX) / 2;
            float centroZ = (minZ + maxZ) / 2;
            float larghezza = maxX - minX;
            float profondita = maxZ - minZ;

            // Disegna cubo che rappresenta l'area consentita
            Vector3 centro = new Vector3(centroX, transform.position.y, centroZ);
            Vector3 dimensione = new Vector3(larghezza, 1f, profondita);

            Gizmos.DrawCube(centro, dimensione);
            Gizmos.DrawWireCube(centro, dimensione);
        }
    }
}