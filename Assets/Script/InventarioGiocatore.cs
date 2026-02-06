using UnityEngine;
using System.Collections.Generic;

public class InventarioGiocatore : MonoBehaviour
{
    [Header("Stato Inventario")]
    public bool haUnOggetto = false;
    public string oggettoInMano = "";
    public OggettoRaccolta.TipoOggetto categoriaInMano;

    [Header("Visuale Mano Sinistra")]
    public GameObject pivotManoSinistra; 
    
    private Transform[] tuttiIModelli;
    private GameObject modelloAttualeVisibile = null;

    // --- NUOVO: Qui ricordiamo quale oggetto fisico abbiamo nascosto dal tavolo ---
    private GameObject oggettoFisicoSulTavolo; 

    void Start()
    {
        if (pivotManoSinistra != null)
        {
            tuttiIModelli = pivotManoSinistra.GetComponentsInChildren<Transform>(true);
        }
    }

    void Update()
    {
        // --- LOGICA RILASCIO (Tasto G) ---
        if (Input.GetKeyDown(KeyCode.G) && haUnOggetto)
        {
            RilasciaOggetto();
        }
    }

    // --- MODIFICA: Ora accettiamo anche l'oggetto fisico come 3° parametro ---
    public void RaccogliOggetto(string nome, OggettoRaccolta.TipoOggetto tipo, GameObject objTavolo)
    {
        haUnOggetto = true;
        oggettoInMano = nome;
        categoriaInMano = tipo;
        oggettoFisicoSulTavolo = objTavolo; // Memorizziamo: "Questo è l'oggetto da riaccendere se premo G"

        AggiornaVisualeMano();
    }

    // Funzione chiamata quando premi G
    public void RilasciaOggetto()
    {
        Debug.Log($"[Inventario] Ho lasciato cadere: {oggettoInMano}");

        // 1. Riaccendiamo l'oggetto originale sul tavolo (così "torna" al suo posto)
        if (oggettoFisicoSulTavolo != null)
        {
            oggettoFisicoSulTavolo.SetActive(true);
            oggettoFisicoSulTavolo = null; // Dimentichiamo il riferimento
        }
        
        // 2. Resettiamo l'effetto Lente se lo avevamo attivo (Importante!)
        // Se lasci cadere la lente, non devi continuare a vedere distorto.
        if (GameManager.instance != null && categoriaInMano == OggettoRaccolta.TipoOggetto.Lente)
        {
            GameManager.instance.ResetEffettoLente();
        }

        // 3. Puliamo l'inventario
        haUnOggetto = false;
        oggettoInMano = "";
        
        // 4. Spegniamo la mano
        AggiornaVisualeMano();
    }

    public void ConsegnaOggetto()
    {
        // Quando consegni all'NPC, l'oggetto NON torna al tavolo (lo prende lui).
        // Quindi svuotiamo il riferimento senza fare SetActive(true).
        oggettoFisicoSulTavolo = null; 

        haUnOggetto = false;
        oggettoInMano = "";
        
        AggiornaVisualeMano();
    }

    void AggiornaVisualeMano()
    {
        if (pivotManoSinistra == null || tuttiIModelli == null) return;

        if (modelloAttualeVisibile != null)
        {
            modelloAttualeVisibile.SetActive(false);
            modelloAttualeVisibile = null;
        }

        if (haUnOggetto && oggettoInMano != "")
        {
            foreach (Transform t in tuttiIModelli)
            {
                if (t.name.ToLower() == oggettoInMano.ToLower())
                {
                    t.gameObject.SetActive(true);
                    modelloAttualeVisibile = t.gameObject;
                    return;
                }
            }
        }
    }
}