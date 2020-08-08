using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] Transform arrowHead;
    Rigidbody2D rb;
    [SerializeField] float arrowHeadRange, arrowDamage;
    bool hitCounter = false, arrowStopped = false;
    [SerializeField] LayerMask hitLayer;

    float angle;
    Vector2 playerPos;
    public enum throwables
    {
        arrow
    }
    public throwables projectile;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void LateUpdate()
    {
        if(projectile == throwables.arrow)
            performRotation();

    }
    private void OnEnable()
    {
        hitCounter = false;
        arrowStopped = false;
        GetComponent<Rigidbody2D>().gravityScale = 1;
    }
    #region Arrow

    /// <summary>
    /// This method finds the angle and rotate the arrow..
    /// </summary>
    private void performRotation()
    {
        if (!arrowStopped)
        {
            transform.eulerAngles = new Vector3(0f, 0f, angle);
            Vector2 velocity = GetComponent<Rigidbody2D>().velocity;
            angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;

            //Used to manage arrow hit
            hitManagement();
        }
    }

    /// <summary>
    /// checks if hits or not.
    /// if not then stop arrow in previous player position
    /// </summary>
    private void hitManagement()
    {
        Collider2D hitObject = Physics2D.OverlapCircle(arrowHead.position, arrowHeadRange, hitLayer);

        if (hitObject != null)
        {

            if (!hitCounter)
            {
                hitObject.GetComponent<CombatManager>().TakeDamage(2, this.transform, Movement.MovementControls.none);
                arrowStopped = true;
                rb.velocity = Vector2.zero;
                rb.gravityScale = 0;
                hitCounter = true;
                StartCoroutine(waitToDeactive());

            }
        }
        else if (Vector2.Distance((Vector2)arrowHead.position, playerPos) <= .5f)
        {
            arrowStopped = true;
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0;
            StartCoroutine(waitToDeactive());
        }
    }
    IEnumerator waitToDeactive()
    {
        transform.eulerAngles = new Vector3(0f, 0f, angle);
        yield return new WaitForSeconds(2f);
        gameObject.SetActive(false);

    }

    /// <summary>
    /// sets the initial rotation of arrow
    /// </summary>

    public void setRotation(float oldAngle, Vector2 playerPos)
    {
        angle = oldAngle;
        this.playerPos = playerPos;
    }

    #endregion

    #region Gizmos
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(arrowHead.position, arrowHeadRange);
    }
    #endregion
}
