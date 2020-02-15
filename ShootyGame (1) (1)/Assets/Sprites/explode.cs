using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class explode : MonoBehaviour
{
    public float radius;
    public float strength;
    // Start is called before the first frame update
    public void Explode()
    {
        //GetComponent<MeshRenderer>().enabled = false;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position,radius);
        foreach (Collider2D hit in colliders)
        {
            if(hit.GetComponent<Rigidbody2D>())
            {
                //hit.GetComponent<Rigidbody2D>().AddForceAtPosition(new Vector2(Mathf.Sin(Vector2.Angle(transform.eulerAngles, hit.transform.eulerAngles)*Mathf.Deg2Rad) * strength, Mathf.Cos(Vector2.Angle(transform.eulerAngles, hit.transform.eulerAngles) * Mathf.Deg2Rad) * strength), transform.position);
                hit.GetComponent<Rigidbody2D>().AddForceAtPosition((-transform.position + hit.transform.position).normalized*strength, transform.position);
                Destroy(gameObject);
            }
        }
    }
}
