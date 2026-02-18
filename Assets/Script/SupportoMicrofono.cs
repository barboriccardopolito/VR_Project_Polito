using UnityEngine;
using System.Collections;

public class SupportoMicrofono : MonoBehaviour
{    
    private string micMontatoQui = "";

    [Header("Transizione Visuale")]
    public Camera cameraGiocatore;
    [Tooltip("La telecamera fissa che inquadra questo stativo per la cutscene")]
    public Camera cameraInquadratura;
    public float velocitaTransizione = 2.5f;

    [Header("Riferimenti Player")]
    public GameObject giocatore;
    public string[] nomiScriptDaDisabilitare;
    private bool inTransizione = false;

    [Header("Impostazioni Asta")]
    [Tooltip("Scrivi 'Boom' o 'Ambisonic' per forzare quest'asta ad accettare SOLO quel microfono. Lascia vuoto per accettarli tutti.")]
    public string tipoMicrofonoAccettato = "";

    [Header("Modelli 3D Figli")]
    public GameObject modelloBoom;
    public GameObject modelloAmbisonic;

    [Header("Audio")]
    public AudioClip suonoPiazzamento;
    private AudioSource audioSource;

    [HideInInspector] public bool microfonoPiazzato = false;
    private Evidenziatore evidenziatore;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f; 

        evidenziatore = GetComponent<Evidenziatore>();
        if (evidenziatore == null) evidenziatore = GetComponentInChildren<Evidenziatore>();

        if (cameraGiocatore == null) cameraGiocatore = Camera.main;

