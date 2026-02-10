using UnityEngine;

public class InventarioGiocatore : MonoBehaviour
{
    [Header("Riferimento Mano")]
    public GameObject manoContainer; // Trascina qui 'ManoSinistra_Pivot'

    [Header("Stato")]
    public bool haUnOggetto = false;
    public string oggettoInMano = "";
    public OggettoRaccolta.TipoOggetto categoriaInMano;

    private GameObject oggettoTavoloNascosto;

    void Start()
    {
        // FIX: Invece di spegnere i figli diretti (che sono le cartelle Lenti/Luci e bloccano tutto),
        // cerchiamo tutti gli oggetti "foglia" e spegniamo solo loro.
        if (manoContainer != null)
        {
            // Prende tutti i componenti, anche quelli nascosti
            Transform[] tutti = manoContainer.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in tutti)
            {
                // Se ha un MeshRenderer, è un oggetto vero (non una cartella), quindi lo nascondiamo
                if (t.GetComponent<MeshRenderer>() != null)
                {
                    t.gameObject.SetActive(false);
                }
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) && haUnOggetto)
        {
            RilasciaOggetto();
        }
    }

    public void RaccogliOggetto(string nome, OggettoRaccolta.TipoOggetto tipo, GameObject objTavolo)
    {
        haUnOggetto = true;
        oggettoInMano = nome;
        categoriaInMano = tipo;
        oggettoTavoloNascosto = objTavolo;

        // Nascondi quello sul tavolo
        if (oggettoTavoloNascosto != null) oggettoTavoloNascosto.SetActive(false);

        // Mostra quello in mano
        AttivaModelloInMano(nome, true);
    }

    public void RilasciaOggetto()
    {
        // Riaccendi quello sul tavolo
        if (oggettoTavoloNascosto != null) oggettoTavoloNascosto.SetActive(true);

        if (GameManager.instance != null && categoriaInMano == OggettoRaccolta.TipoOggetto.Lente)
            GameManager.instance.ResetEffettoLente();

        // Nascondi quello in mano
        AttivaModelloInMano(oggettoInMano, false);

        haUnOggetto = false;
        oggettoInMano = "";
        oggettoTavoloNascosto = null;
    }

    public void ConsegnaOggetto()
    {
        AttivaModelloInMano(oggettoInMano, false);
        oggettoTavoloNascosto = null;
        haUnOggetto = false;
        oggettoInMano = "";
    }

    void AttivaModelloInMano(string nomeModello, bool attiva)
    {
        if (manoContainer == null) return;

        // Cerca in profondità (dentro Lenti, Luci, ecc.)
        Transform[] tuttiIFigli = manoContainer.GetComponentsInChildren<Transform>(true);
        
        foreach (Transform t in tuttiIFigli)
        {
            // Confronto nomi esatto
            if (t.name.Equals(nomeModello, System.StringComparison.OrdinalIgnoreCase))
            {
                t.gameObject.SetActive(attiva);
                
                // --- FIX CRUCIALE ---
                // Se stiamo attivando l'oggetto, assicuriamoci che anche il PADRE (la cartella Lenti/Luci) sia attivo!
                if (attiva && t.parent != manoContainer.transform)
                {
                    t.parent.gameObject.SetActive(true);
                }
                return; 
            }
        }
        
        if (attiva) Debug.LogError($"[Inventario] Non trovo l'oggetto '{nomeModello}' dentro la mano!");
    }
}