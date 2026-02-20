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

        if (tipoReparto == GameManager.Reparto.Produzione) 
        { 
            NPCWander produzioneScript = GetComponent<NPCWander>();
            StartCoroutine(GestisciStatoParlato(4.5f)); 
            if (produzioneScript != null) produzioneScript.InterazioneConPlayer(); 
            else GameManager.instance.CompletaTask(tipoReparto);
            return;
        }

        if (tipoReparto == GameManager.Reparto.Regia && èIlMioTurno) 
        { 
            if (staffScript != null)
            {
                if (!staffScript.haGiaParlato)
                {
                    StartCoroutine(GestisciIntroRegista(staffScript));
                    return;
                }
                else
                {
                    if (RegiaManager.instance != null && RegiaManager.instance.previewInCorso) 
                    {
                        StartCoroutine(GestisciStatoParlato(3f, false)); 
                        staffScript.ReazioneCiak(() => { RegiaManager.instance.AvviaCiak(); });
                    }
                    return;
                }
            }
        }

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
            InterazioneGiocatore player = Object.FindFirstObjectByType<InterazioneGiocatore>();
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

    public IEnumerator GestisciStatoParlato(float durata, bool lanciaLavagna = false)
    {
        staParlando = true;
        yield return new WaitForSeconds(durata); 
        staParlando = false;

        if (lanciaLavagna)
        {
            FocusLavagna scriptLavagna = Object.FindFirstObjectByType<FocusLavagna>();
            if (scriptLavagna != null) scriptLavagna.AvviaInquadratura();
        }
    }

    private IEnumerator GestisciIntroRegista(NPC_Staff regista)
    {
        staParlando = true;

        InterazioneGiocatore playerInteract = Object.FindFirstObjectByType<InterazioneGiocatore>();
        if (playerInteract != null) playerInteract.enabled = false;

        if (playerInteract != null) regista.AttivaInterazione(playerInteract.transform);
        
        regista.AvviaDialogoIniziale();

        float durataIntro = 3f; 
        if (regista.clipsIntroduzione != null && regista.clipsIntroduzione.Length > 0)
        {
            durataIntro = 0f;
            foreach (AudioClip clip in regista.clipsIntroduzione)
            {
                if (clip != null) durataIntro += clip.length + 0.2f;
            }
        }

        yield return new WaitForSeconds(durataIntro);

        if (playerInteract != null) playerInteract.enabled = true;
        staParlando = false;

        if (RegiaManager.instance != null) RegiaManager.instance.AttivaPreview();
        if (GameManager.instance != null) GameManager.instance.MandaAttoriInScena();

        Debug.Log("<color=cyan>[Regia]</color> Introduzione completata! Attori sul set. Sblocca modifiche.");
    }
}