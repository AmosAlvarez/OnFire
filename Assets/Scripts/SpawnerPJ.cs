using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerPJ : MonoBehaviour
{
    private GameController gameController;

    private void Awake()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        Instantiate(gameController.personajes[gameController.personajeSeleccionado], transform.position, Quaternion.identity);
    }

}
