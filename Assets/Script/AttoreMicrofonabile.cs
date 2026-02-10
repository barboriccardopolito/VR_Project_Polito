using UnityEngine;

public class AttoreMicrofonabile : MonoBehaviour
{
    private bool giaMicrofonato = false;
    private Evidenziatore evidenziatore;

    void Start()
    {
        evidenziatore = GetComponent<Evidenziatore>();
        if (evidenziatore == null) evidenziatore = GetComponentInChildren<Evidenziatore>();
    }

    void Update()
    {
        // Illuminalo SOLO se:
        // 1. Dobbiamo mettere i Lavalier
        // 2. Non è ancora stato microfonato
        if (evidenziatore != null)
        {
            if (GameManager.instance.micDaInstallare == "Lavalier" && !giaMicrofonato)
            {
                evidenziatore.Accendi();
            }
            else
            {
                evidenziatore.Spegni();
            }
        }
    }

    public void ProvaAMicrofonare()
    {
        // Controllo di sicurezza: Posso microfonare solo se ho scelto i Lavalier
        if (GameManager.instance.micDaInstallare == "Lavalier")
        {
            if (!giaMicrofonato)
            {
                giaMicrofonato = true;
                GameManager.instance.attoriMicrofonatiAttuali++;
                
                Debug.Log($"<color=green>[Audio]</color> Attore microfonato! ({GameManager.instance.attoriMicrofonatiAttuali}/{GameManager.instance.attoriDaMicrofonare})");
                
                // Feedback audio (opzionale: suono di "zip" o "click")
                // AudioSource.PlayClipAtPoint(suonoInstallazione, transform.position);

                if (GameManager.instance.attoriMicrofonatiAttuali >= GameManager.instance.attoriDaMicrofonare)
                    Debug.Log("<color=yellow>[Task]</color> Tutti gli attori sono pronti! Torna dal Fonico.");
            }
            else
            {
                Debug.Log("Questo attore ha già il microfono.");
            }
        }
        else
        {
            Debug.Log("Non serve microfonare i singoli attori con questo setup (Boom/Ambisonic).");
        }
    }
}