        NascondiTutto();
    }

    void Update()
    {
        GestisciEvidenziatore();
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
        
        if (inTransizione) { evidenziatore.Spegni(); return; }

        bool faseFonico = (GameManager.instance.taskAttuale == GameManager.Reparto.Fonico);
        bool faseRevisione = (GameManager.instance.taskAttuale == GameManager.Reparto.Regia);

        InventarioGiocatore inv = Object.FindFirstObjectByType<InventarioGiocatore>();
        bool hoMicInMano = (inv != null && inv.haUnOggetto && inv.categoriaInMano == OggettoRaccolta.TipoOggetto.Microfono);
        string nomeMic = hoMicInMano ? inv.oggettoInMano : "";

        bool astaCorretta = true;
        if (hoMicInMano && !string.IsNullOrEmpty(tipoMicrofonoAccettato))
        {
            astaCorretta = nomeMic.ToLower().Contains(tipoMicrofonoAccettato.ToLower());
        }

        if (faseFonico)
        {
            if (!microfonoPiazzato && hoMicInMano && astaCorretta) evidenziatore.Accendi();
            else evidenziatore.Spegni();
        }
        else if (faseRevisione)
        {
            if (!ControllaIntroRegista()) 
            {
                evidenziatore.Spegni();
            }
            else 
            {
                if (hoMicInMano && astaCorretta && (!microfonoPiazzato || nomeMic != micMontatoQui)) evidenziatore.Accendi();
                else evidenziatore.Spegni();
            }
        }
        else
        {
            evidenziatore.Spegni();
        }
    }

    public void PiazzaMicrofono() 
    {
        if (inTransizione) return;

        GameManager gm = GameManager.instance;
        InventarioGiocatore inventario = Object.FindFirstObjectByType<InventarioGiocatore>(); 

        if (gm == null || inventario == null) return;
        if (gm.taskAttuale != GameManager.Reparto.Fonico && gm.taskAttuale != GameManager.Reparto.Regia) return;

        bool hoMicInMano = (inventario.haUnOggetto && inventario.categoriaInMano == OggettoRaccolta.TipoOggetto.Microfono);
        string nomeMicInMano = hoMicInMano ? inventario.oggettoInMano : "";

        if (hoMicInMano && !string.IsNullOrEmpty(tipoMicrofonoAccettato))
        {
            if (!IsNameMatch(nomeMicInMano, tipoMicrofonoAccettato)) return;
        }

        if (microfonoPiazzato)
        {
            if (hoMicInMano && nomeMicInMano != micMontatoQui)
            {
                gm.RestituisciOggettoAlTavolo(micMontatoQui); 
                ResettaSupporto(); 
            }
            else return; 
        }
        else if (!hoMicInMano)
        {
            return;
        }

        string nomeMic = inventario.oggettoInMano;
        micMontatoQui = nomeMic; 
        gm.micDaInstallare = nomeMic; 
        
        GameObject micAttivato = null;
        string titoloOlogramma = "";
        string descOlogramma = "";

        if (IsNameMatch(nomeMic, "Boom")) 
        { 
            if (modelloBoom) { modelloBoom.SetActive(true); micAttivato = modelloBoom; }
            titoloOlogramma = "Microfono Boom (Shotgun)";
            descOlogramma = "Pattern polare iper-cardioide. Altissima direzionalità per isolare i dialoghi dal rumore ambientale del set.";
        }
        else if (IsNameMatch(nomeMic, "Ambisonic")) 
        { 
            if (modelloAmbisonic) { modelloAmbisonic.SetActive(true); micAttivato = modelloAmbisonic; }
            titoloOlogramma = "Microfono VR Ambisonic";
            descOlogramma = "Capsula tetraedrica. Cattura il campo sonoro a 360 gradi (A-Format) per un audio spaziale totalmente immersivo.";
        }

        if (micAttivato != null)
        {
            microfonoPiazzato = true;
            gm.supportoPiazzato = true; 
            gm.ApplicaEffettoMicrofono(nomeMic);

            StartCoroutine(GestisciVoloECinematica(micAttivato, titoloOlogramma, descOlogramma, inventario));
        }
    }

    IEnumerator GestisciVoloECinematica(GameObject mic, string titolo, string desc, InventarioGiocatore inv)
    {
        inTransizione = true;
        BloccaGiocatore(true);

        Renderer[] renderersInMano = inv.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderersInMano) r.enabled = false;

        if (cameraInquadratura != null && cameraGiocatore != null)
        {
            Vector3 targetLocalPos = cameraInquadratura.transform.localPosition;
            Quaternion targetLocalRot = cameraInquadratura.transform.localRotation;
            float targetFov = cameraInquadratura.fieldOfView;
            
            Vector3 targetWorldPos = cameraInquadratura.transform.position;
            Quaternion targetWorldRot = cameraInquadratura.transform.rotation;

            cameraInquadratura.transform.position = cameraGiocatore.transform.position;
            cameraInquadratura.transform.rotation = cameraGiocatore.transform.rotation;
            cameraInquadratura.fieldOfView = cameraGiocatore.fieldOfView;

            cameraGiocatore.enabled = false;
            cameraInquadratura.gameObject.SetActive(true);

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * velocitaTransizione;
                float smooth = Mathf.SmoothStep(0f, 1f, t);
                cameraInquadratura.transform.position = Vector3.Lerp(cameraGiocatore.transform.position, targetWorldPos, smooth);
                cameraInquadratura.transform.rotation = Quaternion.Lerp(cameraGiocatore.transform.rotation, targetWorldRot, smooth);
                cameraInquadratura.fieldOfView = Mathf.Lerp(cameraGiocatore.fieldOfView, targetFov, smooth);
                yield return null;
            }

            cameraInquadratura.transform.position = targetWorldPos;
            cameraInquadratura.transform.rotation = targetWorldRot;

            MontaggioMicrofonoCinematica cinematica = GetComponent<MontaggioMicrofonoCinematica>();
            if (cinematica != null) cinematica.AvviaCinematicaMontaggio(mic, titolo, desc);
            else if (suonoPiazzamento != null) audioSource.PlayOneShot(suonoPiazzamento);

            yield return new WaitForSeconds(3.5f);

            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * velocitaTransizione;
                float smooth = Mathf.SmoothStep(0f, 1f, t);
                cameraInquadratura.transform.position = Vector3.Lerp(targetWorldPos, cameraGiocatore.transform.position, smooth);
                cameraInquadratura.transform.rotation = Quaternion.Lerp(targetWorldRot, cameraGiocatore.transform.rotation, smooth);
                cameraInquadratura.fieldOfView = Mathf.Lerp(targetFov, cameraGiocatore.fieldOfView, smooth);
                yield return null;
            }

            cameraInquadratura.gameObject.SetActive(false);
            cameraInquadratura.transform.localPosition = targetLocalPos;
            cameraInquadratura.transform.localRotation = targetLocalRot;
            cameraGiocatore.enabled = true;
        }
        else
        {
            MontaggioMicrofonoCinematica cinematica = GetComponent<MontaggioMicrofonoCinematica>();
            if (cinematica != null) cinematica.AvviaCinematicaMontaggio(mic, titolo, desc);
            yield return new WaitForSeconds(3.5f);
        }

        foreach (Renderer r in renderersInMano) r.enabled = true;
        inv.RimuoviOggetto();
        
        if (GameManager.instance.taskAttuale == GameManager.Reparto.Fonico)
            GameManager.instance.CompletaTask(GameManager.Reparto.Fonico); 

        BloccaGiocatore(false);
        inTransizione = false;
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
    }

    public void ResettaSupporto()
    {
        if (microfonoPiazzato && !string.IsNullOrEmpty(micMontatoQui) && GameManager.instance != null)
        {
            GameManager.instance.RestituisciOggettoAlTavolo(micMontatoQui);
        }

        microfonoPiazzato = false;
        micMontatoQui = "";
        NascondiTutto();
    }

    void NascondiTutto()
    {
        if (modelloBoom) modelloBoom.SetActive(false);
        if (modelloAmbisonic) modelloAmbisonic.SetActive(false);
    }

    private bool IsNameMatch(string input, string target)
    {
        return input.ToLower().Contains(target.ToLower());
    }
}