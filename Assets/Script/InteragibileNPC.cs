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
            bool èIlMioTurno = (GameManager.instance.taskAttuale == tipoReparto);
            bool faseRevisione = (GameManager.instance.taskAttuale == GameManager.Reparto.Regia);
            
            NPC_Staff staffScript = GetComponent<NPC_Staff>();
            bool devoIlluminarmi = false;

            if (tipoReparto == GameManager.Reparto.Regia) 
            {
                if (faseRevisione) devoIlluminarmi = true;
            }
            else if (tipoReparto == GameManager.Reparto.Produzione)
            {
                 if (èIlMioTurno) devoIlluminarmi = true;
            }
            else 
            {
                if (èIlMioTurno && staffScript != null && !staffScript.haGiaParlato)
                    devoIlluminarmi = true;
            }

            if (staParlando) devoIlluminarmi = false;

            if (devoIlluminarmi) evidenziatore.Accendi(); else evidenziatore.Spegni();
        }
    }

    public void Interagisci()
    {
        if (staParlando) return;

        bool èIlMioTurno = (GameManager.instance.taskAttuale == tipoReparto);
        NPC_Staff staffScript = GetComponent<NPC_Staff>();

        // --- GESTIONE PRODUZIONE (Tutorial/Radio) ---
        if (tipoReparto == GameManager.Reparto.Produzione) 
        { 
            NPCWander produzioneScript = GetComponent<NPCWander>();
            
            // IL TRUCCO È QUI: Gli passo 'true' per dirgli che questa è la fine dell'intro!
            StartCoroutine(GestisciStatoParlato(4.5f));            
            if (produzioneScript != null) produzioneScript.InterazioneConPlayer(); 
            else GameManager.instance.CompletaTask(tipoReparto);
            return;
        }

        // --- GESTIONE REGIA (Ciak finale) ---
        if (tipoReparto == GameManager.Reparto.Regia && èIlMioTurno) 
        { 
            if (!RegiaManager.instance.previewInCorso && !RegiaManager.instance.registrazioneInCorso) {
                RegiaManager.instance.AttivaPreview();
                GameManager.instance.MandaAttoriInScena();
                return;
            }
            if (RegiaManager.instance.previewInCorso) {
                if (staffScript != null) {
                    StartCoroutine(GestisciStatoParlato(3f, false)); 
                    staffScript.ReazioneCiak(() => { RegiaManager.instance.AvviaCiak(); });
                }
                else RegiaManager.instance.AvviaCiak();
                return;
            }
        }

        // --- GESTIONE REPARTI TECNICI (Fotografia, Luci, Fonico) ---
        if (!èIlMioTurno)
        {
            Debug.Log($"<color=yellow>[{tipoReparto}]:</color> Non disturbare ora. Non è il mio turno.");
            if (staffScript != null && staffScript.audioNonEIlMioTurno != null)
            {
                StartCoroutine(GestisciStatoParlato(staffScript.audioNonEIlMioTurno.length, false));
                staffScript.GetComponent<AudioSource>().PlayOneShot(staffScript.audioNonEIlMioTurno);
            }
            return;
        }

        if (staffScript != null)
        {
            InterazioneGiocatore player = FindFirstObjectByType<InterazioneGiocatore>();
            if (player != null) staffScript.AttivaInterazione(player.transform);

            if (!staffScript.haGiaParlato)
            {
                StartCoroutine(GestisciStatoParlato(4.5f, false)); 
                staffScript.AvviaDialogoIniziale();
                Debug.Log($"[{tipoReparto}] Briefing iniziale avviato. Ora vai al tavolo!");
                return; 
            }
            else
            {
                string promemoria = "Vai al tavolo e prendi l'attrezzatura!";
                if (tipoReparto == GameManager.Reparto.Fotografia) promemoria = "Monta le lenti sulle macchine da presa!";
                if (tipoReparto == GameManager.Reparto.Luci) promemoria = "Piazza i fari sugli stativi!";
                if (tipoReparto == GameManager.Reparto.Fonico) promemoria = "Piazza il microfono sull'asta!";
                
                Debug.Log($"<color=orange>[{tipoReparto}]:</color> {promemoria}");
            }
        }
        else
        {
            Debug.Log($"[Info]: {messaggioTask}");
        }
    }

    // --- COROUTINE AGGIORNATA ---
    // Ho aggiunto la variabile "lanciaLavagna" per sapere se dobbiamo inquadrare la sceneggiatura a fine timer
    public IEnumerator GestisciStatoParlato(float durata, bool lanciaLavagna = false)
    {
        staParlando = true;
        yield return new WaitForSeconds(durata); // Aspetta che finisca di parlare
        staParlando = false;

        // Se è la produzione che ha finito di parlare, lancia la telecamera sulla lavagna!
        if (lanciaLavagna)
        {
            FocusLavagna scriptLavagna = Object.FindFirstObjectByType<FocusLavagna>();
            if (scriptLavagna != null)
            {
                scriptLavagna.AvviaInquadratura();
            }
            else
            {
                Debug.LogWarning("Script FocusLavagna non trovato nella scena! Assicurati di averlo messo sulla lavagna.");
            }
        }
    }
}