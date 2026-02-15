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
            string messaggioDaMostrare = "";

            // 1. NPC (Se sta parlando, nascondiamo tutto)
            InteragibileNPC npc = hit.collider.GetComponent<InteragibileNPC>();
            if (npc != null)
            {
                if (!npc.staParlando) 
                {
                    trovatoQualcosa = true;
                    messaggioDaMostrare = "[E] PARLA";
                }
            }
            
            // 2. SUPPORTI LUCI (Se c'è già la luce, nascondiamo)
            else if (hit.collider.GetComponent<SupportoLuce>() != null) 
            {
                SupportoLuce luce = hit.collider.GetComponent<SupportoLuce>();
                if (!luce.luceGiaPosizionata && GameManager.instance != null && GameManager.instance.taskAttuale == GameManager.Reparto.Luci)
                {
                    trovatoQualcosa = true;
                    messaggioDaMostrare = "[E] PIAZZA LUCE";
                }
            }

            // 3. SUPPORTI MICROFONI (Se c'è già il mic, nascondiamo)
            else if (hit.collider.GetComponent<SupportoMicrofono>() != null)
            {
                SupportoMicrofono mic = hit.collider.GetComponent<SupportoMicrofono>();
                if (!mic.microfonoPiazzato && GameManager.instance != null && GameManager.instance.taskAttuale == GameManager.Reparto.Fonico)
                {
                    trovatoQualcosa = true;
                    messaggioDaMostrare = "[E] PIAZZA MICROFONO";
                }
            }

            // 4. VIDEOCAMERE (Controlla lo stato dei montaggi)
            else if (hit.collider.GetComponent<SpostamentoCamera>() != null || hit.collider.GetComponentInParent<SpostamentoCamera>() != null) 
            {
                SpostamentoCamera spostaCam = hit.collider.GetComponent<SpostamentoCamera>();
                if (spostaCam == null) spostaCam = hit.collider.GetComponentInParent<SpostamentoCamera>();
                
                if (GameManager.instance != null && spostaCam != null)
                {
                    if (GameManager.instance.taskAttuale == GameManager.Reparto.Fotografia)
                    {
                        if (!spostaCam.lenteMontata) 
                        { 
                            trovatoQualcosa = true; messaggioDaMostrare = "[E] MONTA LENTE"; 
                        }
                        else if (!spostaCam.schermoControllato) 
                        { 
                            trovatoQualcosa = true; messaggioDaMostrare = "[E] CONTROLLA SCHERMO"; 
                        }
                        // Se ha fatto tutto, "trovatoQualcosa" resta false e non mostra niente!
                    }
                    else if (GameManager.instance.taskAttuale == GameManager.Reparto.Regia)
                    {
                        trovatoQualcosa = true;
                        messaggioDaMostrare = "[E] SPOSTA CAMERA";
                    }
                }
            }

            // 5. VALIGIE E OGGETTI RACCOGLIBILI
            else if (hit.collider.GetComponent<SelettoreOggetti>() != null || hit.collider.GetComponent<OggettoRaccolta>() != null)
            {
                SelettoreOggetti selettore = hit.collider.GetComponent<SelettoreOggetti>();
                OggettoRaccolta oggetto = hit.collider.GetComponent<OggettoRaccolta>();

                if (selettore == null && oggetto != null) 
                    selettore = oggetto.GetComponentInParent<SelettoreOggetti>();

                if (selettore != null && selettore.PuoiInteragire())
                {
                    trovatoQualcosa = true;
                    messaggioDaMostrare = "[E] ESAMINA VALIGIA";
                }
                else if (oggetto != null)
                {
                    trovatoQualcosa = true;
                    messaggioDaMostrare = "[E] RACCOGLI " + oggetto.nomeOggetto.ToUpper();
                }
            }

            // 6. ATTORI
            else if (hit.collider.GetComponent<AttoreMicrofonabile>() != null)
            {
                if (GameManager.instance != null && GameManager.instance.micDaInstallare == "Lavalier")
                {
                    trovatoQualcosa = true;
                    messaggioDaMostrare = "[E] MICROFONA ATTORE";
                }
            }

            // 7. MACCHINETTA CAFFE
            else if (hit.collider.GetComponent<MacchinettaCaffe>() != null) 
            {
                trovatoQualcosa = true;
                messaggioDaMostrare = "[E] SPEGNI";
            }
            
            PortaSet porta = hit.collider.GetComponent<PortaSet>();
            if (porta != null && !porta.isOpen) { porta.TentaApertura(); return; }
            // 8. LA PORTA DEL SET
            else if (hit.collider.GetComponent<PortaSet>() != null)
            {
                PortaSet porta = hit.collider.GetComponent<PortaSet>();
                if (!porta.isOpen) // Se è chiusa, mostra il testo
                {
                    trovatoQualcosa = true;
                    if (GameManager.instance != null && GameManager.instance.taskAttuale == GameManager.Reparto.Produzione)
                        messaggioDaMostrare = "BLOCCATA";
                    else
                        messaggioDaMostrare = "[E] APRI PORTA";
                }
            }

            // --- MOSTRA O NASCONDI IL WIDGET FINALE ---
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
        // SPEGNIMENTO IMMEDIATO DEL TESTO AL CLICK
        if (widgetInterazione != null) widgetInterazione.SetActive(false);

        if (cameraGiocatore == null) return;
        Ray raggio = new Ray(cameraGiocatore.position, cameraGiocatore.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(raggio, out hit, distanzaInterazione, layerDaColpire))
        {
            SelettoreOggetti selettore = hit.collider.GetComponent<SelettoreOggetti>();
            OggettoRaccolta obj = hit.collider.GetComponent<OggettoRaccolta>();

            if (selettore == null && obj != null) 
                selettore = obj.GetComponentInParent<SelettoreOggetti>();

            if (selettore != null && selettore.PuoiInteragire())
            {
                selettore.EntraInSelezione();
                return;
            }
            else if (obj != null) 
            { 
                obj.EseguiRaccolta(); 
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