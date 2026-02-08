using UnityEngine;

public class RadioSistema : MonoBehaviour
{
    public bool haLaRadio = false;

    // Funzione chiamata dall'NPC per dare la radio
    public void RiceviRadio()
    {
        haLaRadio = true;
        Debug.Log("RADIO SISTEMA: Radio acquisita!");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            UsaRadio();
        }
    }

    void UsaRadio()
    {
        if (haLaRadio)
        {
            // Assicurati che GameManager esista, altrimenti darà errore
            if (GameManager.instance != null)
            {
                string messaggio = GameManager.instance.OttieniSuggerimentoRadio();
                Debug.Log("<color=cyan>[RADIO]:</color> " + messaggio);
            }
        }
        else
        {
            Debug.Log("Non hai la radio. Vai in Produzione!");
        }
    }
}