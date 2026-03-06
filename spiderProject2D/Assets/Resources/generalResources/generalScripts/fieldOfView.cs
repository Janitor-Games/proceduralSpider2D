using System;
using System.Collections;
using UnityEngine;

public class fieldOfView : MonoBehaviour
{
    [Header("View Values")]
    public String targetTag;
    public Transform rayOrigin;
    public LayerMask targetLayer, blockLayer;
    public float radius;
    public float angle;
    public float checkTime;
    private bool checkFOV;
    private GameObject detectedObject;

    void Start()
    {
        checkFOV=false;
        StartCoroutine(fovCheck());
    }

    void OnEnable()
    {
        StartCoroutine(fovCheck());
    }

    void OnDisable()
    {
        checkFOV=false;
        StopCoroutine(fovCheck());
    }

    void FixedUpdate()
    {
        if (checkFOV)
        {
            checkFOV=false;
            fov();
        }
    }

    public GameObject getDetectedObject()
    {
        return detectedObject;
    }

    private void fov()
    {
        detectedObject = null;
        Collider2D[] overlapCircle = Physics2D.OverlapCircleAll(transform.position, radius, targetLayer);
        if (overlapCircle.Length > 0)
        {
            Transform target = null;
            foreach (Collider2D collider in overlapCircle)
            {
                if (collider.CompareTag(targetTag))
                {
                    target = collider.gameObject.transform;
                    break;
                }
            }
            if (target != null)
            {
                var direction = (target.position - transform.position).normalized;
                if (Vector2.Angle(transform.up, direction) < angle / 2)
                {
                    detectedObject = Physics2D.Linecast(rayOrigin.position, target.position, blockLayer).collider == null ? target.gameObject : null;
                }
            }
        }
    }

    private IEnumerator fovCheck()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkTime);
            checkFOV=true;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.purple;
        Gizmos.color = Color.blueViolet;
        Gizmos.DrawWireSphere(transform.position, radius);
        var ang1 = generalStaticFuncs.getRotatedVector(-angle / 2, transform.up);
        var ang2 = generalStaticFuncs.getRotatedVector(angle / 2, transform.up);
        Gizmos.color = Color.yellowGreen;
        Gizmos.DrawLine(transform.position, transform.position + ang1 * radius);
        Gizmos.DrawLine(transform.position, transform.position + ang2 * radius);
        if (detectedObject)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(rayOrigin.position, detectedObject.transform.position);
        }
    }
}
