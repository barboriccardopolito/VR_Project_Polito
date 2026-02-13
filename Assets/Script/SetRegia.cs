using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GestoreRecitazione : MonoBehaviour
{
    [Header("Attori")]
    public Animator animatoreAttore;
    public Animator animatoreAttrice;
    public AudioSource audioAttore;
    public AudioSource audioAttrice;

    [Header("Oggetti Scenici")]
    public Animator animatoreTappo;

    [Header("Configurazione Dialogo")]
    public List<Battuta> copione; 
    
    [Header("Nomi Animazioni (Case Sensitive)")]
    public string nomeStatoAnimazione = "Talking";
    public string nomeStatoTappo = "Take 001";

    private Coroutine coroutineDialogo;
    private bool inLoop = false;

    [System.Serializable]
    public struct Battuta
    {
        public enum ChiParla { Attore, Attrice }
        public ChiParla chi;
        public AudioClip clipAudio;
        public float pausaDopo; 
    }

    public void AvviaLoopRecitazione()
    {
        inLoop = true;
        RiavviaTutto();
    }

    public void AvviaCiakUnico()
    {
        inLoop = false; 
        RiavviaTutto();
    }

    public void FermaTutto()
    {
        inLoop = false;
        if (coroutineDialogo != null) StopCoroutine(coroutineDialogo);
        if (audioAttore) audioAttore.Stop();
        if (audioAttrice) audioAttrice.Stop();
        
        if (animatoreTappo) animatoreTappo.speed = 0; 
    }

    private void RiavviaTutto()
    {
        if (coroutineDialogo != null) StopCoroutine(coroutineDialogo);
        if (audioAttore) audioAttore.Stop();
        if (audioAttrice) audioAttrice.Stop();
        
        // Attori
        if (animatoreAttore) {
            animatoreAttore.speed = 1; 
            animatoreAttore.Play(nomeStatoAnimazione, 0, 0f);
        }
        if (animatoreAttrice) {
            animatoreAttrice.speed = 1;
            animatoreAttrice.Play(nomeStatoAnimazione, 0, 0f);
        }

        if (animatoreTappo) {
            animatoreTappo.speed = 1; // Assicuriamoci che si muova
            animatoreTappo.Play(nomeStatoTappo, 0, 0f); // Reset a 0
        }

        if (RegiaManager.instance.previewInCorso) inLoop = true; 
        
        coroutineDialogo = StartCoroutine(EseguiCopione());
    }

    IEnumerator EseguiCopione()
    {
        foreach (Battuta battuta in copione)
        {
            AudioSource sourceAttuale = (battuta.chi == Battuta.ChiParla.Attore) ? audioAttore : audioAttrice;

            if (sourceAttuale != null && battuta.clipAudio != null)
            {
                sourceAttuale.clip = battuta.clipAudio;
                sourceAttuale.Play();
                yield return new WaitForSeconds(battuta.clipAudio.length + battuta.pausaDopo);
            }
        }

        if (inLoop)
        {
            RiavviaTutto();
        }
    }
}