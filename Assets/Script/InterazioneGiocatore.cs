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
            if (hit.collider.CompareTag("Interagibile") || hit.collider.CompareTag("Lente") || hit.collider.CompareTag("Raccoglibile"))
            {
                testoSuggerimento.SetActive(true);
                return;
            }
        }
        testoSuggerimento.SetActive(false);
    }

    void TentativoInterazione()
    {
        Ray raggio = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(raggio, out hit, distanzaInterazione))
        {
            if (hit.collider.CompareTag("Interagibile"))
            {
                // 1. Interazione NPC
                InteragibileNPC npc = hit.collider.GetComponent<InteragibileNPC>();
                if (npc != null) npc.Interagisci();

                // 2. Interazione Attore
                AttoreMicrofonabile attore = hit.collider.GetComponent<AttoreMicrofonabile>();
                if (attore != null) attore.ProvaAMicrofonare();

                // 3. Interazione Supporto (Boom/Ambisonic)
                SupportoMicrofono supporto = hit.collider.GetComponent<SupportoMicrofono>();
                if (supporto != null) supporto.PiazzaMicrofono();

                // 4. Interazione Macchinetta Caffè (NUOVO)
                MacchinettaCaffe caffe = hit.collider.GetComponent<MacchinettaCaffe>();
                if (caffe != null) caffe.SpegniMacchinetta();
            }
            else if (hit.collider.CompareTag("Lente") || hit.collider.CompareTag("Raccoglibile"))
            {
                OggettoRaccolta obj = hit.collider.GetComponent<OggettoRaccolta>();
                if (obj != null) obj.EseguiRaccolta();
            }
        }
    }
}