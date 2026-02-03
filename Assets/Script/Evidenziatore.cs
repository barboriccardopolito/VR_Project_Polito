using UnityEngine;

public class Evidenziatore : MonoBehaviour
{
    [Header("Impostazioni")]
    public bool accendiAllAvvio = false; 

    [Header("Visuale")]
    [ColorUsage(true, true)]
    public Color coloreHighlight = new Color(1f, 1f, 0f, 1f); // Giallo
    [Range(0f, 20f)]
    public float intensita = 12f; // Valore alto per HDRP

    private Renderer rend;
    private Material[] materialiIstanza; // Array per gestire vestiti, pelle, ecc.

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend == null) rend = GetComponentInChildren<Renderer>();

        if (rend != null)
        {
            // Prendiamo TUTTI i materiali (vestiti, pelle, accessori)
            materialiIstanza = rend.materials;

            foreach (var mat in materialiIstanza)
            {
                mat.EnableKeyword("_EMISSION");
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                // Impostiamo emissione zero all'inizio
                mat.SetColor("_EmissiveColor", Color.black);
            }

            if (accendiAllAvvio) Accendi();
        }
    }

    public void Accendi()
    {
        if (rend == null || materialiIstanza == null) return;

        float valoreLum = Mathf.Pow(2, intensita);
        Color coloreFinale = coloreHighlight * valoreLum;

        foreach (var mat in materialiIstanza)
        {
            mat.SetColor("_EmissiveColor", coloreFinale);
            mat.SetColor("_EmissionColor", coloreFinale);
            
            if (mat.HasProperty("_EmissiveIntensity"))
                mat.SetFloat("_EmissiveIntensity", valoreLum);
        }

        rend.UpdateGIMaterials();
        Debug.Log($"[Evidenziatore] {gameObject.name} ACCESO - Intensità: {intensita}");
    }

    public void Spegni()
    {
        if (rend == null || materialiIstanza == null) return;

        foreach (var mat in materialiIstanza)
        {
            mat.SetColor("_EmissiveColor", Color.black);
            mat.SetColor("_EmissionColor", Color.black);
            
            if (mat.HasProperty("_EmissiveIntensity"))
                mat.SetFloat("_EmissiveIntensity", 0f);
        }

        rend.UpdateGIMaterials();
    }
}