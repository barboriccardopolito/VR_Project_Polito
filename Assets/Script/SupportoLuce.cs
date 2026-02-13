using UnityEngine;

public class SupportoLuce : MonoBehaviour
{
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

        // Se la task non è quella delle Luci, non fare nulla
        if (gm.taskAttuale != GameManager.Reparto.Luci) return;

        if (luceGiaPosizionata) 
        {
            Debug.Log("Questo supporto ha già una luce.");
            return;
        }

        if (!inventario.haUnOggetto || inventario.categoriaInMano != OggettoRaccolta.TipoOggetto.Luce)
        {
            Debug.Log("Non hai una luce in mano!");
            return;
        }

        string nomeLuce = inventario.oggettoInMano;
        gm.LuceScelta = nomeLuce; 

        bool modelloAttivato = false;

        if (IsNameMatch(nomeLuce, "Softbox")) { if (modelloSoftbox) { modelloSoftbox.SetActive(true); modelloAttivato = true; } }
        else if (IsNameMatch(nomeLuce, "Fresnel")) { if (modelloFresnel) { modelloFresnel.SetActive(true); modelloAttivato = true; } }
        else if (IsNameMatch(nomeLuce, "Artistica")) { if (modelloArtistica) { modelloArtistica.SetActive(true); modelloAttivato = true; } }

        if (modelloAttivato)
        {
            luceGiaPosizionata = true;
            if (suonoPiazzamento != null) audioSource.PlayOneShot(suonoPiazzamento);
            
            Debug.Log($"<color=cyan>Luce piazzata su {gameObject.name}. Controllo stato globale...</color>");
            VerificaCompletamentoLuci();
        }
    }

    void VerificaCompletamentoLuci()
    {
        if (GameManager.instance == null || GameManager.instance.supportiLuciFisici == null) return;

        // Prendiamo i supporti direttamente dalla lista definita nel GameManager
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

        if (completati >= totali && totali > 0)
        {
            InventarioGiocatore inv = FindFirstObjectByType<InventarioGiocatore>();
            if (inv != null) inv.RimuoviOggetto();

            GameManager.instance.LucePosizionataCorrettamente = true;
            GameManager.instance.CompletaTask(GameManager.Reparto.Luci);
            
            Debug.Log("<color=green>TUTTI I SUPPORTI DELLA LISTA SONO PRONTI!</color>");
        }
    }

    // Metodo di pulizia per sicurezza (chiamalo se vuoi resettare la scena)
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