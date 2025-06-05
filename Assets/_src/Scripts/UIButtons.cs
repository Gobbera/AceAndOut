using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIButtons : MonoBehaviour
{
    [SerializeField] private GameObject buttonObj;
    [SerializeField] public Button button;
    [SerializeField] public TMP_Text tmp_Text;
    void Awake()
    {
        button = buttonObj.GetComponent<Button>();
        tmp_Text = buttonObj.GetComponentInChildren<TMP_Text>();
        buttonObj.SetActive(false);
    }
    public void EnableButton()
    {
        buttonObj.SetActive(true);
    }
    public void DisableButton()
    {
        buttonObj.SetActive(false);
    }
    public void EnableInteractableButton()
    {
        button.interactable = true;
    }
    public void DisableInteractableButton()
    {
        button.interactable = false;
    }
    public void changeText(string newText)
    {
        tmp_Text.text = newText;
    }
}
