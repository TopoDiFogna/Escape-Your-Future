﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wasp : MonoBehaviour {

    Vector2 playerpos;
    GameController gc;
    public float speed;
    GameObject player;
    bool chasing = false;
    Vector2 dir;
    bool charge;
    public float chargeTime;
    float timer;
    bool first_charge;

    void Start()
    {
        gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
    }

    void OnEnable()
    {
        gameObject.tag = "Untagged";
        first_charge = true;
        player = GameObject.FindGameObjectWithTag("Player");
        chasing = true;
        charge = false;
        timer = 0f;
    }

	// Update is called once per frame

	void Update () {
        if (chasing && !gc.paused)
        {
            timer += Time.deltaTime;
            if (timer >= chargeTime)
            {
                timer = 0f;
                playerpos = player.transform.position;
                dir = (player.transform.position - transform.position).normalized;
                charge = true;
            }
            if (charge)
            {
                transform.position += new Vector3(speed * dir.x * Time.deltaTime, speed * dir.y * Time.deltaTime, 0);
                if (Vector2.Distance(transform.position, playerpos) <= 0.2)
                {
                    charge = false;
                    if (first_charge)
                    {
                        first_charge = false;
                        gameObject.tag = "Swarm";
                    }
                }
            }
            
        }
	}
}
