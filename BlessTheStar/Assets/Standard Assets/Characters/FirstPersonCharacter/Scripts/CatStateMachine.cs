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
	public float AlertTime;
	public float PatrolTime;
	public float AttackTime;
	float timer;

    Rigidbody rigidBody;
	public Transform target;
	public float speed;

	private Vector3 previousRot;

	private int currentpoint;
	private float percent;

	public Transform[] goalPoints;
	NavMeshAgent agent;
	private Transform goal;

	private float chillTime = 10f;
	private bool chilling = false;

	// Use this for initialization
	void Start () {
        state = Alertness.ALERT;
        viewCone = Mathf.Cos(Mathf.PI / 4);
        rigidBody = GetComponent<Rigidbody>();
		goal = goalPoints [Random.Range (0, goalPoints.Length -1)];

		agent = GetComponent<NavMeshAgent> ();
		agent.destination = goal.position; 
		percent = 0;
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
		Debug.Log(Vector3.Distance (transform.position, goal.position));
		Debug.Log (chilling);
		if (chilling) {
			chillTime -= Time.deltaTime;
			Debug.Log (chillTime);
			if(chillTime <= 0f) {
				chilling = false;
				goal = goalPoints [Random.Range (0, goalPoints.Length)];
				agent.destination = goal.position;
				chillTime = 10f;
			}
			Debug.Log (goal);
		}
		else if (!chilling && Vector3.Distance (transform.position, goal.position) < 1f) {
			//transform.position = goal.position;
			goal = goalPoints [Random.Range (0, goalPoints.Length)];
			agent.destination = goal.position;
			//chilling = true;
		}
    }

    void FixedUpdate()
	{	
		
	//	Debug.Log(percent);
	//	transform.position = curve.GetPointAt(percent);
	//	percent += Time.deltaTime/20;
	//	if(percent >= 1f){
	//		percent -= 1.0f;
	//	}
	//	transform.LookAt (curve.GetPointAt (percent));
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
		if ((state == Alertness.ALERT || state == Alertness.PATROL || state == Alertness.ATTACK) && canSeePlayer())
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
			}// else if(state == Alertness.ALERT){
		//		state = Alertness.SLEEP;
		//	}
		}
    }

    bool canSeePlayer()
    {
		Vector3 diff = player.position - transform.position;
		if (Vector3.Dot (diff, viewDir) > viewCone) {
			RaycastHit hit;
			Physics.Raycast (transform.position, diff.normalized, out hit);
			if (hit.transform.tag == "Player") {
				Debug.DrawLine (transform.position, hit.point);
				return true;
			}
			
		}
		return false;
    }

	public void SetTarget(Transform target)
	{
		this.target = target;
	}
}
