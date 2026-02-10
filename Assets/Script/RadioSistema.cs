using UnityEngine;

public class RadioSistema : MonoBehaviour
{
    public bool haLaRadio = false;

    [Header("Oggetto Fisico sul Player")]
    public GameObject radioAddossoAlPlayer; // Trascina qui la Radio che è figlia del Player

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip suonoRicezione;

    public void RiceviRadio()
    {
        if (haLaRadio) return;

        haLaRadio = true;

        // 1. Accendi la radio che il player ha addosso
        if (radioAddossoAlPlayer != null) 
            radioAddossoAlPlayer.SetActive(true);

        // 2. Suono
        if (audioSource != null && suonoRicezione != null)
            audioSource.PlayOneShot(suonoRicezione);

        Debug.Log("<color=green>[SYSTEM]</color> Radio ricevuta!");
    }
}