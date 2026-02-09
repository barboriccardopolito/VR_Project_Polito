using UnityEngine;
using System.Collections;

public class SpostamentoCamera : MonoBehaviour
{
    [Header("Setup Visuale")]
    public Camera cameraDallAlto; 
    public Camera cameraGiocatore; 

    [Header("Movimento")]
    public float velocitaSpostamento = 3.0f;
    
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
        GestisciEvidenziatore();

        // --- LOGICA MOVIMENTO ---
        if (inModalitaSpostamento)
        {
            GestisciMovimento();

            // Uscita con tasto E (SOLO SE il cooldown è finito)
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (possoUscire)
                {
                    EsciDaModalitaSpostamento();
                }
                else
                {
                    Debug.Log("⏳ Aspetta... sto inizializzando la camera.");
                }
            }
        }
    }

    void GestisciEvidenziatore()
    {
        if (evidenziatore == null) return;

        // Se sono DENTRO la camera, spengo l'anello (altrimenti mi dà fastidio alla vista)
        if (inModalitaSpostamento)
        {
            evidenziatore.Spegni();
            return;
        }

        // Logica di accensione:
        // 1. Siamo nel reparto Fotografia O in Revisione (Regia)
        bool faseFotografia = (GameManager.instance.taskAttuale == GameManager.Reparto.Fotografia);
        bool faseRevisione = (GameManager.instance.taskAttuale == GameManager.Reparto.Regia);
        
        // 2. Abbiamo consegnato la lente al fotografo? (Solo se ho la lente posso muovere la camera)
        bool hoLaLente = (GameManager.instance.lenteSceltaFinale != "");

        // ACCENDITI SE: (È il momento giusto) E (Ho la lente installata)
        if ((faseFotografia || faseRevisione) && hoLaLente)
        {
            evidenziatore.Accendi();
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
        
        BloccaGiocatore(true);

        if (cameraGiocatore != null) cameraGiocatore.enabled = false;
        if (cameraDallAlto != null) cameraDallAlto.gameObject.SetActive(true);

        Debug.Log("[Camera] Spostamento ATTIVO. Usa WASD. (Premi E tra 1 secondo per uscire)");
        
        StartCoroutine(AbilitaUscitaRoutine());
    }

    IEnumerator AbilitaUscitaRoutine()
    {
        yield return new WaitForSeconds(1.0f);
        possoUscire = true;
        Debug.Log("✅ Ora puoi premere E per uscire.");
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

        Vector3 camRight = cameraDallAlto.transform.right;
        Vector3 camForward = cameraDallAlto.transform.up; 

        camRight.y = 0;
        camForward.y = 0;
        camRight.Normalize();
        camForward.Normalize();

        Vector3 move = (camRight * x + camForward * z) * velocitaSpostamento * Time.deltaTime;
        transform.Translate(move, Space.World);
    }

    void BloccaGiocatore(bool blocca)
    {
        if (giocatore == null) return;
        foreach (string nomeScript in nomiScriptDaDisabilitare)
        {
            MonoBehaviour scriptTrovato = giocatore.GetComponent(nomeScript) as MonoBehaviour;
            if (scriptTrovato != null) scriptTrovato.enabled = !blocca;
            else if (cameraGiocatore != null) {
                scriptTrovato = cameraGiocatore.GetComponent(nomeScript) as MonoBehaviour;
                if (scriptTrovato != null) scriptTrovato.enabled = !blocca;
            }
        }
        CharacterController cc = giocatore.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = !blocca;
        
        if (blocca) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
    }
}