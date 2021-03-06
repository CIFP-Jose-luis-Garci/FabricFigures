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

    

    


    private void Start()
    {
        sfxSlider.value = GameManager.sfxVolumen;
        musicSlider.value = GameManager.musicVolumen;
        volumeSlider.value = GameManager.volumeVolumen;

        
        

    }

    private void Update()
    {
        
    }


    public void SetMusicLvl(float musicVol)
    {
        masterMixer.SetFloat("musicVol", musicVol);

        PlayerPrefs.SetFloat("musicVol", musicVol);
        GameManager.musicVolumen = PlayerPrefs.GetFloat("musicVol");

        

    }

    public void SetSfxLvl(float sfxVol)
    {

        masterMixer.SetFloat("sfxVol", sfxVol);
        PlayerPrefs.SetFloat("sfxVol", sfxVol);
        GameManager.sfxVolumen = PlayerPrefs.GetFloat("sfxVol");

        
    }

    public void SetVolumeLvl(float volVol)
    {
        masterMixer.SetFloat("volVol", volVol);

        PlayerPrefs.SetFloat("volVol", volVol);
        GameManager.volumeVolumen = PlayerPrefs.GetFloat("volVol");

        

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
