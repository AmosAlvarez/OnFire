using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generador : MonoBehaviour {

	public GameObject[] obj;
	public float tiempoMin = 1.25f;
	public float tiempoMax = 2.5f;

	// Use this for initialization
	void Start () {
		//Generar ();
		NotificationCenter.DefaultCenter().AddObserver(this,"PersonajeEmpiezaACorrer");
	}

	void PersonajeEmpiezaACorrer (Notification notification){

		Generar ();
	}

	// Update is called once per frame
	void Update () {
		
	}

	void Generar(){
		
		Instantiate (obj[Random.Range(0, obj.Length)], transform.position, Quaternion.identity);
		Invoke("Generar", Random.Range (tiempoMin,tiempoMax));
	}
}
