using UnityEngine;
using UnityEngine.UI; // NECESSARIO per controllare l'Immagine del mirino

public class InterazioneGiocatore : MonoBehaviour
{
    [Header("Collegamenti")]
    public Transform cameraGiocatore;
    public GameObject widgetInterazione; // L'etichetta 3D (Canvas World Space)
    
    [Header("Mirino Dinamico")]
    public Image mirinoUI;               // Trascina qui l'Image del mirino (Canvas 2D)
    public Color coloreRiposo = new Color(1, 1, 1, 0.4f); // Bianco semi-trasparente
    public Color coloreAttivo = Color.red;                // Rosso pieno
    public float scalaRiposo = 1f;       // Grandezza normale
    public float scalaAttiva = 2.5f;     // Grandezza quando punti (si espande)
    public float velocitaAnimazione = 10f; // Quanto è veloce il cambio

    [Header("Settaggi Raycast")]
    public float distanzaInterazione = 4f;
    public LayerMask layerDaColpire;     // Imposta su "Default" e "Interactable"
    public Vector3 offsetGrafico = new Vector3(0, 0.1f, 0);

    // Variabile privata per sapere se stiamo puntando qualcosa
    private bool bersaglioAgganciato = false;

    void Start()
    {
        if (widgetInterazione != null) widgetInterazione.SetActive(false);
    }

    void Update()
    {
        ControlloRaggio();
        AnimaMirino(); // Gestisce il cambio colore/grandezza ogni frame

        if (Input.GetKeyDown(KeyCode.E)) 
        {
            TentativoInterazione();
        }
    }

    void ControlloRaggio()
    {
        if (cameraGiocatore == null) return;

        Ray raggio = new Ray(cameraGiocatore.position, cameraGiocatore.forward);
        RaycastHit hit;

        // Se il raggio colpisce i layer giusti
        if (Physics.Raycast(raggio, out hit, distanzaInterazione, layerDaColpire))
        {
            // Se l'oggetto ha i tag giusti
            if (hit.collider.CompareTag("Interagibile") || 
                hit.collider.CompareTag("Lente") || 
                hit.collider.CompareTag("Raccoglibile"))
            {
                MostraWidget(hit);
                bersaglioAgganciato = true; // <--- ABBIAMO TROVATO QUALCOSA!
                return;
            }
        }

        // Se arriviamo qui, non stiamo guardando nulla di utile
        if (widgetInterazione != null) widgetInterazione.SetActive(false);
        bersaglioAgganciato = false; // <--- NIENTE BERSAGLIO
    }

    // --- NUOVA FUNZIONE PER ANIMARE IL MIRINO ---
    void AnimaMirino()
    {
        if (mirinoUI == null) return;

        // Decidiamo i valori target in base a se abbiamo agganciato qualcosa o no
        Color targetColor = bersaglioAgganciato ? coloreAttivo : coloreRiposo;
        float targetScale = bersaglioAgganciato ? scalaAttiva : scalaRiposo;

        // Usiamo Lerp per passare gradualmente da A a B
        mirinoUI.color = Color.Lerp(mirinoUI.color, targetColor, Time.deltaTime * velocitaAnimazione);
        
        // Applichiamo la scala (mantiene le proporzioni X e Y uguali)
        Vector3 nuovaScala = Vector3.Lerp(mirinoUI.transform.localScale, Vector3.one * targetScale, Time.deltaTime * velocitaAnimazione);
        mirinoUI.transform.localScale = nuovaScala;
    }

    void MostraWidget(RaycastHit hit)
    {
        if (widgetInterazione == null) return;

        widgetInterazione.SetActive(true);
        // Posizionamento anti-compenetrazione
        Vector3 direzione = (cameraGiocatore.position - hit.point).normalized;
        widgetInterazione.transform.position = hit.point + offsetGrafico + (direzione * 0.2f);
        
        widgetInterazione.transform.LookAt(cameraGiocatore);
        widgetInterazione.transform.Rotate(0, 180, 0);
    }

    void TentativoInterazione()
    {
        if (cameraGiocatore == null) return;
        Ray raggio = new Ray(cameraGiocatore.position, cameraGiocatore.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(raggio, out hit, distanzaInterazione, layerDaColpire))
        {
            // Stessa logica di interazione di prima...
             if (hit.collider.CompareTag("Interagibile") || hit.collider.CompareTag("NPC"))
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