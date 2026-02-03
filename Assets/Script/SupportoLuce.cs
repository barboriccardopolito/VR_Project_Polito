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
        Debug.Log("1. Tentativo di piazzare luce..."); // Se non vedi questo, il Raycast del Player non sta chiamando questa funzione.

        if (luceGiaPosizionata) return;

        // Recuperiamo il GameManager
        GameManager gm = FindFirstObjectByType<GameManager>();
        string luceScelta = gm.LuceScelta;

        Debug.Log("2. Luce trovata nel GameManager: '" + luceScelta + "'"); // Cosa stampa qui? È vuoto?

        NascondiTutto();

        bool luceTrovata = false;

        if (luceScelta == "Softbox")
        {
            if (modelloSoftbox != null) { modelloSoftbox.SetActive(true); luceTrovata = true; }
            else Debug.LogError("ERRORE: Manca il collegamento al Modello Softbox nell'Inspector!");
        }
        else if (luceScelta == "Fresnel")
        {
            if (modelloFresnel != null) { modelloFresnel.SetActive(true); luceTrovata = true; }
            else Debug.LogError("ERRORE: Manca il collegamento al Modello Fresnel nell'Inspector!");
        }
        else if (luceScelta == "Artistica")
        {
            if (modelloArtistica != null) { modelloArtistica.SetActive(true); luceTrovata = true; }
            else Debug.LogError("ERRORE: Manca il collegamento al Modello Artistica nell'Inspector!");
        }

        if (luceTrovata)
        {
            luceGiaPosizionata = true;
            gm.LucePosizionataCorrettamente = true;
            Debug.Log("3. SUCCESSO: Luce attivata!");
        }
        else
        {
            Debug.LogWarning("4. FALLITO: Nessuna corrispondenza trovata per il nome: '" + luceScelta + "'");
        }
    }

    void NascondiTutto()
    {
        if(modelloSoftbox) modelloSoftbox.SetActive(false);
        if(modelloFresnel) modelloFresnel.SetActive(false);
        if(modelloArtistica) modelloArtistica.SetActive(false);
    }
}