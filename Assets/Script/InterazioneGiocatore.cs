using UnityEngine;
using UnityEngine.UI; // NECESSARIO per l'Image del mirino
using TMPro; // Se usi TextMeshPro nel widget 3D

public class InterazioneGiocatore : MonoBehaviour
{
    [Header("Collegamenti")]
    public Transform cameraGiocatore;
    public GameObject widgetInterazione; // L'etichetta 3D (Canvas World Space) o Testo UI
    
    [Header("Mirino Dinamico")]
    public Image mirinoUI;               // Trascina qui l'Image del mirino (Canvas 2D)
    public Color coloreRiposo = new Color(1, 1, 1, 0.4f); // Bianco semi-trasparente
    public Color coloreAttivo = Color.red;                // Rosso quando punti qualcosa
    public float scalaRiposo = 1f;       // Grandezza normale
    public float scalaAttiva = 2.5f;     // Grandezza quando punti (si espande)
    public float velocitaAnimazione = 10f; // Quanto è veloce il cambio

    [Header("Settaggi Raycast")]
    public float distanzaInterazione = 4f;
    public LayerMask layerDaColpire;     // Imposta su "Default", "Interactable", "NPC"
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

    // --- RILEVAMENTO VISIVO (Per accendere il mirino) ---
    void ControlloRaggio()
    {
        if (cameraGiocatore == null) return;

        Ray raggio = new Ray(cameraGiocatore.position, cameraGiocatore.forward);
        RaycastHit hit;
        bersaglioAgganciato = false; // Reset ogni frame

        // Se il raggio colpisce qualcosa
        if (Physics.Raycast(raggio, out hit, distanzaInterazione, layerDaColpire))
        {
            // Controlliamo se l'oggetto ha uno script interagibile
            bool trovatoQualcosa = false;

            // 1. NPC e Staff
            if (hit.collider.GetComponent<InteragibileNPC>() != null) trovatoQualcosa = true;
            
            // 2. Oggetti da raccogliere (Lenti, Luci, Mic)
            if (hit.collider.GetComponent<OggettoRaccolta>() != null) trovatoQualcosa = true;

            // 3. Supporti (Luci e Audio Boom/Ambisonic)
            if (hit.collider.GetComponent<SupportoMicrofono>() != null) trovatoQualcosa = true;
            if (hit.collider.GetComponent<SupportoLuce>() != null) trovatoQualcosa = true;

            // 4. Videocamera (Spostamento)
            if (hit.collider.GetComponent<SpostamentoCamera>() != null || hit.collider.CompareTag("Videocamera")) trovatoQualcosa = true;

            // 5. ATTORE (Solo se dobbiamo mettere i Lavalier)
            if (GameManager.instance.micDaInstallare == "Lavalier")
            {
                if (hit.collider.GetComponent<AttoreMicrofonabile>() != null) trovatoQualcosa = true;
            }

            // 6. Macchinetta Caffè
            if (hit.collider.GetComponent<MacchinettaCaffe>() != null) trovatoQualcosa = true;

            // --- RISULTATO ---
            if (trovatoQualcosa)
            {
                MostraWidget(hit);
                bersaglioAgganciato = true;
            }
            else
            {
                if (widgetInterazione != null) widgetInterazione.SetActive(false);
            }
        }
        else
        {
            // Se guardo il vuoto
            if (widgetInterazione != null) widgetInterazione.SetActive(false);
        }
    }

    // --- ANIMAZIONE MIRINO ---
    void AnimaMirino()
    {
        if (mirinoUI == null) return;

        // Decidiamo i valori target
        Color targetColor = bersaglioAgganciato ? coloreAttivo : coloreRiposo;
        float targetScale = bersaglioAgganciato ? scalaAttiva : scalaRiposo;

        // Interpolazione (Lerp)
        mirinoUI.color = Color.Lerp(mirinoUI.color, targetColor, Time.deltaTime * velocitaAnimazione);
        
        Vector3 nuovaScala = Vector3.Lerp(mirinoUI.transform.localScale, Vector3.one * targetScale, Time.deltaTime * velocitaAnimazione);
        mirinoUI.transform.localScale = nuovaScala;
    }

    void MostraWidget(RaycastHit hit)
    {
        if (widgetInterazione == null) return;

        widgetInterazione.SetActive(true);
        // Posizionamento leggermente spostato verso la camera per non entrare nell'oggetto
        Vector3 direzione = (cameraGiocatore.position - hit.point).normalized;
        widgetInterazione.transform.position = hit.point + offsetGrafico + (direzione * 0.2f);
        
        widgetInterazione.transform.LookAt(cameraGiocatore);
        widgetInterazione.transform.Rotate(0, 180, 0); // Correzione rotazione per UI World Space
    }

    // --- ESECUZIONE INTERAZIONE (Tasto E) ---
    void TentativoInterazione()
    {
        if (cameraGiocatore == null) return;
        Ray raggio = new Ray(cameraGiocatore.position, cameraGiocatore.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(raggio, out hit, distanzaInterazione, layerDaColpire))
        {
            // --- ORDINE DI CONTROLLO ---

            // 1. ATTORE (Nuova logica Lavalier)
            AttoreMicrofonabile attore = hit.collider.GetComponent<AttoreMicrofonabile>();
            if (attore != null)
            {
                attore.ProvaAMicrofonare();
                return;
            }

            // 2. NPC (Staff, Regista)
            InteragibileNPC npc = hit.collider.GetComponent<InteragibileNPC>();
            if (npc != null) { npc.Interagisci(); return; }

            // 3. OGGETTI (Lenti, Luci, Mic)
            OggettoRaccolta obj = hit.collider.GetComponent<OggettoRaccolta>();
            if (obj != null) { obj.EseguiRaccolta(); return; }

            // 4. VIDEOCAMERA
            SpostamentoCamera spostaCam = hit.collider.GetComponent<SpostamentoCamera>();
            if (spostaCam == null) spostaCam = hit.collider.GetComponentInParent<SpostamentoCamera>(); // Cerca nel padre se colpisci la lente
            if (spostaCam != null) { spostaCam.Interagisci(); return; }
            if (hit.collider.CompareTag("Videocamera")) // Fallback Tag
            {
                GameManager.instance.cameraPosizionata = true;
                Debug.Log("Camera posizionata via Tag.");
                return;
            }

            // 5. SUPPORTI LUCI
            SupportoLuce supportoLuce = hit.collider.GetComponent<SupportoLuce>();
            if (supportoLuce != null) { supportoLuce.PiazzaLuce(); return; }

            // 6. SUPPORTI MICROFONI (Boom/Ambisonic)
            SupportoMicrofono supportoMic = hit.collider.GetComponent<SupportoMicrofono>();
            if (supportoMic != null) { supportoMic.PiazzaMicrofono(); return; }

            // 7. EXTRA
            MacchinettaCaffe caffe = hit.collider.GetComponent<MacchinettaCaffe>();
            if (caffe != null) { caffe.SpegniMacchinetta(); return; }
        }
    }
}