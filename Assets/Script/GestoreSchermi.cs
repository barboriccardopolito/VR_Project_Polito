using UnityEngine;

public class GestoreSchermi : MonoBehaviour
{
    [Header("Impostazioni")]
    public MeshRenderer[] listaSchermi; // Trascina qui tutti i monitor della scena
    
    [Header("Materiali")]
    public Material materialeVerde; // Riposo / Successo
    public Material materialeRosso; // Allerta / Nuova Task

    // Questa funzione la chiameremo dal GameManager
    public void CambiaStato(bool inAllerta)
    {
        Material materialeScelto = inAllerta ? materialeRosso : materialeVerde;

        foreach (MeshRenderer schermo in listaSchermi)
        {
            if (schermo != null)
            {
                schermo.material = materialeScelto;
            }
        }
    }
}