using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable] public class footstepClipsOfOneMaterial
{
	public PhysicMaterial Material;
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

	void Start ()
	{
		source = GetComponent<AudioSource>();
	}

	void OnTriggerEnter(Collider other)
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
				return thelist.Clips;
			}
		}
		return null;
	}
		
	public void PlayFootstepSound()
	{
		theClips = GetTheClips(theCollidedMaterial);
		int x = Random.Range(0, theClips.Length-1);
		theClip = theClips[x];
		Randomization(VolMin, VolMax, PitchMin, PitchMax);
		source.PlayOneShot(theClip);
	}

	private void Randomization(float volmin, float volmax, float pitchmin, float pitchmax)
	{
		source.volume = Random.Range(volmin, volmax);
		source.pitch = Random.Range(pitchmin, pitchmax);
	}

}
