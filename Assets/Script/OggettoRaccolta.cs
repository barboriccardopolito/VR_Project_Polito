using UnityEngine;

public class OggettoRaccolta : MonoBehaviour
{
    public enum TipoOggetto { Lente, Luce, Microfono }
    
    [Header("Impostazioni Oggetto")]
    public TipoOggetto categoria; 
    public string nomeOggetto; 

    public void EseguiRaccolta()
    {
        InventarioGiocatore inv = FindObjectOfType<InventarioGiocatore>();

        if (inv != null)
        {
            if (!inv.haUnOggetto)
            {
                // Passiamo 'gameObject' così l'inventario sa cosa riattivare se lo lasciamo
                inv.RaccogliOggetto(nomeOggetto, categoria, gameObject);
                gameObject.SetActive(false); 
            }
            else
            {
                Debug.Log("Mani occupate! Premi G per lasciare l'oggetto attuale.");
            }
        }
    }
}