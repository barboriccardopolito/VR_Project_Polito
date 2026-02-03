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
        Debug.Log("--- TASTO E PREMUTO ---"); // 1. Se vedi questo, il tasto funziona

        Ray raggio = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(raggio, out hit, distanzaInterazione))
        {
            Debug.Log("Ho colpito: " + hit.collider.name + " | Tag: " + hit.collider.tag); // 2. Ti dice cosa hai toccato

            if (hit.collider.CompareTag("Interagibile"))
            {
                // --- TEST SPECIFICO PER SUPPORTI LUCI ---
                SupportoLuce supportoLuce = hit.collider.GetComponent<SupportoLuce>();
                if (supportoLuce != null)
                {
                    Debug.Log("TROVATO SCRIPT SUPPORTOLUCE! Provo ad attivare...");
                    supportoLuce.PiazzaLuce();
                    return;
                }
                else
                {
                    Debug.Log("Oggetto Interagibile colpito, ma NON ha lo script SupportoLuce.");
                }

                // --- ALTRI CONTROLLI ---
                InteragibileNPC npc = hit.collider.GetComponent<InteragibileNPC>();
                if (npc != null) { npc.Interagisci(); return; }

                SupportoMicrofono supportoMic = hit.collider.GetComponent<SupportoMicrofono>();
                if (supportoMic != null) { supportoMic.PiazzaMicrofono(); return; }

                MacchinettaCaffe caffe = hit.collider.GetComponent<MacchinettaCaffe>();
                if (caffe != null) { caffe.SpegniMacchinetta(); return; }
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