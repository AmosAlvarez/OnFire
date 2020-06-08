using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class GameController : MonoBehaviour {

	public int puntuacion = 0;

	[SerializeField]
	int monedas = 0;

	Text marcador;

	Text marcadorMax;

	Text cantidadMonedas;

	public int puntuacionMaxima = 0;

	public static GameController gameController;

	private string rutaArchivo;

	public GameObject[] personajes;

	public int personajeSeleccionado = 0;

	private void Awake()
	{
		rutaArchivo = Application.persistentDataPath + "/datos.dat";

		if (gameController == null)
		{
			gameController = this;
			DontDestroyOnLoad(this.gameObject);
		}
		else if(gameController != this)
		{
			Destroy(this.gameObject);
		}
		
		ComprobarEscena();

		Cargar();
	}

	// Use this for initialization
	void Start () 
	{
		//ActualizarMarcador ();
	}

	// Update is called once per frame
	private void Update()
	{
		ComprobarEscena();
		Debug.Log(personajeSeleccionado);
	}

	public void ActualizarMarcador () {

		if (puntuacion < 0)
		{
			puntuacion = 0;
		}

		marcador.text = puntuacion.ToString ();
	}

	public void ComprobarEscena()
	{
		Debug.Log(SceneManager.GetActiveScene().name);

		if (SceneManager.GetActiveScene().name == "Main")
		{
			marcador = GameObject.Find("Marcador").GetComponent<Text>();
		}
		else if (SceneManager.GetActiveScene().name == "Menu")
		{
			marcadorMax = GameObject.Find("Puntos").GetComponent<Text>();
			marcadorMax.text = puntuacionMaxima.ToString();
			cantidadMonedas = GameObject.Find("Monedas").GetComponent<Text>();
			cantidadMonedas.text = monedas.ToString();
		}
	}

	public void Cargar()
	{
		if (File.Exists(rutaArchivo))
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(rutaArchivo, FileMode.Open);

			DatosAGuardar datos = (DatosAGuardar)bf.Deserialize(file);

			puntuacionMaxima = datos.puntuacionMaxima;

			file.Close();
		}
		else
		{
			puntuacionMaxima = 0;
		}
	}

	public void Guardar()
	{
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create(rutaArchivo);

		DatosAGuardar datos = new DatosAGuardar();
		datos.puntuacionMaxima = puntuacionMaxima;
		datos.monedillas = monedas;

		bf.Serialize(file, datos);

		file.Close();
	}
}

[Serializable]
class DatosAGuardar
{
	public int puntuacionMaxima;
	public int monedillas;
}
