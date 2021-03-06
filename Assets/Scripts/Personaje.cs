﻿using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
//using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.UI;

public class Personaje : MonoBehaviour
{
    public Sprite spriteNuevo;
    public Sprite spriteNuevoSleccionado;
    public Sprite spriteSilueta;
    private Image image;
    public int precio;
    public bool desbloqueado;
    public bool seleccionado;

    private Text textoBoton;

    PersonajeGrande personajeGrande;

    public int posicionEnArray;

    GameController gameController;

  

    //GameObject personaje;

    private void Start()
    {
        personajeGrande = GameObject.Find("PJGrande").GetComponent<PersonajeGrande>();

        image = this.gameObject.GetComponent<Image>();

        Activar();

        textoBoton = GameObject.Find("TextoBotonSkins").GetComponent<Text>();

        gameController = GameObject.Find("GameController").GetComponent<GameController>();
    }

    public void Clickado()
    {
        gameController.pjSeleccionado = this.gameObject;

        if (desbloqueado)
        {
            personajeGrande.image.sprite = spriteNuevo;
            
            SpriteState spriteState;
            spriteState = this.gameObject.GetComponent<Button>().spriteState;
            spriteState.selectedSprite = spriteNuevoSleccionado;
            this.gameObject.GetComponent<Button>().spriteState = spriteState;
            GameObject.Find("Botón Unlock").GetComponent<BotonSkins>().personajeAActivar = posicionEnArray;
            textoBoton.text = "SELECT";

        }
        else
        {
            
            personajeGrande.image.sprite = spriteSilueta;
            textoBoton.text = "UNLOCK";
            
        }

        
    }

    public void Activar()
    {
        if (desbloqueado)
        {
            image.sprite = spriteNuevo;
        }
    }
}
