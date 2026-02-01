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
            // Cerchiamo il giocatore per girarci verso di lui
            // Nota: Usa FindObjectOfType per sicurezza, funziona sempre se hai lo script InterazioneGiocatore sul player
            InterazioneGiocatore playerScript = FindObjectOfType<InterazioneGiocatore>();
            if (playerScript != null)
            {
                movimento.AscoltaGiocatore(playerScript.transform);
            }
        }

        InventarioGiocatore inv = FindObjectOfType<InventarioGiocatore>();
        
        // Verifica se stiamo installando qualcosa (qualsiasi mic)
        bool installazioneInCorso = (GameManager.instance.micDaInstallare != "");

        // Se siamo Attori o Supporti durante l'installazione, saltiamo il controllo turno
        bool isSottotaskAttiva = installazioneInCorso;

        // --- CONTROLLO TURNO ---
        // Se non è il mio turno E non stiamo facendo una sottotask di installazione su Attori/Supporti
        if (GameManager.instance.taskAttuale != tipoReparto && !isSottotaskAttiva)
        {
            Debug.Log($"[{tipoReparto}]: Non è il mio turno. Vai da {GameManager.instance.taskAttuale}.");
            return;
        }

        // --- LOGICA RITORNO DAL FONICO ---
        if (tipoReparto == GameManager.Reparto.Fonico && installazioneInCorso)
        {
            bool taskCompletata = false;

            // Caso 1: LAVALIER (Controllo numero attori)
            if (GameManager.instance.micDaInstallare == "Lavalier")
            {
                if (GameManager.instance.attoriMicrofonatiAttuali >= GameManager.instance.attoriDaMicrofonare)
                    taskCompletata = true;
                else
                    Debug.Log($"[Fonico]: Mancano {GameManager.instance.attoriDaMicrofonare - GameManager.instance.attoriMicrofonatiAttuali} attori!");
            }
            // Caso 2: BOOM o AMBISONIC (Controllo piazzamento supporto)
            else if (GameManager.instance.micDaInstallare == "Boom" || GameManager.instance.micDaInstallare == "Ambisonic")
            {
                if (GameManager.instance.supportoPiazzato)
                    taskCompletata = true;
                else
                    Debug.Log($"[Fonico]: Non hai ancora posizionato il {GameManager.instance.micDaInstallare} sul set!");
            }

            if (taskCompletata)
            {
                Debug.Log($"<color=green>[Fonico]:</color> Perfetto! {GameManager.instance.micDaInstallare} installato. Task completata.");
                GameManager.instance.ApplicaEffettoMicrofono(GameManager.instance.micDaInstallare);
                
                // Reset variabili installazione
                GameManager.instance.micDaInstallare = "";
                GameManager.instance.supportoPiazzato = false; 
                
                GameManager.instance.CompletaTask(tipoReparto);
            }
            return;
        }

        // --- LOGICA PRODUZIONE ---
        if (tipoReparto == GameManager.Reparto.Produzione)
        {
            RadioSistema radio = FindObjectOfType<RadioSistema>();
            if (radio != null) radio.haLaRadio = true;
            Debug.Log($"[Produzione]: {messaggioTask}");
            GameManager.instance.CompletaTask(tipoReparto);
            return;
        }

        // --- LOGICA REGIA (NUOVO) ---
        if (tipoReparto == GameManager.Reparto.Regia)
        {
            // Stato 1: Non ho ancora visto la preview
            if (!RegiaManager.instance.previewInCorso && !RegiaManager.instance.registrazioneInCorso)
            {
                RegiaManager.instance.AttivaPreview();
                Debug.Log("<color=cyan>[Regista]:</color> Guarda il monitor. Ecco come verranno le inquadrature con le lenti e le luci che hai scelto. Se sei pronto, interagisci di nuovo per il CIAK.");
                return;
            }

            // Stato 2: Sto guardando la preview, voglio registrare
            if (RegiaManager.instance.previewInCorso)
            {
                Debug.Log("<color=red>[Regista]:</color> Silenzio sul set... AZIONE!");
                RegiaManager.instance.AvviaCiak();
                return;
            }
        }

        // --- LOGICA CONSEGNA OGGETTI ---
        if (inv != null && inv.haUnOggetto)
        {
            bool oggettoCorretto = false;
            if (tipoReparto == GameManager.Reparto.Fotografia && inv.categoriaInMano == OggettoRaccolta.TipoOggetto.Lente) oggettoCorretto = true;
            if (tipoReparto == GameManager.Reparto.Luci && inv.categoriaInMano == OggettoRaccolta.TipoOggetto.Luce) oggettoCorretto = true;
            if (tipoReparto == GameManager.Reparto.Fonico && inv.categoriaInMano == OggettoRaccolta.TipoOggetto.Microfono) oggettoCorretto = true;

            if (oggettoCorretto)
            {
                if (tipoReparto == GameManager.Reparto.Fonico)
                {
                    GameManager.instance.micScelto = inv.oggettoInMano;
                    GameManager.instance.micDaInstallare = inv.oggettoInMano;
                    
                    if (inv.oggettoInMano == "Lavalier")
                        Debug.Log("[Fonico]: Lavalier scelti. Vai a metterli ai 3 attori!");
                    else
                        Debug.Log($"[Fonico]: {inv.oggettoInMano} scelto. Vai a posizionarlo sull'asta corretta nel set!");

                    inv.ConsegnaOggetto();
                }
                else
                {
                    Debug.Log($"[Task]: Grazie per {inv.oggettoInMano}!");
                    if (tipoReparto == GameManager.Reparto.Fotografia) GameManager.instance.lenteSceltaFinale = inv.oggettoInMano;
                    if (tipoReparto == GameManager.Reparto.Luci) GameManager.instance.luceScelta = inv.oggettoInMano;
                    
                    inv.ConsegnaOggetto();
                    GameManager.instance.CompletaTask(tipoReparto);
                }
            }
            else
            {
                Debug.Log($"[Task]: Oggetto sbagliato per {tipoReparto}.");
            }
        }
        else
        {
            Debug.Log($"[Info]: {messaggioTask}");
        }
    }
}