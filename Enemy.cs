using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    public float maxSpeedX;
    public float maxSpeedY;

    private float launchForceX; //Force enemy is launched when hit by player
    private float launchForceY;

    private float prevVelocityX; //Remembers non-zero velocity before hitting a wall for bounce-back
    private float prevVelocityY;
    private bool hit = false;

    private Rigidbody2D rigidBody;

    public GameObject impact;

	// Use this for initialization
	void Start () 
    {
        rigidBody = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () 
    {
		if(rigidBody.velocity.x != 0)
        {
            prevVelocityX = rigidBody.velocity.x;
        }
        if(rigidBody.velocity.y != 0)
        {
            prevVelocityY = rigidBody.velocity.y;
        }
	}

    void FixedUpdate()
    {
        if(Mathf.Abs(rigidBody.velocity.x) > maxSpeedX)
        {
            rigidBody.velocity = new Vector2(Mathf.Sign(rigidBody.velocity.x) * maxSpeedX, rigidBody.velocity.y);
        }

        if(Mathf.Abs(rigidBody.velocity.y) > maxSpeedY)
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, Mathf.Sign(rigidBody.velocity.y) * maxSpeedY);
        }
    }

    //Enemy moves after delay from being hit
    private void ResumeMovement()
    {
        rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;
        rigidBody.AddForce(new Vector2(launchForceX, launchForceY));
        hit = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag == "PlayerAttack")
        {
            launchForceX = other.gameObject.GetComponent<PlayerAttack>().GetLaunchForceX();
            launchForceY = other.gameObject.GetComponent<PlayerAttack>().GetLaunchForceY();
            //freeze position when hit
            rigidBody.constraints = RigidbodyConstraints2D.FreezeAll;

            //average positions of colliders for approximate collision point
            //since ontrigger events don't provide collision data
            Vector3 hitPos = (other.gameObject.transform.position + transform.position) / 2;
            //impact graphic
            Instantiate(impact, hitPos, Quaternion.identity);
            Invoke("ResumeMovement", 0.25f);
        }
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        //Bounce back when hitting boundaries
        if(coll.gameObject.tag == "Wall")
        {
            rigidBody.velocity = new Vector2(-prevVelocityX / 2, rigidBody.velocity.y);
        }
        if(coll.gameObject.tag == "Ground" || coll.gameObject.tag == "Ceiling")
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, -prevVelocityY / 2);
        }
    }

    void OnCollisionStay2D(Collision2D coll)
    {
        //For preventing enemy from being stuck against the wall
        if(coll.gameObject.tag == "Wall")
        {
            if(hit && rigidBody.velocity.x == 0)
            {
                rigidBody.AddForce(new Vector2(-launchForceX / 2, launchForceY));
                hit = false;
            }
        }
    }
}
