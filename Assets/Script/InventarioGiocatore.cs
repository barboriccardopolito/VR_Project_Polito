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
        // --- FIX VISIBILITÀ ---
        // Invece di spegnere i MeshRenderer (che rompe gli oggetti complessi),
        // spegniamo direttamente gli oggetti "figli" delle categorie (Luci, Lenti, Microfoni).
        
        if (manoContainer != null)
        {
            // Cicla attraverso le cartelle principali (Luci, Lenti, Microfoni)
            foreach (Transform categoria in manoContainer.transform)
            {
                // Assicuriamoci che la cartella categoria sia ACCESA, altrimenti non possiamo cercare dentro
                categoria.gameObject.SetActive(true);

                // Cicla attraverso gli oggetti veri e propri (Ambisonic, Fresnel, ecc.)
                foreach (Transform oggetto in categoria)
                {
                    // Spegni l'oggetto radice. I suoi figli (mesh) rimarranno attivi RELATIVAMENTE al padre.
                    // Quando riaccenderemo il padre, si vedrà tutto.
                    oggetto.gameObject.SetActive(false);
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
            // Confronto nomi esatto (ignora maiuscole/minuscole)
            if (t.name.Equals(nomeModello, System.StringComparison.OrdinalIgnoreCase))
            {
                t.gameObject.SetActive(attiva);
                
                // Se stiamo attivando, assicuriamoci che anche la cartella padre (es. Microfoni) sia visibile
                if (attiva && t.parent != manoContainer.transform)
                {
                    t.parent.gameObject.SetActive(true);
                }
                return; 
            }
        }
        
        if (attiva) Debug.LogError($"[Inventario] Non trovo l'oggetto '{nomeModello}' dentro la mano! Controlla i nomi nella Gerarchia.");
    }
}