using UnityEngine;

public class InteragibileNPC : MonoBehaviour
{
    [Header("Impostazioni Reparto")]
    public GameManager.Reparto tipoReparto;

    [Header("Solo per Produzione")]
    public GameObject radioDaAttivare; 

    [TextArea(3, 10)]
    public string messaggioTask;

    private Evidenziatore evidenziatore;

    void Start()
    {
        evidenziatore = GetComponent<Evidenziatore>();
        if (evidenziatore == null) evidenziatore = GetComponentInChildren<Evidenziatore>();
    }

    void Update()
    {
        if (evidenziatore != null)
        {
            bool faseRevisione = (GameManager.instance.taskAttuale == GameManager.Reparto.Regia);
            bool èIlMioTurno = (GameManager.instance.taskAttuale == tipoReparto);
            bool devoIlluminarmi = false;

            if (èIlMioTurno || faseRevisione) devoIlluminarmi = true;
            
            if (tipoReparto == GameManager.Reparto.Luci && GameManager.instance.LuceScelta != "" && !GameManager.instance.LucePosizionataCorrettamente) 
                devoIlluminarmi = true;
            
            if (tipoReparto == GameManager.Reparto.Fonico && GameManager.instance.micDaInstallare != "") 
            {
                bool taskAudioFinita = false;
                 if (GameManager.instance.micDaInstallare == "Lavalier" && GameManager.instance.attoriMicrofonatiAttuali >= GameManager.instance.attoriDaMicrofonare) taskAudioFinita = true;
                 else if ((GameManager.instance.micDaInstallare == "Boom" || GameManager.instance.micDaInstallare == "Ambisonic") && GameManager.instance.supportoPiazzato) taskAudioFinita = true;
                if (!taskAudioFinita) devoIlluminarmi = true;
            }

            if (tipoReparto == GameManager.Reparto.Fotografia && GameManager.instance.lenteSceltaFinale != "" && !GameManager.instance.cameraPosizionata)
                devoIlluminarmi = true;

            if (devoIlluminarmi) evidenziatore.Accendi(); else evidenziatore.Spegni();
        }
    }

