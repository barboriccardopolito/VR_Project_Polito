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
        // Piccola pausa di mezzo secondo al buio per stabilizzare la scena
        yield return new WaitForSeconds(0.5f);

        float timer = 0f;
        Color c = pannelloNero.color;

        while (timer < durata)
        {
            timer += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, timer / durata); // Scala l'Alpha da 1 a 0
            pannelloNero.color = c;
            yield return null;
        }
    pannelloNero.gameObject.SetActive(false);
    }
}