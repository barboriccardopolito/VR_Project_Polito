using UnityEngine;
using UnityEngine.UI; // Necessario per toccare l'Image

public class InterazioneGiocatore : MonoBehaviour
{
    [Header("Collegamenti")]
    public Transform cameraGiocatore;
    public GameObject widgetInterazione; 
    
    [Header("Mirino Dinamico (NUOVO)")]
    public Image mirinoUI;          // Trascina qui l'Image del mirino
    public Color coloreNormale = new Color(1, 1, 1, 0.5f); // Bianco semitrasparente
    public Color coloreAttivo = new Color(1, 0, 0, 0.8f);  // Rosso acceso (o Verde)
    public Vector3 scalaNormale = Vector3.one;
    public Vector3 scalaAttiva = new Vector3(2f, 2f, 2f);  // Diventa doppio quando guardi

    [Header("Settaggi")]
    public float distanzaInterazione = 4f;
    public LayerMask layerDaColpire;
    public Vector3 offsetGrafico = new Vector3(0, 0.1f, 0);

    void Start()
    {
        if (widgetInterazione != null) widgetInterazione.SetActive(false);
        // Setta il mirino a riposo
        if (mirinoUI != null) ResetMirino();
    }

    void Update()
    {
        ControlloRaggio();
        if (Input.GetKeyDown(KeyCode.E)) TentativoInterazione();
    }

    void ControlloRaggio()
    {
        if (cameraGiocatore == null) return;

        Ray raggio = new Ray(cameraGiocatore.position, cameraGiocatore.forward);
        RaycastHit hit;

        if (Physics.Raycast(raggio, out hit, distanzaInterazione, layerDaColpire))
        {
            if (hit.collider.CompareTag("Interagibile") || 
                hit.collider.CompareTag("Lente") || 
                hit.collider.CompareTag("Raccoglibile"))
            {
                MostraWidget(hit);
                AttivaMirino(); // <--- Il mirino reagisce!
                return;
            }
        }

        // Se non colpisco nulla
        if (widgetInterazione != null) widgetInterazione.SetActive(false);
        ResetMirino(); // <--- Il mirino torna normale
    }

    void MostraWidget(RaycastHit hit)
    {
        if (widgetInterazione == null) return;
        widgetInterazione.SetActive(true);
        
        // Calcolo anti-compenetrazione
        Vector3 direzione = (cameraGiocatore.position - hit.point).normalized;
        widgetInterazione.transform.position = hit.point + offsetGrafico + (direzione * 0.2f);
        
        widgetInterazione.transform.LookAt(cameraGiocatore);
        widgetInterazione.transform.Rotate(0, 180, 0);
    }

    // --- NUOVE FUNZIONI MIRINO ---
    void AttivaMirino()
    {
        if (mirinoUI == null) return;
        // Cambia colore e grandezza in modo fluido (Lerp) per renderlo elegante
        mirinoUI.color = Color.Lerp(mirinoUI.color, coloreAttivo, Time.deltaTime * 10f);
        mirinoUI.transform.localScale = Vector3.Lerp(mirinoUI.transform.localScale, scalaAttiva, Time.deltaTime * 10f);
    }

    void ResetMirino()
    {
        if (mirinoUI == null) return;
        mirinoUI.color = Color.Lerp(mirinoUI.color, coloreNormale, Time.deltaTime * 10f);
        mirinoUI.transform.localScale = Vector3.Lerp(mirinoUI.transform.localScale, scalaNormale, Time.deltaTime * 10f);
    }

    void TentativoInterazione()
    {
       // ... (Copia qui la tua logica di interazione di prima) ...
       // ... è uguale a prima ...
        if (cameraGiocatore == null) return;
        Ray raggio = new Ray(cameraGiocatore.position, cameraGiocatore.forward);
        RaycastHit hit;
        if (Physics.Raycast(raggio, out hit, distanzaInterazione, layerDaColpire))
        {
             if (hit.collider.CompareTag("Interagibile"))
            {
                SpostamentoCamera spostaCam = hit.collider.GetComponent<SpostamentoCamera>();
                if (spostaCam == null) spostaCam = hit.collider.GetComponentInParent<SpostamentoCamera>();
                if (spostaCam != null) { spostaCam.Interagisci(); return; }

                SupportoLuce supportoLuce = hit.collider.GetComponent<SupportoLuce>();
                if (supportoLuce != null) { supportoLuce.PiazzaLuce(); return; }
                
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
    }
}