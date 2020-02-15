using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float doubleJumpSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private GameObject bullet;
    [SerializeField] private float bulletSpeed;
    private GameObject newBullet;
    [SerializeField] private float fireRate;
    private float nextFire;
    [SerializeField] private Transform bulletSpawn;
    private Transform bulletSpawnPosStart;
    private Vector3 startingScale;
    [SerializeField] private float maxRunSpeed;
    [SerializeField] float jumpInterval;
    [SerializeField] float doubleJumpInterval;
    private float nextJump;
    private float nextDoubleJump;
    public float groundCheckDist;
    public LayerMask groundLayer;
    private Vector2 startPos;
    [SerializeField] float fireDist;
    private GameObject playerPrefab;
    [SerializeField] float weaponImpactForce;
    [SerializeField] float weaponDamage;
    [SerializeField] GameObject bulletTrail;
    [SerializeField] float health;
    [SerializeField] UnityEngine.Rendering.PostProcessing.PostProcessVolume hitPostProcessing;
    [SerializeField] float hitPostLength;
    [SerializeField] float postDownSpeed;
    [SerializeField] float postUpSpeed;
    [SerializeField] UnityEngine.UI.Slider healthSlider;
    [SerializeField] Transform sprite;
    bool pullUp = false;
    float postEnd;
    float startHealth;
    bool doubleJump;
    // Update is called once per frame 
    void Start()
    {
        startHealth = health;
        rb = gameObject.GetComponent<Rigidbody2D>();
        startingScale = transform.localScale;
        startPos = transform.position;
        //bulletSpawnPosStart = bulletSpawn.transform;
    }
    void Update()
    {
        Aim();
        GroundCheck();
        Fire();
        HitPostProcess(false);
        UI();
        
    }
    private void FixedUpdate()
    {
        Move();
    }
    void Move()
    {
        if (Input.GetButtonDown("Jump") && GroundCheck() && nextJump <= Time.time)
        {
            
            nextJump = jumpInterval + Time.time;
            rb.AddForce(transform.up * jumpSpeed);
            doubleJump = true;
            nextDoubleJump = Time.time + doubleJumpInterval;
        }
        else if (Input.GetButtonDown("Jump") && doubleJump && !GroundCheck() && nextDoubleJump <= Time.time)
        {

            doubleJump = false;
            rb.AddForce(transform.up * doubleJumpSpeed);
            print(Mathf.Sign(rb.velocity.x));
        }
        if (Input.GetAxis("Horizontal") > 0)
        {
            rb.AddRelativeForce(Vector3.right * runSpeed * Time.deltaTime * 100);


        }
        if (Input.GetAxis("Horizontal") < 0)
        {
            rb.AddRelativeForce(Vector3.left * runSpeed * Time.deltaTime * 100);

        }
        
        if(rb.velocity.x >= maxRunSpeed)
        {
            rb.velocity = new Vector2(maxRunSpeed, rb.velocity.y);
        }
        if (rb.velocity.x <= -maxRunSpeed)
        {
            rb.velocity = new Vector2(-maxRunSpeed, rb.velocity.y);
        }
    }
    private void UI()
    {
        healthSlider.value =  health/startHealth;
    }
    void Fire()
    {
        if (Input.GetButton("Fire1") && nextFire <= Time.time)
        {
            
            nextFire = Time.time + fireRate;
            RaycastHit2D hit = Physics2D.Raycast(bulletSpawn.position, bulletSpawn.right);
            if(hit.collider != null)
            {
                //Debug.DrawLine(bulletSpawn.position, hit.point, Color.blue,20,false);
                GameObject trail = Instantiate(bulletTrail);
                trail.GetComponent<LineRenderer>().SetPosition(0, bulletSpawn.transform.position);
                trail.GetComponent<LineRenderer>().SetPosition(1,  hit.point);
                trail.GetComponent<LineRenderer>().material.mainTextureScale = new Vector2(Vector2.Distance(bulletSpawn.transform.position, hit.point),1);
                if (hit.collider.GetComponent<Rigidbody2D>() && !hit.collider.CompareTag("Player"))
                    {
                        hit.collider.GetComponent<Rigidbody2D>().AddForceAtPosition(weaponImpactForce*bulletSpawn.right,hit.point);
                    }
                if(hit.collider.GetComponent<explode>())
                {
                    hit.collider.GetComponent<explode>().Explode();
                }
                if (hit.collider.GetComponent<Ai>())
                {
                    hit.collider.GetComponent<Ai>().Damage(weaponDamage);
                }
                print("fired and hit somethin");
            }
            else
            {
                GameObject trail = Instantiate(bulletTrail);
                trail.GetComponent<LineRenderer>().SetPosition(0, bulletSpawn.transform.position);
                trail.GetComponent<LineRenderer>().SetPosition(1, bulletSpawn.right * fireDist);
                trail.GetComponent<LineRenderer>().material.mainTextureScale =new Vector2( Vector2.Distance(bulletSpawn.transform.position, bulletSpawn.right * fireDist),1);
                trail.GetComponent<Animator>().SetFloat(0, 1 / fireDist);
                //Debug.DrawLine(bulletSpawn.position, bulletSpawn.right.normalized * fireDist, Color.blue,20,false);
                print("fired but didn't hit nothin");
            }
             
        }

    }
    bool GroundCheck()
    {
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x - .35f, transform.position.y-.5f), Vector2.down, groundCheckDist, groundLayer);
        if (hit.collider != null)
        {
            //print(Mathf.Abs(hit.point.y - transform.position.y));
            Debug.DrawRay(new Vector2(transform.position.x - .35f, transform.position.y - .5f), Vector2.down * groundCheckDist, Color.red, 5,true);
            //transform.position = new Vector2(transform.position.x, transform.position.y - groundSnapOffset);
            rb.velocity = new Vector2(rb.velocity.x, 0);
            return true;
        }
        hit = Physics2D.Raycast(new Vector2(transform.position.x + .35f, transform.position.y - .5f), Vector2.down, groundCheckDist, groundLayer);
        if (hit.collider != null)
        {
            //print(Mathf.Abs(hit.point.y - transform.position.y));
            Debug.DrawRay(new Vector2(transform.position.x + .35f, transform.position.y - .5f), Vector2.down * groundCheckDist, Color.red, 5, true);
            //transform.position = new Vector2(transform.position.x, transform.position.y - groundSnapOffset);
            rb.velocity = new Vector2(rb.velocity.x, 0);
            return true;
        }
        else
            return false;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.name == "DeathCube")
            Respawn();
    }

    void Respawn()
    {
        print("die");
        transform.position = startPos;
        health = startHealth;
        hitPostProcessing.weight = 0;
    }
    void Aim()
    {
        //rotation
         Vector3 mousePos = Input.mousePosition;
         Vector3 objectPos = Camera.main.WorldToScreenPoint (bulletSpawn.position);
         mousePos.z = 5.23f;
        if (mousePos.x < Camera.main.WorldToScreenPoint (transform.position).x)
        {
            transform.localScale = new Vector3(-startingScale.x, startingScale.y, startingScale.z);
        }
        if (mousePos.x > Camera.main.WorldToScreenPoint (transform.position).x)
        {
            transform.localScale = new Vector3(startingScale.x, startingScale.y, startingScale.z);
        }
         
         mousePos.x = mousePos.x - objectPos.x;
         mousePos.y = mousePos.y - objectPos.y;
 
         float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
         bulletSpawn.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
         
    }
    public void Damage(float damage)
    {
        health -= damage;
        if(health <=0)
        {
            Respawn();
        }
        HitPostProcess(true);
        
    }
    public void HealthUp(int healthUp)
    {
        health += healthUp;
    }
    void HitPostProcess(bool up)
    {
        if (up)
        {
            postEnd += hitPostLength*(startHealth/health)+Time.time;
            pullUp = true;
            print("Will be fading for:0" + (postEnd - Time.time));
        }
        if(pullUp && postEnd >= Time.time && hitPostProcessing.weight < 1-(health / startHealth))
        {
            hitPostProcessing.weight += postUpSpeed* Time.deltaTime;
        }
        else
        {
            pullUp = false;
        }

        if(hitPostProcessing.weight > 0 && pullUp ==false )
        {
            hitPostProcessing.weight -=postDownSpeed*Time.deltaTime;//this will pull the red down
            
        }
         
    }
    
}
