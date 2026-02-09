using UnityEngine;

public class Evidenziatore : MonoBehaviour
{
    [Header("Impostazioni")]
    public bool AccendiAllAvvio = false;
    public float velocitaPulsazione = 2f;
    public float dimensioneMin = 0.8f;
    public float dimensioneMax = 1.2f;

    [Header("Visuale")]
    public SpriteRenderer anelloGrafico; // Trascina qui l'oggetto Sprite che hai creato
    
    private bool isAttivo = false;
    private Vector3 scalaIniziale;

    void Start()
    {
        if (anelloGrafico == null)
            anelloGrafico = GetComponentInChildren<SpriteRenderer>();

        // Memorizza la scala originale
        if (anelloGrafico != null) scalaIniziale = anelloGrafico.transform.localScale;

        if (AccendiAllAvvio) Accendi();
        else Spegni();
    }

    void Update()
    {
        if (isAttivo && anelloGrafico != null)
        {
            // Effetto Pulsazione (Seno)
            float scala = Mathf.Lerp(dimensioneMin, dimensioneMax, (Mathf.Sin(Time.time * velocitaPulsazione) + 1f) / 2f);
            anelloGrafico.transform.localScale = scalaIniziale * scala;
            
            // Opzionale: Ruota l'anello lentamente
            anelloGrafico.transform.Rotate(Vector3.forward * 10 * Time.deltaTime);
        }
    }

    public void Accendi()
    {
        if (anelloGrafico != null)
        {
            anelloGrafico.enabled = true;
            isAttivo = true;
        }
    }

    public void Spegni()
    {
        if (anelloGrafico != null)
        {
            anelloGrafico.enabled = false;
            isAttivo = false;
        }
    }
}