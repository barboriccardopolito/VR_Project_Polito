using UnityEngine;

public class SupportoLuce : MonoBehaviour
{
    [Header("Trascina qui i modelli figli")]
    public GameObject modelloSoftbox;
    public GameObject modelloFresnel;
    public GameObject modelloArtistica;

    [Header("Audio")]
    public AudioClip suonoPiazzamento;
    private AudioSource audioSource;

    private bool luceGiaPosizionata = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f; // Audio 3D (senti il click provenire dallo stativo)

        NascondiTutto();
    }

    public void PiazzaLuce()
    {
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm == null) return;

        if (luceGiaPosizionata) 
        {
            Debug.Log("Questo supporto ha già una luce.");
            return;
        }

        if (string.IsNullOrEmpty(gm.LuceScelta))
        {
            Debug.Log("Non hai ancora scelto nessuna luce da piazzare!");
            return;
        }

        string nomeLuce = gm.LuceScelta;
        NascondiTutto(); 

        bool luceTrovata = false;

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

        if (luceTrovata)
        {
            luceGiaPosizionata = true;
            gm.LucePosizionataCorrettamente = true;

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

    private bool IsNameMatch(string input, string target)
    {
        return input.IndexOf(target, System.StringComparison.OrdinalIgnoreCase) >= 0;
    }
}