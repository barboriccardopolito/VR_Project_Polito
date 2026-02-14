using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SelettoreOggetti : MonoBehaviour
{
    [Header("Setup Visuale")]
    public Camera cameraDallAlto;
    public Camera cameraGiocatore;
    public Transform puntoIspezione;

    [Header("Riferimenti Player")]
    public GameObject giocatore;
    public string[] nomiScriptDaDisabilitare;
    private InterazioneGiocatore scriptInterazione;
    private CharacterController controllerGiocatore;

    [Header("Impostazioni Task")]
    public GameManager.Reparto taskRichiesta;

    [Header("Oggetti Selezionabili (I Pivot)")]
    public OggettoRaccolta[] oggetti;

    [Header("Impostazioni Ispezione 3D")]
    public float distanzaSfondo = 3.0f;
    public float sensibilitaMouse = 8f;
    public float velocitaAnimazione = 12f;
    [Range(0f, 1f)] public float opacitaSfondo = 0.85f;

    [Header("UI Scheda Tecnica")]
    public GameObject pannelloSchedaUI;
    public TextMeshProUGUI testoTitolo;
    public TextMeshProUGUI testoDescrizione;

    // --- NUOVI RIFERIMENTI PER LE BARRE RGB ---
    [Header("UI - Animazione Barre RGB")]
    public Image barraRossa;
    public Image barraVerde;
    public Image barraBlu;
    [Tooltip("Velocità dell'oscillazione su e giù")]
    public float velocitaAnimazioneRGB = 3f;
    [Range(0f, 1f)] public float altezzaMinimaRGB = 0.1f;
    [Range(0f, 1f)] public float altezzaMassimaRGB = 0.9f;
    private Coroutine coroutineAnimazioneRGB;
    // ------------------------------------------

    [Header("Effetto Macchina Da Scrivere")]
    public float velocitaScrittura = 0.03f;
    public AudioClip suonoBattitura;
    private AudioSource audioScrittura;
    private Coroutine coroutineScrittura;

    private bool inSelezione = false;
    private int indiceAttuale = 0;
    private bool possoUscire = false;

    private Vector3[] posOriginali;
    private Quaternion[] rotOriginali;
    private Vector2 rotazioneOggettoCorrente;

    private Canvas sfondoCanvas;
    private Image immagineSfondo;
    private float targetAlphaSfondo = 0f;

    void Start()
    {
        if (cameraDallAlto != null)
        {
            cameraDallAlto.gameObject.SetActive(false);
            AudioListener al = cameraDallAlto.GetComponent<AudioListener>();
            if (al != null) al.enabled = false;
        }
        if (cameraGiocatore == null) cameraGiocatore = Camera.main;

        if (giocatore != null)
        {
            scriptInterazione = giocatore.GetComponent<InterazioneGiocatore>();
            controllerGiocatore = giocatore.GetComponent<CharacterController>();
        }

        if (pannelloSchedaUI != null) pannelloSchedaUI.SetActive(false);

        audioScrittura = gameObject.AddComponent<AudioSource>();
        audioScrittura.playOnAwake = false;
        audioScrittura.spatialBlend = 0f;

        posOriginali = new Vector3[oggetti.Length];
        rotOriginali = new Quaternion[oggetti.Length];

        for (int i = 0; i < oggetti.Length; i++)
        {
            if (oggetti[i] != null)
            {
                posOriginali[i] = oggetti[i].transform.localPosition;
                rotOriginali[i] = oggetti[i].transform.localRotation;
            }
        }
        CreaSfondoScuro();

        // Assicuro che le barre siano settate correttamente per l'animazione 'Filled'
        if (barraRossa) { barraRossa.type = Image.Type.Filled; barraRossa.fillMethod = Image.FillMethod.Vertical; }
        if (barraVerde) { barraVerde.type = Image.Type.Filled; barraVerde.fillMethod = Image.FillMethod.Vertical; }
        if (barraBlu) { barraBlu.type = Image.Type.Filled; barraBlu.fillMethod = Image.FillMethod.Vertical; }
    }

    void CreaSfondoScuro()
    {
        GameObject canvasObj = new GameObject("SfondoIspezione");
        canvasObj.transform.SetParent(cameraDallAlto.transform);
        sfondoCanvas = canvasObj.AddComponent<Canvas>();
        sfondoCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        sfondoCanvas.worldCamera = cameraDallAlto;
        sfondoCanvas.planeDistance = distanzaSfondo;

        GameObject panelObj = new GameObject("PannelloNero");
        panelObj.transform.SetParent(canvasObj.transform, false);
        immagineSfondo = panelObj.AddComponent<Image>();
        immagineSfondo.color = new Color(0.1f, 0.1f, 0.1f, 0f);

        RectTransform rt = immagineSfondo.rectTransform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;
        canvasObj.SetActive(false);
    }

    public bool PuoiInteragire() { return GameManager.instance != null && GameManager.instance.taskAttuale == taskRichiesta; }

    public void EntraInSelezione()
    {
        if (inSelezione) return;
        inSelezione = true;
        possoUscire = false;

        indiceAttuale = 0;
        for (int i = 0; i < oggetti.Length; i++) { if (oggetti[i].gameObject.activeInHierarchy) { indiceAttuale = i; break; } }

        BloccaGiocatore(true);
        if (cameraGiocatore != null) cameraGiocatore.enabled = false;
        if (cameraDallAlto != null) cameraDallAlto.gameObject.SetActive(true);
        if (scriptInterazione != null) scriptInterazione.enabled = false;

        rotazioneOggettoCorrente = Vector2.zero;
        sfondoCanvas.gameObject.SetActive(true);
        targetAlphaSfondo = opacitaSfondo;

        if (pannelloSchedaUI != null) pannelloSchedaUI.SetActive(true);
        AggiornaSchedaTecnica();

        // --- AVVIA L'ANIMAZIONE RGB ---
        if (coroutineAnimazioneRGB != null) StopCoroutine(coroutineAnimazioneRGB);
        coroutineAnimazioneRGB = StartCoroutine(AnimaBarreRGB());
        // ------------------------------

        StartCoroutine(TimerSblocco());
    }

    IEnumerator TimerSblocco() { yield return new WaitForSeconds(0.5f); possoUscire = true; }

    void Update()
    {
        GestisciAnimazione();

        if (!inSelezione) return;

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) CambiaSelezione(1);
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) CambiaSelezione(-1);

        float mouseX = Input.GetAxis("Mouse X") * sensibilitaMouse;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilitaMouse;
        rotazioneOggettoCorrente.x += mouseY;
        rotazioneOggettoCorrente.y -= mouseX;

        if (Input.GetKeyDown(KeyCode.E) && possoUscire)
        {
            ScegliOggetto();
        }
    }

    void GestisciAnimazione()
    {
        if (immagineSfondo != null)
        {
            Color c = immagineSfondo.color;
            c.a = Mathf.Lerp(c.a, targetAlphaSfondo, Time.deltaTime * velocitaAnimazione);
            immagineSfondo.color = c;
            if (!inSelezione && c.a < 0.01f) sfondoCanvas.gameObject.SetActive(false);
        }

        for (int i = 0; i < oggetti.Length; i++)
        {
            if (oggetti[i] == null || !oggetti[i].gameObject.activeInHierarchy) continue;

            if (inSelezione && i == indiceAttuale)
            {
                Vector3 targetPos = puntoIspezione != null ? puntoIspezione.position : cameraDallAlto.transform.position + (cameraDallAlto.transform.forward * 0.5f);
                oggetti[i].transform.position = Vector3.Lerp(oggetti[i].transform.position, targetPos, Time.deltaTime * velocitaAnimazione);

                Quaternion rotazioneSchermo = cameraDallAlto.transform.rotation;
                Quaternion offsetMouse = Quaternion.Euler(rotazioneOggettoCorrente.x, rotazioneOggettoCorrente.y, 0);
                oggetti[i].transform.rotation = Quaternion.Lerp(oggetti[i].transform.rotation, rotazioneSchermo * offsetMouse, Time.deltaTime * velocitaAnimazione);
            }
            else
            {
                oggetti[i].transform.localPosition = Vector3.Lerp(oggetti[i].transform.localPosition, posOriginali[i], Time.deltaTime * velocitaAnimazione);
                oggetti[i].transform.localRotation = Quaternion.Lerp(oggetti[i].transform.localRotation, rotOriginali[i], Time.deltaTime * velocitaAnimazione);
            }
        }
    }

    void CambiaSelezione(int dir)
    {
        int tentativi = 0;
        do
        {
            indiceAttuale += dir;
            if (indiceAttuale >= oggetti.Length) indiceAttuale = 0;
            else if (indiceAttuale < 0) indiceAttuale = oggetti.Length - 1;
            tentativi++;
        } while (!oggetti[indiceAttuale].gameObject.activeInHierarchy && tentativi < oggetti.Length);

        rotazioneOggettoCorrente = Vector2.zero;

        AggiornaSchedaTecnica();
    }

    void ScegliOggetto()
    {
        inSelezione = false;
        possoUscire = false;
        targetAlphaSfondo = 0f;

        if (pannelloSchedaUI != null) pannelloSchedaUI.SetActive(false);

        if (coroutineScrittura != null) StopCoroutine(coroutineScrittura);
        // --- FERMA L'ANIMAZIONE RGB ---
        if (coroutineAnimazioneRGB != null) StopCoroutine(coroutineAnimazioneRGB);
        // ------------------------------

        if (cameraDallAlto != null) cameraDallAlto.gameObject.SetActive(false);
        if (cameraGiocatore != null) cameraGiocatore.enabled = true;
        if (scriptInterazione != null) scriptInterazione.enabled = true;

        BloccaGiocatore(false);

        if (oggetti[indiceAttuale].gameObject.activeInHierarchy)
        {
            oggetti[indiceAttuale].EseguiRaccolta();
            oggetti[indiceAttuale].transform.localPosition = posOriginali[indiceAttuale];
            oggetti[indiceAttuale].transform.localRotation = rotOriginali[indiceAttuale];
        }
    }

    void AggiornaSchedaTecnica()
    {
        if (pannelloSchedaUI == null) return;

        if (oggetti.Length > 0 && oggetti[indiceAttuale] != null)
        {
            if (coroutineScrittura != null) StopCoroutine(coroutineScrittura);

            string titolo = oggetti[indiceAttuale].nomeTecnico;
            string descrizione = oggetti[indiceAttuale].descrizioneTecnica;
            coroutineScrittura = StartCoroutine(EffettoTestoAnimato(titolo, descrizione));
        }
    }

    // --- NUOVA COROUTINE PER ANIMARE LE BARRE RGB ---
    IEnumerator AnimaBarreRGB()
    {
        float timer = 0f;
        // Continua ad animare finché siamo nella schermata di selezione
        while (inSelezione)
        {
            timer += Time.deltaTime * velocitaAnimazioneRGB;

            // Usiamo la funzione Seno (Mathf.Sin) per creare un movimento oscillatorio fluido.
            // Aggiungiamo un offset diverso (0, 1.5f, 3f) a ciascuna barra per farle muovere in modo indipendente e non sincronizzato.
            // Mathf.Lerp interpola tra l'altezza minima e massima basandosi sul valore dell'onda.

            if (barraRossa)
                barraRossa.fillAmount = Mathf.Lerp(altezzaMinimaRGB, altezzaMassimaRGB, (Mathf.Sin(timer) + 1f) / 2f);

            if (barraVerde)
                barraVerde.fillAmount = Mathf.Lerp(altezzaMinimaRGB, altezzaMassimaRGB, (Mathf.Sin(timer + 1.5f) + 1f) / 2f);

            if (barraBlu)
                barraBlu.fillAmount = Mathf.Lerp(altezzaMinimaRGB, altezzaMassimaRGB, (Mathf.Sin(timer + 3f) + 1f) / 2f);

            yield return null; // Aspetta il prossimo frame
        }
    }
    // ------------------------------------------------

    IEnumerator EffettoTestoAnimato(string titoloCompleto, string descCompleta)
    {
        if (testoTitolo != null)
        {
            testoTitolo.text = titoloCompleto;
            testoTitolo.maxVisibleCharacters = 0;
        }

        if (testoDescrizione != null)
        {
            testoDescrizione.text = descCompleta;
            testoDescrizione.maxVisibleCharacters = 0;
        }

        if (testoTitolo != null)
        {
            for (int i = 0; i < titoloCompleto.Length; i++)
            {
                testoTitolo.maxVisibleCharacters = i + 1;
                char lettera = titoloCompleto[i];
                SuonaTasto(lettera);
                yield return new WaitForSeconds(velocitaScrittura);
            }
        }

        yield return new WaitForSeconds(0.15f);

        if (testoDescrizione != null)
        {
            for (int i = 0; i < descCompleta.Length; i++)
            {
                testoDescrizione.maxVisibleCharacters = i + 1;
                char lettera = descCompleta[i];
                SuonaTasto(lettera);

                if (lettera == '.' || lettera == ':' || lettera == '\n')
                {
                    yield return new WaitForSeconds(velocitaScrittura * 6f);
                }
                else if (lettera == ',' || lettera == ';')
                {
                    yield return new WaitForSeconds(velocitaScrittura * 3f);
                }
                else
                {
                    yield return new WaitForSeconds(velocitaScrittura * 0.5f);
                }
            }
        }
    }

    void SuonaTasto(char lettera)
    {
        if (suonoBattitura != null && audioScrittura != null)
        {
            if (lettera != ' ' && lettera != '.' && lettera != ',')
            {
                audioScrittura.pitch = Random.Range(0.95f, 1.05f);
                audioScrittura.PlayOneShot(suonoBattitura, 0.2f);
            }
        }
    }

    void BloccaGiocatore(bool blocca)
    {
        if (giocatore == null) return;

        if (nomiScriptDaDisabilitare != null)
        {
            foreach (string nomeScript in nomiScriptDaDisabilitare)
            {
                MonoBehaviour s = giocatore.GetComponent(nomeScript) as MonoBehaviour;
                if (s != null) s.enabled = !blocca;
            }
        }

        if (controllerGiocatore != null) controllerGiocatore.enabled = !blocca;

        if (blocca) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
        else { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
    }
}