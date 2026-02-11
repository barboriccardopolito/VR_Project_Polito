using UnityEngine;
using UnityEngine.UI; // Per l'Image del mirino
using TMPro; // NECESSARIO per cambiare il testo nel widget

public class InterazioneGiocatore : MonoBehaviour
{
    [Header("Collegamenti")]
    public Transform cameraGiocatore;
    public GameObject widgetInterazione; // L'oggetto Canvas World Space con dentro il testo
    
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
    
    // Riferimento al componente di testo (lo cerchiamo all'avvio)
    private TextMeshProUGUI testoWidget;

    void Start()
    {
        if (widgetInterazione != null)
        {
            testoWidget = widgetInterazione.GetComponentInChildren<TextMeshProUGUI>();
            widgetInterazione.SetActive(false);
        }
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

    // --- RILEVAMENTO VISIVO (Per accendere il mirino e mostrare testo) ---
    void ControlloRaggio()
    {
        if (cameraGiocatore == null) return;

        Ray raggio = new Ray(cameraGiocatore.position, cameraGiocatore.forward);
        RaycastHit hit;
        bersaglioAgganciato = false; // Reset ogni frame

        // Se il raggio colpisce qualcosa
        if (Physics.Raycast(raggio, out hit, distanzaInterazione, layerDaColpire))
        {
            bool trovatoQualcosa = false;
            string messaggioDaMostrare = "[E] INTERAGISCI"; // Messaggio di default

            // 1. OGGETTI DA RACCOGLIERE (Lenti, Luci, Mic) - PRIORITÀ ALTA
            OggettoRaccolta oggetto = hit.collider.GetComponent<OggettoRaccolta>();
            if (oggetto != null)
            {
                trovatoQualcosa = true;
                // --- QUI LEGGIAMO IL NOME SPECIFICO ---
                messaggioDaMostrare = "[E] RACCOGLI " + oggetto.nomeOggetto.ToUpper();
            }

            // 2. NPC e Staff
            else if (hit.collider.GetComponent<InteragibileNPC>() != null) 
            {
                trovatoQualcosa = true;
                messaggioDaMostrare = "[E] PARLA";
            }
            
            // 3. Supporti (Luci e Audio Boom/Ambisonic)
            else if (hit.collider.GetComponent<SupportoLuce>() != null) 
            {
                trovatoQualcosa = true;
                messaggioDaMostrare = "[E] PIAZZA LUCE";
            }
            else if (hit.collider.GetComponent<SupportoMicrofono>() != null)
            {
                trovatoQualcosa = true;
                messaggioDaMostrare = "[E] PIAZZA MICROFONO";
            }

            // 4. Videocamera (Spostamento)
            else if (hit.collider.GetComponent<SpostamentoCamera>() != null || hit.collider.CompareTag("Videocamera")) 
            {
                trovatoQualcosa = true;
                messaggioDaMostrare = "[E] SPOSTA CAMERA";
            }

            // 5. ATTORE (Solo se dobbiamo mettere i Lavalier)
            else if (GameManager.instance.micDaInstallare == "Lavalier" && hit.collider.GetComponent<AttoreMicrofonabile>() != null)
            {
                trovatoQualcosa = true;
                messaggioDaMostrare = "[E] MICROFONA ATTORE";
            }

            // 6. Macchinetta Caffè
            else if (hit.collider.GetComponent<MacchinettaCaffe>() != null) 
            {
                trovatoQualcosa = true;
                messaggioDaMostrare = "[E] SPEGNI";
            }

            // --- GESTIONE WIDGET ---
            if (trovatoQualcosa)
            {
                MostraWidget(hit, messaggioDaMostrare);
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

    void MostraWidget(RaycastHit hit, string testo)
    {
        if (widgetInterazione == null) return;

        widgetInterazione.SetActive(true);
        
        // Aggiorna il testo se abbiamo il componente
        if (testoWidget != null) testoWidget.text = testo;

        // Posizionamento leggermente spostato verso la camera per non entrare nell'oggetto
        Vector3 direzione = (cameraGiocatore.position - hit.point).normalized;
        widgetInterazione.transform.position = hit.point + offsetGrafico + (direzione * 0.2f);
        
        // Fai guardare il widget verso il giocatore
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

            // 1. OGGETTI (Lenti, Luci, Mic)
            OggettoRaccolta obj = hit.collider.GetComponent<OggettoRaccolta>();
            if (obj != null) { obj.EseguiRaccolta(); return; }

            // 2. ATTORE (Nuova logica Lavalier)
            AttoreMicrofonabile attore = hit.collider.GetComponent<AttoreMicrofonabile>();
            if (attore != null)
            {
                attore.ProvaAMicrofonare();
                return;
            }

            // 3. NPC (Staff, Regista)
            InteragibileNPC npc = hit.collider.GetComponent<InteragibileNPC>();
            if (npc != null) { npc.Interagisci(); return; }

            // 4. VIDEOCAMERA
            SpostamentoCamera spostaCam = hit.collider.GetComponent<SpostamentoCamera>();
            if (spostaCam == null) spostaCam = hit.collider.GetComponentInParent<SpostamentoCamera>(); 
            if (spostaCam != null) { spostaCam.Interagisci(); return; }
            if (hit.collider.CompareTag("Videocamera")) 
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