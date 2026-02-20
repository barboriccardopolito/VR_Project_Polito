using UnityEngine;

public class InventarioGiocatore : MonoBehaviour
{
    [Header("Riferimento Mano")]
    public GameObject manoContainer;

    [Header("Stato")]
    public bool haUnOggetto = false;
    public string oggettoInMano = "";
    public OggettoRaccolta.TipoOggetto categoriaInMano;

    private GameObject oggettoTavoloNascosto;

    void Start()
    {   
        if (manoContainer != null)
        {
            foreach (Transform categoria in manoContainer.transform)
            {
                categoria.gameObject.SetActive(true);

                foreach (Transform oggetto in categoria)
                {
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

        if (oggettoTavoloNascosto != null) oggettoTavoloNascosto.SetActive(false);

        AttivaModelloInMano(nome, true);
    }

    public void RilasciaOggetto()
    {
        if (oggettoTavoloNascosto != null) oggettoTavoloNascosto.SetActive(true);

        if (GameManager.instance != null && categoriaInMano == OggettoRaccolta.TipoOggetto.Lente)
            GameManager.instance.ResetEffettoLente();

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

    public void RimuoviOggetto()
    {
        ConsegnaOggetto();
    }

    void AttivaModelloInMano(string nomeModello, bool attiva)
    {
        if (manoContainer == null) return;

        Transform[] tuttiIFigli = manoContainer.GetComponentsInChildren<Transform>(true);
        
        foreach (Transform t in tuttiIFigli)
        {
            if (t.name.Equals(nomeModello, System.StringComparison.OrdinalIgnoreCase))
            {
                t.gameObject.SetActive(attiva);
                
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