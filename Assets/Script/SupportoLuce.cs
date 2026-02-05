using UnityEngine;

public class SupportoLuce : MonoBehaviour
{
    // IMPORTANTE: Devono essere 'public' per apparire nell'Inspector!
    [Header("Trascina qui i modelli figli")]
    public GameObject modelloSoftbox;
    public GameObject modelloFresnel;
    public GameObject modelloArtistica;

    private bool luceGiaPosizionata = false;

    // --- NUOVA FUNZIONE: Chiamata dal GameManager quando cambi idea ---
    public void ResettaSupporto()
    {
        // 1. Sblocchiamo la logica: il supporto torna disponibile
        luceGiaPosizionata = false;

        // 2. Spegniamo tutto visivamente per pulizia
        NascondiTutto();

        Debug.Log($"[Supporto] {gameObject.name} resettato. Pronto per una nuova luce!");
    }
    // -----------------------------------------------------------------

    public void PiazzaLuce()
    {
        Debug.Log("1. Tentativo di piazzare luce..."); 

        // Se c'è già una luce, non facciamo nulla (a meno che non venga resettato prima)
        if (luceGiaPosizionata) return;

        // Recuperiamo il GameManager
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm == null) return;

        string luceScelta = gm.LuceScelta;

        Debug.Log("2. Luce trovata nel GameManager: '" + luceScelta + "'"); 

        NascondiTutto(); // Spegni tutto prima di accendere quella giusta

        bool luceTrovata = false;

        // Logica di accensione
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
            luceGiaPosizionata = true; // Blocchiamo il supporto finché non viene resettato
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