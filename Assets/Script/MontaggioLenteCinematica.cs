using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MontaggioLenteCinematica : MonoBehaviour
{
    [Header("Dati Macchina da Presa (UI)")]
    public string nomeCamera = "CineCamera 8K HD";
    [TextArea(3, 5)]
    public string descrizioneCamera = "Corpo macchina digitale con sensore Super 35mm. Cattura immagini ad altissima gamma dinamica. Lo standard per le produzioni cinematografiche moderne.";

    [Header("Setup Visuale & Animazione")]
    public Camera cameraIspezione; 
    public Transform puntoMontaggioLente; 
    public float distanzaPartenza = 0.6f; 
    public float gradiRotazione = 720f; 
    public float durataAnimazione = 2.5f;

    [Header("Riferimenti Player & UI")]
    public GameObject giocatore;
    public GameObject hudGiocatore;
    public string[] nomiScriptDaDisabilitare;
    
    [Header("UI Olografica (Usa il tuo Prefab)")]
    [Tooltip("Se lo sfondo non si scurisce, abbassa questo valore (es. 0.5) per evitare che finisca dentro il muro!")]
    public float distanzaSfondo = 0.8f; // Abbassato di default!
    [Range(0f, 1f)] public float opacitaSfondo = 0.85f;
    public float velocitaFadeSfondo = 5f;

    public GameObject pannelloSchedaUI;
    public TextMeshProUGUI testoTitolo;
    public TextMeshProUGUI testoDescrizione;
    public float velocitaScrittura = 0.03f;
    public AudioClip suonoBattitura;
    private AudioSource audioSource;

    [Header("Barre RGB Fotografia")]
    public Image barra1; public Image barra2; public Image barra3;

    private Canvas sfondoCanvas;
    private Image immagineSfondo;
    private float targetAlphaSfondo = 0f;

    private bool cinematicaInCorso = false;
    private bool puoUscire = false;
    private Coroutine coroutineScrittura;

    void Start()
    {
        if (cameraIspezione != null) cameraIspezione.gameObject.SetActive(false);
        if (pannelloSchedaUI != null) pannelloSchedaUI.SetActive(false);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        CreaSfondoScuro();
    }

    void CreaSfondoScuro()
    {
        GameObject canvasObj = new GameObject("SfondoCinematica");
        canvasObj.transform.SetParent(cameraIspezione.transform);
        sfondoCanvas = canvasObj.AddComponent<Canvas>();
        sfondoCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        sfondoCanvas.worldCamera = cameraIspezione;
        sfondoCanvas.planeDistance = distanzaSfondo;

        GameObject panelObj = new GameObject("PannelloNero");
        panelObj.transform.SetParent(canvasObj.transform, false);
        immagineSfondo = panelObj.AddComponent<Image>();
        // Parte totalmente invisibile (Alpha a 0)
        immagineSfondo.color = new Color(0.05f, 0.05f, 0.05f, 0f);

        RectTransform rt = immagineSfondo.rectTransform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;
        canvasObj.SetActive(false);
    }

    public void AvviaCinematicaMontaggio(GameObject lenteFisica)
    {
        if (cinematicaInCorso) return;
        cinematicaInCorso = true;
        puoUscire = false;

        BloccaGiocatore(true);
        if (hudGiocatore != null) hudGiocatore.SetActive(false);
        if (cameraIspezione != null) cameraIspezione.gameObject.SetActive(true);

        sfondoCanvas.gameObject.SetActive(true);
        targetAlphaSfondo = opacitaSfondo; // Inizia a scurire dolcemente

        if (pannelloSchedaUI != null) pannelloSchedaUI.SetActive(true);

        AggiornaSchedaTecnica();
        StartCoroutine(AnimaBarreRGB());
        StartCoroutine(EseguiAvvitamento(lenteFisica.transform));
    }

    void Update()
    {
        // Gestisce la sfumatura morbida dello sfondo nero
        if (immagineSfondo != null)
        {
            Color c = immagineSfondo.color;
            c.a = Mathf.Lerp(c.a, targetAlphaSfondo, Time.deltaTime * velocitaFadeSfondo);
            immagineSfondo.color = c;

            if (!cinematicaInCorso && c.a < 0.01f)
            {
                sfondoCanvas.gameObject.SetActive(false);
            }
        }

        if (cinematicaInCorso && puoUscire && Input.GetKeyDown(KeyCode.E))
        {
            ConcludiCinematica();
        }
    }

    IEnumerator EseguiAvvitamento(Transform lente)
    {
        Vector3 posFinale = lente.position;
        Quaternion rotFinale = lente.rotation;

        Vector3 offsetAlto = (puntoMontaggioLente.up * distanzaPartenza) + (puntoMontaggioLente.forward * 0.1f);
        Vector3 posIniziale = posFinale + offsetAlto;

        lente.Rotate(0, 0, gradiRotazione, Space.Self);
        Quaternion rotIniziale = lente.rotation;

        float timer = 0f;
        while (timer < durataAnimazione)
        {
            float t = timer / durataAnimazione;
            float tSmooth = Mathf.SmoothStep(0, 1, t);

            lente.position = Vector3.Lerp(posIniziale, posFinale, tSmooth);
            lente.rotation = Quaternion.Slerp(rotIniziale, rotFinale, tSmooth);

            timer += Time.deltaTime;
            yield return null;
        }

        lente.position = posFinale;
        lente.rotation = rotFinale;
        puoUscire = true; 
    }

    void ConcludiCinematica()
    {
        cinematicaInCorso = false;
        targetAlphaSfondo = 0f; // Fa dissolvere il nero
        
        if (pannelloSchedaUI != null) pannelloSchedaUI.SetActive(false);
        if (cameraIspezione != null) cameraIspezione.gameObject.SetActive(false);
        
        if (hudGiocatore != null) hudGiocatore.SetActive(true);
        BloccaGiocatore(false);
        
        StartCoroutine(RiattivaInterazioneRitardata());
    }

    void AggiornaSchedaTecnica()
    {
        if (coroutineScrittura != null) StopCoroutine(coroutineScrittura);
        coroutineScrittura = StartCoroutine(EffettoTestoAnimato(nomeCamera, descrizioneCamera));
    }

    IEnumerator EffettoTestoAnimato(string titolo, string desc)
    {
        if (testoTitolo) { testoTitolo.text = titolo; testoTitolo.maxVisibleCharacters = 0; }
        if (testoDescrizione) { testoDescrizione.text = desc; testoDescrizione.maxVisibleCharacters = 0; }

        if (testoTitolo)
        {
            for (int i = 0; i < titolo.Length; i++)
            {
                testoTitolo.maxVisibleCharacters = i + 1;
                SuonaTasto(titolo[i]);
                yield return new WaitForSeconds(velocitaScrittura);
            }
        }
        yield return new WaitForSeconds(0.15f);
        if (testoDescrizione)
        {
            for (int i = 0; i < desc.Length; i++)
            {
                testoDescrizione.maxVisibleCharacters = i + 1;
                SuonaTasto(desc[i]);
                char c = desc[i];
                if (c == '.' || c == ':' || c == '\n') yield return new WaitForSeconds(velocitaScrittura * 6f);
                else if (c == ',' || c == ';') yield return new WaitForSeconds(velocitaScrittura * 3f);
                else yield return new WaitForSeconds(velocitaScrittura * 0.5f);
            }
        }
    }

    void SuonaTasto(char lettera)
    {
        if (audioSource && suonoBattitura && lettera != ' ' && lettera != '.' && lettera != ',')
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(suonoBattitura, 0.2f);
        }
    }

    IEnumerator AnimaBarreRGB()
    {
        float t = 0;
        Color cR = new Color(1f, 0.2f, 0.2f, 0.8f); Color cG = new Color(0.2f, 1f, 0.2f, 0.8f); Color cB = new Color(0.2f, 0.5f, 1f, 0.8f);
        if (barra1) barra1.color = cR; if (barra2) barra2.color = cG; if (barra3) barra3.color = cB;

        while (cinematicaInCorso)
        {
            t += Time.deltaTime * 3f;
            if (barra1) barra1.fillAmount = Mathf.Lerp(0.1f, 0.9f, (Mathf.Sin(t) + 1f) / 2f);
            if (barra2) barra2.fillAmount = Mathf.Lerp(0.1f, 0.9f, (Mathf.Sin(t + 1.5f) + 1f) / 2f);
            if (barra3) barra3.fillAmount = Mathf.Lerp(0.1f, 0.9f, (Mathf.Sin(t + 3f) + 1f) / 2f);
            yield return null;
        }
    }

    void BloccaGiocatore(bool blocca)
    {
        if (!giocatore) return;
        if (nomiScriptDaDisabilitare != null)
        {
            foreach (string n in nomiScriptDaDisabilitare)
            {
                MonoBehaviour s = giocatore.GetComponent(n) as MonoBehaviour;
                if (s) s.enabled = !blocca;
                if (Camera.main)
                {
                    MonoBehaviour sCam = Camera.main.GetComponent(n) as MonoBehaviour;
                    if (sCam) sCam.enabled = !blocca;
                }
            }
        }
        var cc = giocatore.GetComponent<CharacterController>();
        if (cc) cc.enabled = !blocca;
        Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;
    }

    IEnumerator RiattivaInterazioneRitardata()
    {
        yield return new WaitForSeconds(0.2f);
        var interazione = giocatore.GetComponent("InterazioneGiocatore") as MonoBehaviour;
        if (interazione) interazione.enabled = true;
    }
}