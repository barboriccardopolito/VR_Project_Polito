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

    private bool inModalitaSpostamento = false;
    private bool possoUscire = false; // VARIABILE CRITICA PER EVITARE IL BUG

    void Start()
    {
        if (cameraDallAlto != null) cameraDallAlto.gameObject.SetActive(false);
        if (cameraGiocatore == null) cameraGiocatore = Camera.main;
    }

    // Chiamata dallo script InterazioneGiocatore
    public void Interagisci()
    {
        // Se sono già dentro, ignoro la chiamata esterna (gestisco l'uscita in Update)
        // Questo evita che InterazioneGiocatore forzi il rientro mentre cerco di uscire
        if (inModalitaSpostamento) return;

        EntraInModalitaSpostamento();
    }

    void Update()
    {
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

    void EntraInModalitaSpostamento()
    {
        inModalitaSpostamento = true;
        possoUscire = false; // <--- BLOCCO IMMEDIATO
        
        BloccaGiocatore(true);

        if (cameraGiocatore != null) cameraGiocatore.enabled = false;
        if (cameraDallAlto != null) cameraDallAlto.gameObject.SetActive(true);

        Debug.Log("[Camera] Spostamento ATTIVO. Usa WASD. (Premi E tra 1 secondo per uscire)");
        
        // Avvia il timer di sicurezza
        StartCoroutine(AbilitaUscitaRoutine());
    }

    IEnumerator AbilitaUscitaRoutine()
    {
        // Aspetta 1 secondo reale prima di permettere di premere E di nuovo
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

        // SALVATAGGIO STATO
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

        // Movimento relativo alla rotazione della camera (Fix precedente)
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