using UnityEngine;
using System.Collections; 

public class InteragibileNPC : MonoBehaviour
{
    [Header("Impostazioni Reparto")]
    public GameManager.Reparto tipoReparto;

    [Header("Solo per Produzione")]
    public GameObject radioDaAttivare; 

    [TextArea(3, 10)]
    public string messaggioTask;

    [HideInInspector] public bool staParlando = false;

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

            if (tipoReparto == GameManager.Reparto.Fotografia && GameManager.instance.lenteSceltaFinale != "" && !TutteLeCamereMontate())
                devoIlluminarmi = true;

            if (devoIlluminarmi) evidenziatore.Accendi(); else evidenziatore.Spegni();
        }
    }

    public void Interagisci()
    {
        if (staParlando) return;

        NPCWander produzioneScript = GetComponent<NPCWander>();
        if (produzioneScript != null) 
        {
            StartCoroutine(GestisciStatoParlato(4.5f)); 
            produzioneScript.InterazioneConPlayer();
        }

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
                StartCoroutine(GestisciStatoParlato(staffScript.audioNonEIlMioTurno.length));
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
                StartCoroutine(GestisciStatoParlato(4.5f)); 
                staffScript.AvviaDialogoIniziale();
                Debug.Log($"[{tipoReparto}] Ascolta il briefing iniziale.");
                return; 
            }
        }

        InventarioGiocatore inv = FindFirstObjectByType<InventarioGiocatore>();
        
        // --- LOGICA CONSEGNA OGGETTI CON FILTRO DI SICUREZZA ---
        if (inv != null && inv.haUnOggetto)
        {
            bool oggettoCorretto = false;
            if (tipoReparto == GameManager.Reparto.Fotografia && inv.categoriaInMano == OggettoRaccolta.TipoOggetto.Lente) 
            {
                // SE hai una lente, ma NON hai finito di montarla su TUTTE le camere, l'NPC non la prende
                if (!TutteLeCamereMontate())
                {
                    Debug.Log("<color=orange>[Dir. Fotografia]:</color> Vai a montare quella lente sulle macchine da presa, non darla a me!");
                    return; 
                }
                oggettoCorretto = true;
            }
            
            if (tipoReparto == GameManager.Reparto.Luci && inv.categoriaInMano == OggettoRaccolta.TipoOggetto.Luce) 
            {
                // SE hai una luce ma non è posizionata correttamente
                if (!GameManager.instance.LucePosizionataCorrettamente)
                {
                    Debug.Log("<color=orange>[Addetto Luci]:</color> Piazza quel faro sugli stativi prima di tornare da me.");
                    return;
                }
                oggettoCorretto = true;
            }

            if (tipoReparto == GameManager.Reparto.Fonico && inv.categoriaInMano == OggettoRaccolta.TipoOggetto.Microfono)
            {
                bool taskAudioFinita = false;
                if (GameManager.instance.micDaInstallare == "Lavalier" && GameManager.instance.attoriMicrofonatiAttuali >= GameManager.instance.attoriDaMicrofonare) taskAudioFinita = true;
                else if ((GameManager.instance.micDaInstallare == "Boom" || GameManager.instance.micDaInstallare == "Ambisonic") && GameManager.instance.supportoPiazzato) taskAudioFinita = true;
                
                if (!taskAudioFinita)
                {
                    Debug.Log("<color=orange>[Fonico]:</color> Porta quel microfono in posizione, muoviti!");
                    return;
                }
                oggettoCorretto = true;
            }

            if (oggettoCorretto)
            {
                StartCoroutine(GestisciStatoParlato(3.5f)); 

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
                return; // Esce dopo la consegna
            }
        }

        // --- LOGICA FINE TASK (QUANDO NON HAI OGGETTI IN MANO) ---
        bool fotografiaInCorso = (GameManager.instance.taskAttuale == GameManager.Reparto.Fotografia) || (faseRevisione && tipoReparto == GameManager.Reparto.Fotografia);

        if (tipoReparto == GameManager.Reparto.Fotografia && (!inv || !inv.haUnOggetto) && fotografiaInCorso)
        {
            if (GameManager.instance.lenteSceltaFinale != "" && TutteLeCamereMontate())
            {
                if (staffScript != null)
                {
                    Debug.Log("<color=yellow>[Dir. Fotografia]:</color> Controllo finale...");
                    StartCoroutine(GestisciStatoParlato(3.5f)); 
                    staffScript.ReazioneFineTask(() => {
                        GameManager.instance.CompletaTask(tipoReparto);
                    });
                }
                else GameManager.instance.CompletaTask(tipoReparto);
            }
            else if (GameManager.instance.lenteSceltaFinale != "") Debug.LogWarning("[Dir. Fotografia]: Monta le lenti e sposta le camere!");
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
                    StartCoroutine(GestisciStatoParlato(3.5f)); 
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
                      StartCoroutine(GestisciStatoParlato(3.5f)); 
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
            StartCoroutine(GestisciStatoParlato(4.5f)); 
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
                    StartCoroutine(GestisciStatoParlato(3f)); 
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

        if (staffScript == null || staffScript.haGiaParlato) Debug.Log($"[Info]: {messaggioTask}");
    }

    // Funzione helper per controllare se tutte le macchine da presa hanno la lente
    bool TutteLeCamereMontate()
    {
        SpostamentoCamera[] camere = FindObjectsByType<SpostamentoCamera>(FindObjectsSortMode.None);
        if (camere.Length == 0) return true; // Se non ci sono camere, consideriamo fatto
        
        foreach (var cam in camere)
        {
            if (!cam.lenteMontata) return false;
        }
        return true;
    }

    public IEnumerator GestisciStatoParlato(float durata)
    {
        staParlando = true;
        yield return new WaitForSeconds(durata);
        staParlando = false;
    }
}