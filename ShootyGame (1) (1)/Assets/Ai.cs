using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ai : MonoBehaviour
{

    
    [Header("Navigation")]
    Vector2 playerLoc;
    [SerializeField] GameObject player;
    Rigidbody2D rb;
    [SerializeField] float runSpeed;
    [SerializeField] float stopDist;
    [SerializeField] Transform goal;

    [Header("Misc")]
    [SerializeField] float health;

    [Header("Weapon")]
    [SerializeField] [Range(0, 10)] float aimAccuracy;
    [SerializeField] float fireRate;
    [SerializeField] GameObject bulletTrail;
    [SerializeField] float fireDist;
    [SerializeField] float weaponImpactForce;
    [SerializeField] float weaponDamage;
    [SerializeField] Transform bulletSpawn;
    float startHealth;
    Vector3 startScale;
    float nextFire;

    // Start is called before the first frame update
    void Start()
    {
        startHealth = health;
        rb = GetComponent<Rigidbody2D>();
        startScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        Navigate();
        Fire();
        
    }
    private void FixedUpdate()
    {
        Aim();
    }
    Vector2 GetPlayerLoc()
    {
        //Instantiate(testObj, player.transform.position, player.transform.rotation);
        return player.transform.position;
    }
    void Navigate()
    {
        
        if (transform.position.x < GetPlayerLoc().x)
        {
            transform.localScale = new Vector3(startScale.x, transform.localScale.y, transform.localScale.z);//flip sprite to look at player
        }
        else
        {
            transform.localScale = new Vector3(-startScale.x, transform.localScale.y, transform.localScale.z); //flip sprite to look at player
        }
        if (transform.position.x > GetPlayerLoc().x - stopDist)
        {
            rb.AddRelativeForce(-transform.right * runSpeed * Time.deltaTime * 100);//move left
        }
        if (transform.position.x < GetPlayerLoc().x + stopDist)
        {
            rb.AddRelativeForce(transform.right * runSpeed * Time.deltaTime * 100); //move right
        }
    }
    void Aim()
    {
        bulletSpawn.eulerAngles = new Vector3(0, 0, (Mathf.Atan2(player.transform.position.y - transform.position.y, player.transform.position.x-transform.position.x) *180/(Mathf.PI)));

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
    public void Damage(float damage)
    {
        health -= damage;
        if(health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
