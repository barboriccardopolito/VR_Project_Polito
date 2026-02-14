using UnityEngine;

public class OggettoRaccolta : MonoBehaviour
{
    public enum TipoOggetto { Lente, Luce, Microfono }

    [Header("Dati Oggetto")]
    public TipoOggetto categoria; 
    
    [Tooltip("Scrivi qui il nome esatto che vuoi vedere a schermo (es. 'Grandangolo', 'Fresnel')")]
    public string nomeOggetto; 

    // --- NOVITÀ: DATI PER LA SCHEDA TECNICA ---
    [Header("Scheda Tecnica Ispezione")]
    public string nomeTecnico = "Nome Oggetto";
    
    [TextArea(3, 6)] // Allarga il box di testo nell'Inspector
    public string descrizioneTecnica = "Inserisci qui le specifiche tecniche...";
    // -----------------------------------------

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

            // 1. Il giocatore mette l'oggetto in mano
            inventario.RaccogliOggetto(nomeOggetto, categoria, gameObject);
            Debug.Log($"Hai raccolto: {nomeOggetto}");

            // 2. CHIAMATA ALL'NPC A DISTANZA
            AvviaFeedbackRemotoNPC();
        }
    }

    void AvviaFeedbackRemotoNPC()
    {
        // Trova tutti gli NPC nella scena
        NPC_Staff[] tuttiNPC = FindObjectsByType<NPC_Staff>(FindObjectsSortMode.None);

        foreach (NPC_Staff npc in tuttiNPC)
        {
            // Se ho raccolto una Lente e questo NPC è della Fotografia, fallo parlare!
            if (categoria == TipoOggetto.Lente && npc.ruoloNPC == GameManager.Reparto.Fotografia)
            {
                npc.ReazioneConsegnaLente(nomeOggetto);
                break; // Trovato, esci dal ciclo
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