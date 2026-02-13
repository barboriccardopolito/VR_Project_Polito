using UnityEngine;

public class AddettoLuci : MonoBehaviour
{
    public void ParlaConAddettoLuci()
    {
        GameManager gm = FindFirstObjectByType<GameManager>(); // Uso la versione nuova, ma FindObjectOfType va bene uguale

        if (gm == null) 
        {
            Debug.LogError("GameManager non trovato!");
            return;
        }

        if (gm.LucePosizionataCorrettamente == true)
        {
            Debug.Log("NPC: Ottimo lavoro! La luce è piazzata bene. Passiamo al prossimo step.");
        }
        else if (gm.LuceScelta != "")
        {
            Debug.Log("NPC: Vedo che hai preso la " + gm.LuceScelta + ", ma i supporti sono ancora vuoti. Vai a montarla!");
        }
        else
        {
            Debug.Log("NPC: Non hai ancora scelto nessuna luce. Guarda il tavolo qui accanto.");
        }
    }
}