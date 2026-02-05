using UnityEngine;

public class RadioSistema : MonoBehaviour
{
    public bool haLaRadio = false;

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
            string messaggio = GameManager.instance.OttieniSuggerimentoRadio();
            Debug.Log("<color=cyan>[RADIO]:</color> " + messaggio);
        }
        else
        {
            Debug.Log("Non hai la radio. Vai in Produzione!");
        }
    }
}