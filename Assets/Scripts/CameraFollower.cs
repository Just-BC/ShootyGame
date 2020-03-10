using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    public Transform player;
    [SerializeField]
    private float lerpAmount;
    private float startZ;
    [SerializeField]
    private bool lerp;
    
    // Update is called once per frame
    private void Start()
    {
        startZ = transform.position.z;
        
    }
    
    void FixedUpdate()
    {
        if (lerp)
        {
            transform.position = Vector2.Lerp(transform.position, player.position, lerpAmount); //lerpy stuff
            transform.position = new Vector3(transform.position.x, transform.position.y, startZ);
        }
        else
        {
            transform.position = new Vector3(player.position.x, player.position.y, startZ); //just grab x and y of player and apply to camera with no fancy
        }
        
    }
}
