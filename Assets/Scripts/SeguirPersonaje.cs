using UnityEngine;
using System.Collections;

public class SeguirPersonaje : MonoBehaviour {

	private Transform personaje;
	public float separacion = 6f;

	private void Start()
	{
		personaje = GameObject.Find("PJ(Clone)").GetComponent<ControladorPersonaje>().transform;
	}

	// Update is called once per frame
	void Update () {
		transform.position = new Vector3(personaje.position.x+separacion, transform.position.y, transform.position.z);
	}
}
