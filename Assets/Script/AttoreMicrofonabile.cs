using UnityEngine;

public class AttoreMicrofonabile : MonoBehaviour
{
    [Header("Componenti")]
    public GameObject modelloLavalierAddosso;
    
    [Header("Audio Montaggio")]
    public AudioClip suonoMontaggioLavalier;
    
    [Header("Voci Attore (Dialoghi)")]
    [Tooltip("Cosa dice l'attore se ci parli normalmente (es. 'Ciao, sto ripassando il copione')")]
    public AudioClip battutaNormale;
    [Tooltip("Cosa dice appena gli monti il microfono (es. 'Ok, l'hai montato bene')")]
    public AudioClip battutaReazioneMicrofono;

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

        evidenziatore.Accendi();
    }

    public void Interagisci()
    {
        ProvaAMicrofonare(); 
    }

    void FaiParlareAttore()
    {
        if (battutaNormale != null && !audioSource.isPlaying)
        {
            audioSource.PlayOneShot(battutaNormale);
        }
    }

    public void ProvaAMicrofonare()
    {
        InventarioGiocatore inv = Object.FindFirstObjectByType<InventarioGiocatore>();
        bool hoLavalierInMano = (inv != null && inv.haUnOggetto && inv.oggettoInMano.Contains("Lavalier"));
        bool faseFonico = (GameManager.instance != null && GameManager.instance.taskAttuale == GameManager.Reparto.Fonico);

        if (!faseFonico || !hoLavalierInMano)
        {
            FaiParlareAttore();
            return;
        }

        if (isMicrofonato) return;

        isMicrofonato = true;

        if (modelloLavalierAddosso != null) 
            modelloLavalierAddosso.SetActive(true);

        if (suonoMontaggioLavalier != null)
            audioSource.PlayOneShot(suonoMontaggioLavalier);

        if (battutaReazioneMicrofono != null)
            audioSource.PlayOneShot(battutaReazioneMicrofono);

        if (GameManager.instance != null)
        {
            GameManager.instance.attoriMicrofonatiAttuali++;
            Debug.Log($"Attore microfonato! ({GameManager.instance.attoriMicrofonatiAttuali}/{GameManager.instance.attoriDaMicrofonare})");

            if (GameManager.instance.attoriMicrofonatiAttuali >= GameManager.instance.attoriDaMicrofonare)
            {
                if (inv != null) inv.RimuoviOggetto(); 
                GameManager.instance.CompletaTask(GameManager.Reparto.Fonico);
            }
        }
    }
}