using UnityEngine;

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

    void Start()
    {
        if (cameraDallAlto != null) cameraDallAlto.gameObject.SetActive(false);
        if (cameraGiocatore == null) cameraGiocatore = Camera.main;
    }

    public void Interagisci()
    {
        if (inModalitaSpostamento) EsciDaModalitaSpostamento();
        else EntraInModalitaSpostamento();
    }

    void Update()
    {
        if (inModalitaSpostamento)
        {
            GestisciMovimento();
            if (Input.GetKeyDown(KeyCode.E)) EsciDaModalitaSpostamento();
        }
    }

    void EntraInModalitaSpostamento()
    {
        inModalitaSpostamento = true;
        BloccaGiocatore(true);
        if (cameraGiocatore != null) cameraGiocatore.enabled = false;
        if (cameraDallAlto != null) cameraDallAlto.gameObject.SetActive(true);
        Debug.Log("[Camera] Spostamento ATTIVO. Usa WASD.");
    }

    void EsciDaModalitaSpostamento()
    {
        inModalitaSpostamento = false;

        if (cameraDallAlto != null) cameraDallAlto.gameObject.SetActive(false);
        if (cameraGiocatore != null) cameraGiocatore.enabled = true;

        BloccaGiocatore(false);

        // --- PUNTO CRITICO: Salviamo il progresso ---
        if (GameManager.instance != null)
        {
            GameManager.instance.cameraPosizionata = true;
            Debug.Log("<color=green>[Camera] Posizione Confermata e Salvata nel GameManager!</color>");
        }
        else
        {
            Debug.LogError("[Camera] ERRORE: GameManager non trovato! Impossibile salvare.");
        }
        // ---------------------------------------------
    }

    void GestisciMovimento()
        {
            float x = Input.GetAxis("Horizontal"); 
            float z = Input.GetAxis("Vertical");   

            // 1. Prendiamo le direzioni dalla Camera Drone (non dal Mondo)
            // Nota: Per le camere dall'alto (Top-Down), 'transform.up' corrisponde visivamente all'avanti
            Vector3 camRight = cameraDallAlto.transform.right;
            Vector3 camForward = cameraDallAlto.transform.up; 

            // 2. Appiattiamo tutto a zero sull'asse Y (per non volare o sprofondare nel pavimento)
            camRight.y = 0;
            camForward.y = 0;

            // 3. Normalizziamo i vettori (altrimenti muoversi in diagonale sarebbe più veloce)
            camRight.Normalize();
            camForward.Normalize();

            // 4. Calcoliamo il movimento finale combinando gli input con le direzioni della camera
            Vector3 move = (camRight * x + camForward * z) * velocitaSpostamento * Time.deltaTime;
            
            // 5. Applichiamo il movimento
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