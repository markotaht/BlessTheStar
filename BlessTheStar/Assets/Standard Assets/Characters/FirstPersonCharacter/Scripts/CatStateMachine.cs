﻿using UnityEngine;
using System.Collections;
using UnityStandardAssets.Utility;

[RequireComponent(typeof(Rigidbody))]
public class CatStateMachine: MonoBehaviour {

    enum Alertness { SLEEP, ALERT, PATROL, ATTACK};
    Transform player;
    Alertness state;
    float viewCone;
	Vector3 viewDir;
	public float AlertTime;
	public float PatrolTime;
	public float AttackTime;
	float timer;

    Rigidbody rigidBody;
	public Transform target;
	public float speed;

	private Vector3 previousRot;

	public Transform[] patrol;
	private int currentpoint;

	// Use this for initialization
	void Start () {
        state = Alertness.SLEEP;
        viewCone = Mathf.Cos(Mathf.PI / 4);
        rigidBody = GetComponent<Rigidbody>();
		previousRot = new Vector3 (0f, 90f, 0f);
		transform.position = patrol [0].position;
		currentpoint = 0;
    }

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void Noise()
    {
		if (state == Alertness.PATROL || state == Alertness.ATTACK) {
			return;
		}
		if (state == Alertness.ALERT) {
			state = Alertness.PATROL;
			timer = PatrolTime;
		} else {
			state = Alertness.ALERT;
			timer = AlertTime;
		}
    }
	
	// Update is called once per frame
	void Update () {
		if (Mathf.Abs((transform.position - patrol [currentpoint].position).magnitude) < 0.5) {
			currentpoint++;
		}
		if (currentpoint >= patrol.Length) {
			currentpoint = 0;
		}
		transform.position = Vector3.MoveTowards (transform.position, patrol [currentpoint].position, speed * Time.deltaTime);
		Debug.Log (patrol [currentpoint].position);
		Debug.Log (Vector3.MoveTowards (transform.position, patrol [currentpoint].position, speed * Time.deltaTime));
    }

    void FixedUpdate()
    {	
	//	Vector3 dist = target.position - transform.position;
	//	float angle = Vector3.Dot(target.position, transform.position);
	//	Debug.Log (angle);
	//	dist = Quaternion.AngleAxis(-angle,Vector3.up) * dist;
	//	Debug.Log (dist);
		//rigidBody.AddForce (new Vector3(dist.normalized.x, dist.normalized.y, dist.normalized.z));
	//	rigidBody.position = transform.position;
		/*
		Vector3 dir = target.transform.position - transform.position;
		transform.position += dir.normalized * (Time.deltaTime * speed);
		float rotation = Vector2.Dot (new Vector2(dir.x,dir.z), new Vector2(previousRot.x,previousRot.z));
	//	Quaternion rot = Quaternion.AngleAxis (10, Vector3.up);
	//	rigidBody.MoveRotation (rot);
		Quaternion rot = transform.rotation;
		transform.rotation = rot;
		previousRot = dir;
	*/
		if(timer >0)
			timer -= Time.deltaTime;
		
		viewDir = Vector3.forward;
		if ((state == Alertness.ALERT || state == Alertness.PATROL) && canSeePlayer())
        {
            state = Alertness.ATTACK;
			timer = AttackTime;
        }
		Debug.Log (state);
		if (timer < 0) {
			if (state == Alertness.ATTACK) {
				timer = PatrolTime;
				state = Alertness.PATROL;
			} else if (state == Alertness.PATROL) {
				timer = AlertTime;
				state = Alertness.ALERT;
			} else if(state == Alertness.ALERT){
				state = Alertness.SLEEP;
			}
		}
    }

    bool canSeePlayer()
    {
		Vector3 diff = player.position - transform.position;
		return Vector3.Dot(diff, viewDir) > viewCone;
    }

	public void SetTarget(Transform target)
	{
		this.target = target;
	}
}
