using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable] public class footstepClipsOfOneMaterial
{
	//[SerializeField] private string materialName;
	public PhysicMaterial Material;
	public AudioClip[] Clips;
}

public class footstepsController : MonoBehaviour 
{
	public PhysicMaterial theCollidedMaterial;
	//[SerializeField] private footstepClipsOfOneMaterial[] MaterialList;
	public List<footstepClipsOfOneMaterial> MaterialList;
	private AudioSource source;
	private AudioClip[] theClips;
	private AudioClip theClip;


	// Use this for initialization
	void Start ()
	{
		source = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	/*
	void Update()
	{
		RaycastHit hit;
		Ray GroundDetectorRay = new Ray(transform.position, Vector3.down);
		if(Physics.Raycast(GroundDetectorRay, out hit, 5f))
		{
			if(hit.collider.gameObject.tag == "Ground")
			{
				theCollidedMaterial = hit.collider.material;
			}
		}
	}
	*/

	public AudioClip[] GetTheClips(PhysicMaterial p)
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
		source.PlayOneShot(theClip);
	}

}