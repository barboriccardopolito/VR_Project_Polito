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
        // Recuperiamo il GameManager
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm == null) return;

        // Se c'è già una luce piazzata su QUESTO supporto, fermati
        if (luceGiaPosizionata) 
        {
            Debug.Log("Questo supporto ha già una luce.");
            return;
        }

        // Se il giocatore non ha scelto nessuna luce (stringa vuota), fermati
        if (string.IsNullOrEmpty(gm.LuceScelta))
        {
            Debug.Log("Non hai ancora scelto nessuna luce da piazzare!");
            return;
        }

        string nomeLuce = gm.LuceScelta;
        Debug.Log($"[Supporto] Tento di piazzare: '{nomeLuce}'");

        // Spegni tutto prima di accendere quella giusta
        NascondiTutto(); 

        bool luceTrovata = false;

        // --- CONTROLLO FLESSIBILE (Case Insensitive) ---
        // Usiamo .Contains e ignoriamo maiuscole/minuscole
        
        if (IsNameMatch(nomeLuce, "Softbox"))
        {
            if (modelloSoftbox != null) { modelloSoftbox.SetActive(true); luceTrovata = true; }
        }
        else if (IsNameMatch(nomeLuce, "Fresnel"))
        {
            if (modelloFresnel != null) { modelloFresnel.SetActive(true); luceTrovata = true; }
        }
        else if (IsNameMatch(nomeLuce, "Artistica")) // O "Ring", o "LuceArtistica"
        {
            if (modelloArtistica != null) { modelloArtistica.SetActive(true); luceTrovata = true; }
        }

        if (luceTrovata)
        {
            luceGiaPosizionata = true;
            
            // Diciamo al GameManager che ALMENO UNA luce è stata piazzata
            gm.LucePosizionataCorrettamente = true; 
            
            Debug.Log($"<color=green>SUCCESSO:</color> Piazzata {nomeLuce}!");
        }
        else
        {
            Debug.LogWarning($"<color=red>FALLITO:</color> Il nome '{nomeLuce}' non corrisponde a Softbox, Fresnel o Artistica.");
        }
    }

    // Funzione helper per pulire il controllo
    private bool IsNameMatch(string input, string target)
    {
        return input.IndexOf(target, System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    void NascondiTutto()
    {
        if(modelloSoftbox) modelloSoftbox.SetActive(false);
        if(modelloFresnel) modelloFresnel.SetActive(false);
        if(modelloArtistica) modelloArtistica.SetActive(false);
    }
}