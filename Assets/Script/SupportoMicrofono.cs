using UnityEngine;

public class SupportoMicrofono : MonoBehaviour
{
    [Tooltip("Scrivi qui 'Boom' o 'Ambisonic'")]
    public string tipoSupporto; 
    
    [Tooltip("Trascina qui l'oggetto visivo del microfono")]
    public GameObject meshMicrofono; 

    private bool giaPiazzato = false;

    void Start()
    {
        if (meshMicrofono != null) meshMicrofono.SetActive(false);
    }

    public void PiazzaMicrofono()
    {
        if (GameManager.instance.micDaInstallare == tipoSupporto)
        {
            if (!giaPiazzato)
            {
                if (tipoSupporto == "Ambisonic" && GameManager.instance.rumoreCaffeAttivo)
                {
                    Debug.Log("<color=orange>[Attenzione]:</color> Stai piazzando l'Ambisonic con la macchinetta accesa! Questo rumore finirà nella registrazione.");
                    // No return, lasciamo che il giocatore faccia l'errore se vuole.
                }

                giaPiazzato = true;
                if (meshMicrofono != null) meshMicrofono.SetActive(true);
                GameManager.instance.supportoPiazzato = true;
                
                Debug.Log($"<color=green>{tipoSupporto} posizionato!</color>");
            }
            else
            {
                Debug.Log("Hai già piazzato questo microfono.");
            }
        }
        else
        {
            if (GameManager.instance.micDaInstallare != "")
                Debug.Log($"Qui va il {tipoSupporto}, ma tu hai in mano il {GameManager.instance.micDaInstallare}!");
        }
    }
}