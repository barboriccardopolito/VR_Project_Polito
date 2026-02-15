using UnityEngine;

public class GestoreInizioPartita : MonoBehaviour
{
    [Header("Script da Bloccare (es: PlayerMovement)")]
    public string[] nomiScriptMovimento; 

    [Header("Riferimenti Giocatore")]
    public CharacterController controller;

    [Header("Posizione In Piedi")]
    [Tooltip("Un oggetto vuoto (Empty) posizionato vicino alla sedia dove il giocatore apparirà una volta alzato")]
    public Transform puntoDiRilascio;

    private bool miSonoAlzato = false;

    void Start()
    {
        // Appena parte il gioco, spegniamo le gambe!
        BloccaMovimento(true);
    }

    void Update()
    {
        // Se la task della Produzione è finita (il GameManager è passato al prossimo reparto)
        if (!miSonoAlzato && GameManager.instance != null && GameManager.instance.taskAttuale != GameManager.Reparto.Produzione)
        {
            Alzati();
        }
    }

    void Alzati()
    {
        miSonoAlzato = true;

        // Ti teletrasporta al punto in piedi (se lo hai assegnato)
        if (puntoDiRilascio != null && controller != null)
        {
            controller.enabled = false; 
            transform.position = puntoDiRilascio.position;
            controller.enabled = true;
        }

        // Riaccende le gambe!
        BloccaMovimento(false);
        
        Debug.Log("<color=cyan>Ti sei alzato dalla sedia! Ora vai alla porta.</color>");
    }

    void BloccaMovimento(bool blocca)
    {
        if (nomiScriptMovimento != null)
        {
            foreach (string nomeScript in nomiScriptMovimento)
            {
                MonoBehaviour scriptDaBloccare = GetComponent(nomeScript) as MonoBehaviour;
                if (scriptDaBloccare != null)
                {
                    scriptDaBloccare.enabled = !blocca;
                }
            }
        }
    }
}