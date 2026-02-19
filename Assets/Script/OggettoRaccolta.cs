using UnityEngine;

public class OggettoRaccolta : MonoBehaviour
{
    public enum TipoOggetto { Lente, Luce, Microfono }

    [Header("Dati Oggetto")]
    public TipoOggetto categoria; 
    
    [Tooltip("Scrivi qui il nome esatto che vuoi vedere a schermo (es. 'Grandangolo', 'Fresnel')")]
    public string nomeOggetto; 

    [Header("Scheda Tecnica Ispezione")]
    public string nomeTecnico = "Nome Oggetto";
    
    [TextArea(3, 6)]
    public string descrizioneTecnica = "Inserisci qui le specifiche tecniche...";

    private Evidenziatore evidenziatore;

    void Start()
    {
        evidenziatore = GetComponent<Evidenziatore>();
        if (evidenziatore == null) evidenziatore = GetComponentInChildren<Evidenziatore>();

        if (string.IsNullOrEmpty(nomeOggetto))
        {
            nomeOggetto = gameObject.name;
        }
    }

    void Update()
    {
        GestisciEvidenziatore();
    }

    public void EseguiRaccolta()
    {
        InventarioGiocatore inventario = FindFirstObjectByType<InventarioGiocatore>();
        
        if (inventario != null)
        {
            if (inventario.haUnOggetto) 
            {
                Debug.Log("Inventario pieno!");
                return;
            }

            // --- LA MAGIA DELLO SPAZZINO ---
            // Se siamo nella fase del Regista (o se stiamo semplicemente cambiando idea)
            // smontiamo in automatico l'attrezzatura vecchia di questa stessa categoria!
            if (GameManager.instance != null && GameManager.instance.taskAttuale == GameManager.Reparto.Regia)
            {
                SvuotaSupportiInScena(categoria);
            }

            // 1. Il giocatore mette l'oggetto in mano
            inventario.RaccogliOggetto(nomeOggetto, categoria, gameObject);
            Debug.Log($"Hai raccolto: {nomeOggetto}");

            // 2. CHIAMATA ALL'NPC A DISTANZA
            AvviaFeedbackRemotoNPC();
        }
    }

    // --- NUOVA FUNZIONE: SVUOTA I SUPPORTI ---
    void SvuotaSupportiInScena(TipoOggetto cat)
    {
        if (cat == TipoOggetto.Luce)
        {
            SupportoLuce[] luci = FindObjectsByType<SupportoLuce>(FindObjectsSortMode.None);
            foreach (SupportoLuce l in luci) l.ResettaSupporto();
        }
        else if (cat == TipoOggetto.Microfono)
        {
            SupportoMicrofono[] mics = FindObjectsByType<SupportoMicrofono>(FindObjectsSortMode.None);
            foreach (SupportoMicrofono m in mics) m.ResettaSupporto();
        }
        else if (cat == TipoOggetto.Lente)
        {
            SpostamentoCamera[] cams = FindObjectsByType<SpostamentoCamera>(FindObjectsSortMode.None);
            foreach (SpostamentoCamera c in cams) c.ResettaVisualeLenti();
        }
    }

    void AvviaFeedbackRemotoNPC()
    {
        NPC_Staff[] tuttiNPC = FindObjectsByType<NPC_Staff>(FindObjectsSortMode.None);

        foreach (NPC_Staff npc in tuttiNPC)
        {
            if (categoria == TipoOggetto.Lente && npc.ruoloNPC == GameManager.Reparto.Fotografia)
            {
                npc.ReazioneConsegnaLente(nomeOggetto);
                break; 
            }
            else if (categoria == TipoOggetto.Luce && npc.ruoloNPC == GameManager.Reparto.Luci)
            {
                npc.ReazioneConsegnaLuce(nomeOggetto);
                break;
            }
            else if (categoria == TipoOggetto.Microfono && npc.ruoloNPC == GameManager.Reparto.Fonico)
            {
                npc.ReazioneConsegnaMicrofono(nomeOggetto);
                break;
            }
        }
    }

    void GestisciEvidenziatore()
    {
        if (evidenziatore == null || GameManager.instance == null) return;

        GameManager.Reparto taskAttuale = GameManager.instance.taskAttuale;
        bool faseRevisione = (taskAttuale == GameManager.Reparto.Regia); 
        bool devoIlluminarmi = false;

        switch (categoria)
        {
            case TipoOggetto.Lente:
                if (taskAttuale == GameManager.Reparto.Fotografia || faseRevisione) devoIlluminarmi = true;
                break;
            case TipoOggetto.Luce:
                if (taskAttuale == GameManager.Reparto.Luci || faseRevisione) devoIlluminarmi = true;
                break;
            case TipoOggetto.Microfono:
                if (taskAttuale == GameManager.Reparto.Fonico || faseRevisione) devoIlluminarmi = true;
                break;
        }

        if (devoIlluminarmi) evidenziatore.Accendi();
        else evidenziatore.Spegni();
    }
}