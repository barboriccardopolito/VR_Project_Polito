using UnityEngine;
using System.Collections;

public class SelettoreOggetti : MonoBehaviour
{
    [Header("Setup Visuale")]
    public Camera cameraDallAlto;
    public Camera cameraGiocatore;

    [Header("Riferimenti Player")]
    public GameObject giocatore;
    public string[] nomiScriptDaDisabilitare;
    private InterazioneGiocatore scriptInterazione;

    [Header("Impostazioni Task")]
    public GameManager.Reparto taskRichiesta;
    
    [Header("Oggetti Selezionabili (Da Sinistra a Destra)")]
    public OggettoRaccolta[] oggetti;

    private bool inSelezione = false;
    private int indiceAttuale = 0;
    private bool possoUscire = false;

    void Start()
    {
        if (cameraDallAlto != null) cameraDallAlto.gameObject.SetActive(false);
        if (cameraGiocatore == null) cameraGiocatore = Camera.main;
        if (giocatore != null) scriptInterazione = giocatore.GetComponent<InterazioneGiocatore>();
    }

    public bool PuoiInteragire()
    {
        return GameManager.instance != null && GameManager.instance.taskAttuale == taskRichiesta;
    }

    public void EntraInSelezione()
    {
        if (inSelezione) return;

        inSelezione = true;
        possoUscire = false;

        // Trova il primo oggetto visibile da evidenziare
        indiceAttuale = 0;
        for (int i = 0; i < oggetti.Length; i++)
        {
            if (oggetti[i].gameObject.activeInHierarchy) { indiceAttuale = i; break; }
        }

        BloccaGiocatore(true);

        if (cameraGiocatore != null) cameraGiocatore.enabled = false;
        if (cameraDallAlto != null) cameraDallAlto.gameObject.SetActive(true);
        if (scriptInterazione != null) scriptInterazione.enabled = false; // Spegne il mirino

        AggiornaEvidenziatori();
        
        Debug.Log("<color=cyan>[Valigia]</color> Usa A/D per scorrere le opzioni. Premi E per raccogliere.");
        StartCoroutine(TimerSblocco());
    }

    IEnumerator TimerSblocco()
    {
        yield return new WaitForSeconds(0.5f);
        possoUscire = true;
    }

    void Update()
    {
        if (!inSelezione) return;

        // Scorrimento Destra/Sinistra
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) CambiaSelezione(1);
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) CambiaSelezione(-1);

        // Conferma Selezione
        if (Input.GetKeyDown(KeyCode.E) && possoUscire)
        {
            ScegliOggetto();
        }
    }

    void CambiaSelezione(int direzione)
    {
        int tentativi = 0;
        do
        {
            indiceAttuale += direzione;
            if (indiceAttuale >= oggetti.Length) indiceAttuale = 0;
            else if (indiceAttuale < 0) indiceAttuale = oggetti.Length - 1;
            tentativi++;
        }
        // Salta l'oggetto se è stato già raccolto (disattivato)
        while (!oggetti[indiceAttuale].gameObject.activeInHierarchy && tentativi < oggetti.Length);

        AggiornaEvidenziatori();
    }

    void AggiornaEvidenziatori()
    {
        for (int i = 0; i < oggetti.Length; i++)
        {
            Evidenziatore ev = oggetti[i].GetComponent<Evidenziatore>();
            if (ev != null)
            {
                if (i == indiceAttuale && oggetti[i].gameObject.activeInHierarchy) ev.Accendi();
                else ev.Spegni();
            }
        }
    }

    void ScegliOggetto()
    {
        inSelezione = false;
        possoUscire = false;

        if (cameraDallAlto != null) cameraDallAlto.gameObject.SetActive(false);
        if (cameraGiocatore != null) cameraGiocatore.enabled = true;
        if (scriptInterazione != null) scriptInterazione.enabled = true;

        BloccaGiocatore(false);

        // Spegne tutti gli evidenziatori prima di uscire
        foreach (var obj in oggetti)
        {
            Evidenziatore ev = obj.GetComponent<Evidenziatore>();
            if (ev != null) ev.Spegni();
        }

        // Simula fisicamente la raccolta dell'oggetto illuminato
        if (oggetti[indiceAttuale].gameObject.activeInHierarchy)
        {
            oggetti[indiceAttuale].EseguiRaccolta();
        }
    }

    void BloccaGiocatore(bool blocca)
    {
        if (giocatore == null) return;

        if (nomiScriptDaDisabilitare != null)
        {
            foreach (string nomeScript in nomiScriptDaDisabilitare)
            {
                MonoBehaviour scriptPlayer = giocatore.GetComponent(nomeScript) as MonoBehaviour;
                if (scriptPlayer != null) scriptPlayer.enabled = !blocca;

                if (cameraGiocatore != null)
                {
                    MonoBehaviour scriptCam = cameraGiocatore.GetComponent(nomeScript) as MonoBehaviour;
                    if (scriptCam != null) scriptCam.enabled = !blocca;
                }
            }
        }

        CharacterController cc = giocatore.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = !blocca;

        if (blocca) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
    }
}