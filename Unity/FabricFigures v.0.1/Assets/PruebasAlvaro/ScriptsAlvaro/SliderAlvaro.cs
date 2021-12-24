using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderAlvaro : MonoBehaviour
{
    //Array que contendrá los sprites de la animación
    [SerializeField] Sprite[] spritesArray;
    //Sprite de la escena que cambiará y su renderer
    //Los arrastro en Unity
    [SerializeField] GameObject sprite;
    SpriteRenderer spriteRenderer;
    //Slider que hará cambiar los sprites
    Slider slider;
    
    void Start()
    {
        //Mediante código, accedo al componente Slider propio
        slider = GetComponent<Slider>();

        //Accedo al renderer del sprite
        spriteRenderer = sprite.GetComponent<SpriteRenderer>();
        //Pongo el primer elemento del array como sprite
        spriteRenderer.sprite = spritesArray[0];

        //Le doy al Slider un valor máximo con el nº de elementos del array menos uno
        slider.maxValue = spritesArray.Length - 1;
        //Me aseguro de que usa valores enteros
        slider.wholeNumbers = true;


    }

    //Este método lo tiene que llamar el slider cada vez que cambia su valor
    public void CambiarSprites()
    {
        //Obtengo el valor al que ha cambiado el slider
        //Lo tengo que convertir en entero ya que es un float por defecto
        int valor = (int)slider.value;
        //Le asigno al sprite que va a cambiar el sprite correspondiente del array
        spriteRenderer.sprite = spritesArray[valor];
    }
}
