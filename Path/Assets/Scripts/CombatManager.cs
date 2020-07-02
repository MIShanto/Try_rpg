using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    ObjectPooler objectPooler;
    Animator animator;
    Movement characterMovement;
    Coroutine attackCoroutine;
    Rigidbody2D rb;


    [Header("Hitbox section")]
    [Header("Insert in UP, RIGHT, DOWN, LEFT sequence in hitbox array")]
    [SerializeField][Tooltip("Last hitbox is for charged attack")] Transform[] hitboxes;

    [SerializeField] float leftHitboxRange, downHitboxRange, rightHitboxRange, upHitboxRange, midHitboxRange;
    [SerializeField] LayerMask hitLayer;

    [Header("Health functionality")]
    public float maxHealth;
    public float currentHealth;
    [SerializeField] int swordDamage;
    [SerializeField] int chargedDamage;
    
    [HideInInspector] public bool isDead, isAttacking = false, isBlocked;
    

    Vector2 playerPos;
    Vector2 arrowPos;

    private void Start()
    {
        objectPooler = ObjectPooler.Instance;
        animator = GetComponent<Animator>();
        characterMovement = GetComponent<Movement>();
        rb = GetComponent<Rigidbody2D>();

        currentHealth = maxHealth;
        isDead = false;
        isBlocked = false;
    }

    private void FixedUpdate()
    {
        if(characterMovement.player != null)
            playerPos = characterMovement.player.transform.position;
        arrowPos = transform.position;
    }



    #region Combat
    public void performAttack(float attackIndex, float nextAttackTime)
    {
        if(!isAttacking)
            attackCoroutine = StartCoroutine(StartAttack(attackIndex,nextAttackTime));
    }

    /// <summary>
    /// This method sets the attack animation according to the attackIndex
    /// attackIndex are responsible for playing diferent kinds of attack animation within character Attack blend tree..
    /// </summary>
    IEnumerator StartAttack(float attackIndex, float nextAttackTime)
    {

        isAttacking = true;
        rb.velocity = Vector2.zero;

        //play attack animation..
        animator.SetBool("Attack", isAttacking);
        animator.SetFloat("AttackIndex", attackIndex);

        yield return new WaitForSeconds(nextAttackTime);
        StopAttack();

    }
    /// <summary>
    /// This method is set to stop the attack
    /// </summary>
    public void StopAttack()
    {
       
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            isAttacking = false;
            animator.SetBool("Attack", isAttacking);

            if (characterMovement.MovementControl != Movement.MovementControls.chargedAttack)
            {
                characterMovement.MovementControl = Movement.MovementControls.walk;
                characterMovement.OnFreezeInputDisable();
            }
            
        }
    }

    /// <summary>
    /// This is called when character performs block operation.
    /// Movement is paused
    /// </summary>
    public void OnBlockEnable()
    {
        isBlocked = true;
        animator.SetBool("IsBlocked", isBlocked);

        rb.velocity = Vector2.zero;
        if (characterMovement.path != null)
            characterMovement.path.enabled = false;
        animator.SetFloat("Speed", 0f);
    }
    public void OnBlockDisable()
    {
        isBlocked = false;
        animator.SetBool("IsBlocked", isBlocked);
        characterMovement.MovementControl = Movement.MovementControls.walk;
        if (characterMovement.path != null)
            characterMovement.path.enabled = false;

    }

    #endregion

    #region Hitbox

    /// <summary>
    /// This methods deals the damage to characters when hit..
    /// </summary>
    public void AttackUp()
    {
        Debug.Log("up");
        Collider2D[] hitArea = Physics2D.OverlapCircleAll(hitboxes[0].position, upHitboxRange, hitLayer);

        foreach (Collider2D hitObject in hitArea)
        {
            hitObject.GetComponent<CombatManager>().TakeDamage(swordDamage,this.transform, characterMovement.MovementControl);

        }

    }
    public void AttackRight()
    {
        Debug.Log("right");
        Collider2D[] hitArea = Physics2D.OverlapCircleAll(hitboxes[1].position, rightHitboxRange, hitLayer);

        foreach (Collider2D hitObject in hitArea)
        {
            hitObject.GetComponent<CombatManager>().TakeDamage(swordDamage, this.transform, characterMovement.MovementControl);

        }

    }
    public void AttackDown()
    {
        Debug.Log("down");
        Collider2D[] hitArea = Physics2D.OverlapCircleAll(hitboxes[2].position, downHitboxRange, hitLayer);

        foreach (Collider2D hitObject in hitArea)
        {
            hitObject.GetComponent<CombatManager>().TakeDamage(swordDamage, this.transform, characterMovement.MovementControl);

        }

    }
    public void AttackLeft()
    {
        Debug.Log("left");
        Collider2D[] hitArea = Physics2D.OverlapCircleAll(hitboxes[3].position, leftHitboxRange, hitLayer);

        foreach (Collider2D hitObject in hitArea)
        {
            hitObject.GetComponent<CombatManager>().TakeDamage(swordDamage, this.transform, characterMovement.MovementControl);

        }

    }
    public void ChargeAttack()
    {
        Debug.Log("mid");
        Collider2D hitObject = Physics2D.OverlapCircle(hitboxes[4].position, midHitboxRange, hitLayer);

        if (hitObject != null)
        {
            hitObject.GetComponent<CombatManager>().TakeDamage(chargedDamage, this.transform, characterMovement.MovementControl);
            characterMovement.characterHit = true;
            //characterMovement.chargedAttackTime = -1f;
            
            //StopAttack();

        }


    }

    //Get visuals of the hitboxes area..
    private void OnDrawGizmos()
    {
        if (hitboxes.Length != 0 ) {
            Gizmos.DrawWireSphere(hitboxes[0].position, upHitboxRange);
            Gizmos.DrawWireSphere(hitboxes[1].position, rightHitboxRange);
            Gizmos.DrawWireSphere(hitboxes[2].position, downHitboxRange);
            Gizmos.DrawWireSphere(hitboxes[3].position, leftHitboxRange);
            if(hitboxes.Length == 5)
                Gizmos.DrawWireSphere(hitboxes[4].position, midHitboxRange);
        }
    }
    #endregion

    #region Health system



    /// <summary>
    /// This method takes the damage by characters and handles damage afterward animations..
    /// characters dont get damage when blocking is enable
    /// </summary>
    public void TakeDamage(float damage, Transform otherCharacter, Movement.MovementControls AttackState)
    {
        // if player hits enemy then this section of codes will activate..
        if(characterMovement.character != Movement.characters.player)
        {
            if (!isDead)
            {
                characterMovement.otherCharacterFacingDirection =
                            otherCharacter.GetComponent<Movement>().myFacingDirection;

                if (!isBlocked)
                {
                    //decrease health..
                    currentHealth -= damage;

                    // DEMO...(knock off)
                    if (AttackState == Movement.MovementControls.chargedAttack && currentHealth > 0)
                    {
                        //Debug.Log(otherCharacter.GetComponent<Movement>().myFacingDirection); 

                        characterMovement.MovementControl = Movement.MovementControls.knockedOff;
                        characterMovement.OnFreezeInputEnable();
                    }
                    else
                    {
                        animator.SetTrigger("GetHit");
                        characterMovement.OnHitPush();
                    }
                }
                else
                {
                    //decrease half health..
                    currentHealth -= damage*0.5f;

                    OnBlockDisable();
                    characterMovement.OnHitPush();
                }
                
            }
        }
        // if enemy hits player then this section of codes will activate..
        else
        {
            if(!isDead)
            { 
                currentHealth -= damage;

                if (AttackState == Movement.MovementControls.chargedAttack && currentHealth>0)
                {
                    //Debug.Log(otherCharacter.GetComponent<Movement>().facingDirection); 
                    characterMovement.otherCharacterFacingDirection =
                            otherCharacter.GetComponent<Movement>().myFacingDirection;
                    characterMovement.MovementControl = Movement.MovementControls.knockedOff;
                    characterMovement.OnFreezeInputEnable();
                }
                else if (AttackState != Movement.MovementControls.none)
                {
                    characterMovement.otherCharacterFacingDirection =
                             otherCharacter.GetComponent<Movement>().myFacingDirection;
                    animator.SetTrigger("GetHit");
                    characterMovement.OnHitPush();
                }
            }
        }

        if (currentHealth <= 0)
        {
            isDead = true;
            Die();
        }

    }

    /// <summary>
    /// Handles death animation of character..
    /// </summary>
    private void Die()
    {
        //play die animation..
        animator.SetBool("IsDead", isDead);

        StopAllCoroutines();

        //destroy enemy..
        if(gameObject.tag != "Player")
            Destroy(gameObject, 2f);
    }


    #endregion

    #region Archer

    /// <summary>
    /// this method is called during archer attack animation via trigger..
    /// </summary>
    public void LaunchArrow()
    {
        GameObject arrow = objectPooler.SpawnFromPool("Arrow", arrowPos, Quaternion.identity);

        Vector2 Vo = calculateVelocity(arrowPos, playerPos, 1);

        arrow.GetComponent<Rigidbody2D>().velocity = Vo;

        arrow.GetComponent<Projectile>().setRotation(Mathf.Acos(Vo.x / Vo.magnitude) * Mathf.Rad2Deg, playerPos);
    }

    /// <summary>
    /// Used to calculate the velocity vector and the trajectory..
    /// </summary>
    private Vector2 calculateVelocity(Vector2 arrowPos, Vector2 playerPos, float fightTime)
    {
        Vector2 distance = playerPos - arrowPos;
        Vector2 distanceX = distance;
        distanceX.y = 0f;

        float Sy = distance.y;
        float Sx = distanceX.magnitude;

        float Vx = Sx / fightTime;
        float Vy = Sy / fightTime + 0.5f * Mathf.Abs(Physics.gravity.y) * fightTime;

        Vector2 result = distanceX.normalized;
        result *= Vx;
        result.y = Vy;


        return result;
    }

    #endregion



}
