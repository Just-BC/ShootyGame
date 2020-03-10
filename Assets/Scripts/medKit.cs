using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class medKit : MonoBehaviour
{
    [SerializeField] int health;
    // Start is called before the first frame update
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Player>().HealthUp(health);
            Destroy(gameObject);
        }
    }
}
