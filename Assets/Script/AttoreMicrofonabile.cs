using UnityEngine;

public class AttoreMicrofonabile : MonoBehaviour
{
    [Header("Componenti")]
    public GameObject modelloLavalierAddosso;
    
    [Header("Audio")]
    public AudioClip suonoMontaggioLavalier;
    private AudioSource audioSource;

    private bool isMicrofonato = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f;

        if (modelloLavalierAddosso != null) 
            modelloLavalierAddosso.SetActive(false);
    }

    public void ProvaAMicrofonare()
    {
        if (GameManager.instance.micDaInstallare != "Lavalier") 
        {
            Debug.Log("Non serve il Lavalier ora!");
            return;
        }

        if (isMicrofonato) return;

        isMicrofonato = true;

        if (modelloLavalierAddosso != null) 
            modelloLavalierAddosso.SetActive(true);

        if (suonoMontaggioLavalier != null)
        {
            audioSource.PlayOneShot(suonoMontaggioLavalier);
        }

        GameManager.instance.attoriMicrofonatiAttuali++;
        Debug.Log($"Attore microfonato! ({GameManager.instance.attoriMicrofonatiAttuali}/{GameManager.instance.attoriDaMicrofonare})");

        if (GameManager.instance.attoriMicrofonatiAttuali >= GameManager.instance.attoriDaMicrofonare)
        {
            Debug.Log("Tutti gli attori sono pronti!");
            GameManager.instance.CompletaTask(GameManager.Reparto.Fonico);
        }
    }
}