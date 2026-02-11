using UnityEngine;

public class AttoreMicrofonabile : MonoBehaviour
{
    [Header("Componenti")]
    public GameObject modelloLavalierAddosso; // Il modello 3D del microfono sul petto (spento all'inizio)
    
    [Header("Audio")]
    public AudioClip suonoMontaggioLavalier; // TRASCINA QUI IL TUO EFFETTO AUDIO!
    private AudioSource audioSource;

    private bool isMicrofonato = false;

    void Start()
    {
        // Setup Audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f; // Audio 3D (lo senti provenire dall'attore)

        // Assicuriamoci che il microfono visivo sia spento all'inizio
        if (modelloLavalierAddosso != null) 
            modelloLavalierAddosso.SetActive(false);
    }

    // Questa funzione viene chiamata da InterazioneGiocatore quando premi E
    public void ProvaAMicrofonare()
    {
        // 1. Controllo: Dobbiamo mettere i Lavalier?
        if (GameManager.instance.micDaInstallare != "Lavalier") 
        {
            Debug.Log("Non serve il Lavalier ora!");
            return;
        }

        // 2. Controllo: L'abbiamo già messo a questo attore?
        if (isMicrofonato) return;

        // --- ESECUZIONE ---
        isMicrofonato = true;

        // Accendi la grafica del microfono
        if (modelloLavalierAddosso != null) 
            modelloLavalierAddosso.SetActive(true);

        // RIPRODUCI L'EFFETTO SONORO (La novità)
        if (suonoMontaggioLavalier != null)
        {
            audioSource.PlayOneShot(suonoMontaggioLavalier);
        }

        // Avvisa il GameManager
        GameManager.instance.attoriMicrofonatiAttuali++;
        Debug.Log($"Attore microfonato! ({GameManager.instance.attoriMicrofonatiAttuali}/{GameManager.instance.attoriDaMicrofonare})");

        // Se abbiamo finito tutti gli attori, completiamo la task
        if (GameManager.instance.attoriMicrofonatiAttuali >= GameManager.instance.attoriDaMicrofonare)
        {
            Debug.Log("Tutti gli attori sono pronti!");
            GameManager.instance.CompletaTask(GameManager.Reparto.Fonico);
        }
    }
}