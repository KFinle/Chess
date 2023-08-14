using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject UIParent;
    [SerializeField] private Text resultText;

    public void HideUI()
    {
        UIParent.SetActive(false);
    }

    /// <summary>
    /// Display end game screen
    /// </summary>
    /// <param name="winner"></param>
    public void OnGameFinished(string winner)
    {
        UIParent.SetActive(true);
        resultText.text = (winner + " WINS!");
    }


    public void OnGameStalemate()
    {
        UIParent.SetActive(true);
        resultText.text = ("STALEMATE");

    }

}
