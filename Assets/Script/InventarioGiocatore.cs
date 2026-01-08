using UnityEngine;

public class InventarioGiocatore : MonoBehaviour
{
    public string oggettoInMano = ""; 
    public OggettoRaccolta.TipoOggetto categoriaInMano;
    public bool haUnOggetto = false;
    
    [HideInInspector] 
    public GameObject riferimentoOggettoFisico;

    public void RaccogliOggetto(string nome, OggettoRaccolta.TipoOggetto tipo, GameObject oggettoFisico)
    {
        oggettoInMano = nome;
        categoriaInMano = tipo;
        haUnOggetto = true;
        riferimentoOggettoFisico = oggettoFisico;

        // APPLICAZIONE IMMEDIATA EFFETTO VISIVO
        if (tipo == OggettoRaccolta.TipoOggetto.Lente)
        {
            GameManager.instance.ApplicaEffettoLente(nome);
        }

        Debug.Log("Hai raccolto: " + nome);
    }

    public void LasciaOggetto()
    {
        if (!haUnOggetto) return;

        // Se lasciamo la lente, resettiamo la vista
        if (categoriaInMano == OggettoRaccolta.TipoOggetto.Lente)
        {
            GameManager.instance.ResetEffettoLente();
        }

        Debug.Log("Hai lasciato: " + oggettoInMano);

        if (riferimentoOggettoFisico != null)
        {
            riferimentoOggettoFisico.SetActive(true);
        }

        oggettoInMano = "";
        haUnOggetto = false;
        riferimentoOggettoFisico = null;
    }

    public void ConsegnaOggetto()
    {
        // Quando consegniamo, l'oggetto sparisce definitivamente
        oggettoInMano = "";
        haUnOggetto = false;
        riferimentoOggettoFisico = null;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) && haUnOggetto)
        {
            LasciaOggetto();
        }
    }
}