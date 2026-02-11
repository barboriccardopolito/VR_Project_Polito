using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 
using System.Collections;
using TMPro;

public class GestoreFinale : MonoBehaviour
{
    [Header("Collegamenti UI")]
    public GameObject gruppoFinale;      // L'oggetto padre che contiene tutto (spento all'inizio)
    public TextMeshProUGUI testoTitoli;  // Il componente di testo che deve scorrere
    public Image pannelloNero;           // L'immagine di sfondo

    [Header("Impostazioni Scorrimento")]
    public float velocitaScorrimento = 100f; // Velocità di salita
    public float puntoDiArrestoY = 2500f;    // A che altezza finisce il gioco (aumenta se il testo è lungo)
    public string nomeScenaMenu = "MenuPrincipale"; 

    // --- CREDITI STATICI (Modifica questi nomi come vuoi) ---
    private string ruoliConosciuti = 
        "\n\n--- IL TEAM DI PRODUZIONE ---\n" +
        "Direttore Fotografia\n" +
        "Direttore Luci\n" +
        "Fonico di Presa Diretta\n" +
        "Regista\n" +
        "Produzione\n";

    private string creditiFinali = 
        "\n\n--- SVILUPPO ---\n" +
        "Game Design: Riccardo\n" +
        "Game Design: Francesco\n" +
        "Game Design: Leonardo\n" +
        "Game Design: Stefano\n" +
        "\n\nGRAZIE PER AVER GIOCATO!";

    public void AvviaTitoliDiCoda()
    {
        // 1. Prima di partire, costruiamo il testo con le scelte del giocatore
        ImpostaTestoFinale();

        // 2. Avvia l'animazione
        StartCoroutine(SequenzaFinale());
    }

    void ImpostaTestoFinale()
    {
        if (GameManager.instance == null) return;

        // Recuperiamo i dati dal GameManager
        string lente = GameManager.instance.lenteSceltaFinale;
        string luce = GameManager.instance.LuceScelta;
        string mic = GameManager.instance.micScelto; // O micDaInstallare a seconda di cosa salvi
        
        // Se micScelto è vuoto, proviamo a prendere quello della task
        if (string.IsNullOrEmpty(mic)) mic = GameManager.instance.micDaInstallare;

        int numAttori = GameManager.instance.attoriMicrofonatiAttuali;
        int totAttori = GameManager.instance.attoriDaMicrofonare;

        // Costruiamo la stringa finale
        string statistiche = 
            "--- RIEPILOGO PRODUZIONE ---\n\n" +
            $"LENTE UTILIZZATA: <color=yellow>{lente}</color>\n" +
            $"ILLUMINAZIONE: <color=yellow>{luce}</color>\n" +
            $"MICROFONO: <color=yellow>{mic}</color>\n" +
            $"ATTORI MICROFONATI: <color=yellow>{numAttori} su {totAttori}</color>\n";

        // Uniamo tutto: Statistiche + Ruoli Fissi + Crediti
        if (testoTitoli != null)
        {
            testoTitoli.text = statistiche + ruoliConosciuti + creditiFinali;
        }
    }

    IEnumerator SequenzaFinale()
    {
        // 1. Attiva il pannello finale
        if (gruppoFinale != null) gruppoFinale.SetActive(true);

        // 2. Dissolvenza a nero (rapida)
        if (pannelloNero != null)
        {
            pannelloNero.canvasRenderer.SetAlpha(0f);
            pannelloNero.CrossFadeAlpha(1f, 1.0f, false);
        }

        // Aspetta che sia tutto nero prima di far partire il testo
        yield return new WaitForSeconds(1.5f);

        // 3. Fai scorrere i titoli verso l'alto
        // Usiamo rectTransform per muovere il testo UI
        RectTransform rt = testoTitoli.GetComponent<RectTransform>();
        
        // Resetta la posizione in basso (fuori schermo)
        rt.anchoredPosition = new Vector2(0, -500); 

        while (rt.anchoredPosition.y < puntoDiArrestoY)
        {
            rt.anchoredPosition += new Vector2(0, velocitaScorrimento * Time.deltaTime);
            yield return null;
        }

        // 4. Pausa finale drammatica a testo fermo
        yield return new WaitForSeconds(4.0f);

        // 5. Torna al Menu Principale
        Cursor.lockState = CursorLockMode.None; 
        Cursor.visible = true;
        SceneManager.LoadScene(nomeScenaMenu);
    }
}