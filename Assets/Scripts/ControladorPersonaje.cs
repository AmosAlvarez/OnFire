using UnityEngine;
using System.Collections;
using FIMSpace.Jiggling;
using UnityEngine.UI;

public class ControladorPersonaje : MonoBehaviour {

	public float fuerzaSalto = 100f;
	public float fuerzaCaida = 100f;

	private bool enSuelo = true;
	public Transform comprobadorSuelo;
	public Transform comprobadorSueloDos;
	public LayerMask mascaraSuelo;

	private bool dobleSalto = false;

	private Animator animator;

	private bool corriendo = false;
	private bool cayendo = false;
	public float velocidad = 12f;

	Rigidbody2D rb;

	private GameController gameController;

	FJiggling_Simple jigg;

	public bool caida;
	public bool dash;
	public bool lento;
	public bool xxx;

	void Awake(){
		animator = GetComponent<Animator>();
		gameController = GameObject.Find("GameController").GetComponent<GameController>();
		
	}

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody2D> ();
		jigg = GameObject.Find("PJ(Clone)").GetComponent<FJiggling_Simple>();
	}

	void FixedUpdate(){
		if(corriendo)
		{
			rb.velocity = new Vector2(velocidad, rb.velocity.y);
		}
		animator.SetFloat("VelX", rb.velocity.x);
		enSuelo = Physics2D.OverlapArea(comprobadorSuelo.position, comprobadorSueloDos.position, mascaraSuelo);
		animator.SetBool("isGrounded", enSuelo);
		if(enSuelo){
			dobleSalto = false;
			cayendo = false;
		}

		if ((corriendo) && (!cayendo)&&jumpAllowed)
		{
			cayendo = true;
			rb.velocity = new Vector2(rb.velocity.x, -fuerzaCaida);
		}
	}
	
	// Update is called once per frame
	void Update () {

		//ACTIVAR PARA VERSIÓN PC (USO CON RATÓN)
		/*
		if (Input.GetMouseButtonDown(0))
		{
			Salto();
		}

		if (Input.GetMouseButtonDown(1))
		{
			Caida();
		}
		*/
		SwipeCheck();
	}

	public void Salto()
	{
		if (corriendo)
		{
			// Hacemos que salte si puede saltar
			if (enSuelo || !dobleSalto)
			{
				rb.velocity = new Vector2(rb.velocity.x, fuerzaSalto);
				jigg.StartJiggle();
				//rigidbody2D.AddForce(new Vector2(0, fuerzaSalto));
				if (!dobleSalto && !enSuelo)
				{
					jigg.StartJiggle();
					dobleSalto = true;
				}
			}
		}
		else
		{
			corriendo = true;
			//Scroll.enMovimiento = true;
			NotificationCenter.DefaultCenter().PostNotification(this, "PersonajeEmpiezaACorrer");
		}
	}

	public void Caida()
	{
		if ((corriendo) && (!cayendo))
		{
			cayendo = true;
			rb.velocity = new Vector2(rb.velocity.x, -fuerzaCaida);
			//rb.AddForce (new Vector2(rb.velocity.x, 0));
		}
	}

	public void ComprobarVelocidad()
	{
		if (gameController.puntuacion <= 50)
		{
			velocidad = 10f;
		}
		else if (gameController.puntuacion > 50 && gameController.puntuacion <= 100)
		{
			velocidad = 11f;
		}
		else if (gameController.puntuacion > 100 && gameController.puntuacion <= 150)
		{
			velocidad = 12f;
		}
		else if (gameController.puntuacion > 150 && gameController.puntuacion <= 200)
		{
			velocidad = 13f;
		}
		else
		{
			velocidad = 14f;
		}
	}

	private Vector2 startTouchPosition, endTouchPosition;
	private bool jumpAllowed = false;

	private void SwipeCheck()
	{
		if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
		{
			startTouchPosition = Input.GetTouch(0).position;
		}
		if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
		{
			endTouchPosition = Input.GetTouch(0).position;

			if (endTouchPosition.y < startTouchPosition.y)
			{
				jumpAllowed = true;
			}
		}
	}
}
