using UnityEngine;

public class InterazioneGiocatore : MonoBehaviour
{
    public float distanzaInterazione = 5f;
    public GameObject testoSuggerimento;

    void Update()
    {
        ControlloRaggio();
        if (Input.GetKeyDown(KeyCode.E)) TentativoInterazione();
    }

    void ControlloRaggio()
    {
        Ray raggio = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(raggio, out hit, distanzaInterazione))
        {
            // Mostriamo il testo se il tag è giusto
            if (hit.collider.CompareTag("Interagibile") || hit.collider.CompareTag("Lente") || hit.collider.CompareTag("Raccoglibile"))
            {
                if (testoSuggerimento) testoSuggerimento.SetActive(true);
                return;
            }
        }
        if (testoSuggerimento) testoSuggerimento.SetActive(false);
    }

    void TentativoInterazione()
    {
        Debug.Log("--- TASTO E PREMUTO ---"); 

        Ray raggio = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(raggio, out hit, distanzaInterazione))
        {
            Debug.Log("Ho colpito: " + hit.collider.name + " | Tag: " + hit.collider.tag); 

            if (hit.collider.CompareTag("Interagibile"))
            {
                // --- 1. NUOVO: CONTROLLO VIDEOCAMERA (Spostamento) ---
                // Cerchiamo lo script sull'oggetto colpito o sul suo genitore (per sicurezza)
                SpostamentoCamera spostaCam = hit.collider.GetComponent<SpostamentoCamera>();
                if (spostaCam == null) spostaCam = hit.collider.GetComponentInParent<SpostamentoCamera>();

                if (spostaCam != null)
                {
                    Debug.Log("TROVATO SpostamentoCamera! Interagisco...");
                    spostaCam.Interagisci();
                    return; // Interrompiamo qui, abbiamo trovato la camera
                }

                // --- 2. CONTROLLO SUPPORTI LUCI ---
                SupportoLuce supportoLuce = hit.collider.GetComponent<SupportoLuce>();
                if (supportoLuce != null)
                {
                    Debug.Log("TROVATO SCRIPT SUPPORTOLUCE! Provo ad attivare...");
                    supportoLuce.PiazzaLuce();
                    return;
                }
                
                // --- 3. ALTRI CONTROLLI (NPC, Mic, Caffè) ---
                InteragibileNPC npc = hit.collider.GetComponent<InteragibileNPC>();
                if (npc != null) { npc.Interagisci(); return; }

                SupportoMicrofono supportoMic = hit.collider.GetComponent<SupportoMicrofono>();
                if (supportoMic != null) { supportoMic.PiazzaMicrofono(); return; }

                MacchinettaCaffe caffe = hit.collider.GetComponent<MacchinettaCaffe>();
                if (caffe != null) { caffe.SpegniMacchinetta(); return; }

                // Se arrivi qui, è un oggetto "Interagibile" ma senza script noti
                Debug.Log("Oggetto Interagibile colpito, ma non ha script specifici (Luce, Camera, NPC, ecc).");
            }
            else if (hit.collider.CompareTag("Lente") || hit.collider.CompareTag("Raccoglibile"))
            {
                OggettoRaccolta obj = hit.collider.GetComponent<OggettoRaccolta>();
                if (obj != null) obj.EseguiRaccolta();
            }
        }
        else
        {
            Debug.Log("Raggio non ha colpito nulla (sei troppo lontano?)");
        }
    }
}