    public void Interagisci()
    {
        NPCWander produzioneScript = GetComponent<NPCWander>();
        if (produzioneScript != null) produzioneScript.InterazioneConPlayer();

        NPC_Staff staffScript = GetComponent<NPC_Staff>();
        bool faseRevisione = (GameManager.instance.taskAttuale == GameManager.Reparto.Regia);
        bool èIlMioTurno = (GameManager.instance.taskAttuale == tipoReparto);

        
        bool possoInteragire = èIlMioTurno || faseRevisione; 
        
        bool installazioneLuciInCorso = (GameManager.instance.LuceScelta != "");
        bool installazioneAudioInCorso = (GameManager.instance.micDaInstallare != "");
        
        if (tipoReparto == GameManager.Reparto.Luci && installazioneLuciInCorso) possoInteragire = true;
        if (tipoReparto == GameManager.Reparto.Fonico && installazioneAudioInCorso) possoInteragire = true;

        if (!possoInteragire)
        {
            Debug.Log($"<color=yellow>[{tipoReparto}]:</color> Non disturbare ora. Non è il mio turno.");
            if (staffScript != null && staffScript.audioNonEIlMioTurno != null)
            {
                 staffScript.GetComponent<AudioSource>().PlayOneShot(staffScript.audioNonEIlMioTurno);
            }
            return;
        }

        if (staffScript != null)
        {
            InterazioneGiocatore player = FindFirstObjectByType<InterazioneGiocatore>();
            if (player != null) staffScript.AttivaInterazione(player.transform);

            if (èIlMioTurno && !staffScript.haGiaParlato && (!faseRevisione || tipoReparto == GameManager.Reparto.Regia))
            {
                staffScript.AvviaDialogoIniziale();
                Debug.Log($"[{tipoReparto}] Ascolta il briefing iniziale.");
                return; 
            }
        }

        InventarioGiocatore inv = FindFirstObjectByType<InventarioGiocatore>();
        bool fotografiaInCorso = (GameManager.instance.taskAttuale == GameManager.Reparto.Fotografia) || (faseRevisione && tipoReparto == GameManager.Reparto.Fotografia);

        if (tipoReparto == GameManager.Reparto.Fotografia && (!inv || !inv.haUnOggetto) && fotografiaInCorso)
        {
            if (GameManager.instance.lenteSceltaFinale != "" && GameManager.instance.cameraPosizionata)
            {
                if (staffScript != null)
                {
                    Debug.Log("<color=yellow>[Dir. Fotografia]:</color> Controllo finale...");
                    staffScript.ReazioneFineTask(() => {
                        GameManager.instance.CompletaTask(tipoReparto);
                    });
                }
                else GameManager.instance.CompletaTask(tipoReparto);
            }
            else if (GameManager.instance.lenteSceltaFinale != "") Debug.LogWarning("[Dir. Fotografia]: Sposta la videocamera!");
            else if (staffScript != null && staffScript.haGiaParlato) Debug.Log("[Dir. Fotografia]: Portami una lente.");
            return;
        }

        if (tipoReparto == GameManager.Reparto.Luci && installazioneLuciInCorso && (!inv || !inv.haUnOggetto))
        {
            if (GameManager.instance.LucePosizionataCorrettamente) 
            {
                if (staffScript != null)
                {
                    Debug.Log("<color=yellow>[Addetto Luci]:</color> Controllo finale...");
                    staffScript.ReazioneFineTask(() => {
                        GameManager.instance.CompletaTask(tipoReparto); 
                    });
                }
                else GameManager.instance.CompletaTask(tipoReparto);
            } 
            else Debug.Log($"[Addetto Luci]: Monta la {GameManager.instance.LuceScelta}.");
            return;
        }

        if (tipoReparto == GameManager.Reparto.Fonico && installazioneAudioInCorso && (!inv || !inv.haUnOggetto))
        {
             bool taskCompletata = false;
             if (GameManager.instance.micDaInstallare == "Lavalier" && GameManager.instance.attoriMicrofonatiAttuali >= GameManager.instance.attoriDaMicrofonare) taskCompletata = true;
             else if ((GameManager.instance.micDaInstallare == "Boom" || GameManager.instance.micDaInstallare == "Ambisonic") && GameManager.instance.supportoPiazzato) taskCompletata = true;

             if (taskCompletata) {
                 if (staffScript != null)
                 {
                      Debug.Log("<color=green>[Fonico]:</color> Controllo setup audio...");
                      staffScript.ReazioneFineTask(() => 
                      {
                          GameManager.instance.ApplicaEffettoMicrofono(GameManager.instance.micDaInstallare);
                          GameManager.instance.micDaInstallare = "";
                          GameManager.instance.supportoPiazzato = false; 
                          GameManager.instance.CompletaTask(tipoReparto);
                      });
                 }
                 else 
                 {
                      GameManager.instance.ApplicaEffettoMicrofono(GameManager.instance.micDaInstallare);
                      GameManager.instance.micDaInstallare = "";
                      GameManager.instance.supportoPiazzato = false; 
                      GameManager.instance.CompletaTask(tipoReparto);
                 }
             } 
             else Debug.Log($"[Fonico]: Finisci di installare il {GameManager.instance.micDaInstallare}.");
             return;
        }

        if (tipoReparto == GameManager.Reparto.Produzione) { 
            if (evidenziatore != null) evidenziatore.Spegni();
            if (produzioneScript != null) produzioneScript.InterazioneConPlayer(); 
            else GameManager.instance.CompletaTask(tipoReparto);
            return;
        }
        
        if (tipoReparto == GameManager.Reparto.Regia && èIlMioTurno) { 
            if (!RegiaManager.instance.previewInCorso && !RegiaManager.instance.registrazioneInCorso) {
                RegiaManager.instance.AttivaPreview();
                GameManager.instance.MandaAttoriInScena();
                Debug.Log("<color=cyan>[Regista]:</color> Attori in posizione! Guarda i monitor.");
                return;
            }
            if (RegiaManager.instance.previewInCorso) {
                if (staffScript != null)
                {
                    Debug.Log("<color=red>[Regista]:</color> Chiamo l'azione...");
                    staffScript.ReazioneCiak(() => 
                    {
                        RegiaManager.instance.AvviaCiak();
                    });
                }
                else
                {
                    RegiaManager.instance.AvviaCiak();
                }
                return;
            }
        }

        if (inv != null && inv.haUnOggetto)
        {
            bool oggettoCorretto = false;
            if (tipoReparto == GameManager.Reparto.Fotografia && inv.categoriaInMano == OggettoRaccolta.TipoOggetto.Lente) oggettoCorretto = true;
            if (tipoReparto == GameManager.Reparto.Luci && inv.categoriaInMano == OggettoRaccolta.TipoOggetto.Luce) oggettoCorretto = true;
            if (tipoReparto == GameManager.Reparto.Fonico && inv.categoriaInMano == OggettoRaccolta.TipoOggetto.Microfono) oggettoCorretto = true;

            if (oggettoCorretto)
            {
                if (tipoReparto == GameManager.Reparto.Fotografia)
                {
                    if (GameManager.instance.lenteSceltaFinale != "") GameManager.instance.RestituisciOggettoAlTavolo(GameManager.instance.lenteSceltaFinale);
                    GameManager.instance.lenteSceltaFinale = inv.oggettoInMano;
                    GameManager.instance.ResetEffettoLente();
                    GameManager.instance.cameraPosizionata = false;

                    if (staffScript != null) staffScript.ReazioneConsegnaLente(inv.oggettoInMano);
                    inv.ConsegnaOggetto();
                }
                else if (tipoReparto == GameManager.Reparto.Luci)
                {
                    if (GameManager.instance.LuceScelta != "") GameManager.instance.RestituisciOggettoAlTavolo(GameManager.instance.LuceScelta);
                    GameManager.instance.ResettaVisualeSupportiLuci(); 
                    GameManager.instance.LuceScelta = inv.oggettoInMano;
                    GameManager.instance.LucePosizionataCorrettamente = false; 

                    if (staffScript != null) staffScript.ReazioneConsegnaLuce(inv.oggettoInMano);
                    inv.ConsegnaOggetto();
                }
                else if (tipoReparto == GameManager.Reparto.Fonico)
                {
                    if (GameManager.instance.micScelto != "") GameManager.instance.RestituisciOggettoAlTavolo(GameManager.instance.micScelto);
                    GameManager.instance.micScelto = inv.oggettoInMano;
                    GameManager.instance.micDaInstallare = inv.oggettoInMano;
                    GameManager.instance.attoriMicrofonatiAttuali = 0; 
                    GameManager.instance.supportoPiazzato = false; 
                    
                    if (staffScript != null) staffScript.ReazioneConsegnaMicrofono(inv.oggettoInMano);
                    inv.ConsegnaOggetto();
                }
            }
            else Debug.Log($"[Task]: Oggetto errato!");
        }
        else
        {
            if (faseRevisione && tipoReparto != GameManager.Reparto.Regia) Debug.Log($"[{tipoReparto}]: Se vuoi cambiare qualcosa, portami l'attrezzatura nuova.");
            else if (staffScript == null || staffScript.haGiaParlato) Debug.Log($"[Info]: {messaggioTask}");
        }
    }
}