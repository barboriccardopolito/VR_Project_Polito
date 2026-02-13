using UnityEngine;
using UnityEngine.UI; 
using TMPro; 

public class InterazioneGiocatore : MonoBehaviour
{
    [Header("Collegamenti")]
    public Transform cameraGiocatore;
    public GameObject widgetInterazione; 
    
    [Header("Mirino Dinamico")]
    public Image mirinoUI;               
    public Color coloreRiposo = new Color(1, 1, 1, 0.4f); 
    public Color coloreAttivo = Color.red;                
    public float scalaRiposo = 1f;       
    public float scalaAttiva = 2.5f;     
    public float velocitaAnimazione = 10f; 

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

// 1. OGGETTI RACCOGLIBILI
            OggettoRaccolta oggetto = hit.collider.GetComponent<OggettoRaccolta>();
            if (oggetto != null)
            {
                trovatoQualcosa = true;
                SelettoreOggetti selettore = oggetto.GetComponentInParent<SelettoreOggetti>();
                
                // Se l'oggetto è in una valigia, offri di esaminarla
                if (selettore != null && selettore.PuoiInteragire())
                {
                    messaggioDaMostrare = "[E] ESAMINA VALIGIA";
                }
                else
                {
                    messaggioDaMostrare = "[E] RACCOGLI " + oggetto.nomeOggetto.ToUpper();
                }
            }

            // 2. NPC (Modificato per nascondere il prompt se sta parlando)
            else if (hit.collider.GetComponent<InteragibileNPC>() != null) 
            {
                InteragibileNPC npc = hit.collider.GetComponent<InteragibileNPC>();
                if (npc != null && !npc.staParlando) // Controllo stato parlato
                {
                    trovatoQualcosa = true;
                    messaggioDaMostrare = "[E] PARLA";
                }
                else
                {
                    // Se sta parlando, nascondiamo forzatamente il widget
                    if (widgetInterazione != null) widgetInterazione.SetActive(false);
                }
            }
            
            // 3. SUPPORTI LUCI
            else if (hit.collider.GetComponent<SupportoLuce>() != null) 
            {
                trovatoQualcosa = true;
                messaggioDaMostrare = "[E] PIAZZA LUCE";
            }

            // 4. SUPPORTI MICROFONI
            else if (hit.collider.GetComponent<SupportoMicrofono>() != null)
            {
                trovatoQualcosa = true;
                messaggioDaMostrare = "[E] PIAZZA MICROFONO";
            }

            // 5. VIDEOCAMERE (Lenti e Spostamento)
            else if (hit.collider.GetComponent<SpostamentoCamera>() != null || hit.collider.CompareTag("Videocamera")) 
            {
                trovatoQualcosa = true;
                SpostamentoCamera spostaCam = hit.collider.GetComponent<SpostamentoCamera>();
                if (spostaCam == null) spostaCam = hit.collider.GetComponentInParent<SpostamentoCamera>();
                
                if (GameManager.instance != null && spostaCam != null)
                {
                    if (GameManager.instance.taskAttuale == GameManager.Reparto.Fotografia)
                    {
                        if (!spostaCam.lenteMontata)
                            messaggioDaMostrare = "[E] MONTA LENTE";
                        else if (spostaCam.lenteMontata && !spostaCam.schermoControllato)
                            messaggioDaMostrare = "[E] CONTROLLA SCHERMO";
                        else
                            messaggioDaMostrare = "TELECAMERA PRONTA";
                    }
                    else if (GameManager.instance.taskAttuale == GameManager.Reparto.Regia)
                    {
                        messaggioDaMostrare = "[E] SPOSTA CAMERA";
                    }
                    else
                    {
                        messaggioDaMostrare = "TELECAMERA"; 
                    }
                }
            }

            // 6. ATTORI (Microfonaggio Lavalier)
            else if (GameManager.instance.micDaInstallare == "Lavalier" && hit.collider.GetComponent<AttoreMicrofonabile>() != null)
            {
                trovatoQualcosa = true;
                messaggioDaMostrare = "[E] MICROFONA ATTORE";
            }

            // 7. MACCHINETTA CAFFE
            else if (hit.collider.GetComponent<MacchinettaCaffe>() != null) 
            {
                trovatoQualcosa = true;
                messaggioDaMostrare = "[E] SPEGNI";
            }

            // GESTIONE FINALE VISUALIZZAZIONE
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
            if (obj != null) 
            { 
                SelettoreOggetti selettore = obj.GetComponentInParent<SelettoreOggetti>();
                if (selettore != null && selettore.PuoiInteragire())
                {
                    selettore.EntraInSelezione();
                }
                else
                {
                    obj.EseguiRaccolta(); 
                }
                return; 
            }

            AttoreMicrofonabile attore = hit.collider.GetComponent<AttoreMicrofonabile>();
            if (attore != null) { attore.ProvaAMicrofonare(); return; }

            InteragibileNPC npc = hit.collider.GetComponent<InteragibileNPC>();
            if (npc != null) { npc.Interagisci(); return; }

            SpostamentoCamera spostaCam = hit.collider.GetComponent<SpostamentoCamera>();
            if (spostaCam == null) spostaCam = hit.collider.GetComponentInParent<SpostamentoCamera>(); 
            if (spostaCam != null) { spostaCam.Interagisci(); return; }

            if (hit.collider.CompareTag("Videocamera")) 
            {
                GameManager.instance.cameraPosizionata = true;
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