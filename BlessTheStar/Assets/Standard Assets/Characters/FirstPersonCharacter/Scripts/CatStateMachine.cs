using UnityEngine;
using System.Collections;
using UnityStandardAssets.Utility;


[RequireComponent(typeof(Rigidbody))]
public class CatStateMachine: MonoBehaviour {

    enum Alertness { SLEEP, ALERT, PATROL, ATTACK};
    Transform player;
    Alertness state;
    float viewCone;
	Vector3 viewDir;
	public float sleepTime;
	public float AlertTime;
	public float PatrolTime;
	public float AttackTime;
	float timer;

    Rigidbody rigidBody;
	public float speed;

	public Transform[] goalPoints;
	NavMeshAgent agent;
	private Transform goal;

	private float chillTime = 10f;
	private bool chilling = false;

	// Use this for initialization
	void Start () {
        state = Alertness.SLEEP;
		timer = sleepTime;
        viewCone = Mathf.Cos(Mathf.PI / 3);
        rigidBody = GetComponent<Rigidbody>();
		goal = goalPoints [Random.Range (0, goalPoints.Length -1)];

		agent = GetComponent<NavMeshAgent> ();
		agent.destination = goal.position; 
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
		if (state != Alertness.SLEEP) {
			if (chilling) {
				chillTime -= Time.deltaTime;
			//	Debug.Log (chillTime <= 0f);
				if (chillTime <= 0f) {
					chilling = false;
					goal = goalPoints [Random.Range (0, goalPoints.Length)];
				//	agent.enabled = true;
					agent.destination = goal.position;
					chillTime = 10f;
				}
				//	Debug.Log (goal);
			} else if (!chilling && Vector3.Distance (transform.position, goal.position) < 3f && state != Alertness.ATTACK) {
				//transform.position = goal.position;
				//goal = goalPoints [Random.Range (0, goalPoints.Length)];
				//agent.destination = goal.position;
			//	agent.enabled = false;
				if (goal.tag == "Sleep") {
					timer = sleepTime;
					state = Alertness.SLEEP;
				} else {
					chilling = true;
				}
			}
		}
    }

    void FixedUpdate()
	{	
		
		if (timer > 0) {
			timer -= Time.deltaTime;
			Debug.Log (Time.deltaTime);
		}
		
		viewDir = transform.rotation * Vector3.forward;
		if ((state == Alertness.ALERT || state == Alertness.PATROL || state == Alertness.ATTACK) && canSeePlayer ()) {
			state = Alertness.ATTACK;
			agent.speed = 10f;
			goal = player.transform;
		//	agent.enabled = true;
			agent.destination = goal.position;
			timer = AttackTime;
		} else if (state == Alertness.ATTACK) {
			goal = player.transform;
		}
		Debug.Log (timer);
		if (timer < 0) {
			if (state == Alertness.ATTACK) {
				timer = PatrolTime;
				state = Alertness.PATROL;
				agent.speed = 3.5f;
				goal = goalPoints [Random.Range (0, goalPoints.Length)];
				//	agent.enabled = true;
				agent.destination = goal.position;
			} else if (state == Alertness.PATROL) {
				timer = AlertTime;
				state = Alertness.ALERT;
				//Mover around/move to some sleep position
			} else if (state == Alertness.ALERT) {
				state = Alertness.SLEEP;
				timer = sleepTime;
			} else {
				timer = PatrolTime;
				state = Alertness.PATROL;
				goal = goalPoints [Random.Range (0, goalPoints.Length)];
			}
		}
    }

    bool canSeePlayer()
    {
		Vector3 diff = player.position - transform.Find("Eyes").gameObject.transform.position;
		if (Vector3.Dot (diff.normalized, viewDir) > viewCone) {
			RaycastHit hit;
			Physics.Raycast ( transform.Find("Eyes").gameObject.transform.position, diff.normalized, out hit);
			if (hit.transform.tag == "Player") {
				Debug.DrawLine ( transform.Find("Eyes").gameObject.transform.position, hit.point);
				return true;
			}
			
		}
		return false;
    }
}
