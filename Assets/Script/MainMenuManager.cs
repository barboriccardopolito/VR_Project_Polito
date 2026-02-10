using UnityEngine;
using UnityEngine.SceneManagement; // Fondamentale per cambiare scena

public class MainMenuManager : MonoBehaviour
{
    [Header("Nome della scena di gioco")]
    // Scrivi qui il nome ESATTO della tua scena di gioco (es. "ScenaPrincipale" o "SetCinematografico")
    public string nomeScenaGioco = "InserisciQuiIlNomeDellaTuaScena"; 

    public void AvviaGioco()
    {
        Debug.Log("Avvio nuova partita...");
        // Carica la scena del gioco
        SceneManager.LoadScene(nomeScenaGioco);
    }

    public void EsciDalGioco()
    {
        Debug.Log("CHIUSURA GIOCO RICHIESTA");
        Application.Quit();
    }
}