using UnityEngine;
using System.Collections;

public class SupportoLuce : MonoBehaviour
{    
    private string luceMontataQui = "";

    [Header("Transizione Visuale")]
    public Camera cameraGiocatore;
    [Tooltip("La telecamera fissa che inquadra questo stativo per la cutscene")]
    public Camera cameraInquadratura;
    public float velocitaTransizione = 2.5f;

    [Header("Riferimenti Player")]
    public GameObject giocatore;
    public string[] nomiScriptDaDisabilitare;
    private bool inTransizione = false;

    [Header("Modelli 3D Figli")]
    public GameObject modelloSoftbox;
    public GameObject modelloFresnel;
    public GameObject modelloArtistica;

    [Header("Audio")]
    public AudioClip suonoPiazzamento;
    private AudioSource audioSource;

    [HideInInspector] public bool luceGiaPosizionata = false;

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

        bool faseLuci = (GameManager.instance.taskAttuale == GameManager.Reparto.Luci);
        bool faseRevisione = (GameManager.instance.taskAttuale == GameManager.Reparto.Regia);

        InventarioGiocatore inv = Object.FindFirstObjectByType<InventarioGiocatore>();
        bool hoLuceInMano = (inv != null && inv.haUnOggetto && inv.categoriaInMano == OggettoRaccolta.TipoOggetto.Luce);

        if (faseLuci)
        {
            if (!luceGiaPosizionata && hoLuceInMano) evidenziatore.Accendi();
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
                if (hoLuceInMano && (!luceGiaPosizionata || inv.oggettoInMano != luceMontataQui)) evidenziatore.Accendi();
                else evidenziatore.Spegni();
            }
        }
        else
        {
            evidenziatore.Spegni();
        }
    }

    public void PiazzaLuce()
    {
        if (inTransizione) return;

        GameManager gm = GameManager.instance;
        InventarioGiocatore inventario = Object.FindFirstObjectByType<InventarioGiocatore>(); 
        
        if (gm == null || inventario == null) return;
        if (gm.taskAttuale != GameManager.Reparto.Luci && gm.taskAttuale != GameManager.Reparto.Regia) return;

        bool hoLuceInMano = (inventario.haUnOggetto && inventario.categoriaInMano == OggettoRaccolta.TipoOggetto.Luce);
        string nomeLuceInMano = hoLuceInMano ? inventario.oggettoInMano : "";

        if (luceGiaPosizionata) 
        {
            if (hoLuceInMano && nomeLuceInMano != luceMontataQui)
            {
                gm.RestituisciOggettoAlTavolo(luceMontataQui); 
                ResettaSupporto(); 
            }
            else return;
        }
        else if (!hoLuceInMano)
        {
            return;
        }

        string nomeLuce = inventario.oggettoInMano;
        luceMontataQui = nomeLuce; 
        gm.LuceScelta = nomeLuce; 
        
        GameObject luceAttivata = null;
        string titoloOlogramma = "";
        string descOlogramma = "";

        if (IsNameMatch(nomeLuce, "Softbox")) 
        { 
            if (modelloSoftbox) { modelloSoftbox.SetActive(true); luceAttivata = modelloSoftbox; }
            titoloOlogramma = "Pannello LED Softbox";
            descOlogramma = "Luce diffusa con pannello a nido d'ape. Avvolge morbidamente il soggetto attenuando le ombre e le imperfezioni del viso.";
        }
        else if (IsNameMatch(nomeLuce, "Fresnel")) 
        { 
            if (modelloFresnel) { modelloFresnel.SetActive(true); luceAttivata = modelloFresnel; }
            titoloOlogramma = "Proiettore Fresnel 2K";
            descOlogramma = "Temperatura 5600K (Daylight). La lente a gradini produce un fascio di luce duro e incisivo. Ideale come Key Light.";
        }
        else if (IsNameMatch(nomeLuce, "Artistica")) 
        { 
            if (modelloArtistica) { modelloArtistica.SetActive(true); luceAttivata = modelloArtistica; }
            titoloOlogramma = "Tubo LED RGB Pixel";
            descOlogramma = "Emettitore a spettro cromatico completo. Ottimo per essere usato come luce pratica in scena o per riflessi creativi.";
        }

        if (luceAttivata != null)
        {
            luceGiaPosizionata = true;
            StartCoroutine(GestisciVoloECinematica(luceAttivata, titoloOlogramma, descOlogramma, inventario));
        }
    }

    IEnumerator GestisciVoloECinematica(GameObject luce, string titolo, string desc, InventarioGiocatore inv)
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

            MontaggioLuceCinematica cinematica = GetComponent<MontaggioLuceCinematica>();
            if (cinematica != null) cinematica.AvviaCinematicaMontaggio(luce, titolo, desc);
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
            MontaggioLuceCinematica cinematica = GetComponent<MontaggioLuceCinematica>();
            if (cinematica != null) cinematica.AvviaCinematicaMontaggio(luce, titolo, desc);
            yield return new WaitForSeconds(3.5f);
        }

        foreach (Renderer r in renderersInMano) r.enabled = true;
        VerificaCompletamentoLuci();
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

    void VerificaCompletamentoLuci()
    {
        if (GameManager.instance == null || GameManager.instance.supportiLuciFisici == null) return;

        GameObject[] supportiDaControllare = GameManager.instance.supportiLuciFisici;
        int totali = supportiDaControllare.Length;
        int completati = 0;

        foreach (GameObject obj in supportiDaControllare)
        {
            if (obj == null) continue;
            SupportoLuce script = obj.GetComponent<SupportoLuce>();
            if (script != null && script.luceGiaPosizionata) completati++;
        }

        if (completati >= totali && totali > 0)
        {
            if (GameManager.instance.taskAttuale == GameManager.Reparto.Luci)
            {
                InventarioGiocatore inv = Object.FindFirstObjectByType<InventarioGiocatore>();
                if (inv != null) inv.RimuoviOggetto();

                GameManager.instance.LucePosizionataCorrettamente = true;
                GameManager.instance.CompletaTask(GameManager.Reparto.Luci);
            }
            else if (GameManager.instance.taskAttuale == GameManager.Reparto.Regia)
            {
                InventarioGiocatore inv = Object.FindFirstObjectByType<InventarioGiocatore>();
                if (inv != null) inv.RimuoviOggetto();
            }
        }
    }

    public void ResettaSupporto()
    {
        if (luceGiaPosizionata && !string.IsNullOrEmpty(luceMontataQui) && GameManager.instance != null)
        {
            GameManager.instance.RestituisciOggettoAlTavolo(luceMontataQui);
        }
        
        luceGiaPosizionata = false;
        luceMontataQui = "";
        NascondiTutto();
    }

    void NascondiTutto()
    {
        if(modelloSoftbox) modelloSoftbox.SetActive(false);
        if(modelloFresnel) modelloFresnel.SetActive(false);
        if(modelloArtistica) modelloArtistica.SetActive(false);
    }

    private bool IsNameMatch(string input, string target)
    {
        return input.ToLower().Contains(target.ToLower());
    }
}