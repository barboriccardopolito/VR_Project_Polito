using UnityEngine;

public class AttoreMicrofonabile : MonoBehaviour
{
    private bool giaMicrofonato = false;

    public void ProvaAMicrofonare()
    {
        // Controlliamo la stringa invece del vecchio bool
        if (GameManager.instance.micDaInstallare == "Lavalier")
        {
            if (!giaMicrofonato)
            {
                giaMicrofonato = true;
                GameManager.instance.attoriMicrofonatiAttuali++;
                Debug.Log($"Attore microfonato! ({GameManager.instance.attoriMicrofonatiAttuali}/{GameManager.instance.attoriDaMicrofonare})");
                
                if (GameManager.instance.attoriMicrofonatiAttuali == GameManager.instance.attoriDaMicrofonare)
                    Debug.Log("Tutti microfonati! Torna dal Fonico.");
            }
            else
            {
                Debug.Log("Attore già microfonato.");
            }
        }
    }
}