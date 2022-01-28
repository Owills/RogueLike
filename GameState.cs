using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameState : MonoBehaviour
{
	public void ResetGame()
	{
		NetworkManager.singleton.ServerChangeScene(SceneManager.GetActiveScene().name);
		GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCharacter>().Reset();
	}
}
