using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    //VARIABLES

    //Components
    PlayerManager playerManager;

    //Sprites & text
    [SerializeField] Image fadeScreen;
    int fadeSpeed = 5;
    public Sprite[] healthbarArray;
    [SerializeField] Text currencyText;
    public Image healthbar;

    void Start()
    {
        playerManager = gameObject.GetComponent<PlayerManager>();
    }

    public void HealthBarUpdate()
    {
        if (playerManager.currHealth > healthbarArray.Length)
            return;

        healthbar.sprite = healthbarArray[playerManager.currHealth];
    }

    public void CurrencyUpdate()
    {
        currencyText.text = playerManager.charCurrency.ToString();
    }

    public IEnumerator FadeToBlack(bool fadeIn)
    {
        Color objectColor = fadeScreen.color;

        if (fadeIn)
        {
            while (fadeScreen.color.a > 0)
            {
                float fadeAmount = objectColor.a - (fadeSpeed * Time.deltaTime);

                objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
                fadeScreen.color = objectColor;
                yield return null;
            }
        }

        else
        {
            while (fadeScreen.color.a < 1)
            {
                float fadeAmount = objectColor.a + (fadeSpeed * Time.deltaTime);

                objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
                fadeScreen.color = objectColor;
                yield return null;
            }
        }
    }
}
