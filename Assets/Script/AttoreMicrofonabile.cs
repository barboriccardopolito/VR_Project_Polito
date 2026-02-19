using UnityEngine;

public class AttoreMicrofonabile : MonoBehaviour
{
    [Header("Componenti")]
    public GameObject modelloLavalierAddosso;
    
    [Header("Audio")]
    public AudioClip suonoMontaggioLavalier;
    private AudioSource audioSource;

    [HideInInspector] public bool isMicrofonato = false;
    private Evidenziatore evidenziatore;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f;

        evidenziatore = GetComponent<Evidenziatore>();
        if (evidenziatore == null) evidenziatore = GetComponentInChildren<Evidenziatore>();

        if (modelloLavalierAddosso != null) 
            modelloLavalierAddosso.SetActive(false);
    }

    void Update()
    {
        GestisciEvidenziatore();
    }

    void GestisciEvidenziatore()
    {
        if (evidenziatore == null || GameManager.instance == null) return;

        bool faseFonico = (GameManager.instance.taskAttuale == GameManager.Reparto.Fonico);
        
        InventarioGiocatore inv = Object.FindFirstObjectByType<InventarioGiocatore>();
        bool hoLavalierInMano = (inv != null && inv.haUnOggetto && inv.oggettoInMano.Contains("Lavalier"));

        // L'attore si illumina SOLO se sei il fonico, hai il Lavalier in mano e non l'hai ancora microfonato
        if (faseFonico && hoLavalierInMano && !isMicrofonato)
        {
            evidenziatore.Accendi();
        }
        else
        {
            evidenziatore.Spegni();
        }
    }

    // Aggiungo il metodo standard Interagisci che usano gli altri tuoi script
    public void Interagisci()
    {
        ProvaAMicrofonare();
    }

    public void ProvaAMicrofonare()
    {
        // Controlliamo direttamente l'inventario del giocatore, non il GameManager!
        InventarioGiocatore inv = Object.FindFirstObjectByType<InventarioGiocatore>();
        bool hoLavalierInMano = (inv != null && inv.haUnOggetto && inv.oggettoInMano.Contains("Lavalier"));

        if (!hoLavalierInMano) 
        {
            return;
        }

        if (isMicrofonato) return;

        isMicrofonato = true;

        if (modelloLavalierAddosso != null) 
            modelloLavalierAddosso.SetActive(true);

        if (suonoMontaggioLavalier != null)
            audioSource.PlayOneShot(suonoMontaggioLavalier);

        if (GameManager.instance != null)
        {
            GameManager.instance.attoriMicrofonatiAttuali++;
            Debug.Log($"Attore microfonato! ({GameManager.instance.attoriMicrofonatiAttuali}/{GameManager.instance.attoriDaMicrofonare})");

            if (GameManager.instance.attoriMicrofonatiAttuali >= GameManager.instance.attoriDaMicrofonare)
            {
                inv.RimuoviOggetto(); // Consuma il Lavalier dalla mano solo quando hai finito con tutti
                GameManager.instance.CompletaTask(GameManager.Reparto.Fonico);
            }
        }
    }
}