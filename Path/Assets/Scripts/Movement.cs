using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class Movement : MonoBehaviour
{
    public enum characters
    {
        player,enemy
    }
    public characters character;

    [HideInInspector] public AIPath path;
    [HideInInspector]
    public GameObject player;
    CombatManager myCombatManager;

    Rigidbody2D rb;
    Animator animator;

    [HideInInspector] public  float characterMoveSpeed;
    float moveSpeed;

    [HideInInspector]public float nextAttackTime, attackIndex;

    [HideInInspector]public float dashSpeed, startDashTime;
    float dashTime;

    [HideInInspector] public float rollSpeed, startRollTime;
    float rollTime;

    [HideInInspector] public float knockSpeed, startKnockTime;
    float knockTime;

    [HideInInspector] public float chargedAttackSpeed, startAttackTime, chargedAttackTime, chargedDistance, waitAfterAttackDuration;
    [SerializeField] public float stamina;
    float staminaDecreaseRate = 20;
    bool waiting = false;

    [SerializeField] LayerMask obstacleLayer;
    [HideInInspector]public float enemyHealingAmount;

    [HideInInspector] public Vector2 direction, myFacingDirection, otherCharacterFacingDirection;
    [HideInInspector] public bool isCharacterControllable = true, isHealer = false, healing = false;
    [HideInInspector] public float speedFactor = 1;
    int cnt = 0;
    bool enemyPlayerDistanceAdjustment = false;
    Coroutine healingCoroutine;


    //movement states..
    public  enum MovementControls
    {
        walk, dash, roll, attack, block, chargedAttack, knockedOff, none
    }
    public  MovementControls MovementControl;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        myCombatManager = GetComponent<CombatManager>();
        animator = GetComponent<Animator>();

        player = GameObject.FindGameObjectWithTag("Player");
        path = GetComponent<AIPath>();

        dashTime = startDashTime;
        rollTime = startRollTime;
        knockTime = startKnockTime;
        chargedAttackTime = startAttackTime;

        MovementControl = MovementControls.walk;

    }

    void Update()
    {
        if (!myCombatManager.isDead)
        {
            if (character == characters.player)
            {
                DoPlayerStuffsInUpdate();
            }
            else if (character == characters.enemy)
            {
                DoEnemyStuffsInUpdate();
            }
        }
    }

    private void FixedUpdate()
    {
        if (!myCombatManager.isDead)
        {
            if (character == characters.player)
            {
                DoPlayerStuffsInFixedUpdate();
            }
            else if (character == characters.enemy)
            {
                DoEnemyStuffsInFixedUpdate();
            }
        }
    }


    #region player

    /// <summary>
    /// Actions will perfoem if character is player selected in update..
    /// </summary>
    private void DoPlayerStuffsInUpdate()
    {
        if(isCharacterControllable)
                 ProcessInput();

        if (MovementControl == MovementControls.attack)
            myCombatManager.performAttack(attackIndex, nextAttackTime / speedFactor);

        AnimateMovementPlayer();

       
    }

    /// <summary>
    /// Actions will perfoem if character is player selected in fixed update to handle physics system properly..
    /// </summary>

    private void DoPlayerStuffsInFixedUpdate()
    {
        if (MovementControl == MovementControls.walk && isCharacterControllable)
            MovePlayer();
        else if (MovementControl == MovementControls.dash)
            PerfomDash();
        else if (MovementControl == MovementControls.roll)
            PerfomRoll();
        else if (MovementControl == MovementControls.block)
            myCombatManager.OnBlockEnable();
        else if (MovementControl == MovementControls.chargedAttack)
            PerfomChargedAttack();
        else if (MovementControl == MovementControls.knockedOff)
            PerfomKnockOff(otherCharacterFacingDirection);

    }



    /// <summary>
    /// Method to Animate player sprites during movement.
    /// </summary>
    private void AnimateMovementPlayer()
    {
        if (direction != Vector2.zero)
        {
            animator.SetFloat("Horizontal", direction.x);
            animator.SetFloat("Vertical", direction.y);
            myFacingDirection = direction;
            
        }
        animator.SetFloat("Speed",moveSpeed);
    }

    /// <summary>
    /// Moves the player..
    /// </summary>
    private void MovePlayer()
    {
        rb.velocity = direction * characterMoveSpeed * speedFactor;

    }

    /// <summary>
    /// Handles the input for player..
    /// </summary>
    private void ProcessInput()
    {
        if (MovementControl != MovementControls.block)
        {
            //Input for doing dash..
            if (Input.GetKeyDown(KeyCode.Space))
            {
                MovementControl = MovementControls.dash;
                OnFreezeInputEnable();
            }

            //Input for doing roll..
            else if (Input.GetKeyDown(KeyCode.R))
            {
                MovementControl = MovementControls.roll;
                OnFreezeInputEnable();
            }

            //Input for doing attack
            else if (Input.GetKeyDown(KeyCode.RightShift))
            {
                MovementControl = MovementControls.attack;
                attackIndex = 0;
                OnFreezeInputEnable();
            }
            
            //Input for doing charged attack
            else if (Input.GetKeyDown(KeyCode.P) && stamina >= staminaDecreaseRate)
            {
                attackIndex = 1;
                MovementControl = MovementControls.chargedAttack;
                OnFreezeInputEnable();
            }
           
            //Input for movement of the player..
            else
            {

                direction.x = Input.GetAxisRaw("Horizontal");
                direction.y = Input.GetAxisRaw("Vertical");

                direction.Normalize();

                moveSpeed = Mathf.Clamp(direction.magnitude, 0.0f, 1.0f);

                MovementControl = MovementControls.walk;
            }
        }
        //Input for doing block
        if (Input.GetKey(KeyCode.B))
        {

            MovementControl = MovementControls.block;
        }
        else if (Input.GetKeyUp(KeyCode.B))
        {
            if(myCombatManager.isBlocked)
                myCombatManager.OnBlockDisable();
        }
        //Input for change speed
        if (Input.GetKeyDown(KeyCode.T))
        {
            ChangeSpeed();
        }
        

    }

    /// <summary>
    /// This method performs the attack operation
    /// </summary>

    #endregion

    #region enemy

    /// <summary>
    /// Actions will perfoem if character is enemy selected..
    /// </summary>
    private void DoEnemyStuffsInUpdate()
      {
        // movement animation
        AnimateMovementEnemy();

    }
    private void DoEnemyStuffsInFixedUpdate()
    {
        if (isCharacterControllable && MovementControl == MovementControls.walk)
        {
            EnemyPathCheck();
        }
        else if (MovementControl == MovementControls.chargedAttack)
        {
            PerfomChargedAttack();
        }
        else if (MovementControl == MovementControls.knockedOff)
        {
            PerfomKnockOff(otherCharacterFacingDirection);
        }
        else if (MovementControl == MovementControls.block)
            myCombatManager.OnBlockEnable();
        else if (MovementControl == MovementControls.attack)
        {
            myCombatManager.performAttack(attackIndex, nextAttackTime);
        }
        if(enemyPlayerDistanceAdjustment)
            HandlePath();
    }

    /// <summary>
    /// Animates the enemy character during movement..
    /// This need to polish.. will cover aste aste
    /// </summary>
    private void AnimateMovementEnemy()
    {
        // aro better kisu paile replace kore nite hobe..(need optimization).. 
        direction = (player.transform.position - transform.position);

        direction.Normalize();
        // used to set enemy face to the last direction
        if (direction != Vector2.zero)
        {
            animator.SetFloat("Horizontal", direction.x);
            animator.SetFloat("Vertical", direction.y);
            myFacingDirection = direction;
        }
    }

    /// <summary>
    /// This checks if enemy has reached destination or not..
    /// </summary>
    private void EnemyPathCheck()
    {
        //if destination reached enemy will play attack animation otherwise play move animation
        if (!path.reachedDestination && player != null && !player.GetComponent<CombatManager>().isDead)
        {


            if (gameObject.tag == "ChargedEnemy" &&
                    !Physics2D.Raycast(transform.position, direction,
                    Vector2.Distance(transform.position, player.transform.position), obstacleLayer) &&
                    Vector2.Distance(transform.position, player.transform.position) <= chargedDistance)
            {
                MovementControl = MovementControls.chargedAttack;
                AttackEnemySettings();
            }
            else
            {
                MoveEnemy();
            }
        }
        else
        {
            if (!player.GetComponent<CombatManager>().isDead && player != null)
            {
                if (gameObject.tag == "ChargedEnemy")
                {
                    MovementControl = MovementControls.chargedAttack;
                    AttackEnemySettings();
                }
                 /*if (gameObject.tag == "Archer" &&
                            Vector2.Distance(transform.position, player.transform.position) <= 2f)
                {
                    Reposition();
                }*/

                else if (gameObject.CompareTag("Swordman"))
                {
                    if (isHealer)
                    {
                        animator.SetFloat("Speed", 0f);
                        path.endReachedDistance = 3f;
                        if ((myCombatManager.currentHealth < myCombatManager.maxHealth) && !healing)
                        {
                            healing = true;

                            if (healingCoroutine != null)
                            {
                                StopCoroutine(healingCoroutine);
                            }
                            healingCoroutine = StartCoroutine(SlowHeal());
                        }
                    }
                    else
                    {
                        MovementControl = MovementControls.attack;
                        path.endReachedDistance = 1f;
                        AttackEnemySettings();
                    }
                }

                else
                {
                    MovementControl = MovementControls.attack;
                    AttackEnemySettings();
                }
            }
        }
    }
    /// <summary>
    /// Enemy heals slowly (it will wait half a second each time)...
    /// </summary>
    /// <returns></returns>
    IEnumerator SlowHeal()
    {
        while (healing && myCombatManager.currentHealth < myCombatManager.maxHealth)
        {
            yield return new WaitForSeconds(0.5f);

            myCombatManager.currentHealth += enemyHealingAmount;

            if (myCombatManager.currentHealth > myCombatManager.maxHealth)
            {
                myCombatManager.currentHealth = myCombatManager.maxHealth;
            }
        }

        healing = false;
    }

    public void Reposition()
    {
        MoveEnemy();
        enemyPlayerDistanceAdjustment = true;
    }

    /// <summary>
    /// This is used for archer type enemy where enemy will reposition if player comes too close to enemy.
    /// </summary>
    private void HandlePath()
    {
        OnFreezeInputEnable();
        rb.velocity = -myFacingDirection * 3f;

        if (Vector2.Distance(transform.position, player.transform.position) >= path.endReachedDistance)
        {
            OnFreezeInputDisable();
            enemyPlayerDistanceAdjustment = false;
        }
    }

    /// <summary>
    /// This is used to set attack animation of enemy and freeze input during attack
    /// </summary>
    private void AttackEnemySettings()
    {
        animator.SetFloat("Speed", 0f);
        attackIndex = 0;
        OnFreezeInputEnable();
    }

    /// <summary>
    /// This is used to move enemy and do some additional task related to movement..
    /// </summary>
    private void MoveEnemy()
    {
        myCombatManager.StopAttack();
        if (MovementControl != MovementControls.walk)
            MovementControl = MovementControls.walk;
        animator.SetFloat("Speed", 1f);
    }

    #endregion

    #region Other operations 

    /// <summary>
    /// This method completes roll in the last facing direction.
    /// </summary>
    private void PerfomRoll()
    {
        if (rollTime <= 0)
        {
            rollTime = startRollTime;
            rb.velocity = Vector2.zero;
            animator.SetBool("Roll", false);
            cnt = 0;
            GetComponent<Movement>().MovementControl = MovementControls.walk;
            OnFreezeInputDisable();
        }
        else
        {
            
            rollTime -= Time.deltaTime;

            if (cnt == 0)
            {
                animator.SetBool("Roll", true);
                rb.velocity = myFacingDirection * rollSpeed;
                cnt = 1;
            }
            
        }
    }

    /// <summary>
    /// This method completes the dash in the last facing direction.
    /// </summary>
    void PerfomDash()
    {
        if (dashTime <= 0)
        {
            dashTime = startDashTime;
            rb.velocity = Vector2.zero;
            cnt = 0;
            GetComponent<Movement>().MovementControl = MovementControls.walk;
            OnFreezeInputDisable();
        }
        else
        {
            dashTime -= Time.deltaTime;

            if (cnt == 0)
            {
                rb.velocity = myFacingDirection * dashSpeed;
                cnt = 1;
            }
        }
    }

    /// <summary>
    /// Performs the knockOff oeration for character..
    /// Add a foce to hit direction
    /// </summary>
    void PerfomKnockOff(Vector2 knockDirection)
    {
        if (knockTime <= 0)
        {
            knockTime = startKnockTime;
            rb.velocity = Vector2.zero;
            animator.SetBool("KnockOff", false);
            cnt = 0;
            GetComponent<Movement>().MovementControl = MovementControls.walk;
            OnFreezeInputDisable();
        }
        else
        {
            knockTime -= Time.deltaTime;

            if (cnt == 0)
            {

                animator.SetBool("KnockOff", true);
                rb.velocity = knockDirection * knockSpeed;
                cnt = 1;
            }
            
        }
    }

    /// <summary>
    /// This method completes the charged attack in the last facing direction.
    /// </summary>
    void PerfomChargedAttack()
    {
        if (!waiting)
        {
            if (chargedAttackTime <= 0)
            {

                chargedAttackTime = startAttackTime;
                rb.velocity = Vector2.zero;

                cnt = 0;
                stamina -= staminaDecreaseRate;

                StartCoroutine(WaitAfterAttack());
            }
            else
            {

                myCombatManager.performAttack(attackIndex, startAttackTime);
                myCombatManager.ChargeAttack();
                chargedAttackTime -= Time.deltaTime;
                if (cnt == 0)
                {
                    rb.velocity = myFacingDirection * chargedAttackSpeed;
                    cnt = 1;
                }
            }
        }
    }

    IEnumerator WaitAfterAttack()
    {
        waiting = true;

        //Stops enemy movement animation.

        yield return new WaitForSeconds(waitAfterAttackDuration);
        MovementControl = MovementControls.walk;
        OnFreezeInputDisable();
        waiting = false;

        
    }

    public void OnHitPush()
    {
        OnFreezeInputEnable();

        rb.AddForce(otherCharacterFacingDirection * 1000);

        OnFreezeInputDisable();
    }

    /// <summary>
    /// This method handles slow down effect.
    /// need to implement..
    /// </summary>
    public void ChangeSpeed()
    {
        if (speedFactor != 1)
        {
            speedFactor = 1;
            animator.SetFloat("SpeedChanger", speedFactor);
        }
        else
        {
            speedFactor = 0.5f;
            animator.SetFloat("SpeedChanger", speedFactor);
        }
    }

    #endregion

    #region Vulnerability 
    /// <summary>
    /// These method is used to restore all activity that is triggered by OnFreezeEnable() method..
    /// </summary>
    public void OnFreezeInputDisable()
    {
        isCharacterControllable = true;
        if (path != null)
            path.enabled = true;

    }
    /// <summary>
    /// These method prevents any others task to perfom during dead and hurt animation state(for now)..
    /// </summary>
    public void OnFreezeInputEnable()
    {
        isCharacterControllable = false;
        direction = Vector2.zero;

        if (path != null)
            path.enabled = false;
    }
    #endregion
}
