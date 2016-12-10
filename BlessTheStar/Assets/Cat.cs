using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Cat: MonoBehaviour {

    enum Alertness { SLEEP, ALERT, PATROL, ATTACK};
    Transform player;
    Alertness state;
    float viewCone;

    Rigidbody rigidBody;

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

    void Noise()
    {
        state = Alertness.ALERT;
    }
	
	// Update is called once per frame
	void Update () {

    }

    void FixedUpdate()
    {
        if (state == Alertness.ALERT && canSeePlayer())
        {
            state = Alertness.ATTACK;
        }
    }

    bool canSeePlayer()
    {
        return Vector3.Dot(player.position, transform.position) > viewCone;
    }
}
