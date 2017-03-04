using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundGeneration : MonoBehaviour {
    public AudioSource hit;
    public AudioClip playOnHit;

	// Use this for initialization
	void Start () {
		
	}

    // Update is called once per frame
    void FixedUpdate() {
        Vector3 fwd = transform.TransformDirection(Vector3.forward);
        Vector3 forward = transform.TransformDirection(Vector3.forward) * 10;
        Debug.DrawRay(transform.position, forward, Color.green);
        if (Physics.Raycast(transform.position, fwd, 10))
        {
            hit.clip = playOnHit;
            hit.Play();

        }
    }



    private IEnumerator WaitForSound(float waitTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime);
        }
    }


}
