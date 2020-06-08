using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BotonSkins : MonoBehaviour
{
    private Text textoBoton;
    public int personajeAActivar;

    public void ActivarSkin()
    {
        textoBoton = GameObject.Find("TextoBotonSkins").GetComponent<Text>();

        if (textoBoton.text == "SELECT")
        {
            GameObject.Find("GameController").GetComponent<GameController>().personajeSeleccionado = personajeAActivar;
            SceneManager.LoadScene("Menu");
        }
        
    }
}
