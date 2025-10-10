using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    Image levelButtonImage;
    [SerializeField] Sprite finishSprite;
    [SerializeField] Sprite notFinishSprite;
    [SerializeField] GameObject tickIcon;

    [SerializeField] int buttonId;
    int currentLevel;

    // Start is called before the first frame update
    void Start()
    {
        levelButtonImage = GetComponent<Image>();
        SetButtonImage();
    }

    public void SetButtonImage()
    {
        //currentLevel = PlayerPrefs.GetInt(StringManager.currentLevelId, 1);
        if (buttonId < PlayerPrefs.GetInt(StringManager.currentLevelId))
        {
            levelButtonImage.sprite = notFinishSprite;
            tickIcon.SetActive(true);
        }
        else
        {
            levelButtonImage.sprite = finishSprite;
            tickIcon.SetActive(false);
        }

        if (buttonId == 1)
        {
            if (PlayerPrefs.GetInt(StringManager.hasPlayLevel1) == 0)
            {
                tickIcon.SetActive(false);
            }
            else
            {
                tickIcon.SetActive(true);
            }
        }
    }
}