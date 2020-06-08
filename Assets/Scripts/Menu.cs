using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour {

	public Button scoreButton;
	public Button rankButton;
	private GameController gameController;

	private void Awake()
	{
		gameController = GameObject.Find("GameController").GetComponent<GameController>();
	}

	private void Start()
	{
		if (gameController.puntuacionMaxima > 0)
		{
			scoreButton.interactable = true;
			rankButton.interactable = true;
		}
	}

	public void CargarEscena(string escena)
	{
		SceneManager.LoadScene (escena);
	}
}
