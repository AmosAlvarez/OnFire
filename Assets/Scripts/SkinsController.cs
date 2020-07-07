using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinsController : MonoBehaviour
{

    GameController gameController;

    public List<GameObject> pjs;

    // Start is called before the first frame update
    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();

		for (int i = 0; i < GameObject.Find("GameController").GetComponent<GameController>().pjsDesbloqueados.Count; i++)
		{
			for (int j = 0; j < pjs.Count; j++)
			{
				if (GameObject.Find("GameController").GetComponent<GameController>().pjsDesbloqueados[i] == pjs[j].GetComponent<Personaje>().posicionEnArray)
				{
					pjs[j].GetComponent<Personaje>().desbloqueado = true;
				}
			}
		}
	}

}
