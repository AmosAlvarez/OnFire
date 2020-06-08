using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Destructor : MonoBehaviour {

	public string escena;

	private GameController gameController;

	private void Awake()
	{
		gameController = GameObject.Find("GameController").GetComponent<GameController>();
	}

	void OnTriggerEnter2D (Collider2D other){
		
		if (other.tag == "Player") 
		{
			//Destroy (other.gameObject);

			if(gameController.puntuacion > gameController.puntuacionMaxima)
			{
				gameController.puntuacionMaxima = gameController.puntuacion;
				gameController.Guardar();
				Debug.Log("Puntuacion Max es: " + gameController.puntuacionMaxima);
				Debug.Log("Puntuacion es: " + gameController.puntuacion);
			}
			else
			{
				Debug.Log("Puntuacion maxima no superada");
			}

			NotificationCenter.DefaultCenter().PostNotification(this, "PersonajeHaMuerto");
			gameController.puntuacion = 0;
			Debug.Log("Puntuacion ahora es: " + gameController.puntuacion);
			SceneManager.LoadScene(escena);
			//HACER QUE SE MUESTRE LA PUNTUACION MAXIMA

		}
		if (other.tag == "Plataforma" || other.tag == "Llama") {

			//Debug.Log ("Hola");
			Destroy (other.gameObject);
		}
	}

	void OnCollisionEnter2D (Collision2D other){

		if (other.gameObject.CompareTag("Plataforma")){
			Destroy (other.gameObject);
		}
	}
}
