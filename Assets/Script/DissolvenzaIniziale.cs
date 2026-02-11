using UnityEngine;
using UnityEngine.UI; 
using System.Collections;

public class DissolvenzaIniziale : MonoBehaviour
{
    public Image pannelloNero; // Assicurati di collegarlo nell'Inspector!
    public float durata = 3.0f;

    void Awake() 
    {
        // 1. Appena l'oggetto si sveglia, FORZA il nero totale
        if (pannelloNero != null)
        {
            pannelloNero.gameObject.SetActive(true);
            pannelloNero.color = new Color(0, 0, 0, 1); // Nero Opaco (Alpha 1)
        }
    }

    void Start()
    {
        if (pannelloNero != null)
        {
            StartCoroutine(FaiDissolvenza());
        }
    }

    IEnumerator FaiDissolvenza()
    {
        // (Opzionale) Aspetta mezzo secondo al buio per stabilizzare la scena
        yield return new WaitForSeconds(0.5f);

        float timer = 0f;
        Color c = pannelloNero.color; // Prendiamo il colore attuale (nero)

        // 2. CICLO DI DISSOLVENZA (La parte che mancava)
        while (timer < durata)
        {
            timer += Time.deltaTime;
            
            // Calcola l'Alpha: va da 1 (nero) a 0 (trasparente) man mano che il tempo passa
            float nuovoAlpha = Mathf.Lerp(1f, 0f, timer / durata);
            
            c.a = nuovoAlpha;
            pannelloNero.color = c;
            
            yield return null; // Aspetta il frame successivo
        }

        // 3. SPEGNIMENTO FINALE
        // Assicuriamoci che sia trasparente al 100%
        c.a = 0f;
        pannelloNero.color = c;
        
        // Disattiva l'oggetto per liberare i click del mouse
        pannelloNero.gameObject.SetActive(false); 
        Debug.Log("Pannello Nero SPENTO"); 
    }
}