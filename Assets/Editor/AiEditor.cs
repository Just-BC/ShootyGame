using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(Ai))]
public class AiEditor : Editor
{
    private void OnSceneGUI()
    {
        Ai ai = (Ai)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(ai.transform.position, Vector3.forward, Vector3.up, 360, ai.viewDist);
        if (ai.dir > 0)
        {
            Vector3 viewAngleA = new Vector3(Mathf.Cos(-ai.viewAngle / 2 * Mathf.Deg2Rad), Mathf.Sin(-ai.viewAngle / 2 * Mathf.Deg2Rad), 0);
            Vector3 viewAngleB = new Vector3(Mathf.Cos(ai.viewAngle / 2 * Mathf.Deg2Rad), Mathf.Sin(ai.viewAngle / 2 * Mathf.Deg2Rad), 0);
            Handles.DrawLine(ai.transform.position, ai.transform.position + viewAngleA * ai.viewAngle);
            Handles.DrawLine(ai.transform.position, ai.transform.position + viewAngleB * ai.viewAngle);

        }
        else
        {
            Vector3 viewAngleA = new Vector3(Mathf.Cos((-ai.viewAngle / 2 + 180) * Mathf.Deg2Rad), Mathf.Sin((-ai.viewAngle / 2 + 180) * Mathf.Deg2Rad), 0);
            Vector3 viewAngleB = new Vector3(Mathf.Cos((ai.viewAngle / 2 + 180) * Mathf.Deg2Rad), Mathf.Sin((ai.viewAngle / 2 + 180) * Mathf.Deg2Rad), 0);
            Handles.DrawLine(ai.transform.position, ai.transform.position + viewAngleA * ai.viewAngle);
            Handles.DrawLine(ai.transform.position, ai.transform.position + viewAngleB * ai.viewAngle);
        }

        

    }
}
