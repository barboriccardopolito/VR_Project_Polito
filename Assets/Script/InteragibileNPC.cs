using UnityEngine;

public class InteragibileNPC : MonoBehaviour
{
    [Header("Impostazioni Reparto")]
    public GameManager.Reparto tipoReparto;

    [Header("Solo per Produzione")]
    public GameObject radioDaAttivare; 

    [TextArea(3, 10)]
    public string messaggioTask;

    // Riferimento allo script dell'anello luminoso
    private Evidenziatore evidenziatore;

    void Start()
    {
        // Trova lo script Evidenziatore sullo stesso oggetto o sui figli
        evidenziatore = GetComponent<Evidenziatore>();
        if (evidenziatore == null)
            evidenziatore = GetComponentInChildren<Evidenziatore>();
    }

    void Update()
    {
        // --- LOGICA ACCENSIONE ANELLO LUMINOSO ---
        if (evidenziatore != null)
        {
            // Recupera lo stato del gioco
            bool faseRevisione = (GameManager.instance.taskAttuale == GameManager.Reparto.Regia);
            bool èIlMioTurno = (GameManager.instance.taskAttuale == tipoReparto);
            
            bool devoIlluminarmi = false;

            // 1. È il mio turno?
            if (èIlMioTurno) devoIlluminarmi = true;
            
            // 2. Siamo in fase finale (tutti attivi per modifiche)?
            if (faseRevisione) devoIlluminarmi = true;
            
            // 3. Sto aspettando che il giocatore finisca un'installazione per me?
            // Luci: Ho scelto la luce ma non ho finito di piazzarla
            if (tipoReparto == GameManager.Reparto.Luci && GameManager.instance.LuceScelta != "" && !GameManager.instance.LucePosizionataCorrettamente) 
                devoIlluminarmi = true;
            
            // Fonico: Ho scelto il mic ma non ho finito di installarlo
            if (tipoReparto == GameManager.Reparto.Fonico && GameManager.instance.micDaInstallare != "") 
            {
                bool taskAudioFinita = false;
                 if (GameManager.instance.micDaInstallare == "Lavalier" && GameManager.instance.attoriMicrofonatiAttuali >= GameManager.instance.attoriDaMicrofonare) taskAudioFinita = true;
                 else if ((GameManager.instance.micDaInstallare == "Boom" || GameManager.instance.micDaInstallare == "Ambisonic") && GameManager.instance.supportoPiazzato) taskAudioFinita = true;
                
                if (!taskAudioFinita) devoIlluminarmi = true;
            }

            // Fotografia: Ho scelto la lente ma non ho piazzato la camera
            if (tipoReparto == GameManager.Reparto.Fotografia && GameManager.instance.lenteSceltaFinale != "" && !GameManager.instance.cameraPosizionata)
                devoIlluminarmi = true;

            // Applica stato
            if (devoIlluminarmi) evidenziatore.Accendi();
            else evidenziatore.Spegni();
        }
    }

