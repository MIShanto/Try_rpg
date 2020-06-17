using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public enum throwables
    {
        arrow
    }
    public throwables projectile;

    float angle;


    void LateUpdate()
    {
        if(projectile == throwables.arrow)
            performRotation();

    }

    #region Arrow

    /// <summary>
    /// This method finds the angle and rotate the arrow..
    /// </summary>
    private void performRotation()
    {
        transform.eulerAngles = new Vector3(0f, 0f, angle);
        Vector2 velocity = GetComponent<Rigidbody2D>().velocity;
        angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        

    }

    /// <summary>
    /// This method gets the initial angle of the projectile aka arrow..
    /// </summary>
    public void setRotation(float oldAngle)
    {
        angle = oldAngle;
    }

    #endregion
}
