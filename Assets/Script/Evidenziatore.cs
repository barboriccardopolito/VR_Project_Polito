using UnityEngine;

public class Evidenziatore : MonoBehaviour
{
    [Header("Configurazione Freccia 2D")]
    [Tooltip("Trascina qui il Prefab della tua freccia 2D (Sprite)")]
    public GameObject prefabFreccia;
    
    [Header("Posizione e Animazione")]
    [Tooltip("Quanto in alto sopra l'oggetto deve stare la freccia")]
    public float altezzaBase = 1.2f; 
    [Tooltip("Velocità del galleggiamento su e giù")]
    public float velocitaAnimazione = 3f;
    [Tooltip("Quanto si muove su e giù")]
    public float ampiezzaAnimazione = 0.15f;

    private GameObject frecciaIstata;
    private bool isAttivo = false;
    private Collider targetCollider;
    private Renderer targetRenderer;
    private Camera cameraPrincipale;

    void Start()
    {
        targetCollider = GetComponent<Collider>();
        targetRenderer = GetComponent<Renderer>();
        cameraPrincipale = Camera.main;

        // Pulizia per sicurezza se avevi vecchi projector
        Projector oldProjector = GetComponent<Projector>();
        if (oldProjector != null) oldProjector.enabled = false;
        Projector childProjector = GetComponentInChildren<Projector>();
        if (childProjector != null) childProjector.enabled = false;
    }

    public void Accendi()
    {
        if (isAttivo) return; 

        if (prefabFreccia == null)
        {
            Debug.LogWarning($"Manca il Prefab della freccia sull'oggetto: {gameObject.name}");
            return;
        }

        if (frecciaIstata == null)
        {
            frecciaIstata = Instantiate(prefabFreccia);
            // Togliamo collider allo sprite per evitare bug col raggio visivo
            Collider[] colliders = frecciaIstata.GetComponentsInChildren<Collider>();
            foreach (Collider c in colliders) Destroy(c);
        }
        
        frecciaIstata.SetActive(true);
        isAttivo = true;
    }

    public void Spegni()
    {
        if (!isAttivo) return;

        if (frecciaIstata != null)
        {
            frecciaIstata.SetActive(false);
        }
        isAttivo = false;
    }

    void LateUpdate()
    {
        if (!isAttivo || frecciaIstata == null) return;

        Vector3 centroOggetto = transform.position;
        if (targetCollider != null) centroOggetto = targetCollider.bounds.center;
        else if (targetRenderer != null) centroOggetto = targetRenderer.bounds.center;

        float animazioneY = Mathf.Sin(Time.time * velocitaAnimazione) * ampiezzaAnimazione;
        frecciaIstata.transform.position = centroOggetto + Vector3.up * (altezzaBase + animazioneY);

        if (cameraPrincipale == null) cameraPrincipale = Camera.main;
        
        if (cameraPrincipale != null)
        {
            frecciaIstata.transform.forward = -cameraPrincipale.transform.forward;
        }
    }

    void OnDestroy()
    {
        if (frecciaIstata != null) Destroy(frecciaIstata);
    }
}