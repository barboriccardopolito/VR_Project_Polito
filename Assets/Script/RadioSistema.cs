using UnityEngine;
using TMPro;
using System.Collections;

public class RadioSistema : MonoBehaviour
{
    [Header("Impostazioni")]
    public bool haLaRadio = false; // Diventa true dopo aver parlato con la Produzione
    public GameObject fumettoUI;   // Il pannello/nuvoletta che contiene il testo
    public TextMeshProUGUI testoRadio; // Il testo dentro la nuvoletta
    public float durataMessaggio = 5f;

    private bool messaggioInCorso = false;

    void Update()
    {
        // Se premo R (o il tasto che preferisci) e ho la radio
        if (Input.GetKeyDown(KeyCode.R) && haLaRadio)
        {
            ChiediSuggerimento();
        }
    }

    public void ChiediSuggerimento()
    {
        if (messaggioInCorso) return; // Non sovrapporre i messaggi

        // 1. Chiediamo al CERVELLO (GameManager) cosa dire
        string messaggio = GameManager.instance.OttieniSuggerimentoRadio();

        // 2. La radio (BOCCA) lo mostra
        StartCoroutine(MostraMessaggioRoutine(messaggio));
    }

    IEnumerator MostraMessaggioRoutine(string testo)
    {
        messaggioInCorso = true;
        
        if (fumettoUI != null) fumettoUI.SetActive(true);
        if (testoRadio != null) testoRadio.text = testo;

        // Suono bip radio (opzionale)
        // AudioSource.PlayClipAtPoint(suonoRadio, transform.position);

        yield return new WaitForSeconds(durataMessaggio);

        if (fumettoUI != null) fumettoUI.SetActive(false);
        if (testoRadio != null) testoRadio.text = "";

        messaggioInCorso = false;
    }
}