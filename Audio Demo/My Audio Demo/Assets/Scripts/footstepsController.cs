using UnityEngine;
using System.Collections;

public class footstepsController : MonoBehaviour 
	{

	[SerializeField] private AudioClip[] clips;
	private AudioSource source;
	private AudioClip theClip;

	// Use this for initialization
	void Start () {
	
		source = GetComponent<AudioSource>();

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void PlayFootstepSound()
	{
		int x = Random.Range(1,4);
		theClip = clips[x];
		source.PlayOneShot(theClip);
	}

    }
