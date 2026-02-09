using UnityEngine;

public class SupportoMicrofono : MonoBehaviour
{
    [Tooltip("Scrivi qui 'Boom' o 'Ambisonic' (Esattamente come nello script OggettoRaccolta)")]
    public string tipoSupporto; 
    
    [Tooltip("Trascina qui l'oggetto visivo del microfono (quello che appare quando lo monti)")]
    public GameObject meshMicrofono; 

    // Riferimento all'anello luminoso
    private Evidenziatore evidenziatore;

    private bool giaPiazzato = false;

    void Start()
    {
        // 1. Cerca l'evidenziatore (se l'hai messo come figlio o sull'oggetto stesso)
        evidenziatore = GetComponent<Evidenziatore>();
        if (evidenziatore == null) evidenziatore = GetComponentInChildren<Evidenziatore>();

        // 2. Nascondi la mesh del microfono all'inizio (il supporto è vuoto)
        if (meshMicrofono != null) meshMicrofono.SetActive(false);
    }

    void Update()
    {
        // --- LOGICA LUCE GUIDA ---
        if (evidenziatore != null)
        {
            // Si illumina SOLO se:
            // A. Il GameManager dice che dobbiamo installare PROPRIO questo tipo di microfono
            // B. E non l'abbiamo ancora piazzato (altrimenti resterebbe acceso per sempre)
            bool devoIlluminarmi = (GameManager.instance.micDaInstallare == tipoSupporto) && !giaPiazzato;

            if (devoIlluminarmi) evidenziatore.Accendi();
            else evidenziatore.Spegni();
        }
    }

    public void PiazzaMicrofono()
    {
        // Controlla se è il supporto giusto
        if (GameManager.instance.micDaInstallare == tipoSupporto)
        {
            if (!giaPiazzato)
            {
                // Controllo extra per il rumore ambientale (solo per Ambisonic)
                if (tipoSupporto == "Ambisonic" && GameManager.instance.rumoreCaffeAttivo)
                {
                    Debug.Log("<color=orange>[Attenzione]:</color> Stai piazzando l'Ambisonic con la macchinetta accesa! Questo rumore finirà nella registrazione.");
                }

                giaPiazzato = true;
                
                // Mostra il microfono montato
                if (meshMicrofono != null) meshMicrofono.SetActive(true);
                
                // Aggiorna il GameManager
                GameManager.instance.supportoPiazzato = true;
                
                Debug.Log($"<color=green>{tipoSupporto} posizionato!</color>");

                // Nota: L'Update al prossimo frame vedrà che 'giaPiazzato' è true 
                // e spegnerà automaticamente l'anello. Magia! ✨
            }
            else
            {
                Debug.Log("Hai già piazzato questo microfono.");
            }
        }
        else
        {
            // Feedback se sbagli supporto
            if (GameManager.instance.micDaInstallare != "")
                Debug.Log($"Qui va il {tipoSupporto}, ma tu devi montare il {GameManager.instance.micDaInstallare}!");
            else
                Debug.Log("Non hai nessun microfono da installare. Parla prima col Fonico.");
        }
    }
}