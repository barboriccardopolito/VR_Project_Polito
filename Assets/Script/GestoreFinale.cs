using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 
using System.Collections;
using TMPro;

public class GestoreFinale : MonoBehaviour
{
    [Header("Collegamenti UI")]
    public GameObject gruppoFinale;
    public TextMeshProUGUI testoTitoli;  
    public Image pannelloNero;           

    [Header("Impostazioni Scorrimento")]
    public float velocitaScorrimento = 100f;
    [Tooltip("Da quanto in basso deve partire il testo? (es. -1000)")]
    public float puntoDiPartenzaY = 0f;
    public float puntoDiArrestoY = 2500f;
    public string nomeScenaMenu = "MenuPrincipale"; 

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
        ImpostaTestoFinale();
        StartCoroutine(SequenzaFinale());
    }

    void ImpostaTestoFinale()
    {
        if (GameManager.instance == null) return;

        string lente = GameManager.instance.lenteSceltaFinale;
        string luce = GameManager.instance.LuceScelta;
        string mic = GameManager.instance.micScelto;
        
        if (string.IsNullOrEmpty(mic)) mic = GameManager.instance.micDaInstallare;

        int numAttori = GameManager.instance.attoriMicrofonatiAttuali;
        int totAttori = GameManager.instance.attoriDaMicrofonare;

        string statistiche = 
            "--- RIEPILOGO PRODUZIONE ---\n\n" +
            $"LENTE UTILIZZATA: <color=yellow>{lente}</color>\n" +
            $"ILLUMINAZIONE: <color=yellow>{luce}</color>\n" +
            $"MICROFONO: <color=yellow>{mic}</color>\n" +
            $"ATTORI MICROFONATI: <color=yellow>{numAttori} su {totAttori}</color>\n";

        if (testoTitoli != null)
        {
            testoTitoli.text = statistiche + ruoliConosciuti + creditiFinali;
        }
    }

    IEnumerator SequenzaFinale()
    {
        if (testoTitoli != null)
        {
            RectTransform rtIniziale = testoTitoli.GetComponent<RectTransform>();
            rtIniziale.anchoredPosition = new Vector2(0, puntoDiPartenzaY);
        }

        if (gruppoFinale != null) gruppoFinale.SetActive(true);

        if (pannelloNero != null)
        {
            pannelloNero.canvasRenderer.SetAlpha(0f);
            pannelloNero.CrossFadeAlpha(1f, 1.0f, false);
        }

        yield return new WaitForSeconds(1.5f);

        RectTransform rt = testoTitoli.GetComponent<RectTransform>();
        
        while (rt.anchoredPosition.y < puntoDiArrestoY)
        {
            rt.anchoredPosition += new Vector2(0, velocitaScorrimento * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(4.0f);

        Cursor.lockState = CursorLockMode.None; 
        Cursor.visible = true;
        SceneManager.LoadScene(nomeScenaMenu);
    }
}