using UnityEngine;
using System.Collections;
namespace UnityStandardAssets.Characters.FirstPerson
{
	public class NewBehaviourScript : MonoBehaviour {

		public AudioSource clip;
		public bool win;
		void OnTriggerEnter(Collider other){
			if (other.tag == "Player") {
				GameObject ctrl = other.gameObject;
				int old_score = ctrl.GetComponent<RigidbodyFirstPersonController>().getScore();
				float old_time=  ctrl.GetComponent<RigidbodyFirstPersonController>().getTime ();
				if (PlayerPrefs.GetInt ("highscore") < old_score) {
					PlayerPrefs.GetInt ("highscore", old_score);
				}

				if (win) {
					if (PlayerPrefs.GetInt ("highscore") < old_score) {
						PlayerPrefs.GetFloat ("besttime", old_time);
					}
				} else {
					clip.PlayOneShot (clip.clip);
				}
				Application.LoadLevel (0);
				Cursor.lockState = CursorLockMode.None;
			}
		}
	}
}
