﻿using UnityEngine;
using System.Collections;
using Pathfinding;

[RequireComponent (typeof (Rigidbody2D))]
[RequireComponent (typeof (Seeker))]
public class EnemyAI : MonoBehaviour {

	// What to chase?
	public Transform target;
	
	// How many times each second we will update our path
	public float updateRate = 2f;
	
	// Caching
	private Seeker seeker;
	private Rigidbody2D rb;
	
	//The calculated path
	public Path path;
	
	//The AI's speed per second
	public float speed = 450f;
	public ForceMode2D fMode;
	
	[HideInInspector]
	public bool pathIsEnded = false;
	
	// The max distance from the AI to a waypoint for it to continue to the next waypoint
	public float nextWaypointDistance = 3;
	
	// The waypoint we are currently moving towards
	private int currentWaypoint = 0;

    private bool searchingForPlayer = false;
	
	void Start () {
		seeker = GetComponent<Seeker>();
		rb = GetComponent<Rigidbody2D>();
		
		if (target != null)
        {
            if (!searchingForPlayer)
            {
                searchingForPlayer = true;
                StartCoroutine(SearchForPlayer());
            }
			return;
		}
	}

    IEnumerator SearchForPlayer()
    {
        GameObject sResult = GameObject.FindGameObjectWithTag("Player");
        if (sResult != null)
        {
            target = sResult.transform;
            searchingForPlayer = false;
            StartCoroutine(UpdatePath());
            yield return false;
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(SearchForPlayer());
        }
    }

    IEnumerator UpdatePath () {
        if (target == null)
        {
            if (!searchingForPlayer)
            {
                searchingForPlayer = true;
                StartCoroutine(SearchForPlayer());
            }
            yield return false;
        }

        // Start a new path to the target position, return the result to the OnPathComplete method
        seeker.StartPath (transform.position, target.position, OnPathComplete);
		
		yield return new WaitForSeconds ( 1f/updateRate );
		StartCoroutine (UpdatePath());
	}
	
	public void OnPathComplete (Path p) {
		if (!p.error) {
			path = p;
			currentWaypoint = 0;
		}
	}
	
	void FixedUpdate () {
        if (target == null)
        {
            if (!searchingForPlayer)
            {
                searchingForPlayer = true;
                StartCoroutine(SearchForPlayer());
            }
            return;
        }

        //TODO: Always look at player?

        if (path == null)
			return;
		
		if (currentWaypoint >= path.vectorPath.Count) {
			if (pathIsEnded)
				return;
			
			pathIsEnded = true;
			return;
		}
		pathIsEnded = false;
	
		//Direction to the next waypoint
		Vector3 dir = ( path.vectorPath[currentWaypoint] - transform.position ).normalized;
		dir *= speed * Time.fixedDeltaTime;
		
		//Move the AI
		rb.AddForce (dir, fMode);
		
		float dist = Vector3.Distance (transform.position, path.vectorPath[currentWaypoint]);
		if (dist < nextWaypointDistance) {
			currentWaypoint++;
			return;
		}
	}
	
}
