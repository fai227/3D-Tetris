using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Images")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Image holdImage;
    [SerializeField] private Image[] nextImages;

    [Header("Infomation")]
    [SerializeField] private Text numberText;
    [SerializeField] private Text informationText;

    private void Start()
    {
        // UI表示
        if (GameManager.gameMode == GameManager.GameMode.FourtyLines)
        {
            informationText.transform.parent.gameObject.SetActive(true);
            informationText.text = "Line Left";
            numberText.text = Option.LINE_OF_40_LINES.ToString();
        }
        else if (GameManager.gameMode == GameManager.GameMode.ScoreAttack)
        {
            informationText.transform.parent.gameObject.SetActive(true);
            informationText.text = "Time Left";
            numberText.text = Option.SCORE_ATTACK_TIME.ToString();
        }
        else
        {
            informationText.transform.parent.gameObject.SetActive(false);
        }
    }

    public void SetScore(int value) => scoreText.text = value.ToString("00000000");

    public void SetNext(int one, int two, int three, int four, int five)
    {
        foreach (Image nextImage in nextImages)
        {
            foreach (Transform n in nextImage.transform)
            {
                GameObject.Destroy(n.gameObject);
            }
        }
        Instantiate(MinoManager.instance.getMinoUI(five), nextImages[0].rectTransform);
        Instantiate(MinoManager.instance.getMinoUI(four), nextImages[1].rectTransform);
        Instantiate(MinoManager.instance.getMinoUI(three), nextImages[2].rectTransform);
        Instantiate(MinoManager.instance.getMinoUI(two), nextImages[3].rectTransform);
        Instantiate(MinoManager.instance.getMinoUI(one), nextImages[4].rectTransform);
    }

    public void SetHold(int id)
    {
        foreach (Transform n in holdImage.transform)
        {
            GameObject.Destroy(n.gameObject);
        }
        Instantiate(MinoManager.instance.getMinoUI(id), holdImage.rectTransform);
    }

    public void SetNumber(int number)
    {
        numberText.text = number.ToString();

        // 10以下で色を変える
        if (number < 10) numberText.color = Color.yellow;
    }
}
