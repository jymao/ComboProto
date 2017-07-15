using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Impact : MonoBehaviour {

    private float startTime;
    private float lifeTime = 0.25f;

	// Use this for initialization
	void Start () {
        startTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		if(Time.time - startTime > lifeTime)
        {
            Destroy(gameObject);
        }
	}
}
