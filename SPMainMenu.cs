using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SPMainMenu : MonoBehaviour
{
    
	public void PlayGame(){
		SceneManager.LoadScene("JoshTest");
	}

	public void QuitGame(){
		Debug.Log("GAME QUIT");
		Application.Quit();
	}

}
