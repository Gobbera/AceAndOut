using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameLog : MonoBehaviour
{
    private TMP_Text TMP_text;
    void Awake()
    {
        TMP_text = GetComponent<TMP_Text>();
    }

    public void changeText(string newText)
    {
        TMP_text.text = newText;
    }

    public void AnnouncePlayer(string playerName)
    {
        changeText("Vez de: " + playerName);
    }

    public IEnumerator StartCountdown()
    {   
        changeText("Iniciando o Jogo em 3");
        yield return new WaitForSeconds(1f);
        changeText("Iniciando o Jogo em 2");
        yield return new WaitForSeconds(1f);
        changeText("Iniciando o Jogo em 1");
        yield return new WaitForSeconds(1f);
    }
}
