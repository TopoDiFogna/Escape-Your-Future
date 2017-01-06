﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OstrichController : MonoBehaviour
{

    public int m_scratch_layer = 8;
    public int m_player_layer = 9;
    GameController gc;
    public GameObject m_paint;

    [Range(0, 100)]
    public float m_speed = 20f;
    public float m_underwater_speed = 10f;

    [Range(0, 200)]
    public float m_Jump_force = 10f;
    float normal_jump_force;
    public float diving_jump_force = 5f;

    int lay;
    Rigidbody2D rb;
    public Vector3 checkPointPosition;

    public bool jumping = false;
    private bool dead = false;
    public float timeToDismount = 0.5f;
    public int cycles_to_dismount = 50;

    [Range(0.5f, 5.0f)]
    public float timeToDie = 1.0f;

    private float m_horizontal = 0f;
    private float m_vertical = 0f;
    private float m_axis_jump = 0f;

    private bool facing_right = true;

    bool active = false;
    bool with_player = false;
    bool in_water = false;
    bool dismounting = false;
    BoxCollider2D boxcoll;
    CircleCollider2D circlecoll;
    PolygonCollider2D polycoll;
    GameObject player;

    public Vector2 min_cam_bounds;
    public Vector2 max_cam_bounds;


    // Use this for initialization
    void Start()
    {
        gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        boxcoll = GetComponent<BoxCollider2D>();
        circlecoll = GetComponent<CircleCollider2D>();
        polycoll = GetComponent<PolygonCollider2D>();
        normal_jump_force = m_Jump_force;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gc.Pause && with_player && !dismounting)
        {
            m_horizontal = Input.GetAxisRaw("Horizontal");
            m_vertical = Input.GetAxisRaw("Vertical");
            m_axis_jump = Input.GetAxisRaw("Jump");

            if (facing_right && m_horizontal < 0)
            {
                // TODO GIRARE STRUZZO E PLAYER sr.flipX = true;
                facing_right = false;
            }
            else if (!facing_right && m_horizontal > 0)
            {
                // TODO GIRARE STRUZZO E PLAYER sr.flipX = false;
                facing_right = true;
            }
        }
    }

    private void FixedUpdate()
    {
        if (dead)
        {
            //donothing
        }
        else
        {
            if (with_player && !dismounting)
            {
                if (!in_water)
                {
                    rb.velocity = new Vector2(m_horizontal * m_speed, rb.velocity.y);
                    if (m_axis_jump > 0 && !jumping)
                    {
                        rb.AddForce(Vector2.up * m_Jump_force, ForceMode2D.Impulse);
                        jumping = true;
                    }
                }
                if (in_water)
                {
                    rb.velocity = new Vector2(m_horizontal * m_underwater_speed, m_vertical * m_underwater_speed);
              
                }
            }
        }

    }

    private void OnCollisionEnter2D(Collision2D coll)
    {
        if (jumping == true && coll.gameObject.tag == "Terrain" && with_player)
        {
            jumping = false;
        }
        if(coll.gameObject.tag == "Player" && active)
        {
            Debug.Log("salgo sullo struzzo");
            player = coll.gameObject;
            player.SetActive(false);
            checkPointPosition = transform.position;
            GameObject.FindObjectOfType<CameraController>().m_target = gameObject;
            //TODO animazione del player che sale sullo struzzo e poi fa enable dei comandi
            with_player = true;
        }
    }


    private void OnCollisionExit2D(Collision2D coll)
    {
        if (jumping == false && coll.gameObject.tag == "Terrain" && with_player)
        {
            jumping = true;
        }
    }

    private void OnCollisionStay2D(Collision2D coll)
    {
        if (jumping == true && coll.gameObject.tag == "Terrain" && with_player)
        {
            jumping = false;
        }

    }

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.tag == "Scratch" && !active)
        {
            boxcoll.enabled = false;
            circlecoll.enabled = true;
            polycoll.enabled = true;
            rb.isKinematic = false;
            gameObject.layer = m_player_layer;
            gameObject.GetComponent<SpriteRenderer>().sortingOrder = 3;
            active = true;
        }
        if (coll.gameObject.tag == "Scratch" && active)
        {
            gameObject.layer = m_player_layer;
        }
        if(coll.gameObject.tag == "Water")
        {
            in_water = true;
            jumping = true;
        }
    }

    private void OnTriggerStay2D(Collider2D coll)
    {
        if (coll.gameObject.tag == "Scratch" && active)
        {
            gameObject.layer = m_scratch_layer;
        }
    }

    private void OnTriggerExit2D(Collider2D coll)
    {
        if (coll.gameObject.tag == "Scratch" && active)
        {
            gameObject.layer = m_player_layer;
        }
        if(coll.gameObject.tag == "Water")
        {
            in_water = false;
        }
    }

    public IEnumerator DismountFromOstrich(Vector3 position, Vector3 ostrich)
    {
        if (with_player)
        {
            dismounting = true;
            with_player = false;
            rb.isKinematic = true;
            rb.velocity = Vector2.zero;
            float time = 0;
            for(int i= 0; i<cycles_to_dismount; i++)
            {
                yield return new WaitForSeconds(timeToDismount / cycles_to_dismount);
                time += timeToDismount / cycles_to_dismount;
                transform.position = Vector3.Lerp(transform.position, ostrich, time);
            }
            rb.isKinematic = false;
            //TODO far partire animazione player di dismount e aspettare che finisca 
            player.transform.position = position;
            player.layer = m_player_layer;
            player.SetActive(true);
            GameObject.FindObjectOfType<CameraController>().m_target = player;

            dismounting = false;
        }
    }

    public void DieWithFade()
    {
        if (!dead)
        {
            dead = true;
            //StopAnimation();
            CameraFade.StartAlphaFade(Color.black, false, timeToDie * 2f, 0f); // Fades out the screen to black   
            StartCoroutine(ResetScene());
            if (SceneManager.GetActiveScene().name == "Level3_Maya")
            {
                GameObject.FindGameObjectWithTag("Boulder").GetComponent<BoulderManager>().Set_Reset_Boulder(false);
                foreach (GameObject g in GameObject.FindGameObjectsWithTag("SpawnedSkeleton"))
                {
                    g.SetActive(false);
                }
            }
        }
    }

    private IEnumerator ResetScene()
    {
        yield return new WaitForSeconds(timeToDie);
        transform.position = checkPointPosition;
        CameraController camControl = Camera.main.GetComponent<CameraController>();
        camControl.M_minBounds = min_cam_bounds;
        camControl.M_maxBounds = max_cam_bounds;
        yield return new WaitForSeconds(1f);
        CameraFade.instance.Die();
        dead = false;
    }
}
