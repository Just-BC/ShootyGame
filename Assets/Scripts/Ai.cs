﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ai : MonoBehaviour
{

    
    [Header("Navigation")]
    Vector2 playerLoc;
    [SerializeField] GameObject player;
    Rigidbody2D rb;
    [SerializeField] float speed;
    [SerializeField] float stopDist;
    [SerializeField] Transform goal;
    [SerializeField] Transform lastGoal;
    public List<Transform> visibleTargets = new List<Transform>();
    [SerializeField] List<Transform> Waypoints;
    [SerializeField] float wayPointTime;
    [SerializeField] float wayPointStopDist;
    public float viewDist;
    public float viewAngle;
    [SerializeField] LayerMask obstacleMask;
    [SerializeField] AnimationCurve curve;

    [Header("Modes/Behavior")]
    [SerializeField] string mode;
    [SerializeField] float patrolSpeed;
    [SerializeField] float inspectSpeed;
    [SerializeField] LayerMask playerMask;
    [SerializeField] Transform headPos;
    [SerializeField] Transform questionMark;



    [Header("Misc")]
    [SerializeField] float health;
    [SerializeField] GameObject medKit;

    [Header("Weapon")]
    [SerializeField] [Range(0.0001f, 1)] float aimAccuracy;
    [SerializeField] float fireRate;
    [SerializeField] GameObject bulletTrail;
    [SerializeField] float fireDist;
    [SerializeField] float weaponImpactForce;
    [SerializeField] float weaponDamage;
    [SerializeField] Transform bulletSpawn;

    int currentGoal = 0;
    float startHealth;
    float startSpeed;
    Vector3 startScale;
    public int dir;
    float nextFire;
    bool inspect;
    float lastCheck = 0;
    [SerializeField] float inspectTime;
    [SerializeField] float inspectPointTime;
    [SerializeField] float distToGoal;
    [SerializeField] bool wayPointTimerOn;
    [SerializeField] Transform wayPointTransform;
    [SerializeField] Vector2 lastPlayerPos;

    void Start()
    {
        transform.parent = null;
        startHealth = health;//record the starting health
        startSpeed = speed;//record the starting speed;
        rb = GetComponent<Rigidbody2D>();//find the rigid body
        startScale = transform.localScale; //record the starting scale
        wayPointTransform.parent = null;
        //Clear all waypoint parents
        foreach (Transform g in Waypoints)
        {
            g.transform.SetParent(null);
        }
    }
    void Update()
    {
        Logic();
    }

    ///MODES/BEHAVIOR//////////////////////////////////////
    void Logic()
    {
        dir = (int) Mathf.Sign(startScale.x / transform.localScale.x);
        if (inspect)
        {
            Inspect();
            mode = "inspect";
            questionMark.gameObject.SetActive(true);
        }
        else if (FindTargets())
        {
            mode = "fight";
            Fight();
            questionMark.gameObject.SetActive(false);
        }
        
        else
        {
            mode = "patrol";
            Patrol();
            questionMark.gameObject.SetActive(false);
        }

    }
    ///NAVIGATION//////////////////////////////////////
    IEnumerator WayPointTimer()
    {

        wayPointTimerOn = true;
        
        yield return new WaitForSeconds(wayPointTime);
        FindWaypoint();
        wayPointTimerOn = false;
    }
    IEnumerator InspectTimer()
    {
        print("started to inspect!");
        inspect = true;
        yield return new WaitForSeconds(inspectTime);
        inspect = false;
        lastGoal = null;
    }
    void Inspect()
    {
        //this function will randomly pick goals to move to in a range near the players last position
        if (lastCheck < Time.time)
        {
            goal = wayPointTransform;
            Vector3 newWayPointPos;
            float r = Random.Range(0.5f, 5f);
            if (Random.Range(-1f, 1f) <= 0)//flip direction of goal offset randomly
            {
                r = -1;
            }
            newWayPointPos = new Vector3 (lastPlayerPos.x + r,0,0);
            
            wayPointTransform.position = newWayPointPos;
            lastCheck = Time.time + inspectPointTime;
        }
        Move(goal, inspectSpeed);
    }
    void Patrol()
    {
        speed = patrolSpeed;//set the speed to patrol speed
        
        if (goal==null | goal == player.transform)
        {
            FindWaypoint();//set the goal to a new waypoint
            
        }
        distToGoal = Mathf.Abs(goal.transform.position.x - transform.position.x);
        if (distToGoal <= wayPointStopDist && !wayPointTimerOn)
            StartCoroutine(WayPointTimer());
        Move(goal, patrolSpeed);

        
    }
    bool FindTargets()
    {
        headPos.eulerAngles = new Vector3(0, 0,(Mathf.Atan2(player.transform.position.y - transform.position.y, player.transform.position.x - transform.position.x) * 180 / (Mathf.PI)));
        bool isInView = (headPos.eulerAngles.z -180 < viewAngle && headPos.eulerAngles.z -180 > -viewAngle && dir <0) | (headPos.eulerAngles.z - 360 < viewAngle && headPos.eulerAngles.z - 360 > -viewAngle && dir > 0);
        if (Physics2D.Raycast(headPos.position, headPos.right, Vector2.Distance(headPos.position, player.transform.position), playerMask)==false && isInView)
        {
            goal = player.transform;
            lastGoal = player.transform;
            lastPlayerPos = player.transform.position;
            return true;
        }
        else if (lastGoal == player.transform && !inspect)
        {
            StartCoroutine(InspectTimer());
            return false;
        }
        else
        {
            if(goal == player.transform)
                goal = null;
                lastGoal = null;
            return false;
        }
        
    }
    void FindWaypoint()
    {

        if (Waypoints.Count == currentGoal)
        {
            currentGoal = 0;
        }
        goal = Waypoints[currentGoal];
        currentGoal++;

    }
    private void Move(Transform g, float s)
    {
        if (transform.position.x < g.transform.position.x)
        {
            transform.localScale = new Vector3(startScale.x, transform.localScale.y, transform.localScale.z);
        }//flip sprite right to look at the goal
        else
        {
            transform.localScale = new Vector3(-startScale.x, transform.localScale.y, transform.localScale.z);
        }//flip sprite left to look the goal
        if (transform.position.x < (g.transform.position.x + wayPointStopDist))
        {
            rb.AddRelativeForce(transform.right * s * Time.deltaTime * 100);
        }//move right
        if (transform.position.x > (g.transform.position.x - wayPointStopDist))
        {
            rb.AddRelativeForce(-transform.right * s * Time.deltaTime * 100);
        }//move left
    }


    ///WEAPON//////////////////////////////////////////
    void Fight()
    {
        speed = startSpeed;
        Fire();
        Aim();
        distToGoal = Vector2.Distance(transform.position, goal.transform.position);
        Move(goal, speed);
        
    }
    void Aim()
    {
        float angle = (Mathf.Atan2(player.transform.position.y - transform.position.y, player.transform.position.x - transform.position.x) * 180 / (Mathf.PI));
        //angle += +Random.Range(-1, 1) * (1 / aimAccuracy);
        bulletSpawn.eulerAngles = new Vector3(0, 0, angle);

    }
    void Fire()
    {
        if (nextFire <= Time.time)
        {
            RaycastHit2D hit;
            nextFire = Time.time + fireRate;
            hit = Physics2D.Raycast(bulletSpawn.position, bulletSpawn.right, fireDist);

            if (hit.collider)
            {
                Debug.DrawLine(bulletSpawn.position, hit.point, Color.green, 5, false);
                GameObject trail = Instantiate(bulletTrail);
                trail.GetComponent<LineRenderer>().SetPosition(0, bulletSpawn.transform.position);
                trail.GetComponent<LineRenderer>().SetPosition(1, hit.point);
                trail.GetComponent<LineRenderer>().material.mainTextureScale = new Vector2(Vector2.Distance(bulletSpawn.transform.position, hit.point), 1);
                if (hit.collider.GetComponent<Rigidbody2D>() && hit.collider.name != name | (hit.transform.parent && hit.transform.parent.name != name) && !hit.collider.GetComponent<Player>())
                {
                    hit.collider.GetComponent<Rigidbody2D>().AddForceAtPosition(weaponImpactForce * bulletSpawn.right, hit.point);
                }
                if (hit.collider.GetComponent<explode>())
                    hit.collider.GetComponent<explode>().Explode();

                if (hit.collider.GetComponent<Player>())
                    hit.collider.GetComponent<Player>().Damage(weaponDamage);

                print("fired and hit"+ hit.collider.name);
            }
            else
            {
                GameObject trail = Instantiate(bulletTrail);
                trail.GetComponent<LineRenderer>().SetPosition(0, bulletSpawn.transform.position);
                trail.GetComponent<LineRenderer>().SetPosition(1, bulletSpawn.right * fireDist);
                trail.GetComponent<LineRenderer>().material.mainTextureScale = new Vector2(Vector2.Distance(bulletSpawn.transform.position, bulletSpawn.right * fireDist), 1);
                trail.GetComponent<Animator>().SetFloat(0, 1 / fireDist);
                Debug.DrawLine(bulletSpawn.position, bulletSpawn.right.normalized * fireDist, Color.blue, 20, false);
                print("fired but didn't hit nothin");
            }
            
        }
    }

    ///MISC////////////////////////////////////////////

    public void Damage(float damage)
    {
        health -= damage;
        if(health <= 0)
        {
            Death();
        }
    }
    void Death()
    {
        Instantiate(medKit, transform.position, transform.rotation, null);
        Destroy(gameObject);
    }
    
}
