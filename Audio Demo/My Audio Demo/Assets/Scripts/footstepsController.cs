using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable] public class footstepClipsOfOneMaterial
{
	public PhysicMaterial Material;
	[Range(0,1)] public float Volume = 1f;
	public AudioClip[] Clips;
}

[RequireComponent(typeof (Collider))]
[RequireComponent(typeof (AudioSource))]
public class footstepsController : MonoBehaviour 
{
	private PhysicMaterial theCollidedMaterial;
	[SerializeField] private string FloorTag;

	[Header("Randomization")]
	[SerializeField] [Range(0,1)] private float VolMin;
	[SerializeField] [Range(0,1)] private float VolMax;
	[SerializeField] private float PitchMin;
	[SerializeField] private float PitchMax;
	[Space(10)]

	public List<footstepClipsOfOneMaterial> MaterialList;
	private AudioSource source;
	private AudioClip[] theClips;
	private AudioClip theClip;
	private int randomNumber;
	private float clipsVol;

	void Start ()
	{
		source = GetComponent<AudioSource>();
	}

	void OnTriggerStay(Collider other)
	{
		if(other.gameObject.tag == FloorTag)
		{
			theCollidedMaterial = other.sharedMaterial;
		}
	}

	private AudioClip[] GetTheClips(PhysicMaterial p)
	{
		foreach (var footstepClipsOfOneMaterial in MaterialList)
		{
			if (footstepClipsOfOneMaterial.Material == p)
			{
				footstepClipsOfOneMaterial thelist = footstepClipsOfOneMaterial;
				clipsVol = thelist.Volume;
				return thelist.Clips;
			}
		}
		return null;
	}
		
	public void PlayFootstepSound()
	{
		theClips = GetTheClips(theCollidedMaterial);
		Randomization(VolMin, VolMax, PitchMin, PitchMax, theClips.Length);
		theClip = theClips[randomNumber];
		source.PlayOneShot(theClip, clipsVol);
	}

	private void Randomization(float volmin, float volmax, float pitchmin, float pitchmax, int arraylength)
	{
		source.volume = Random.Range(volmin, volmax);
		source.pitch = Random.Range(pitchmin, pitchmax);
		int lastnumber = randomNumber;
		while (randomNumber == lastnumber)
		{
			randomNumber = Random.Range(0, arraylength);
		}
	}

}
