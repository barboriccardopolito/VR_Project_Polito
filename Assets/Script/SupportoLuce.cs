using UnityEngine;

public class SupportoLuce : MonoBehaviour
{
    [Header("Trascina qui i modelli figli")]
    public GameObject modelloSoftbox;
    public GameObject modelloFresnel;
    public GameObject modelloArtistica;

    [Header("Audio")]
    public AudioClip suonoPiazzamento; // TRASCINA QUI IL TUO SFX (Click/Metallo)
    private AudioSource audioSource;

    private bool luceGiaPosizionata = false;

    void Start()
    {
        // Setup Componente Audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f; // Audio 3D (senti il click provenire dallo stativo)

        // Assicuriamoci che tutto sia spento all'inizio
        NascondiTutto();
    }

    public void PiazzaLuce()
    {
        // Recuperiamo il GameManager
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm == null) return;

        // Se c'è già una luce, fermati
        if (luceGiaPosizionata) 
        {
            Debug.Log("Questo supporto ha già una luce.");
            return;
        }

        // Se il giocatore non ha scelto nessuna luce
        if (string.IsNullOrEmpty(gm.LuceScelta))
        {
            Debug.Log("Non hai ancora scelto nessuna luce da piazzare!");
            return;
        }

        string nomeLuce = gm.LuceScelta;
        NascondiTutto(); 

        bool luceTrovata = false;

        // --- CONTROLLO FLESSIBILE (Case Insensitive) ---
        if (IsNameMatch(nomeLuce, "Softbox"))
        {
            if (modelloSoftbox != null) { modelloSoftbox.SetActive(true); luceTrovata = true; }
        }
        else if (IsNameMatch(nomeLuce, "Fresnel"))
        {
            if (modelloFresnel != null) { modelloFresnel.SetActive(true); luceTrovata = true; }
        }
        else if (IsNameMatch(nomeLuce, "Artistica")) 
        {
            if (modelloArtistica != null) { modelloArtistica.SetActive(true); luceTrovata = true; }
        }

        // --- SUCCESSO ---
        if (luceTrovata)
        {
            luceGiaPosizionata = true;
            gm.LucePosizionataCorrettamente = true;

            // RIPRODUCI IL SUONO (La novità)
            if (suonoPiazzamento != null)
            {
                audioSource.PlayOneShot(suonoPiazzamento);
            }
            
            Debug.Log($"<color=green>SUCCESSO: Piazzata {nomeLuce}!</color>");
        }
        else
        {
            Debug.LogWarning($"<color=red>FALLITO:</color> Il nome '{nomeLuce}' non corrisponde a Softbox, Fresnel o Artistica.");
        }
    }

    public void ResettaSupporto()
    {
        luceGiaPosizionata = false;
        NascondiTutto();
        Debug.Log($"[Supporto] {gameObject.name} resettato.");
    }

    void NascondiTutto()
    {
        if(modelloSoftbox) modelloSoftbox.SetActive(false);
        if(modelloFresnel) modelloFresnel.SetActive(false);
        if(modelloArtistica) modelloArtistica.SetActive(false);
    }

    // Helper per ignorare maiuscole/minuscole
    private bool IsNameMatch(string input, string target)
    {
        return input.IndexOf(target, System.StringComparison.OrdinalIgnoreCase) >= 0;
    }
}