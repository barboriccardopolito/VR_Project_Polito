using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Necessario per l'Image
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("Impostazioni")]
    // Scrivi qui il nome ESATTO della tua scena di gioco
    public string nomeScenaGioco = "Final_Scene"; 
    
    [Header("Dissolvenza")]
    public Image pannelloDissolvenza; // Trascina qui il pannello nero
    public float durataDissolvenza = 1.0f; // Quanto ci mette a diventare nero

    public void AvviaGioco()
    {
        // Invece di caricare subito, avvia la coroutine
        StartCoroutine(SequenzaAvvio());
    }

    public void EsciDalGioco()
    {
        Debug.Log("CHIUSURA GIOCO RICHIESTA");
        Application.Quit();
    }

    IEnumerator SequenzaAvvio()
    {
        Debug.Log("Inizio dissolvenza...");

        if (pannelloDissolvenza != null)
        {
            // 1. Accendi il pannello
            pannelloDissolvenza.gameObject.SetActive(true);
            
            // 2. Assicurati che parta trasparente
            Color c = pannelloDissolvenza.color;
            c.a = 0f;
            pannelloDissolvenza.color = c;

            float timer = 0f;
            while (timer < durataDissolvenza)
            {
                timer += Time.deltaTime;
                // Aumenta l'Alpha da 0 (trasparente) a 1 (nero)
                c.a = Mathf.Lerp(0f, 1f, timer / durataDissolvenza);
                pannelloDissolvenza.color = c;
                yield return null;
            }
        }

        // 3. Aspetta un istante col nero totale (opzionale, per pulizia)
        yield return new WaitForSeconds(0.2f);

        // 4. Carica la scena
        Debug.Log("Caricamento scena...");
        SceneManager.LoadScene(nomeScenaGioco);
    }
}