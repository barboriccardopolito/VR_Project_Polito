using UnityEngine;

public class InteragibileNPC : MonoBehaviour
{
    public GameManager.Reparto tipoReparto;
    [TextArea(3, 10)]
    public string messaggioTask;

    public void Interagisci()
    {
        NPCWander movimento = GetComponent<NPCWander>();
        if (movimento != null)
        {
            InterazioneGiocatore playerScript = FindObjectOfType<InterazioneGiocatore>();
            if (playerScript != null) movimento.AscoltaGiocatore(playerScript.transform);
        }

        InventarioGiocatore inv = FindObjectOfType<InventarioGiocatore>();

        // 1. VERIFICHE PRELIMINARI
        bool installazioneAudioInCorso = (GameManager.instance.micDaInstallare != "");
        // NUOVO: Controlliamo se stiamo installando le luci (se abbiamo scelto una luce ma la task non è finita)
        bool installazioneLuciInCorso = (GameManager.instance.LuceScelta != "" && GameManager.instance.taskAttuale == GameManager.Reparto.Luci);

        bool isSottotaskAttiva = installazioneAudioInCorso || installazioneLuciInCorso;

        // --- CONTROLLO TURNO ---
        if (GameManager.instance.taskAttuale != tipoReparto && !isSottotaskAttiva)
        {
            Debug.Log($"[{tipoReparto}]: Non è il mio turno. Vai da {GameManager.instance.taskAttuale}.");
            return;
        }

        // ------------------------------------------------------------------
        // LOGICA REPARTO LUCI (MODIFICATO PER GESTIRE L'INSTALLAZIONE)
        // ------------------------------------------------------------------
        if (tipoReparto == GameManager.Reparto.Luci && installazioneLuciInCorso)
        {
            // Controlliamo se il giocatore ha finito di piazzare le luci sui supporti
            if (GameManager.instance.LucePosizionataCorrettamente == true)
            {
                Debug.Log("<color=yellow>[Addetto Luci]:</color> Ottimo lavoro! Le luci sono piazzate e orientate correttamente. Task Completata.");
                
                // Chiudiamo la task
                GameManager.instance.CompletaTask(tipoReparto); 
                
                // (Opzionale) Reset delle variabili per pulizia
                // GameManager.instance.LuceScelta = ""; 
            }
            else
            {
                // Il giocatore ha la luce scelta ma non l'ha ancora messa sui supporti
                Debug.Log($"[Addetto Luci]: Hai preso la {GameManager.instance.LuceScelta}, ma i supporti sono ancora vuoti! Vai a montarla sui treppiedi.");
            }
            return; // Usciamo qui, non serve controllare l'inventario
        }
        // ------------------------------------------------------------------

        // --- LOGICA RITORNO DAL FONICO ---
        if (tipoReparto == GameManager.Reparto.Fonico && installazioneAudioInCorso)
        {
             // ... (TUTTO IL CODICE DEL FONICO RIMANE UGUALE A PRIMA) ...
             // Copia pure il blocco del Fonico che avevi già, non cambia nulla qui
             // ...
             // Per brevità non lo riscrivo tutto qui, ma lascia il tuo codice originale del Fonico
             // Se vuoi te lo incollo, ma è identico al tuo snippet.
        }


        // --- LOGICA PRODUZIONE ---
        if (tipoReparto == GameManager.Reparto.Produzione)
        {
            // ... (TUTTO UGUALE A PRIMA) ...
             RadioSistema radio = FindObjectOfType<RadioSistema>();
             if (radio != null) radio.haLaRadio = true;
             Debug.Log($"[Produzione]: {messaggioTask}");
             GameManager.instance.CompletaTask(tipoReparto);
             return;
        }

        // --- LOGICA REGIA ---
        if (tipoReparto == GameManager.Reparto.Regia)
        {
             // ... (TUTTO UGUALE A PRIMA) ...
             // Lascia il tuo codice Regia qui
        }

        // --- LOGICA CONSEGNA OGGETTI (INIZIO TASK) ---
        if (inv != null && inv.haUnOggetto)
        {
            bool oggettoCorretto = false;
            if (tipoReparto == GameManager.Reparto.Fotografia && inv.categoriaInMano == OggettoRaccolta.TipoOggetto.Lente) oggettoCorretto = true;
            if (tipoReparto == GameManager.Reparto.Luci && inv.categoriaInMano == OggettoRaccolta.TipoOggetto.Luce) oggettoCorretto = true;
            if (tipoReparto == GameManager.Reparto.Fonico && inv.categoriaInMano == OggettoRaccolta.TipoOggetto.Microfono) oggettoCorretto = true;

            if (oggettoCorretto)
            {
                // GESTIONE SPECIFICA LUCI (NUOVO)
                if (tipoReparto == GameManager.Reparto.Luci)
                {
                    // 1. Memorizziamo la luce scelta
                    GameManager.instance.LuceScelta = inv.oggettoInMano; // Es: "Fresnel"
                    GameManager.instance.LucePosizionataCorrettamente = false; // Reset stato installazione

                    Debug.Log($"[Addetto Luci]: Hai scelto {inv.oggettoInMano}. Ottima scelta. Ora vai a montarla sui supporti (Softbox Sinistra/Destra)!");
                    
                    // 2. Togliamo l'oggetto dalla mano
                    inv.ConsegnaOggetto();

                    // 3. NON chiamiamo CompletaTask() qui! Dobbiamo aspettare l'installazione.
                }
                // GESTIONE SPECIFICA FONICO
                else if (tipoReparto == GameManager.Reparto.Fonico)
                {
                    // ... (Tuo codice originale Fonico) ...
                    GameManager.instance.micScelto = inv.oggettoInMano;
                    GameManager.instance.micDaInstallare = inv.oggettoInMano;
                    inv.ConsegnaOggetto();
                    // Anche qui NON completiamo la task, aspettiamo l'installazione
                }
                // GESTIONE ALTRI (FOTOGRAFIA, ECC) - Consegna Immediata
                else
                {
                    Debug.Log($"[Task]: Grazie per {inv.oggettoInMano}!");
                    if (tipoReparto == GameManager.Reparto.Fotografia) GameManager.instance.lenteSceltaFinale = inv.oggettoInMano;
                    
                    inv.ConsegnaOggetto();
                    GameManager.instance.CompletaTask(tipoReparto); // Loro finiscono subito
                }
            }
            else
            {
                Debug.Log($"[Task]: Oggetto sbagliato per {tipoReparto}.");
            }
        }
        else
        {
            // Messaggio standard se non ho oggetti e non sto facendo task speciali
            Debug.Log($"[Info]: {messaggioTask}");
        }
    }
}