using UnityEngine;

public class SupportoLuce : MonoBehaviour
{
// IMPORTANTE: Devono essere 'public' per apparire nell'Inspector!
    [Header("Trascina qui i modelli figli")]
    public GameObject modelloSoftbox;
    public GameObject modelloFresnel;
    public GameObject modelloArtistica;

    private bool luceGiaPosizionata = false;
    public void PiazzaLuce()
    {
        if (luceGiaPosizionata) return; // Se c'è già una luce, non fare nulla

        // 1. Chiediamo al GameManager quale luce ha scelto l'utente
        // (Assumo tu abbia una variabile nel GameManager chiamata 'LuceScelta')
        string luceScelta = FindObjectOfType<GameManager>().LuceScelta; 

        if (string.IsNullOrEmpty(luceScelta))
        {
            Debug.Log("Devi prima scegliere una luce dal menu!");
            return;
        }

        // 2. Attiviamo il modello giusto
        NascondiTutto();
        
        if (luceScelta == "Softbox") modelloSoftbox.SetActive(true);
        else if (luceScelta == "Fresnel") modelloFresnel.SetActive(true);
        else if (luceScelta == "Artistica") modelloArtistica.SetActive(true);

        // 3. Avvisiamo il GameManager che il lavoro è fatto
        luceGiaPosizionata = true;
        FindObjectOfType<GameManager>().LucePosizionataCorrettamente = true;
        
        Debug.Log("Luce montata correttamnte!");
    }

    void NascondiTutto()
    {
        if(modelloSoftbox) modelloSoftbox.SetActive(false);
        if(modelloFresnel) modelloFresnel.SetActive(false);
        if(modelloArtistica) modelloArtistica.SetActive(false);
    }
}