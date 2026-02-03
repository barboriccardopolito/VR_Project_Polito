using UnityEngine;

public class InteragibileNPC : MonoBehaviour
{
    public GameManager.Reparto tipoReparto;

    // --- ECCO LA RIGA CHE MANCAVA ---
    [Header("Solo per Produzione")]
    public GameObject radioDaAttivare; // Trascina qui la radio sotto la Main Camera
    // -------------------------------

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

        // 1. VERIFICHE PRELIMINARI
        bool installazioneAudioInCorso = (GameManager.instance.micDaInstallare != "");
        bool installazioneLuciInCorso = (GameManager.instance.LuceScelta != "" && GameManager.instance.taskAttuale == GameManager.Reparto.Luci);

        bool isSottotaskAttiva = installazioneAudioInCorso || installazioneLuciInCorso;

        // --- CONTROLLO TURNO ---
        if (GameManager.instance.taskAttuale != tipoReparto && !isSottotaskAttiva)
        {
            Debug.Log($"[{tipoReparto}]: Non è il mio turno. Vai da {GameManager.instance.taskAttuale}.");
            return;
        }

        // --- LOGICA REPARTO LUCI ---
        if (tipoReparto == GameManager.Reparto.Luci && installazioneLuciInCorso)
        {
            if (GameManager.instance.LucePosizionataCorrettamente == true)
            {
                Debug.Log("<color=yellow>[Addetto Luci]:</color> Ottimo lavoro! Task Completata.");
                GameManager.instance.CompletaTask(tipoReparto); 
            }
            else
            {
                Debug.Log($"[Addetto Luci]: Hai preso la {GameManager.instance.LuceScelta}, ma i supporti sono vuoti! Montala.");
            }
            return;
        }

        // --- LOGICA RITORNO DAL FONICO ---
        if (tipoReparto == GameManager.Reparto.Fonico && installazioneAudioInCorso)
        {
             // (Codice standard del fonico per chiudere la task)
             bool taskCompletata = false;
             if (GameManager.instance.micDaInstallare == "Lavalier" && GameManager.instance.attoriMicrofonatiAttuali >= GameManager.instance.attoriDaMicrofonare) taskCompletata = true;
             else if ((GameManager.instance.micDaInstallare == "Boom" || GameManager.instance.micDaInstallare == "Ambisonic") && GameManager.instance.supportoPiazzato) taskCompletata = true;

             if (taskCompletata)
             {
                 Debug.Log($"<color=green>[Fonico]:</color> Ottimo! {GameManager.instance.micDaInstallare} installato.");
                 GameManager.instance.ApplicaEffettoMicrofono(GameManager.instance.micDaInstallare);
                 GameManager.instance.micDaInstallare = "";
                 GameManager.instance.supportoPiazzato = false; 
                 GameManager.instance.CompletaTask(tipoReparto);
             }
             else
             {
                 Debug.Log($"[Fonico]: Non hai ancora finito di installare il {GameManager.instance.micDaInstallare}.");
             }
             return;
        }

        // --- LOGICA PRODUZIONE (CON RADIO VISIVA) ---
        if (tipoReparto == GameManager.Reparto.Produzione)
        {
            RadioSistema radio = FindFirstObjectByType<RadioSistema>();
            if (radio != null) radio.haLaRadio = true;

            // ATTIVAZIONE VISIVA DELLA RADIO
            if (radioDaAttivare != null)
            {
                radioDaAttivare.SetActive(true); // Accende l'oggetto in mano
            }

            Debug.Log($"[Produzione]: {messaggioTask}");
            GameManager.instance.CompletaTask(tipoReparto);
            return;
        }

        // --- LOGICA REGIA ---
        if (tipoReparto == GameManager.Reparto.Regia)
        {
            if (!RegiaManager.instance.previewInCorso && !RegiaManager.instance.registrazioneInCorso)
            {
                RegiaManager.instance.AttivaPreview();
                Debug.Log("<color=cyan>[Regista]:</color> Guarda il monitor. Se sei pronto, interagisci di nuovo per il CIAK.");
                return;
            }
            if (RegiaManager.instance.previewInCorso)
            {
                Debug.Log("<color=red>[Regista]:</color> AZIONE!");
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
                if (tipoReparto == GameManager.Reparto.Luci)
                {
                    GameManager.instance.LuceScelta = inv.oggettoInMano;
                    GameManager.instance.LucePosizionataCorrettamente = false;
                    Debug.Log($"[Addetto Luci]: Hai scelto {inv.oggettoInMano}. Vai a montarla sui supporti!");
                    inv.ConsegnaOggetto();
                }
                else if (tipoReparto == GameManager.Reparto.Fonico)
                {
                    GameManager.instance.micScelto = inv.oggettoInMano;
                    GameManager.instance.micDaInstallare = inv.oggettoInMano;
                    Debug.Log($"[Fonico]: {inv.oggettoInMano} scelto. Vai a installarlo!");
                    inv.ConsegnaOggetto();
                }
                else
                {
                    Debug.Log($"[Task]: Grazie per {inv.oggettoInMano}!");
                    if (tipoReparto == GameManager.Reparto.Fotografia) GameManager.instance.lenteSceltaFinale = inv.oggettoInMano;
                    inv.ConsegnaOggetto();
                    GameManager.instance.CompletaTask(tipoReparto);
                }
            }
            else
            {
                Debug.Log($"[Task]: Oggetto sbagliato.");
            }
        }
        else
        {
            Debug.Log($"[Info]: {messaggioTask}");
        }
    }
}