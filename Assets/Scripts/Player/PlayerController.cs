﻿using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public int m_scratch_layer = 8;
    GameController gc;
    public GameObject m_paint;

    [Range(0, 100)]
    public float m_speed = 20f;

    [Range(0, 100)]
    public float m_climbing_speed = 15f;

    [Range(0, 200)]
    public float m_Jump_force = 10f;

    // Player Properties
    int lay;
    Rigidbody2D rb;
    Animator animator;
    SpriteRenderer sr;

    // Status variables
    public bool climbing = false;
    public bool jumping = false;
    public bool sliding = false;
    private bool dead = false;

    public bool Dead
    {
        get { return dead; }
    }

    public bool IsNearLadder = false;

    private bool facing_right = true;

    //Controls variables
    private float m_horizontal = 0f;
    private float m_vertical = 0f;

    private Vector3 checkPointPosition;
    public Vector3 CheckPointPosition
    {
        get { return checkPointPosition; }
        set { checkPointPosition = value; }
    }


    //Death variables
    [Range(0.5f, 5.0f)]
    public float timeToDie = 1.0f;


    // Use this for initialization
    private void Start()
    {
        gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        lay = gameObject.layer;
        rb = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.GetComponent<Animator>();
        sr = gameObject.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (!gc.Pause)
        {
            m_horizontal = Input.GetAxisRaw("Horizontal");
            m_vertical = Input.GetAxisRaw("Vertical");
            if (!sliding && !dead)
            {
                animator.SetFloat("Horizontal", Mathf.Abs(m_horizontal));
            }

            if (facing_right && rb.velocity.x < 0)
            {
                sr.flipX = true;
                facing_right = false;
            }
            else if (!facing_right && rb.velocity.x > 0)
            {
                sr.flipX = false;
                facing_right = true;
            }

            if (Input.GetKeyDown(KeyCode.W) && !jumping && !dead)
            {
                if (!IsNearLadder)
                {
                    rb.AddForce(Vector2.up * m_Jump_force, ForceMode2D.Impulse);
                    jumping = true;
                    animator.SetBool("Jumping", true);
                    StartCoroutine(StopJumpAnimation());
                }
                else
                {
                    climbing = true;
                    rb.isKinematic = true;
                }
            }
            if(!IsNearLadder && rb.isKinematic)
            {
                rb.isKinematic = false;
            }
        }
    }

    private IEnumerator StopJumpAnimation()
    {
        yield return new WaitForSeconds(0.1f);
        animator.SetBool("Jumping", false);
    }

    public void StopAnimation()
    {
        animator.SetFloat("Horizontal", 0);
        animator.SetBool("Jumping", false);
    }

    private void FixedUpdate()
    {
        if (dead || (sliding && !IsNearLadder))
        {
            //donothing
        }
        else if (!climbing && !sliding)
        {
            rb.velocity = new Vector2(m_horizontal * m_speed, rb.velocity.y);
        }
        else if(climbing)
        {
            
            if (m_horizontal != 0)
            {
                climbing = false;
                rb.isKinematic = false;
                rb.velocity = new Vector2(m_horizontal * m_speed, rb.velocity.y);
            }
            else
                rb.velocity = new Vector2(0, m_vertical * m_climbing_speed);
        }

    }

    private void OnCollisionEnter2D(Collision2D coll)
    {
        if (jumping == true && coll.gameObject.tag == "Terrain")
        {
            jumping = false;
            animator.SetBool("Jumping", false);
        }
    }


    private void OnCollisionExit2D(Collision2D coll)
    {
        if (jumping == false && coll.gameObject.tag == "Terrain")
        {
            jumping = true;
        }
    }

    private void OnCollisionStay2D(Collision2D coll)
    {
        if (jumping == true && coll.gameObject.tag == "Terrain")
        {
            jumping = false;
        }

    }

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.tag == "Scratch")
        {
            gameObject.layer = m_scratch_layer;
        }

        if (coll.gameObject.tag == "Climbable")
        {
            IsNearLadder = true;
        }


    }

    private void OnTriggerStay2D(Collider2D coll)
    {
        if (coll.gameObject.tag == "Scratch")
        {
            gameObject.layer = m_scratch_layer;
        }
        if (coll.gameObject.tag == "Climbable")
        {
            IsNearLadder = true;
            if (climbing)
            {
                rb.isKinematic = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D coll)
    {
        if (coll.gameObject.tag == "Scratch")
        {
            gameObject.layer = lay;
        }

        if (coll.gameObject.tag == "Climbable")
        {
            IsNearLadder = false;
        }
    }

    public void DieWithFade()
    {
        if (!dead)
        {
            dead = true;
            StopAnimation();
            CameraFade.StartAlphaFade(Color.black, false, timeToDie * 2f, 0f); // Fades out the screen to black   
            StartCoroutine(ResetScene());
        }
    }

    private IEnumerator ResetScene()
    {
        yield return new WaitForSeconds(timeToDie);
        transform.position = checkPointPosition;
        yield return new WaitForSeconds(1f);
        CameraFade.instance.Die();
        dead = false;
    }
}