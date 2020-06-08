using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class Llamita : MonoBehaviour {

	GameController pollo;
	Oscuridad osc;
	public int puntos;

	private OscuridadPost oscuro;

	Vignette vignette;

	ControladorPersonaje pj;

	private void Awake()
	{
		oscuro = GameObject.Find("Post-process Volume").GetComponent<OscuridadPost>();
		oscuro.volume.profile.TryGetSettings(out vignette);
		//pj = GameObject.Find("PJ").GetComponent<ControladorPersonaje>();
	}

	// Use this for initialization
	void Start () 
	{
		pj = GameObject.Find("PJ(Clone)").GetComponent<ControladorPersonaje>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter2D (Collider2D other){

		if (other.tag == "Player") {
			pollo = GameObject.Find("GameController").GetComponent<GameController>();
			Destroy (this.gameObject);
			pollo.puntuacion = pollo.puntuacion - puntos;
			pollo.ActualizarMarcador();
			pj.ComprobarVelocidad();
			vignette.intensity.value = 0f;
			//Debug.Log (pollo.puntuacion);
		}
	}
}
