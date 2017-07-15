using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour {

    private float launchForceX;
    private float launchForceY;
    private int damage;

    private float startTime;
    private float lifeTime;

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

    public void SetLaunchForce(float launchForceX, float launchForceY)
    {
        this.launchForceX = launchForceX;
        this.launchForceY = launchForceY;
    }

    public void SetDamage(int damage)
    {
        this.damage = damage;
    }

    public void SetLifeTime(float time)
    {
        lifeTime = time;
    }

    public float GetLaunchForceX()
    {
        return launchForceX;
    }
    public float GetLaunchForceY()
    {
        return launchForceY;
    }

    public float GetDamage()
    {
        return damage;
    }

}
