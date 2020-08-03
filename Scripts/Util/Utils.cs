using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour
{
     RaycastHit2D raycast(Vector2 offset , Vector2 rayDirection, float length , LayerMask layer) {
        Vector2 pos = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(pos + offset ,rayDirection ,length ,layer);
        Color color = hit ? Color.red : Color.green;
        Debug.DrawRay(pos+ offset , rayDirection* length, color);
        return hit;
    }
}
