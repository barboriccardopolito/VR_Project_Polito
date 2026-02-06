using UnityEngine;

public class OggettoRaccolta : MonoBehaviour
{
    public enum TipoOggetto { Lente, Luce, Microfono }
    
    [Header("Configurazione Oggetto")]
    public TipoOggetto tipo;
    public string nomeOggetto; 

    public void EseguiRaccolta()
    {
        InventarioGiocatore inv = FindFirstObjectByType<InventarioGiocatore>();
        
        if (inv != null)
        {
            // Controllo opzionale: Se hai già qualcosa, non farti raccogliere altro
            if (inv.haUnOggetto)
            {
                Debug.Log("Hai le mani piene! Premi G per lasciare l'oggetto prima di prenderne un altro.");
                return; 
            }

            // --- MODIFICA: Passiamo 'this.gameObject' come terzo argomento ---
            // Così l'inventario sa esattamente chi siamo e può riattivarci con G
            inv.RaccogliOggetto(nomeOggetto, tipo, this.gameObject);
            
            // Ci nascondiamo dal tavolo
            gameObject.SetActive(false);
        }
    }
}