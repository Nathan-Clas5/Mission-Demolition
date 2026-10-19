using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BreakOnImpact : MonoBehaviour
{
    [SerializeField, Min(0f)]
    private float breakSpeed = 8f;

    private void OnCollisionEnter(Collision collision)
    {
        // Only break when hit by a projectile.
        if (collision.gameObject.GetComponent<Projectile>() == null)
            return;

        if (collision.relativeVelocity.magnitude >= breakSpeed)
        {
            Destroy(gameObject);
        }
    }
}