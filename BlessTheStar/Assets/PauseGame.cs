﻿using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;

public class PauseGame : MonoBehaviour {

	public Transform canvas;
	public Transform player;
	public Text score;

	private int highScore;
	private float bestTime;


	void Start () {
		Time.timeScale = 0;
		AudioListener.pause = true;
		highScore = PlayerPrefs.GetInt ("highscore");
		bestTime = PlayerPrefs.GetFloat ("besttime");
		if (bestTime == 0f) {
			score.text = "Best score: -\nBest time: -";
		} else {
			score.text = "Best score: " + highScore.ToString () + "\nBest time: " + bestTime.ToString ();
		}
		Cursor.lockState = CursorLockMode.Confined;
		Cursor.visible = true;

	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape))
		{
			Pause ();
		}
	}

	public void Pause() {
		if (canvas.gameObject.activeInHierarchy == false)
		{
			StartPause ();
		} else {
			StopPause ();
		}
	}

	public void StartPause() {

		canvas.gameObject.SetActive (true);
		Time.timeScale = 0;
		player.GetComponent<RigidbodyFirstPersonController> ().enabled = false;
		Cursor.lockState = CursorLockMode.Confined;
		Cursor.visible = true;
		AudioListener.pause = true;
	}

	public void StopPause() {
		canvas.gameObject.SetActive (false);
		Time.timeScale = 1;
		player.GetComponent<RigidbodyFirstPersonController> ().enabled = true;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		AudioListener.pause = false;
	}

	public void Exit() {
		#if UNITY_EDITOR
		// set playmode to stop
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit ();
		#endif
	}

}
