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

	// Use this for initialization
	void Start () {
        state = Alertness.SLEEP;
        viewCone = Mathf.Cos(Mathf.PI / 4);
        rigidBody = GetComponent<Rigidbody>();
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
		Vector3 dir = target.transform.position - transform.position;
		Quaternion rot = Quaternion.identity;
		float rota = Vector2.Dot (new Vector2 (target.transform.position.x, target.transform.position.z), new Vector2 (transform.position.x, transform.position.z));
		transform.position += dir * (Time.deltaTime * speed);
		rot.eulerAngles = new Vector3 (90f, rota, 0f);
		rigidBody.MoveRotation (rot);
	
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
