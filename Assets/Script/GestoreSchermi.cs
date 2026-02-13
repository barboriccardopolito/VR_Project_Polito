using UnityEngine;

public class GestoreSchermi : MonoBehaviour
{
    [Header("Impostazioni")]
    public MeshRenderer[] listaSchermi;
    
    [Header("Materiali")]
    public Material materialeVerde;
    public Material materialeRosso;

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