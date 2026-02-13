using UnityEngine;
using UnityEngine.UI; 
using System.Collections;

public class DissolvenzaIniziale : MonoBehaviour
{
    public Image pannelloNero;
    public float durata = 3.0f;

    void Awake() 
    {
        if (pannelloNero != null)
        {
            pannelloNero.gameObject.SetActive(true);
            pannelloNero.color = new Color(0, 0, 0, 1);
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
        yield return new WaitForSeconds(0.5f);

        float timer = 0f;
        Color c = pannelloNero.color;

        while (timer < durata)
        {
            timer += Time.deltaTime;
            
            float nuovoAlpha = Mathf.Lerp(1f, 0f, timer / durata);
            
            c.a = nuovoAlpha;
            pannelloNero.color = c;
            
            yield return null;
        }

        c.a = 0f;
        pannelloNero.color = c;
        
        pannelloNero.gameObject.SetActive(false); 
        Debug.Log("Pannello Nero SPENTO"); 
    }
}