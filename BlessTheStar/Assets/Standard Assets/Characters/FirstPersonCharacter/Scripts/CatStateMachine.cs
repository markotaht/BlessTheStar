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
		
		if(timer >0)
			timer -= Time.deltaTime;
		
		viewDir = transform.rotation * Vector3.forward;
		if ((state == Alertness.ALERT || state == Alertness.PATROL || state == Alertness.ATTACK) && canSeePlayer ()) {
			state = Alertness.ATTACK;
			goal = player.transform;
			timer = AttackTime;
		} else if (state == Alertness.ATTACK) {
			goal = player.transform;
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
		if (Vector3.Dot (diff.normalized, viewDir) > viewCone) {
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
