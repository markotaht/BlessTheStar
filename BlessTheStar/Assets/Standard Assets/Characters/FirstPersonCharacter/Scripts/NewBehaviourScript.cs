using UnityEngine;
using System.Collections;
namespace UnityStandardAssets.Characters.FirstPerson
{
public class NewBehaviourScript : MonoBehaviour {

	public bool win;
	void OnTriggerEnter(Collider other){
		if (other.tag == "Player") {
			GameObject ctrl = other.gameObject;
			if (win) {
				PlayerPrefs.SetInt ("highscore", ctrl.GetComponent<RigidbodyFirstPersonController>().getScore());
					PlayerPrefs.SetFloat ("besttime", ctrl.GetComponent<RigidbodyFirstPersonController>().getTime ());
			}
			Application.LoadLevel (0);
			Cursor.lockState = CursorLockMode.None;
		}
	}
}
}
