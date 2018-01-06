using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseBehavior : MonoBehaviour {

	private Rigidbody body;
	public float speed;
	public 

	// Use this for initialization
	void Start () {
		body = GetComponent<Rigidbody>();
	}

	void FixedUpdate() {
		transform.Translate (new Vector3 (0, 0, 1 * speed) * Time.deltaTime);
	}

	void OnCollisionEnter (Collision col) {
		if(col.gameObject.name.Contains("Wall")) {
			transform.Rotate (new Vector3 (0,90,00));
		}
	}

}
