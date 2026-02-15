using UnityEngine;

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

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f; 

        NascondiTutto();
    }

    public void PiazzaLuce()
    {
        GameManager gm = GameManager.instance;
        InventarioGiocatore inventario = FindFirstObjectByType<InventarioGiocatore>(); 
        
        if (gm == null || inventario == null) return;
        if (gm.taskAttuale != GameManager.Reparto.Luci && gm.taskAttuale != GameManager.Reparto.Regia) return;

        bool hoLuceInMano = (inventario.haUnOggetto && inventario.categoriaInMano == OggettoRaccolta.TipoOggetto.Luce);
        string nomeLuceInMano = hoLuceInMano ? inventario.oggettoInMano : "";

        // --- CASO 1: LA LUCE E' GIA' SU QUESTO STATIVO ---
        if (luceGiaPosizionata) 
        {
            if (hoLuceInMano && nomeLuceInMano != luceMontataQui)
            {
                // CAMBIO LUCE! Restituisce la vecchia al tavolo e accetta la nuova.
                gm.RestituisciOggettoAlTavolo(luceMontataQui); 
                ResettaSupporto(); 
            }
            else
            {
                // Se hai in mano la stessa luce o non hai niente, ignoralo. Niente spam in console.
                return;
            }
        }
        else if (!hoLuceInMano) // Se lo stativo è vuoto ma non hai niente in mano
        {
            Debug.Log("Non hai una luce in mano!");
            return;
        }

        // --- CASO 2: MONTAGGIO (Primo o Cambio) ---
        string nomeLuce = inventario.oggettoInMano;
        luceMontataQui = nomeLuce; // Salva la memoria della nuova luce!
        gm.LuceScelta = nomeLuce; 
        
        // VARIABILI PER LA CINEMATICA
        GameObject luceAttivata = null;
        string titoloOlogramma = "";
        string descOlogramma = "";

        // RICONOSCIMENTO LUCE E ASSEGNAZIONE TESTI
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
            
            // --- LANCIO DELLA CINEMATICA ---
            MontaggioLuceCinematica cinematica = GetComponent<MontaggioLuceCinematica>();
            if (cinematica != null)
            {
                cinematica.AvviaCinematicaMontaggio(luceAttivata, titoloOlogramma, descOlogramma);
            }
            else
            {
                if (suonoPiazzamento != null) audioSource.PlayOneShot(suonoPiazzamento);
            }
            
            Debug.Log($"<color=cyan>Luce piazzata su {gameObject.name}. Controllo stato globale...</color>");
            VerificaCompletamentoLuci();
        }
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
            if (script != null && script.luceGiaPosizionata)
            {
                completati++;
            }
        }

        Debug.Log($"<color=white>Stato Luci (da lista GM): {completati} su {totali} supporti pronti.</color>");

        // SOLO SE tutte le luci sono state piazzate E non l'avevamo già fatto prima, chiudiamo la task!
        if (completati >= totali && totali > 0)
        {
            // Se eravamo al "Primo Giro" (non in regia), svuotiamo le mani e passiamo al prossimo reparto.
            if (GameManager.instance.taskAttuale == GameManager.Reparto.Luci)
            {
                InventarioGiocatore inv = FindFirstObjectByType<InventarioGiocatore>();
                if (inv != null) inv.RimuoviOggetto();

                GameManager.instance.LucePosizionataCorrettamente = true;
                GameManager.instance.CompletaTask(GameManager.Reparto.Luci);
                
                Debug.Log("<color=green>TUTTI I SUPPORTI DELLA LISTA SONO PRONTI! Task Completata!</color>");
            }
            // Se siamo in Regia, non forziamo la chiusura della task (che è già Regia)
            // e svuotiamo le mani per evitare bug visivi dell'inventario.
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