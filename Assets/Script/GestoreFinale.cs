using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Serve per tornare al Menu
using System.Collections;
using TMPro;

public class GestoreFinale : MonoBehaviour
{
    [Header("Collegamenti UI")]
    public GameObject gruppoFinale;      // L'oggetto padre che contiene tutto (spento all'inizio)
    public RectTransform testoTitoli;    // Il testo che deve salire
    public Image pannelloNero;           // Per la dissolvenza (opzionale)

    [Header("Impostazioni")]
    public float velocitaScorrimento = 100f; // A che velocità sale il testo
    public float puntoDiArrestoY = 1500f;    // Quando il testo arriva qui, finisce il gioco (dipende dalla lunghezza del testo)
    public string nomeScenaMenu = "MenuPrincipale"; // Come si chiama la scena del menu?

    public void AvviaTitoliDiCoda()
    {
        StartCoroutine(SequenzaFinale());
    }

    IEnumerator SequenzaFinale()
    {
        // 1. Attiva il pannello finale
        if (gruppoFinale != null) gruppoFinale.SetActive(true);

        // 2. Assicurati che il nero sia visibile (dissolvenza rapida opzionale)
        if (pannelloNero != null)
        {
            pannelloNero.canvasRenderer.SetAlpha(0f);
            pannelloNero.CrossFadeAlpha(1f, 1.0f, false); // Diventa nero in 1 secondo
        }

        yield return new WaitForSeconds(1.0f); // Aspetta che sia tutto nero

        // 3. Fai scorrere i titoli verso l'alto
        while (testoTitoli.anchoredPosition.y < puntoDiArrestoY)
        {
            testoTitoli.anchoredPosition += new Vector2(0, velocitaScorrimento * Time.deltaTime);
            yield return null;
        }

        // 4. Pausa finale drammatica
        yield return new WaitForSeconds(3.0f);

        // 5. Torna al Menu Principale
        Cursor.lockState = CursorLockMode.None; // Sblocca il mouse
        Cursor.visible = true;
        SceneManager.LoadScene(nomeScenaMenu);
    }
}