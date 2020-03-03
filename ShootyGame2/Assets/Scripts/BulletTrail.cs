using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTrail : MonoBehaviour
{
    [SerializeField] private float waitTime;
    public IEnumerator Timer()
    {
        yield return new WaitForSeconds(waitTime);
        Destroy(gameObject);
        
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Timer());
    }

}