    public void Interagisci()
    {
        // --- 0. GESTIONE MOVIMENTO & ANIMAZIONE NPC ---
        
        // A. Cerca lo script della PRODUZIONE (Seduta -> Alzata)
        NPCWander produzioneScript = GetComponent<NPCWander>();
        if (produzioneScript != null)
        {
            produzioneScript.InterazioneConPlayer();
        }

        // B. Cerca lo script dello STAFF (Fotografia, Luci, Fonico - Camminata -> Stop -> Gesticola)
        NPC_Staff staffScript = GetComponent<NPC_Staff>();
        if (staffScript != null)
        {
            // Trova il player per girarsi verso di lui
            InterazioneGiocatore player = FindFirstObjectByType<InterazioneGiocatore>();
            if (player != null) staffScript.AttivaInterazione(player.transform);
        }

        InventarioGiocatore inv = FindFirstObjectByType<InventarioGiocatore>();

        // --- STATI DI GIOCO ---
        bool faseRevisione = (GameManager.instance.taskAttuale == GameManager.Reparto.Regia);
        
        // Fotografia è "in corso" se è il turno attuale OPPURE se siamo in revisione e parli col fotografo
        bool fotografiaInCorso = (GameManager.instance.taskAttuale == GameManager.Reparto.Fotografia) || (faseRevisione && tipoReparto == GameManager.Reparto.Fotografia);
        
        bool installazioneLuciInCorso = (GameManager.instance.LuceScelta != "");
        bool installazioneAudioInCorso = (GameManager.instance.micDaInstallare != "");
        bool èIlMioTurno = (GameManager.instance.taskAttuale == tipoReparto);

        // Posso interagire se è il mio turno, se sto installando qualcosa per questo reparto, o se sono in revisione generale
        bool possoInteragire = èIlMioTurno || installazioneAudioInCorso || installazioneLuciInCorso || faseRevisione;

        // Se non tocca a me e non sono il regista (che è sempre disponibile in revisione), blocco
        if (!possoInteragire && tipoReparto != GameManager.Reparto.Regia)
        {
            Debug.Log($"[{tipoReparto}]: Non disturbare ora. Non è il mio turno.");
            return;
        }

        // --- 1. LOGICA FOTOGRAFIA (Conferma Finale - Step 3) ---
        // Qui arrivi DOPO aver consegnato la lente e DOPO aver mosso la camera
        if (tipoReparto == GameManager.Reparto.Fotografia && (!inv || !inv.haUnOggetto) && fotografiaInCorso)
        {
            // Caso A: Hai consegnato la lente E hai mosso la camera
            if (GameManager.instance.lenteSceltaFinale != "" && GameManager.instance.cameraPosizionata)
            {
                Debug.Log("<color=yellow>[Dir. Fotografia]:</color> Ottimo lavoro con la camera. Task completata!");
                GameManager.instance.CompletaTask(tipoReparto);
            }
            // Caso B: Hai consegnato la lente MA NON hai ancora mosso la camera
            else if (GameManager.instance.lenteSceltaFinale != "")
            {
                Debug.LogWarning("[Dir. Fotografia]: Ho la lente, ma non hai ancora spostato la videocamera! Vai a posizionarla (Premi E sulla camera).");
            }
            // Caso C: Non hai ancora portato nulla
            else
            {
                Debug.Log("[Dir. Fotografia]: Portami una lente dal tavolo per iniziare.");
            }
            return;
        }

        // --- 2. LOGICA LUCI (Verifica Installazione) ---
        if (tipoReparto == GameManager.Reparto.Luci && installazioneLuciInCorso && (!inv || !inv.haUnOggetto))
        {
            if (GameManager.instance.LucePosizionataCorrettamente) {
                Debug.Log("<color=yellow>[Addetto Luci]:</color> Installazione confermata.");
                GameManager.instance.CompletaTask(tipoReparto); 
            } else {
                Debug.Log($"[Addetto Luci]: I supporti sono vuoti! Vai a montare la {GameManager.instance.LuceScelta}.");
            }
            return;
        }

        // --- 3. LOGICA FONICO (Verifica Installazione) ---
        if (tipoReparto == GameManager.Reparto.Fonico && installazioneAudioInCorso && (!inv || !inv.haUnOggetto))
        {
             bool taskCompletata = false;
             
             // Controllo Lavalier: devo aver microfonato tutti gli attori richiesti
             if (GameManager.instance.micDaInstallare == "Lavalier" && GameManager.instance.attoriMicrofonatiAttuali >= GameManager.instance.attoriDaMicrofonare) 
                 taskCompletata = true;
             // Controllo Boom/Ambisonic: devo aver piazzato il supporto
             else if ((GameManager.instance.micDaInstallare == "Boom" || GameManager.instance.micDaInstallare == "Ambisonic") && GameManager.instance.supportoPiazzato) 
                 taskCompletata = true;

             if (taskCompletata) {
                 Debug.Log($"<color=green>[Fonico]:</color> Setup Audio completato.");
                 GameManager.instance.ApplicaEffettoMicrofono(GameManager.instance.micDaInstallare);
                 
                 // Reset variabili per evitare loop o bug futuri
                 GameManager.instance.micDaInstallare = "";
                 GameManager.instance.supportoPiazzato = false; 
                 
                 GameManager.instance.CompletaTask(tipoReparto);
             } else {
                 Debug.Log($"[Fonico]: Finisci di installare il {GameManager.instance.micDaInstallare}.");
             }
             return;
        }

        // --- 4. PRODUZIONE ---
        if (tipoReparto == GameManager.Reparto.Produzione) { 
            
            // Spegniamo l'anello subito per pulizia visiva
            if (evidenziatore != null) evidenziatore.Spegni();

            // Deleghiamo TUTTO allo script NPCWander che gestisce audio e consegna
            if (produzioneScript != null)
            {
                // Questo farà partire la Coroutine con i dialoghi
                produzioneScript.InterazioneConPlayer(); 
            }
            else
            {
                // Fallback di sicurezza se manca lo script (non dovrebbe succedere)
                Debug.LogError("Manca lo script NPCWander sull'NPC Produzione!");
                GameManager.instance.CompletaTask(tipoReparto);
            }

            // NON chiamiamo più CompletaTask qui, lo farà NPCWander alla fine dell'audio!
            return;
        }
        
        // --- 5. REGIA (Ciak e Preview) ---
        if (tipoReparto == GameManager.Reparto.Regia) { 
            if (!RegiaManager.instance.previewInCorso && !RegiaManager.instance.registrazioneInCorso) {
                
                // 1. Attiva i monitor (Preview video)
                RegiaManager.instance.AttivaPreview();

                // 2. Fai comparire gli attori sul set! (Scambio gruppi)
                GameManager.instance.MandaAttoriInScena();

                Debug.Log("<color=cyan>[Regista]:</color> Attori in posizione! Guarda i monitor. Se vuoi cambiare qualcosa, parla con i capi reparto.");
                return;
            }
            if (RegiaManager.instance.previewInCorso) {
                Debug.Log("<color=red>[Regista]:</color> AZIONE! (Avvio Registrazione)");
                RegiaManager.instance.AvviaCiak();
                return;
            }
        }

        // --- 6. CONSEGNA OGGETTI (Step 2 del flusso) ---
        // Se ho un oggetto in mano e sto parlando col reparto giusto
        if (inv != null && inv.haUnOggetto)
        {
            bool oggettoCorretto = false;
            if (tipoReparto == GameManager.Reparto.Fotografia && inv.categoriaInMano == OggettoRaccolta.TipoOggetto.Lente) oggettoCorretto = true;
            if (tipoReparto == GameManager.Reparto.Luci && inv.categoriaInMano == OggettoRaccolta.TipoOggetto.Luce) oggettoCorretto = true;
            if (tipoReparto == GameManager.Reparto.Fonico && inv.categoriaInMano == OggettoRaccolta.TipoOggetto.Microfono) oggettoCorretto = true;

            if (oggettoCorretto)
            {
                // CONSEGNA AL DIRETTORE DELLA FOTOGRAFIA
                if (tipoReparto == GameManager.Reparto.Fotografia)
                {
                    // Se avevi già una lente, la rimettiamo sul tavolo (scambio)
                    if (GameManager.instance.lenteSceltaFinale != "")
                        GameManager.instance.RestituisciOggettoAlTavolo(GameManager.instance.lenteSceltaFinale);

                    GameManager.instance.lenteSceltaFinale = inv.oggettoInMano;
                    
                    // --- PUNTO CRUCIALE: RESET EFFETTO ---
                    // Quando consegni la lente, togliamo l'effetto visivo così il giocatore vede bene per muoversi
                    GameManager.instance.ResetEffettoLente();
                    // -------------------------------------

                    GameManager.instance.cameraPosizionata = false; // Reset stato camera: devi riposizionarla con la nuova lente

                    Debug.Log($"[Dir. Fotografia]: Grazie per la {inv.oggettoInMano}. Ora vai alla videocamera e sistema l'inquadratura.");
                    inv.ConsegnaOggetto();
                }
                // CONSEGNA ALL'ADDETTO LUCI
                else if (tipoReparto == GameManager.Reparto.Luci)
                {
                    if (GameManager.instance.LuceScelta != "") 
                        GameManager.instance.RestituisciOggettoAlTavolo(GameManager.instance.LuceScelta);
                    
                    GameManager.instance.ResettaVisualeSupportiLuci(); // Toglie le luci vecchie dalla scena
                    
                    GameManager.instance.LuceScelta = inv.oggettoInMano;
                    GameManager.instance.LucePosizionataCorrettamente = false; 
                    
                    Debug.Log($"[Addetto Luci]: Cambio in {inv.oggettoInMano}? Vai a montarla sui supporti!");
                    inv.ConsegnaOggetto();
                }
                // CONSEGNA AL FONICO
                else if (tipoReparto == GameManager.Reparto.Fonico)
                {
                    if (GameManager.instance.micScelto != "")
                        GameManager.instance.RestituisciOggettoAlTavolo(GameManager.instance.micScelto);

                    GameManager.instance.micScelto = inv.oggettoInMano;
                    GameManager.instance.micDaInstallare = inv.oggettoInMano;
                    
                    // Reset progressi installazione
                    GameManager.instance.attoriMicrofonatiAttuali = 0; 
                    GameManager.instance.supportoPiazzato = false; 
                    
                    Debug.Log($"[Fonico]: Cambio piano in {inv.oggettoInMano}. Installalo!");
                    inv.ConsegnaOggetto();
                }
            }
            else
            {
                Debug.Log($"[Task]: Questo oggetto ({inv.oggettoInMano}) non serve a questo reparto!");
            }
        }
        else
        {
            // --- 7. MESSAGGI DI CORTESIA (Se non ho oggetti e non ho finito) ---
            if (faseRevisione && tipoReparto != GameManager.Reparto.Regia)
                Debug.Log($"[{tipoReparto}]: Se vuoi cambiare qualcosa, portami l'attrezzatura nuova.");
            else
                Debug.Log($"[Info]: {messaggioTask}");
        }
    }
}