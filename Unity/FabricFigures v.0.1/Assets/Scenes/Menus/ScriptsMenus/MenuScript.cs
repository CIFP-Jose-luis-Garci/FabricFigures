using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MenuScript : MonoBehaviour
{
    //Audio

    [SerializeField] Slider volumeSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;

    [SerializeField] AudioMixer masterMixer;

    //Text

    [SerializeField] Text volumeText;
    [SerializeField] Text musicText;
    [SerializeField] Text sfxText;

    bool reset;


    private void Start()
    {
        sfxSlider.value = GameManager.sfxVolumen;
        musicSlider.value = GameManager.musicVolumen;
        volumeSlider.value = GameManager.volumeVolumen;

        /*musicText.text = musicSlider.value.ToString();
        sfxText.text = sfxSlider.value.ToString();
        volumeText.text = volumeSlider.value.ToString();*/
        reset = false;

    }

    private void Update()
    {
        HudText();
    }


    public void SetMusicLvl(float musicVol)
    {
        masterMixer.SetFloat("musicVol", musicVol);

        PlayerPrefs.SetFloat("musicVol", musicVol);
        GameManager.musicVolumen = PlayerPrefs.GetFloat("musicVol");

        if (reset)
        {
            GameManager.musicVolumen = -5f;
            reset = false;

        }

    }

    public void SetSfxLvl(float sfxVol)
    {

        masterMixer.SetFloat("sfxVol", sfxVol);
        PlayerPrefs.SetFloat("sfxVol", sfxVol);
        GameManager.sfxVolumen = PlayerPrefs.GetFloat("sfxVol");

        if (reset)
        {
            GameManager.sfxVolumen = -5f;
            reset = false;

        }
    }

    public void SetVolumeLvl(float volVol)
    {
        masterMixer.SetFloat("volVol", volVol);

        PlayerPrefs.SetFloat("volVol", volVol);
        GameManager.volumeVolumen = PlayerPrefs.GetFloat("volVol");

        if (reset)
        {
            GameManager.volumeVolumen = -2f;
            reset = false;
        }

    }

    public void Reset()
    {
       reset = true;
    }

    public void HudText()
    {
        
        musicText.text = musicSlider.value.ToString();
        sfxText.text = sfxSlider.value.ToString();
        volumeText.text = volumeSlider.value.ToString();



    }

    




    public void CargarEscena(int escena)
    {
        SceneManager.LoadScene(escena);

    }

    public void ExitGame()
    {

        Application.Quit();

    }

}
