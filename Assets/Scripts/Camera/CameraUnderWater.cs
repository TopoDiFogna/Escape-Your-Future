﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraUnderWater : MonoBehaviour {

    public float min_bound_delta_y = 10.8f;
    public float max_bound_delta_y = 10.8f;
    public float min_bound_x;
    public float max_bound_x;
    bool activated = false;


	// Use this for initialization
	void Start () {
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if((collision.tag == "Ostrich" || collision.tag == "Player") && !activated)
        {
            CameraController camControl = Camera.main.GetComponent<CameraController>();
            camControl.M_minBounds = new Vector2(min_bound_x, camControl.M_minBounds.y - min_bound_delta_y);
            camControl.M_maxBounds = new Vector2(max_bound_x, camControl.M_maxBounds.y - max_bound_delta_y);
            activated = true;
        }

    

    }
}
