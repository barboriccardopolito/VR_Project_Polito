using UnityEngine;
using System.Collections;

public class FocusLavagna : MonoBehaviour
{
    [Header("Telecamere")]
    [Tooltip("La telecamera fissa posizionata davanti alla lavagna")]
    public Camera cameraLavagna;
    [Tooltip("La telecamera normale del giocatore (se lasci vuoto la trova da solo)")]
    public Camera cameraGiocatore;

    [Header("Giocatore")]
    public GameObject giocatore;
    public string[] nomiScriptDaDisabilitare;

    public bool isFocusAttivo = false; 

    void Start()
    {
        if (cameraGiocatore == null) 
            cameraGiocatore = Camera.main;

        if (cameraLavagna != null)
        {
            if (cameraLavagna.GetComponent<AudioListener>() == null)
                cameraLavagna.gameObject.AddComponent<AudioListener>();

            cameraLavagna.gameObject.SetActive(false);
            AudioListener al = cameraLavagna.GetComponent<AudioListener>();
            if (al != null) al.enabled = false;
        }
    }

    public void AvviaInquadratura()
    {
        if (!isFocusAttivo) StartCoroutine(SequenzaLavagna());
    }

    IEnumerator SequenzaLavagna()
    {
        isFocusAttivo = true;

        if (cameraLavagna == null)
        {
            Debug.LogError("ERRORE GRAVE: Non hai assegnato la Camera_Lavagna nello script FocusLavagna!");
            isFocusAttivo = false;
            yield break;
        }

        BloccaGiocatore(true);

        if (cameraGiocatore != null) cameraGiocatore.gameObject.SetActive(false);
        
        cameraLavagna.gameObject.SetActive(true);
        AudioListener al = cameraLavagna.GetComponent<AudioListener>();
        if (al != null) al.enabled = true;

        yield return new WaitForSeconds(0.5f);

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.E));

        cameraLavagna.gameObject.SetActive(false);
        if (al != null) al.enabled = false;
        
        if (cameraGiocatore != null) cameraGiocatore.gameObject.SetActive(true);

        yield return new WaitUntil(() => !Input.GetKey(KeyCode.E));
        yield return new WaitForSeconds(0.1f);

        BloccaGiocatore(false);
        isFocusAttivo = false;
    }

    void BloccaGiocatore(bool blocca)
    {
        if (giocatore == null) return;

        if (nomiScriptDaDisabilitare != null)
        {
            foreach (string nomeScript in nomiScriptDaDisabilitare)
            {
                MonoBehaviour sPlayer = giocatore.GetComponent(nomeScript) as MonoBehaviour;
                if (sPlayer != null) sPlayer.enabled = !blocca;
                
                if (cameraGiocatore != null)
                {
                    MonoBehaviour sCam = cameraGiocatore.GetComponent(nomeScript) as MonoBehaviour;
                    if (sCam != null) sCam.enabled = !blocca;
                }
            }
        }

        CharacterController cc = giocatore.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = !blocca;
        
        if (blocca) 
        { 
            Cursor.lockState = CursorLockMode.Locked; 
            Cursor.visible = false; 
        }
    }
}