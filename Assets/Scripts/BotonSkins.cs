using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BotonSkins : MonoBehaviour
{
    private Text textoBoton;
    public int personajeAActivar;
    SkinsController skinsController;

    public void ActivarSkin()
    {
        textoBoton = GameObject.Find("TextoBotonSkins").GetComponent<Text>();

        if (textoBoton.text == "SELECT")
        {
            GameObject.Find("GameController").GetComponent<GameController>().personajeSeleccionado = personajeAActivar;
            SceneManager.LoadScene("Menu");
        }
        else if (textoBoton.text == "UNLOCK")
        {
            if (GameObject.Find("GameController").GetComponent<GameController>().pjSeleccionado.GetComponent<Personaje>().precio <= GameObject.Find("GameController").GetComponent<GameController>().monedas)
            {
                GameObject.Find("GameController").GetComponent<GameController>().monedas -= GameObject.Find("GameController").GetComponent<GameController>().pjSeleccionado.GetComponent<Personaje>().precio;
                GameObject.Find("GameController").GetComponent<GameController>().pjSeleccionado.GetComponent<Personaje>().desbloqueado = true;
                GameObject.Find("GameController").GetComponent<GameController>().pjSeleccionado.GetComponent<Personaje>().Activar();
                GameObject.Find("GameController").GetComponent<GameController>().pjsDesbloqueados.Add(GameObject.Find("GameController").GetComponent<GameController>().pjSeleccionado.GetComponent<Personaje>().posicionEnArray);
                GameObject.Find("GameController").GetComponent<GameController>().Guardar();
            }
            
        }
        
    }
}
