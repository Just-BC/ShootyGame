using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    public bool onGround = false;
    //[SerializeField]
    //private float thresholdValue;
    //private float nextGround;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        print(collision.gameObject.name);
        if (collision.gameObject.CompareTag("Ground"))
        {
            onGround = true;
            
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        print(collision.gameObject.name);
        if (collision.gameObject.CompareTag("Ground"))
        {
            onGround = false;
        }
    }
    }
