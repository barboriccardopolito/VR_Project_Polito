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
    public GameObject hudGiocatore;
    public string[] nomiScriptDaDisabilitare;
    private InterazioneGiocatore scriptInterazione;
    private CharacterController controllerGiocatore;

    [Header("Impostazioni Task")]
    public GameManager.Reparto taskRichiesta;
    
    [Tooltip("Trascina qui l'NPC che gestisce questo reparto per aspettare che finisca di parlare")]
    public NPC_Staff npcDiRiferimento;

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

    [Header("UI - Animazione Barre Contestuali")]
    public Image barra1; 
    public Image barra2; 
    public Image barra3; 
    public float velocitaAnimazioneBarre = 3f;
    [Range(0f, 1f)] public float altezzaMinimaBarre = 0.1f;
    [Range(0f, 1f)] public float altezzaMassimaBarre = 0.9f;
    private Coroutine coroutineAnimazioneBarre;

    [Header("UI - Indicatori Carosello")]
    public Image[] palliniIndicatori;
    public Color colorePallinoAttivo = Color.white;
    public Color colorePallinoInattivo = new Color(1f, 1f, 1f, 0.3f); 

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

    private Evidenziatore evidenziatore;

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

        if (barra1) { barra1.type = Image.Type.Filled; barra1.fillMethod = Image.FillMethod.Vertical; }
        if (barra2) { barra2.type = Image.Type.Filled; barra2.fillMethod = Image.FillMethod.Vertical; }
        if (barra3) { barra3.type = Image.Type.Filled; barra3.fillMethod = Image.FillMethod.Vertical; }

        ImpostaColoriBarre();

        evidenziatore = GetComponent<Evidenziatore>();
        if (evidenziatore == null) evidenziatore = GetComponentInChildren<Evidenziatore>();
    }

    void ImpostaColoriBarre()
    {
        float alpha = 0.8f; 
        if (taskRichiesta == GameManager.Reparto.Fotografia)
        {
            if (barra1) barra1.color = new Color(1f, 0.2f, 0.2f, alpha); 
            if (barra2) barra2.color = new Color(0.2f, 1f, 0.2f, alpha); 
            if (barra3) barra3.color = new Color(0.2f, 0.5f, 1f, alpha); 
        }
        else if (taskRichiesta == GameManager.Reparto.Fonico)
        {
            if (barra1) barra1.color = new Color(0.1f, 0.9f, 0.1f, alpha); 
            if (barra2) barra2.color = new Color(0.9f, 0.9f, 0.1f, alpha); 
            if (barra3) barra3.color = new Color(0.9f, 0.1f, 0.1f, alpha); 
        }
        else if (taskRichiesta == GameManager.Reparto.Luci)
        {
            if (barra1) barra1.color = new Color(1f, 0.4f, 0f, alpha);   
            if (barra2) barra2.color = new Color(1f, 1f, 1f, alpha);     
            if (barra3) barra3.color = new Color(0.3f, 0.7f, 1f, alpha); 
        }
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

    public bool PuoiInteragire() 
    { 
        return GameManager.instance != null && (GameManager.instance.taskAttuale == taskRichiesta || GameManager.instance.taskAttuale == GameManager.Reparto.Regia); 
    }

    void Update()
    {
        GestisciAnimazione();
        GestisciEvidenziatore(); 

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

    private bool ControllaIntroRegista()
    {
        InteragibileNPC[] tuttiNPC = Object.FindObjectsByType<InteragibileNPC>(FindObjectsSortMode.None);
        foreach (InteragibileNPC npc in tuttiNPC)
        {
            if (npc.tipoReparto == GameManager.Reparto.Regia)
            {
                NPC_Staff staff = npc.GetComponent<NPC_Staff>();
                if (staff != null) return staff.haGiaParlato;
            }
        }
        return false;
    }

    void GestisciEvidenziatore()
    {
        if (evidenziatore == null || GameManager.instance == null) return;

        if (inSelezione)
        {
            evidenziatore.Spegni();
            return;
        }

        bool taskAttiva = (GameManager.instance.taskAttuale == taskRichiesta);
        bool faseRevisione = (GameManager.instance.taskAttuale == GameManager.Reparto.Regia);

        InventarioGiocatore inv = Object.FindFirstObjectByType<InventarioGiocatore>();
        bool hoOggettoInMano = (inv != null && inv.haUnOggetto);
        
        bool npcHaParlato = (npcDiRiferimento == null || npcDiRiferimento.haGiaParlato);

        if (taskAttiva)
        {
            if (!hoOggettoInMano && npcHaParlato) 
                evidenziatore.Accendi();
            else 
                evidenziatore.Spegni(); 
        }
        else if (faseRevisione)
        {
            if (ControllaIntroRegista() && !hoOggettoInMano) evidenziatore.Accendi();
            else evidenziatore.Spegni();
        }
        else
        {
            evidenziatore.Spegni();
        }
    }

    public void EntraInSelezione()
    {
        if (inSelezione) return;

        inSelezione = true;
        possoUscire = false;
        
        // Cerca il primo oggetto attivo da mostrare
        indiceAttuale = 0;
        for (int i = 0; i < oggetti.Length; i++)
        {
            if (oggetti[i] != null && oggetti[i].gameObject.activeInHierarchy)
            {
                indiceAttuale = i;
                break;
            }
        }

        BloccaGiocatore(true);
        
        if (cameraGiocatore != null) 
        {
            cameraGiocatore.enabled = false; 
            cameraGiocatore.gameObject.SetActive(false); 
        }
        
        if (cameraDallAlto != null) 
        {
            cameraDallAlto.gameObject.SetActive(true);
            AudioListener al = cameraDallAlto.GetComponent<AudioListener>();
            if (al != null) al.enabled = true; 
        }

        if (hudGiocatore != null) hudGiocatore.SetActive(false);

        if (pannelloSchedaUI != null) pannelloSchedaUI.SetActive(true);
        AggiornaSchedaTecnica();
        AggiornaPallini(); 
        
        if (coroutineAnimazioneBarre != null) StopCoroutine(coroutineAnimazioneBarre);
        coroutineAnimazioneBarre = StartCoroutine(AnimaBarreContestuali());

        if (sfondoCanvas != null) sfondoCanvas.gameObject.SetActive(true);
        targetAlphaSfondo = opacitaSfondo;

        StartCoroutine(TimerSblocco());
    }

    IEnumerator TimerSblocco() 
    { 
        yield return new WaitForSeconds(0.5f); 
        possoUscire = true; 
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
        AggiornaPallini(); 
    }

    // --- LOGICA PALLINI POTENZIATA E RISOLTA ---
    void AggiornaPallini()
    {
        if (palliniIndicatori == null || palliniIndicatori.Length == 0) return;

        for (int i = 0; i < palliniIndicatori.Length; i++)
        {
            if (palliniIndicatori[i] != null)
            {
                // Un pallino si accende SOLO se c'è un oggetto in quello slot ED È ATTIVO SUL TAVOLO
                if (i < oggetti.Length && oggetti[i] != null && oggetti[i].gameObject.activeInHierarchy)
                {
                    palliniIndicatori[i].gameObject.SetActive(true);
                    
                    if (i == indiceAttuale)
                        palliniIndicatori[i].color = colorePallinoAttivo;
                    else
                        palliniIndicatori[i].color = colorePallinoInattivo;
                }
                else
                {
                    palliniIndicatori[i].gameObject.SetActive(false);
                }
            }
        }
    }

    void ScegliOggetto()
    {
        if (!inSelezione) return;

        inSelezione = false;
        possoUscire = false;
        targetAlphaSfondo = 0f;

        if (pannelloSchedaUI != null) pannelloSchedaUI.SetActive(false);

        if (coroutineScrittura != null) StopCoroutine(coroutineScrittura);
        if (coroutineAnimazioneBarre != null) StopCoroutine(coroutineAnimazioneBarre);

        if (cameraDallAlto != null) 
        {
            cameraDallAlto.gameObject.SetActive(false);
            AudioListener al = cameraDallAlto.GetComponent<AudioListener>();
            if (al != null) al.enabled = false;
        }
        
        if (cameraGiocatore != null) 
        {
            cameraGiocatore.gameObject.SetActive(true); 
            cameraGiocatore.enabled = true; 
        }
        
        if (hudGiocatore != null) hudGiocatore.SetActive(true);

        BloccaGiocatore(false);

        StartCoroutine(EseguiScambioERiattiva());
    }

    // --- FIX SUI MODELLI 3D AL TAVOLO ---
    void SvuotaSupportiInScenaERiattivaTavolo()
    {
        // Variabile per tenere traccia del nome dell'oggetto che stiamo per tirare giù dal set
        string nomeOggettoDaRiattivare = "";

        if (taskRichiesta == GameManager.Reparto.Luci)
        {
            SupportoLuce[] supporti = Object.FindObjectsByType<SupportoLuce>(FindObjectsSortMode.None);
            foreach (SupportoLuce s in supporti)
            {
                if (s.luceGiaPosizionata) nomeOggettoDaRiattivare = GameManager.instance.LuceScelta; 
                s.ResettaSupporto();
            }
        }
        else if (taskRichiesta == GameManager.Reparto.Fotografia)
        {
            SpostamentoCamera[] camere = Object.FindObjectsByType<SpostamentoCamera>(FindObjectsSortMode.None);
            foreach (SpostamentoCamera c in camere)
            {
                if (c.lenteMontata) nomeOggettoDaRiattivare = GameManager.instance.lenteSceltaFinale;
                c.ResettaVisualeLenti();
            }
        }
        else if (taskRichiesta == GameManager.Reparto.Fonico)
        {
            MonoBehaviour[] tutti = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (MonoBehaviour mb in tutti)
            {
                if (mb.GetType().Name.Contains("Microfono"))
                {
                    nomeOggettoDaRiattivare = GameManager.instance.micScelto;
                    if (string.IsNullOrEmpty(nomeOggettoDaRiattivare)) nomeOggettoDaRiattivare = GameManager.instance.micDaInstallare;
                    
                    mb.SendMessage("ResettaSupporto", SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        // SE C'ERA UN OGGETTO SUL SET, ORA LO RIACCENDIAMO FISICAMENTE SUL TAVOLO!
        if (!string.IsNullOrEmpty(nomeOggettoDaRiattivare))
        {
            foreach (OggettoRaccolta ogg in oggetti)
            {
                if (ogg != null && ogg.nomeOggetto.Contains(nomeOggettoDaRiattivare))
                {
                    ogg.gameObject.SetActive(true); 
                    break;
                }
            }
        }
    }

    IEnumerator EseguiScambioERiattiva()
    {
        InventarioGiocatore inv = Object.FindFirstObjectByType<InventarioGiocatore>();

        // 1. Pulizia set e riattivazione 3D sul tavolo
        SvuotaSupportiInScenaERiattivaTavolo();

        // 2. FASE DI RESTITUZIONE dell'oggetto eventualmente in mano
        if (inv != null && inv.haUnOggetto)
        {
            if (GameManager.instance != null) GameManager.instance.RestituisciOggettoAlTavolo(inv.oggettoInMano);
            inv.RilasciaOggetto();
            yield return new WaitForEndOfFrame(); 
        }

        // 3. FASE DI RACCOLTA (e spegnimento dal tavolo) del nuovo oggetto
        try 
        {
            if (oggetti[indiceAttuale] != null && oggetti[indiceAttuale].gameObject.activeInHierarchy)
            {
                oggetti[indiceAttuale].EseguiRaccolta();
                oggetti[indiceAttuale].transform.localPosition = posOriginali[indiceAttuale];
                oggetti[indiceAttuale].transform.localRotation = rotOriginali[indiceAttuale];
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Errore durante la raccolta: " + e.Message);
        }

        yield return new WaitUntil(() => !Input.GetKey(KeyCode.E));
        yield return new WaitForSeconds(0.1f);
        
        InterazioneGiocatore interazione = Object.FindFirstObjectByType<InterazioneGiocatore>();
        if (interazione != null) interazione.enabled = true;
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

    IEnumerator AnimaBarreContestuali()
    {
        float timer = 0f;
        float nextVUStep = 0f;
        float targetVU1 = 0f, targetVU2 = 0f, targetVU3 = 0f;

        while (inSelezione)
        {
            timer += Time.deltaTime;

            switch (taskRichiesta)
            {
                case GameManager.Reparto.Fotografia:
                    if (barra1) barra1.fillAmount = Mathf.Lerp(altezzaMinimaBarre, altezzaMassimaBarre, (Mathf.Sin(timer * velocitaAnimazioneBarre) + 1f) / 2f);
                    if (barra2) barra2.fillAmount = Mathf.Lerp(altezzaMinimaBarre, altezzaMassimaBarre, (Mathf.Sin(timer * velocitaAnimazioneBarre + 1.5f) + 1f) / 2f);
                    if (barra3) barra3.fillAmount = Mathf.Lerp(altezzaMinimaBarre, altezzaMassimaBarre, (Mathf.Sin(timer * velocitaAnimazioneBarre + 3f) + 1f) / 2f);
                    break;

                case GameManager.Reparto.Fonico:
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
                    break;

                case GameManager.Reparto.Luci:
                    float speedLuci = velocitaAnimazioneBarre * 0.2f; 
                    if (barra1) barra1.fillAmount = Mathf.Lerp(0.6f, 0.9f, (Mathf.Sin(timer * speedLuci) + 1f) / 2f);
                    if (barra2) barra2.fillAmount = Mathf.Lerp(0.8f, 1f, (Mathf.Sin(timer * speedLuci + 2f) + 1f) / 2f);
                    if (barra3) barra3.fillAmount = Mathf.Lerp(0.3f, 0.5f, (Mathf.Sin(timer * speedLuci + 4f) + 1f) / 2f);
                    break;
            }

            yield return null;
        }
    }

    IEnumerator EffettoTestoAnimato(string titoloCompleto, string descCompleta)
    {
        if (testoTitolo != null) { testoTitolo.text = titoloCompleto; testoTitolo.maxVisibleCharacters = 0; }
        if (testoDescrizione != null) { testoDescrizione.text = descCompleta; testoDescrizione.maxVisibleCharacters = 0; }

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

                if (lettera == '.' || lettera == ':' || lettera == '\n') yield return new WaitForSeconds(velocitaScrittura * 6f);
                else if (lettera == ',' || lettera == ';') yield return new WaitForSeconds(velocitaScrittura * 3f);
                else yield return new WaitForSeconds(velocitaScrittura * 0.5f);
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
                MonoBehaviour sPlayer = giocatore.GetComponent(nomeScript) as MonoBehaviour;
                if (sPlayer != null) sPlayer.enabled = !blocca;
                
                if (cameraGiocatore != null)
                {
                    MonoBehaviour sCam = cameraGiocatore.GetComponent(nomeScript) as MonoBehaviour;
                    if (sCam != null) sCam.enabled = !blocca;
                }
            }
        }
        
        if (controllerGiocatore != null) controllerGiocatore.enabled = !blocca;
        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false;
    }
}