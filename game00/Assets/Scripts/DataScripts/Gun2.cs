﻿using UnityEngine;
using System.Collections;
using System.Net.Sockets;

public class Gun2 : MonoBehaviour
{
	public Rigidbody2D rocket;				// Prefab of the rocket.
	public float speed = 20f;				// The speed the rocket will fire at.


	private PlayerControl2 playerCtrl;		// Reference to the PlayerControl script.
	private Animator anim;                  // Reference to the Animator component.

	

	void Awake()
	{
		// Setting up the references.
		anim = transform.root.gameObject.GetComponent<Animator>();
		playerCtrl = transform.root.GetComponent<PlayerControl2>();
	}


	void Update ()
	{
		
	}

	public void Fire()
    {
		anim.SetTrigger("Shoot");
		GetComponent<AudioSource>().Play();

        if (playerCtrl.facingRight)
        {
			Rigidbody2D bulletInstance = Instantiate(rocket, transform.position, Quaternion.Euler(new Vector3(0, 0, 0))) as Rigidbody2D;
			bulletInstance.velocity = new Vector2(speed, 0);
        }
        else
        {
			Rigidbody2D bulletInstance = Instantiate(rocket, transform.position, Quaternion.Euler(new Vector3(0, 0, 180f))) as Rigidbody2D;
			bulletInstance.velocity = new Vector2(-speed, 0);
        }
    }
}
