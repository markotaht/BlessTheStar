using UnityEngine;
using System.Collections;

public class GrammoforLeverHandler : MonoBehaviour
{
    private AudioSource m_audioSource;
    
	// Use this for initialization
	void Start ()
	{
	    m_audioSource = GetComponent<AudioSource>();

	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider other)
    {
        AudioSource grammofonAudioSource = GameObject.FindGameObjectWithTag("Grammofon").GetComponent<AudioSource>();

        
        grammofonAudioSource.PlayOneShot(grammofonAudioSource.clip);
    }
}
