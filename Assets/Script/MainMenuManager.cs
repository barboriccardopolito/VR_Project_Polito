using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("Impostazioni")]
    public string nomeScenaGioco = "Final_Scene"; 
    
    [Header("Dissolvenza")]
    public Image pannelloDissolvenza;
    public float durataDissolvenza = 1.0f;

    public void AvviaGioco()
    {
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
            pannelloDissolvenza.gameObject.SetActive(true);
            
            Color c = pannelloDissolvenza.color;
            c.a = 0f;
            pannelloDissolvenza.color = c;

            float timer = 0f;
            while (timer < durataDissolvenza)
            {
                timer += Time.deltaTime;
                c.a = Mathf.Lerp(0f, 1f, timer / durataDissolvenza);
                pannelloDissolvenza.color = c;
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.2f);

        Debug.Log("Caricamento scena...");
        SceneManager.LoadScene(nomeScenaGioco);
    }
}