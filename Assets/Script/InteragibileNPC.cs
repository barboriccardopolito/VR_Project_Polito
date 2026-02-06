using UnityEngine;

public class InteragibileNPC : MonoBehaviour
{
    public GameManager.Reparto tipoReparto;

    [Header("Solo per Produzione")]
    public GameObject radioDaAttivare; 

    [TextArea(3, 10)]
    public string messaggioTask;

    public void Interagisci()
    {
        NPCWander movimento = GetComponent<NPCWander>();
        if (movimento != null)
        {
            InterazioneGiocatore playerScript = FindFirstObjectByType<InterazioneGiocatore>();
            if (playerScript != null) movimento.AscoltaGiocatore(playerScript.transform);
        }

        InventarioGiocatore inv = FindFirstObjectByType<InventarioGiocatore>();

        bool faseRevisione = (GameManager.instance.taskAttuale == GameManager.Reparto.Regia);
        bool fotografiaInCorso = (GameManager.instance.taskAttuale == GameManager.Reparto.Fotografia) || (faseRevisione && tipoReparto == GameManager.Reparto.Fotografia);
        
        bool installazioneLuciInCorso = (GameManager.instance.LuceScelta != "");
        bool installazioneAudioInCorso = (GameManager.instance.micDaInstallare != "");
        bool èIlMioTurno = (GameManager.instance.taskAttuale == tipoReparto);

        bool possoInteragire = èIlMioTurno || installazioneAudioInCorso || installazioneLuciInCorso || faseRevisione;

        if (!possoInteragire && tipoReparto != GameManager.Reparto.Regia)
        {
            Debug.Log($"[{tipoReparto}]: Non disturbare ora.");
            return;
        }

        // --- 1. LOGICA FOTOGRAFIA (Conferma Finale - Step 3) ---
        // Qui arrivi DOPO aver consegnato la lente e DOPO aver mosso la camera
        if (tipoReparto == GameManager.Reparto.Fotografia && !inv.haUnOggetto && fotografiaInCorso)
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
                Debug.LogWarning("[Dir. Fotografia]: Ho la lente, ma non hai ancora spostato la videocamera! Vai a posizionarla (E).");
            }
            // Caso C: Non hai ancora portato nulla
            else
            {
                Debug.Log("[Dir. Fotografia]: Portami una lente dal tavolo per iniziare.");
            }
            return;
        }

        // --- 2. LOGICA LUCI ---
        if (tipoReparto == GameManager.Reparto.Luci && installazioneLuciInCorso && !inv.haUnOggetto)
        {
            if (GameManager.instance.LucePosizionataCorrettamente) {
                Debug.Log("<color=yellow>[Addetto Luci]:</color> Installazione confermata.");
                GameManager.instance.CompletaTask(tipoReparto); 
            } else {
                Debug.Log($"[Addetto Luci]: Supporti vuoti! Monta la {GameManager.instance.LuceScelta}.");
            }
            return;
        }

        // --- 3. LOGICA FONICO ---
        if (tipoReparto == GameManager.Reparto.Fonico && installazioneAudioInCorso && !inv.haUnOggetto)
        {
             bool taskCompletata = false;
             if (GameManager.instance.micDaInstallare == "Lavalier" && GameManager.instance.attoriMicrofonatiAttuali >= GameManager.instance.attoriDaMicrofonare) taskCompletata = true;
             else if ((GameManager.instance.micDaInstallare == "Boom" || GameManager.instance.micDaInstallare == "Ambisonic") && GameManager.instance.supportoPiazzato) taskCompletata = true;

             if (taskCompletata) {
                 Debug.Log($"<color=green>[Fonico]:</color> Setup Audio completato.");
                 GameManager.instance.ApplicaEffettoMicrofono(GameManager.instance.micDaInstallare);
                 GameManager.instance.micDaInstallare = "";
                 GameManager.instance.supportoPiazzato = false; 
                 GameManager.instance.CompletaTask(tipoReparto);
             } else {
                 Debug.Log($"[Fonico]: Finisci di installare il {GameManager.instance.micDaInstallare}.");
             }
             return;
        }

        // --- 4. PRODUZIONE & REGIA ---
        if (tipoReparto == GameManager.Reparto.Produzione) { 
            RadioSistema radio = FindFirstObjectByType<RadioSistema>();
            if (radio != null) radio.haLaRadio = true;
            if (radioDaAttivare != null) radioDaAttivare.SetActive(true);
            Evidenziatore myGlow = GetComponent<Evidenziatore>();
            if (myGlow != null) myGlow.Spegni();
            GameManager.instance.CompletaTask(tipoReparto);
            return;
        }

        if (tipoReparto == GameManager.Reparto.Regia) { 
            if (!RegiaManager.instance.previewInCorso && !RegiaManager.instance.registrazioneInCorso) {
                RegiaManager.instance.AttivaPreview();
                Debug.Log("<color=cyan>[Regista]:</color> Guarda i monitor. Modifica quello che vuoi.");
                return;
            }
            if (RegiaManager.instance.previewInCorso) {
                Debug.Log("<color=red>[Regista]:</color> AZIONE!");
                RegiaManager.instance.AvviaCiak();
                return;
            }
        }

        // --- 5. CONSEGNA OGGETTI (Step 2 del tuo flusso) ---
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
                    // Gestione scambio (se avevi già una lente scelta)
                    if (GameManager.instance.lenteSceltaFinale != "")
                        GameManager.instance.RestituisciOggettoAlTavolo(GameManager.instance.lenteSceltaFinale);

                    GameManager.instance.lenteSceltaFinale = inv.oggettoInMano;
                    
                    // --- MODIFICA FONDAMENTALE QUI ---
                    // Quando consegni la lente, SPEGNIAMO l'effetto (Reset).
                    // Così torni a vedere normale per andare a spostare la camera.
                    GameManager.instance.ResetEffettoLente();
                    // ---------------------------------

                    GameManager.instance.cameraPosizionata = false; // Devi riposizionare la camera

                    Debug.Log($"[Dir. Fotografia]: Grazie per la {inv.oggettoInMano}. Ora vai alla videocamera e sistema l'inquadratura (WASD).");
                    inv.ConsegnaOggetto();
                }
                else if (tipoReparto == GameManager.Reparto.Luci)
                {
                    if (GameManager.instance.LuceScelta != "") 
                        GameManager.instance.RestituisciOggettoAlTavolo(GameManager.instance.LuceScelta);
                    
                    GameManager.instance.ResettaVisualeSupportiLuci();
                    GameManager.instance.LuceScelta = inv.oggettoInMano;
                    GameManager.instance.LucePosizionataCorrettamente = false; 
                    Debug.Log($"[Addetto Luci]: Cambio in {inv.oggettoInMano}? Vai a montarla sui supporti!");
                    inv.ConsegnaOggetto();
                }
                else if (tipoReparto == GameManager.Reparto.Fonico)
                {
                    if (GameManager.instance.micScelto != "")
                        GameManager.instance.RestituisciOggettoAlTavolo(GameManager.instance.micScelto);

                    GameManager.instance.micScelto = inv.oggettoInMano;
                    GameManager.instance.micDaInstallare = inv.oggettoInMano;
                    GameManager.instance.attoriMicrofonatiAttuali = 0; 
                    GameManager.instance.supportoPiazzato = false; 
                    Debug.Log($"[Fonico]: Cambio piano in {inv.oggettoInMano}. Installalo!");
                    inv.ConsegnaOggetto();
                }
            }
            else
            {
                Debug.Log($"[Task]: Oggetto sbagliato per questo reparto.");
            }
        }
        else
        {
            if (faseRevisione && tipoReparto != GameManager.Reparto.Regia)
                Debug.Log($"[{tipoReparto}]: Portami un nuovo oggetto se vuoi fare modifiche.");
            else
                Debug.Log($"[Info]: {messaggioTask}");
        }
    }
}