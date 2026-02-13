using UnityEngine;

public class OggettoRaccolta : MonoBehaviour
{
    public enum TipoOggetto { Lente, Luce, Microfono }

    [Header("Dati Oggetto")]
    public TipoOggetto categoria; // Che tipo è? (Lente, Luce...)
    
    [Tooltip("Scrivi qui il nome esatto che vuoi vedere a schermo (es. 'Grandangolo', 'Fresnel')")]
    public string nomeOggetto;    // Es. "Grandangolo", "Cinematografica", "Boom"

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

            inventario.RaccogliOggetto(nomeOggetto, categoria, gameObject);
            
            Debug.Log($"Hai raccolto: {nomeOggetto}");
        }
    }

    void GestisciEvidenziatore()
    {
        if (evidenziatore == null) return;
        
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