using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    float lastChecked =0;
    public float activationDist;
    [SerializeField] float checkTime;
    public bool Check(bool close)
    {
        if(Time.time>=lastChecked && close == false)
        {
            lastChecked = Time.time+checkTime;
            return true;
        }
        else
        {
            return false;
        }
    }
}
