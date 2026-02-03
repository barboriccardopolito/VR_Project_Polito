using UnityEngine;

public class AddettoLuci : MonoBehaviour
{
    // Questa funzione viene chiamata quando premi E sull'NPC
    public void ParlaConAddettoLuci()
    {
        // Troviamo il GameManager per leggere lo stato della missione
        GameManager gm = FindFirstObjectByType<GameManager>(); // Uso la versione nuova, ma FindObjectOfType va bene uguale

        if (gm == null) 
        {
            Debug.LogError("GameManager non trovato!");
            return;
        }

        if (gm.LucePosizionataCorrettamente == true)
        {
            // CASO 3: SUCCESSO
            Debug.Log("NPC: Ottimo lavoro! La luce è piazzata bene. Passiamo al prossimo step.");
            // Qui puoi aggiungere codice per dare punti o chiudere la quest
        }
        else if (gm.LuceScelta != "")
        {
            // CASO 2: LUCE PRESA MA NON PIAZZATA
            Debug.Log("NPC: Vedo che hai preso la " + gm.LuceScelta + ", ma i supporti sono ancora vuoti. Vai a montarla!");
        }
        else
        {
            // CASO 1: INIZIALE
            Debug.Log("NPC: Non hai ancora scelto nessuna luce. Guarda il tavolo qui accanto.");
        }
    }
}