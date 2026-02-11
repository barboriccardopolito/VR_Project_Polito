using UnityEngine;

public class OggettoRaccolta : MonoBehaviour
{
    public enum TipoOggetto { Lente, Luce, Microfono }

    [Header("Dati Oggetto")]
    public TipoOggetto categoria; // Che tipo è? (Lente, Luce...)
    
    [Tooltip("Scrivi qui il nome esatto che vuoi vedere a schermo (es. 'Grandangolo', 'Fresnel')")]
    public string nomeOggetto;    // Es. "Grandangolo", "Cinematografica", "Boom"

    // Riferimento all'evidenziatore
    private Evidenziatore evidenziatore;

    void Start()
    {
        // Cerca lo script Evidenziatore su questo oggetto o nei figli
        evidenziatore = GetComponent<Evidenziatore>();
        if (evidenziatore == null) evidenziatore = GetComponentInChildren<Evidenziatore>();

        // --- AGGIUNTA DI SICUREZZA ---
        // Se nell'Inspector lasci il campo vuoto, usa il nome del file 3D come fallback
        if (string.IsNullOrEmpty(nomeOggetto))
        {
            nomeOggetto = gameObject.name;
        }
    }

    void Update()
    {
        // Gestione costante dell'anello luminoso
        GestisciEvidenziatore();
    }

    public void EseguiRaccolta()
    {
        InventarioGiocatore inventario = FindFirstObjectByType<InventarioGiocatore>();
        
        if (inventario != null)
        {
            // Se l'inventario è pieno, non raccogliere
            if (inventario.haUnOggetto) 
            {
                Debug.Log("Inventario pieno!");
                return;
            }

            // Passa se stesso (gameObject) per essere gestito dall'inventario
            inventario.RaccogliOggetto(nomeOggetto, categoria, gameObject);
            
            Debug.Log($"Hai raccolto: {nomeOggetto}");
        }
    }

    // --- GESTIONE LUCI ---
    void GestisciEvidenziatore()
    {
        if (evidenziatore == null) return;
        
        // Se il GameManager non esiste (es. test isolato), esci
        if (GameManager.instance == null) return;

        GameManager.Reparto taskAttuale = GameManager.instance.taskAttuale;
        bool faseRevisione = (taskAttuale == GameManager.Reparto.Regia); // O fase finale
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