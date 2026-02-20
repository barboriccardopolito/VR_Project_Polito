using UnityEngine;

public class PortaSet : MonoBehaviour
{
    [Header("Impostazioni Porta")]
    [Tooltip("Il valore esatto della rotazione Y quando la porta è aperta")]
    public float rotazioneApertaY = -120f; 
    public float velocitaApertura = 3f;
    
    [HideInInspector] public bool isOpen = false;
    
    private Quaternion rotazioneAperta;

    void Start()
    {
        Vector3 angoliAttuali = transform.localEulerAngles;
        rotazioneAperta = Quaternion.Euler(angoliAttuali.x, rotazioneApertaY, angoliAttuali.z);
    }

    void Update()
    {
        if (isOpen)
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, rotazioneAperta, Time.deltaTime * velocitaApertura);
        }
    }

    public void TentaApertura()
    {
        Debug.Log("Hai cliccato la porta!");
        
        if (GameManager.instance != null && GameManager.instance.taskAttuale == GameManager.Reparto.Produzione)
        {
            Debug.Log("<color=orange>La porta è bloccata. Ascolta prima la Produzione e prova la radio!</color>");
        }
        else
        {
            isOpen = true;
            Debug.Log("<color=green>Apertura porta in corso verso -120 Y...</color>");
            
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }
    }
}