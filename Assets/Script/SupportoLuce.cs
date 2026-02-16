using UnityEngine;
using System.Collections;

public class SupportoLuce : MonoBehaviour
{    
    private string luceMontataQui = "";

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

        NascondiTutto();
    }

    void Update()
    {
        GestisciEvidenziatore();
    }

    void GestisciEvidenziatore()
    {
        if (evidenziatore == null || GameManager.instance == null) return;

        bool faseLuci = (GameManager.instance.taskAttuale == GameManager.Reparto.Luci);
        bool faseRevisione = (GameManager.instance.taskAttuale == GameManager.Reparto.Regia);

        InventarioGiocatore inv = FindFirstObjectByType<InventarioGiocatore>();
        bool hoLuceInMano = (inv != null && inv.haUnOggetto && inv.categoriaInMano == OggettoRaccolta.TipoOggetto.Luce);

        if (faseLuci)
        {
            if (!luceGiaPosizionata && hoLuceInMano) evidenziatore.Accendi();
            else evidenziatore.Spegni();
        }
        else if (faseRevisione)
        {
            if (hoLuceInMano && luceGiaPosizionata && inv.oggettoInMano != luceMontataQui) evidenziatore.Accendi();
            else evidenziatore.Spegni();
        }
        else
        {
            evidenziatore.Spegni();
        }
    }

    public void PiazzaLuce()
    {
        GameManager gm = GameManager.instance;
        InventarioGiocatore inventario = FindFirstObjectByType<InventarioGiocatore>(); 
        
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
            Debug.Log("Non hai una luce in mano!");
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
            
            MontaggioLuceCinematica cinematica = GetComponent<MontaggioLuceCinematica>();
            if (cinematica != null) 
            {
                cinematica.AvviaCinematicaMontaggio(luceAttivata, titoloOlogramma, descOlogramma);
                
                // --- AVVIA LA MAGIA PER NASCONDERE L'OGGETTO IN MANO ---
                StartCoroutine(NascondiLuceInManoDuranteCinematica(inventario));
            }
            else if (suonoPiazzamento != null) 
            {
                audioSource.PlayOneShot(suonoPiazzamento);
            }
            
            VerificaCompletamentoLuci();
        }
    }

    // --- NUOVA COROUTINE PER GESTIRE LA VISIBILITÀ ---
    IEnumerator NascondiLuceInManoDuranteCinematica(InventarioGiocatore inv)
    {
        // 1. Trova la telecamera del giocatore per sapere quando finirà la cutscene
        Camera cameraGiocatore = inv.GetComponentInChildren<Camera>(true);

        // 2. Trova i "Renderer" (disegnatori 3D) dell'oggetto in mano e li spegne
        Renderer[] renderersInMano = inv.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderersInMano) r.enabled = false;

        // 3. Diamo il tempo alla cinematica di iniziare e disabilitare la camera del giocatore
        yield return new WaitForSeconds(0.2f);

        // 4. Aspetta pazientemente finché la camera del giocatore non torna attiva
        if (cameraGiocatore != null)
        {
            yield return new WaitUntil(() => cameraGiocatore.gameObject.activeInHierarchy && cameraGiocatore.enabled);
        }
        else
        {
            // Fallback di sicurezza: se non trova la camera, aspetta 4 secondi
            yield return new WaitForSeconds(4f);
        }

        // 5. Cinematica finita! Riaccende l'oggetto in mano per il prossimo stativo
        foreach (Renderer r in renderersInMano) r.enabled = true;
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
                InventarioGiocatore inv = FindFirstObjectByType<InventarioGiocatore>();
                if (inv != null) inv.RimuoviOggetto();

                GameManager.instance.LucePosizionataCorrettamente = true;
                GameManager.instance.CompletaTask(GameManager.Reparto.Luci);
            }
            else if (GameManager.instance.taskAttuale == GameManager.Reparto.Regia)
            {
                InventarioGiocatore inv = FindFirstObjectByType<InventarioGiocatore>();
                if (inv != null) inv.RimuoviOggetto();
            }
        }
    }

    public void ResettaSupporto()
    {
        luceGiaPosizionata = false;
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