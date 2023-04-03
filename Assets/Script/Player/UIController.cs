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

    public void SetScore(int value) => scoreText.text = value.ToString();

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
}
