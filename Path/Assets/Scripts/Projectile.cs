using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class Projectile : MonoBehaviour
{
    [SerializeField] Transform head;
    [SerializeField] Transform targetForMissile;
    Rigidbody2D rb;
    [SerializeField] float headRange;
    [HideInInspector] public bool isSelfDestroyable;

    [HideInInspector] public float selfDestroyTime, missileSpeed, missileRotationSpeed, flightTime,
                                   dis_ObjPrimaryRange, dis_ObjSecondaryRange;
    bool hitCounter = false, arrowStopped = false;
    [SerializeField] LayerMask hitLayer, enemyLayer, obstacleLayer;
    [HideInInspector] public GameObject MT;

    float angle;
    Vector2 playerPos;
    public enum throwables
    {
        arrow, missile, distractionObject
    }
    public throwables projectile;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (isSelfDestroyable)
            StartCoroutine(RunSelfDestroy());
    }

    IEnumerator RunSelfDestroy()
    {
        yield return new WaitForSeconds(selfDestroyTime);
        gameObject.SetActive(false);
        if (projectile == throwables.missile)
            MT.GetComponent<CombatManager>().missileCount--;

    }

    void LateUpdate()
    {
        if (projectile == throwables.arrow & !arrowStopped)
        {
            performRotation();
            //Used to manage arrow hit
            HitManagement();
        }
        else if (projectile == throwables.distractionObject)
        {
            performRotation();
        }

        }
    private void FixedUpdate()
    {
        if (projectile == throwables.missile)
        {
            OnMissileLaunch();
            HitManagement();
        }
    }

    private void OnMissileLaunch()
    {
        Vector2 direction = (Vector2)targetForMissile.position - rb.position;

        direction.Normalize();

        float rotateAmount = Vector3.Cross(direction, transform.up).z;

        rb.angularVelocity = -rotateAmount * missileRotationSpeed;

        rb.velocity = transform.up * missileSpeed;
    }

    private void OnEnable()
    {
        if (projectile == throwables.arrow)
        {
            arrowStopped = false;
            GetComponent<Rigidbody2D>().gravityScale = 1;
        }

        hitCounter = false;
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

    #endregion

    public void SetFlightTime(float time)
    {
        flightTime = time;
        StartCoroutine(PerformNextTasksForDistraction());
    }
    IEnumerator PerformNextTasksForDistraction()
    {
        yield return new WaitForSeconds(flightTime);

        StopAndDisable();

        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(transform.position, dis_ObjPrimaryRange, enemyLayer);

        foreach (Collider2D obj in hitObjects)
        {
            
            RaycastHit2D ray = Physics2D.Raycast(transform.position, obj.gameObject.transform.position,
                Vector2.Distance(transform.position, obj.gameObject.transform.position), obstacleLayer);
            if (ray.collider != null)
            {
                // if find collider check for secondary distance..
                if (Vector2.Distance(transform.position, obj.gameObject.transform.position) <= dis_ObjSecondaryRange)
                {
                    PrepareForInvestigation(obj);

                }
            }
            else
            {
                PrepareForInvestigation(obj);
            }
        }
           

    }

    private void PrepareForInvestigation(Collider2D obj)
    {
        obj.GetComponent<Movement>().doInvestigation = true;
        obj.GetComponent<Movement>().distractionObjPosition = this.gameObject.transform;
        obj.GetComponent<AIDestinationSetter>().target = null;  
    }

    #region hit manager

    /// <summary>
    /// checks if hits or not.
    /// if not then stop arrow in previous player position
    /// </summary>
    private void HitManagement()
    {
        Collider2D hitObject = Physics2D.OverlapCircle(head.position, headRange, hitLayer);

        if (hitObject != null)
        {

            if (!hitCounter)
            {
                if (projectile == throwables.arrow)
                {
                    hitObject.GetComponent<CombatManager>().TakeDamage(2, this.transform, Movement.MovementControls.none);
                    arrowStopped = true;
                    rb.velocity = Vector2.zero;
                    rb.gravityScale = 0;
                    hitCounter = true;
                    StartCoroutine(waitToDeactive());
                }
                else if (projectile == throwables.missile)
                {
                    hitObject.GetComponent<CombatManager>().TakeDamage(4, this.transform, Movement.MovementControls.none);
                    hitCounter = true;
                    MT.GetComponent<CombatManager>().missileCount--;

                    gameObject.SetActive(false);
                }

            }
        }
        else if (Vector2.Distance((Vector2)head.position, playerPos) <= .5f)
        {
            arrowStopped = true;
            StopAndDisable();
        }
    }

    private void StopAndDisable()
    {
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0;
        StartCoroutine(waitToDeactive());
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
        if (head != null)
            Gizmos.DrawWireSphere(head.position, headRange);

    }
    #endregion
}
