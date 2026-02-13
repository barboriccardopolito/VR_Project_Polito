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
    public LayerMask layerDaColpire;
    public Vector3 offsetGrafico = new Vector3(0, 0.1f, 0);

    private bool bersaglioAgganciato = false;
    
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
        AnimaMirino();

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
        bersaglioAgganciato = false;

        if (Physics.Raycast(raggio, out hit, distanzaInterazione, layerDaColpire))
        {
            bool trovatoQualcosa = false;
            string messaggioDaMostrare = "[E] INTERAGISCI";

            OggettoRaccolta oggetto = hit.collider.GetComponent<OggettoRaccolta>();
            if (oggetto != null)
            {
                trovatoQualcosa = true;
                messaggioDaMostrare = "[E] RACCOGLI " + oggetto.nomeOggetto.ToUpper();
            }

            else if (hit.collider.GetComponent<InteragibileNPC>() != null) 
            {
                trovatoQualcosa = true;
                messaggioDaMostrare = "[E] PARLA";
            }
            
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

            else if (hit.collider.GetComponent<SpostamentoCamera>() != null || hit.collider.CompareTag("Videocamera")) 
            {
                trovatoQualcosa = true;
                messaggioDaMostrare = "[E] SPOSTA CAMERA";
            }

            else if (GameManager.instance.micDaInstallare == "Lavalier" && hit.collider.GetComponent<AttoreMicrofonabile>() != null)
            {
                trovatoQualcosa = true;
                messaggioDaMostrare = "[E] MICROFONA ATTORE";
            }

            else if (hit.collider.GetComponent<MacchinettaCaffe>() != null) 
            {
                trovatoQualcosa = true;
                messaggioDaMostrare = "[E] SPEGNI";
            }

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
            if (widgetInterazione != null) widgetInterazione.SetActive(false);
        }
    }

    void AnimaMirino()
    {
        if (mirinoUI == null) return;

        Color targetColor = bersaglioAgganciato ? coloreAttivo : coloreRiposo;
        float targetScale = bersaglioAgganciato ? scalaAttiva : scalaRiposo;

        mirinoUI.color = Color.Lerp(mirinoUI.color, targetColor, Time.deltaTime * velocitaAnimazione);
        
        Vector3 nuovaScala = Vector3.Lerp(mirinoUI.transform.localScale, Vector3.one * targetScale, Time.deltaTime * velocitaAnimazione);
        mirinoUI.transform.localScale = nuovaScala;
    }

    void MostraWidget(RaycastHit hit, string testo)
    {
        if (widgetInterazione == null) return;

        widgetInterazione.SetActive(true);
        
        if (testoWidget != null) testoWidget.text = testo;

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
            OggettoRaccolta obj = hit.collider.GetComponent<OggettoRaccolta>();
            if (obj != null) { obj.EseguiRaccolta(); return; }

            AttoreMicrofonabile attore = hit.collider.GetComponent<AttoreMicrofonabile>();
            if (attore != null)
            {
                attore.ProvaAMicrofonare();
                return;
            }

            InteragibileNPC npc = hit.collider.GetComponent<InteragibileNPC>();
            if (npc != null) { npc.Interagisci(); return; }

            SpostamentoCamera spostaCam = hit.collider.GetComponent<SpostamentoCamera>();
            if (spostaCam == null) spostaCam = hit.collider.GetComponentInParent<SpostamentoCamera>(); 
            if (spostaCam != null) { spostaCam.Interagisci(); return; }
            if (hit.collider.CompareTag("Videocamera")) 
            {
                GameManager.instance.cameraPosizionata = true;
                Debug.Log("Camera posizionata via Tag.");
                return;
            }

            SupportoLuce supportoLuce = hit.collider.GetComponent<SupportoLuce>();
            if (supportoLuce != null) { supportoLuce.PiazzaLuce(); return; }

            SupportoMicrofono supportoMic = hit.collider.GetComponent<SupportoMicrofono>();
            if (supportoMic != null) { supportoMic.PiazzaMicrofono(); return; }

            MacchinettaCaffe caffe = hit.collider.GetComponent<MacchinettaCaffe>();
            if (caffe != null) { caffe.SpegniMacchinetta(); return; }
        }
    }
}