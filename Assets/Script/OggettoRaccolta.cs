using UnityEngine;

public class OggettoRaccolta : MonoBehaviour
{
    public enum TipoOggetto { Lente, Luce, Microfono }

    [Header("Dati Oggetto")]
    public TipoOggetto categoria; // Che tipo è?
    public string nomeOggetto;    // Es. "50mm", "Gelatina Blu", "Boom"

    // Riferimento all'evidenziatore
    private Evidenziatore evidenziatore;

    void Start()
    {
        // Cerca lo script Evidenziatore su questo oggetto o nei figli
        evidenziatore = GetComponent<Evidenziatore>();
        if (evidenziatore == null) evidenziatore = GetComponentInChildren<Evidenziatore>();
    }

    void Update()
    {
        // Gestione costante dell'anello luminoso
        GestisciEvidenziatore();
    }

    // --- QUESTA È LA FUNZIONE CHE MANCAVA ---
    public void EseguiRaccolta()
    {
        InventarioGiocatore inventario = FindFirstObjectByType<InventarioGiocatore>();
        
        if (inventario != null)
        {
            // Controlla se abbiamo già le mani piene (opzionale, ma consigliato)
            if (inventario.haUnOggetto)
            {
                Debug.Log("Hai già un oggetto in mano! Posalo o consegnalo prima.");
                return;
            }

            // 1. Passa i dati all'inventario
            inventario.haUnOggetto = true;
            inventario.oggettoInMano = nomeOggetto;
            inventario.categoriaInMano = categoria;
            
            Debug.Log($"Hai raccolto: {nomeOggetto} ({categoria})");

            // 2. Aggiorna la UI (se hai uno script UI collegato, altrimenti lo ignora)
            // inventario.AggiornaIconaUI(); 

            // 3. Nascondi questo oggetto dalla scena (perché ora è "in tasca")
            gameObject.SetActive(false);
        }
    }

    // --- GESTIONE LUCI ---
    void GestisciEvidenziatore()
    {
        if (evidenziatore == null) return;

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