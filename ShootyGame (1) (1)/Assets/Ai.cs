using System.Collections;
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
    [SerializeField] LayerMask playerMask;
    [SerializeField] Transform headPos;



    [Header("Misc")]
    [SerializeField] float health;
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
    float nextFire;
    float distToGoal;
    [SerializeField] bool wayPointTimerOn;

    void Start()
    {
        startHealth = health;//record the starting health
        startSpeed = speed;//record the starting speed;
        rb = GetComponent<Rigidbody2D>();//find the rigid body
        startScale = transform.localScale; //record the starting scale
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

        if (FindTargets())
        {
            mode = "fight";
            Fight();
        }
        else
        {
            mode = "patrol";
            Patrol();
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
    Vector2 GetPlayerLoc()
    {
        //Instantiate(testObj, player.transform.position, player.transform.rotation);
        return player.transform.position;
    }
    void Patrol()
    {
        speed = patrolSpeed;//set the speed to patrol speed

        if(goal==null | goal == player.transform)
            FindWaypoint();//set the goal to a new waypoint

        distToGoal = Vector2.Distance(transform.position, goal.transform.position);
        if (distToGoal <= wayPointStopDist && !wayPointTimerOn)
            StartCoroutine(WayPointTimer());
        if (goal != null)
        {
            if (transform.position.x < goal.transform.position.x)
            {
                transform.localScale = new Vector3(startScale.x, transform.localScale.y, transform.localScale.z);
            }//flip sprite right to look at the goal
            else
            {
                transform.localScale = new Vector3(-startScale.x, transform.localScale.y, transform.localScale.z);
            }//flip sprite left to look the goal

            if (transform.position.x > goal.transform.position.x - stopDist/10)
            {
                rb.AddRelativeForce(-transform.right * speed * curve.Evaluate(1 - distToGoal) * Time.deltaTime * 100);
            }//move left

            if (transform.position.x < goal.transform.position.x + stopDist/10)
            {
                rb.AddRelativeForce(transform.right * speed * curve.Evaluate(1 - distToGoal) * Time.deltaTime * 100);
            }//move right
        }//move towards the goal
        //print(goal.name);
    }
    bool FindTargets()
    {
        headPos.LookAt(player.transform.position);
        bool withinViewingRange = headPos.eulerAngles.x < viewAngle && headPos.eulerAngles.x > -viewAngle;
        if (Physics2D.Raycast(headPos.position, headPos.forward, Vector2.Distance(headPos.position, player.transform.position), playerMask)==false && withinViewingRange)
        {
            goal = player.transform;
            return true;
        }
        else
        {
            if(goal == player.transform)
                goal = null;
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


    ///WEAPON//////////////////////////////////////////
    void Fight()
    {
        speed = startSpeed;
        Fire();
        Aim();
        distToGoal = Vector2.Distance(transform.position, goal.transform.position);
        if (transform.position.x < goal.transform.position.x)
        {
            transform.localScale = new Vector3(startScale.x, transform.localScale.y, transform.localScale.z);//flip sprite to look at the goal
        }
        else
        {
            transform.localScale = new Vector3(-startScale.x, transform.localScale.y, transform.localScale.z); //flip sprite to look the goal
        }
        if (transform.position.x > goal.transform.position.x - stopDist)
        {
            rb.AddRelativeForce(-transform.right * speed * Time.deltaTime * 100);//move left
        }
        if (transform.position.x < goal.transform.position.x + stopDist)
        {
            rb.AddRelativeForce(transform.right * speed * Time.deltaTime * 100); //move right
        }
    }
    void Aim()
    {
        float angle = (Mathf.Atan2(player.transform.position.y - transform.position.y, player.transform.position.x - transform.position.x) * 180 / (Mathf.PI));
        angle += +Random.Range(-1, 1) * (1 / aimAccuracy);
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
            Destroy(gameObject);
        }
    }
}
