using UnityEngine;
using System.Collections;

public class LuceTremolante : MonoBehaviour
{
    [Header("Componenti")]
    [Tooltip("Trascina qui il componente Light del faretto")]
    public Light luceFaretto;
    [Tooltip("Trascina qui il modello 3D che ha il materiale emissivo (opzionale)")]
    public Renderer meshLampadina;

    [Header("Impostazioni Intensità")]
    public float intensitaNormale = 10000f; 
    public float intensitaMinima = 0f;      

    [Header("Ritmo del Tremolio")]
    public float durataScattoMin = 0.05f;
    public float durataScattoMax = 0.15f;
    public float pausaTraScattiMin = 0.5f;
    public float pausaTraScattiMax = 3.0f;
    
    [Tooltip("Probabilità (da 0 a 1) che la luce si spenga del tutto")]
    [Range(0f, 1f)] public float probabilitaSpegnimento = 0.5f;

    [Header("Audio (Sfarfallio Elettrico)")]
    public AudioSource audioSource;
    public AudioClip suonoScintilla;
    [Range(0f, 1f)] public float volumeScintilla = 0.6f;

    private Material matLampadina;
    private Color coloreEmissioneOriginale;
    private string nomeProprietaEmissione = "_EmissiveColor";

    void Start()
    {
        if (luceFaretto == null) luceFaretto = GetComponent<Light>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (meshLampadina != null)
        {
            matLampadina = meshLampadina.material;
            if (matLampadina.HasProperty(nomeProprietaEmissione))
            {
                coloreEmissioneOriginale = matLampadina.GetColor(nomeProprietaEmissione);
            }
            else if (matLampadina.HasProperty("_EmissionColor")) 
            {
                nomeProprietaEmissione = "_EmissionColor";
                coloreEmissioneOriginale = matLampadina.GetColor(nomeProprietaEmissione);
            }
        }

        StartCoroutine(EffettoTremolio());
    }

    IEnumerator EffettoTremolio()
    {
        RiaccendiLuce();

        while (true)
        {
            yield return new WaitForSeconds(Random.Range(pausaTraScattiMin, pausaTraScattiMax));

            int numeroScatti = Random.Range(1, 4);

            for (int i = 0; i < numeroScatti; i++)
            {
                float nuovaIntensita = (Random.value < probabilitaSpegnimento) ? intensitaMinima : Random.Range(intensitaMinima, intensitaNormale / 2f);
                ApplicaIntensita(nuovaIntensita);

                if (audioSource != null && suonoScintilla != null)
                {
                    audioSource.pitch = Random.Range(0.85f, 1.15f);
                    audioSource.PlayOneShot(suonoScintilla, volumeScintilla);
                }

                yield return new WaitForSeconds(Random.Range(durataScattoMin, durataScattoMax));

                RiaccendiLuce();

                yield return new WaitForSeconds(Random.Range(0.02f, 0.08f));
            }
        }
    }

    void ApplicaIntensita(float intensita)
    {
        if (luceFaretto != null) luceFaretto.intensity = intensita;

        if (matLampadina != null && coloreEmissioneOriginale != default)
        {
            float moltiplicatore = intensita / intensitaNormale;
            matLampadina.SetColor(nomeProprietaEmissione, coloreEmissioneOriginale * moltiplicatore);
        }
    }

    void RiaccendiLuce()
    {
        if (luceFaretto != null) luceFaretto.intensity = intensitaNormale;
        if (matLampadina != null && coloreEmissioneOriginale != default)
        {
            matLampadina.SetColor(nomeProprietaEmissione, coloreEmissioneOriginale);
        }
    }
}