using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plataforma : MonoBehaviour {

	private bool haColisionadoConElJugador = false;
	public int puntos;
	GameController pollo;

	ControladorPersonaje pj;

	private void Awake()
	{
		//pj = GameObject.Find("PJ").GetComponent<ControladorPersonaje>();
	}

	// Use this for initialization
	void Start () {
		pollo = GameObject.Find("GameController").GetComponent<GameController> ();
		pj = GameObject.Find("PJ(Clone)").GetComponent<ControladorPersonaje>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnCollisionEnter2D (Collision2D col){

		if (!haColisionadoConElJugador && col.gameObject.tag == "Player") {
			haColisionadoConElJugador = true;
			pollo.puntuacion = pollo.puntuacion + puntos;
			pollo.ActualizarMarcador();
			pj.ComprobarVelocidad();
		}
	}
}
