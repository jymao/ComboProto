using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public float moveForce;
    public float maxSpeed;
    public float jumpForce;
    public int power; //dmg to enemy health, not used yet

    private float horizInput;
    private Rigidbody2D rigidBody;
    private BoxCollider2D boxCollider;

    private bool faceLeft = false;
    private bool isAttacking = false;
    private bool inAir = true;
    private bool isJumping = false;

    private int jumpCounter = 0; //keep track of how many jumps done, for double jumping

    private SpriteRenderer spriteRend;
    private Sprite idleSprite;
    public Sprite runSprite;
    public Sprite atkUpSprite;
    public Sprite atkDownSprite;
    public Sprite atkSideSprite;

    public GameObject hitbox;

	// Use this for initialization
	void Start () 
    {
        rigidBody = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRend = GetComponent<SpriteRenderer>();
        idleSprite = spriteRend.sprite;
	}
	
	// Update is called once per frame
	void Update () 
    {
        horizInput = Input.GetAxisRaw("Horizontal");

        if(!isAttacking)
        {
            //Appropriate sprite for in air or on ground
            SetDefaultSprite();

            //Direction
            //turn left
            if (horizInput < 0 && !faceLeft)
            {
                RotateCharacter(true);
            }
            //turn right
            else if (horizInput > 0 && faceLeft)
            {
                RotateCharacter(false);
            }

            //Attack Input
            if(Input.GetKey(KeyCode.UpArrow))
            {
                if(Input.GetKeyDown(KeyCode.Z))
                {
                    StartCoroutine(SetUpAttack(false));
                }
                else if (Input.GetKeyDown(KeyCode.X))
                {
                    StartCoroutine(SetUpAttack(true));
                }
            }
            else if (Input.GetKey(KeyCode.LeftArrow) || (Input.GetKey(KeyCode.RightArrow)))
            {
                if(Input.GetKeyDown(KeyCode.Z))
                {
                    StartCoroutine(SetSideAttack(false));
                }
                else if (Input.GetKeyDown(KeyCode.X))
                {
                    StartCoroutine(SetSideAttack(true));
                }
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    StartCoroutine(SetDownAttack(false));
                }
                else if (Input.GetKeyDown(KeyCode.X))
                {
                    StartCoroutine(SetDownAttack(true));
                }
            }
            
            //Jump
            if(Input.GetKeyDown(KeyCode.Space) && jumpCounter < 2)
            {
                isJumping = true;
                jumpCounter++;
            }

            //Set gravity scale to make character more floaty
            if(Input.GetKey(KeyCode.Space))
            {
                rigidBody.gravityScale = 0.6f;
            }
            //Set gravity scale to make character fall faster
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                rigidBody.gravityScale = 5f;
            }
            else
            {
                rigidBody.gravityScale = 2f;
            }
            
        }

	}

    //Movement is handled here
    void FixedUpdate()
    {
        //Freeze horizontal movement if no input so no sliding
        if(horizInput == 0)
        {
            rigidBody.velocity = new Vector2(0, rigidBody.velocity.y);
        }

        //Add force to move sideways if not at max speed yet
        if (horizInput * rigidBody.velocity.x < maxSpeed)
        {
            rigidBody.AddForce(new Vector2(horizInput * moveForce, 0));
        }

        //Cap off speed if exceeding max
        if(Mathf.Abs(rigidBody.velocity.x) > maxSpeed)
        {
            rigidBody.velocity = new Vector2(Mathf.Sign(rigidBody.velocity.x) * maxSpeed, rigidBody.velocity.y);
        }

        if(isJumping)
        {
            rigidBody.AddForce(new Vector2(0, jumpForce));
            isJumping = false;
        }
    }

    //Character turning to face proper direction
    private void RotateCharacter(bool turnLeft)
    {
        if(turnLeft)
        {
            transform.rotation = Quaternion.AngleAxis(180, Vector3.up);
            faceLeft = true;
        }
        else
        {
            transform.rotation = Quaternion.AngleAxis(0, Vector3.up);
            faceLeft = false;
        }

        boxCollider.offset = new Vector2(-boxCollider.offset.x, boxCollider.offset.y);
    }

    //Set sprite depending on whether character is running/in air
    private void SetDefaultSprite()
    {
        boxCollider.size = new Vector2(0.8f, 2.2f);
        boxCollider.offset = new Vector2(0.4f, 0);

        //Jumping (using run sprite for now)
        if(inAir)
        {
            spriteRend.sprite = runSprite;
        }
        //Running
        else if (horizInput != 0)
        {
            spriteRend.sprite = runSprite;
        }
        else
        {
            spriteRend.sprite = idleSprite;
            boxCollider.size = new Vector2(0.35f, 2.2f);
            boxCollider.offset = new Vector2(-0.05f, 0);
        }
    }

    private IEnumerator SetUpAttack(bool isStrong)
    {
        isAttacking = true;
        spriteRend.sprite = atkUpSprite;
        //Freeze motion when attacking
        rigidBody.constraints = RigidbodyConstraints2D.FreezeAll;

        //Force applied to enemy
        float launchForceX = 1000;
        float launchForceY = 1000;
        Vector3 hitboxPos = transform.position + new Vector3(0.85f, 1.5f, 0);
        if(faceLeft)
        {
            launchForceX *= -1;
            hitboxPos = transform.position + new Vector3(-0.85f, 1.5f, 0);
        }

        //Create attack hitbox
        GameObject attack = (GameObject) Instantiate(hitbox, hitboxPos, Quaternion.identity);
        attack.transform.parent = transform;
        PlayerAttack attackScript = attack.GetComponent<PlayerAttack>();

        //Strong or normal variant of attack
        if(isStrong)
        {
            attackScript.SetLaunchForce(2 * launchForceX, 2 * launchForceY);
            attackScript.SetLifeTime(0.75f);
            yield return new WaitForSeconds(0.75f);
        }
        else
        {
            attackScript.SetLaunchForce(launchForceX, launchForceY);
            attackScript.SetLifeTime(0.25f);
            yield return new WaitForSeconds(0.25f);
        }

        //Resume movement
        rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;
        SetDefaultSprite();
        isAttacking = false;
    }

    private IEnumerator SetSideAttack(bool isStrong)
    {
        isAttacking = true;
        spriteRend.sprite = atkSideSprite;
        rigidBody.constraints = RigidbodyConstraints2D.FreezeAll;

        float launchForceX = 1000;
        float launchForceY = 0;
        Vector3 hitboxPos = transform.position + new Vector3(1.3f, 0.5f, 0);
        if (faceLeft)
        {
            launchForceX *= -1;
            hitboxPos = transform.position + new Vector3(-1.3f, 0.5f, 0);
        }

        GameObject attack = (GameObject)Instantiate(hitbox, hitboxPos, Quaternion.identity);
        attack.transform.parent = transform;
        PlayerAttack attackScript = attack.GetComponent<PlayerAttack>();

        if (isStrong)
        {
            attackScript.SetLaunchForce(2 * launchForceX, 2 * launchForceY);
            attackScript.SetLifeTime(0.75f);
            yield return new WaitForSeconds(0.75f);
        }
        else
        {
            attackScript.SetLaunchForce(launchForceX, launchForceY);
            attackScript.SetLifeTime(0.25f);
            yield return new WaitForSeconds(0.25f);
        }

        rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;
        SetDefaultSprite();
        isAttacking = false;
    }

    private IEnumerator SetDownAttack(bool isStrong)
    {
        isAttacking = true;
        spriteRend.sprite = atkDownSprite;
        rigidBody.constraints = RigidbodyConstraints2D.FreezeAll;

        float launchForceX = 1000;
        float launchForceY = -1000;
        Vector3 hitboxPos = transform.position + new Vector3(1.1f, -1f, 0);
        if (faceLeft)
        {
            launchForceX *= -1;
            hitboxPos = transform.position + new Vector3(-1.1f, -1f, 0);
        }

        GameObject attack = (GameObject)Instantiate(hitbox, hitboxPos, Quaternion.identity);
        attack.transform.parent = transform;
        PlayerAttack attackScript = attack.GetComponent<PlayerAttack>();

        if (isStrong)
        {
            attackScript.SetLaunchForce(2 * launchForceX, 2 * launchForceY);
            attackScript.SetLifeTime(0.75f);
            yield return new WaitForSeconds(0.75f);
        }
        else
        {
            attackScript.SetLaunchForce(launchForceX, launchForceY);
            attackScript.SetLifeTime(0.25f);
            yield return new WaitForSeconds(0.25f);
        }

        rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;
        SetDefaultSprite();
        isAttacking = false;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if(other.gameObject.tag == "Ground")
        {
            inAir = false;
            jumpCounter = 0;
        }
    }

    void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.tag == "Ground")
        {
            inAir = true;
        }
    }
}
