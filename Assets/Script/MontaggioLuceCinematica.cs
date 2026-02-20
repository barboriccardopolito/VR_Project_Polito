using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MontaggioLuceCinematica : MonoBehaviour
{
    [Header("Setup Visuale & Animazione")]
    public Camera cameraIspezione; 
    public Transform puntoMontaggioLuce; 
    [Tooltip("Quanto lontano a destra parte l'oggetto")]
    public float distanzaPartenza = 0.8f; 
    [Tooltip("Gradi totali di rotazione durante l'avvicinamento (es. 360 per un giro completo)")]
    public float gradiRotazioneTotale = 360f; 
    public float durataAnimazione = 2.0f;

    [Header("Riferimenti Player & UI")]
    public GameObject giocatore;
    public GameObject hudGiocatore;
    public string[] nomiScriptDaDisabilitare;
    
    [Header("UI Olografica (Usa il Prefab)")]
    public float distanzaSfondo = 0.4f; 
    [Range(0f, 1f)] public float opacitaSfondo = 0.85f;
    public float velocitaFadeSfondo = 5f;

    public GameObject pannelloSchedaUI;
    public TextMeshProUGUI testoTitolo;
    public TextMeshProUGUI testoDescrizione;
    public float velocitaScrittura = 0.03f;
    public AudioClip suonoBattitura;
    private AudioSource audioSource;

    [Header("Barre Termometro Kelvin (Luci)")]
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
        GameObject canvasObj = new GameObject("SfondoCinematicaLuci");
        canvasObj.transform.SetParent(cameraIspezione.transform);
        sfondoCanvas = canvasObj.AddComponent<Canvas>();
        sfondoCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        sfondoCanvas.worldCamera = cameraIspezione;
        sfondoCanvas.planeDistance = distanzaSfondo;

        GameObject panelObj = new GameObject("PannelloNero");
        panelObj.transform.SetParent(canvasObj.transform, false);
        immagineSfondo = panelObj.AddComponent<Image>();
        immagineSfondo.color = new Color(0.05f, 0.05f, 0.05f, 0f);

        RectTransform rt = immagineSfondo.rectTransform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;
        canvasObj.SetActive(false);
    }

    public void AvviaCinematicaMontaggio(GameObject luceFisica, string nomeOlogramma, string descOlogramma)
    {
        if (cinematicaInCorso) return;
        cinematicaInCorso = true;
        puoUscire = false;

        BloccaGiocatore(true);
        if (hudGiocatore != null) hudGiocatore.SetActive(false);
        if (cameraIspezione != null) cameraIspezione.gameObject.SetActive(true);

        sfondoCanvas.gameObject.SetActive(true);
        targetAlphaSfondo = opacitaSfondo; 

        if (pannelloSchedaUI != null) pannelloSchedaUI.SetActive(true);

        AggiornaSchedaTecnica(nomeOlogramma, descOlogramma);
        StartCoroutine(AnimaBarreLuci());
        
        StartCoroutine(EseguiAvvitamentoLaterale(luceFisica.transform));
    }

    void Update()
    {
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

    IEnumerator EseguiAvvitamentoLaterale(Transform luce)
    {
        Vector3 posFinale = luce.position;
        Quaternion rotFinale = luce.rotation;

        Vector3 asseAvvitamento = puntoMontaggioLuce.right; 
        
        Vector3 posIniziale = posFinale + (asseAvvitamento * distanzaPartenza);

        float timer = 0f;
        while (timer < durataAnimazione)
        {
            float t = timer / durataAnimazione;
            float tSmooth = Mathf.SmoothStep(0, 1, t);

            luce.position = Vector3.Lerp(posIniziale, posFinale, tSmooth);
            
            float gradiCorrenti = Mathf.Lerp(gradiRotazioneTotale, 0, tSmooth);

            luce.rotation = rotFinale;
            luce.RotateAround(luce.position, asseAvvitamento, gradiCorrenti);

            timer += Time.deltaTime;
            yield return null;
        }

        luce.position = posFinale;
        luce.rotation = rotFinale;
        puoUscire = true; 
    }

    void ConcludiCinematica()
    {
        cinematicaInCorso = false;
        targetAlphaSfondo = 0f; 
        
        if (pannelloSchedaUI != null) pannelloSchedaUI.SetActive(false);
        if (cameraIspezione != null) cameraIspezione.gameObject.SetActive(false);
        
        if (hudGiocatore != null) hudGiocatore.SetActive(true);
        BloccaGiocatore(false);
        
        StartCoroutine(RiattivaInterazioneRitardata());
    }

    void AggiornaSchedaTecnica(string titolo, string desc)
    {
        if (coroutineScrittura != null) StopCoroutine(coroutineScrittura);
        coroutineScrittura = StartCoroutine(EffettoTestoAnimato(titolo, desc));
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

    IEnumerator AnimaBarreLuci()
    {
        float t = 0;
        Color cArancio = new Color(1f, 0.4f, 0f, 0.8f); 
        Color cBianco = new Color(1f, 1f, 1f, 0.8f); 
        Color cAzzurro = new Color(0.3f, 0.7f, 1f, 0.8f);
        
        if (barra1) barra1.color = cArancio; 
        if (barra2) barra2.color = cBianco; 
        if (barra3) barra3.color = cAzzurro;

        while (cinematicaInCorso)
        {
            t += Time.deltaTime * 0.6f; 
            if (barra1) barra1.fillAmount = Mathf.Lerp(0.6f, 0.9f, (Mathf.Sin(t) + 1f) / 2f);
            if (barra2) barra2.fillAmount = Mathf.Lerp(0.8f, 1f, (Mathf.Sin(t + 2f) + 1f) / 2f);
            if (barra3) barra3.fillAmount = Mathf.Lerp(0.3f, 0.5f, (Mathf.Sin(t + 4f) + 1f) / 2f);
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