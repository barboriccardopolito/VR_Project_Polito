using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MontaggioMicrofonoCinematica : MonoBehaviour
{
    [Header("Setup Visuale & Animazione")]
    public Camera cameraIspezione; 
    public Transform puntoMontaggioMic; 
    public float distanzaPartenza = 1.0f; // Scende dall'alto
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

    [Header("Barre VU Meter (Audio)")]
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
        GameObject canvasObj = new GameObject("SfondoCinematicaAudio");
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

    public void AvviaCinematicaMontaggio(GameObject micFisico, string nomeOlogramma, string descOlogramma)
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
        StartCoroutine(AnimaVUMeter());
        StartCoroutine(EseguiIncastro(micFisico.transform));
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

    IEnumerator EseguiIncastro(Transform mic)
    {
        Vector3 posFinale = mic.position;
        Quaternion rotFinale = mic.rotation;

        // Scende dall'alto (Asse Y)
        Vector3 posIniziale = posFinale + (Vector3.up * distanzaPartenza);
        
        // Un po' di rotazione iniziale per dare l'idea dell'incastro
        mic.Rotate(0, 90f, 0, Space.Self);
        Quaternion rotIniziale = mic.rotation;

        float timer = 0f;
        while (timer < durataAnimazione)
        {
            float t = timer / durataAnimazione;
            float tSmooth = Mathf.SmoothStep(0, 1, t);

            mic.position = Vector3.Lerp(posIniziale, posFinale, tSmooth);
            mic.rotation = Quaternion.Slerp(rotIniziale, rotFinale, tSmooth);

            timer += Time.deltaTime;
            yield return null;
        }

        mic.position = posFinale;
        mic.rotation = rotFinale;
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

    IEnumerator AnimaVUMeter()
    {
        float timer = 0f;
        float nextVUStep = 0f;
        float targetVU1 = 0f, targetVU2 = 0f, targetVU3 = 0f;
        
        // Colori classici da Mixer Audio: Verde, Giallo, Rosso
        Color cVerde = new Color(0.1f, 0.9f, 0.1f, 0.8f); 
        Color cGiallo = new Color(0.9f, 0.9f, 0.1f, 0.8f); 
        Color cRosso = new Color(0.9f, 0.1f, 0.1f, 0.8f);
        
        if (barra1) barra1.color = cVerde; 
        if (barra2) barra2.color = cGiallo; 
        if (barra3) barra3.color = cRosso;

        while (cinematicaInCorso)
        {
            timer += Time.deltaTime;
            // Aggiorna i valori a scatti, proprio come un vero livello audio
            if (timer > nextVUStep)
            {
                targetVU1 = Random.Range(0.4f, 1f);   
                targetVU2 = Random.Range(0.1f, 0.7f); 
                targetVU3 = Random.Range(0.0f, 0.3f); 
                nextVUStep = timer + 0.1f; 
            }
            
            if (barra1) barra1.fillAmount = Mathf.Lerp(barra1.fillAmount, targetVU1, Time.deltaTime * 20f);
            if (barra2) barra2.fillAmount = Mathf.Lerp(barra2.fillAmount, targetVU2, Time.deltaTime * 20f);
            if (barra3) barra3.fillAmount = Mathf.Lerp(barra3.fillAmount, targetVU3, Time.deltaTime * 20f);
            
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