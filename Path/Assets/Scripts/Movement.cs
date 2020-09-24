using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System;

public class Movement : MonoBehaviour
{
    public enum characters
    {
        player,enemy, npc, none
    }
    public characters character;
    [HideInInspector] public characters previousCharacter;


    [HideInInspector] public AIPath path;
    AIDestinationSetter aIDestinationSetter;
    Seeker seeker;
    CombatManager myCombatManager;

    Rigidbody2D rb;
    Animator animator;

    [SerializeField] Transform cutsceneDestinationTarget;

    [HideInInspector]
    public Transform player;

    [HideInInspector] public  float speedMultiplierDuringAttack, moveSpeedMultiplierDuringAttack = 1;
    float moveSpeed;
    public float characterMoveSpeed;


    [HideInInspector]public float nextAttackTime, attackIndex, getPushedForce;

    [HideInInspector]public float dashSpeed, startDashTime;
    float dashTime;

    [HideInInspector] public float rollSpeed, startRollTime;
    float rollTime;

    [HideInInspector] public float knockSpeed, startKnockTime;
    float knockTime;

    [HideInInspector] public float chargedAttackSpeed, startAttackTime, chargedAttackTime, chargedDistance,
        waitAfterAttackDuration,  chargeAndLookoutArea, stamina, staminaDecreaseRate;

    // charged attack var start
    bool waiting = false; 
    [HideInInspector]public bool characterHit = false;
    // charged attack var end

    //critical hit section
    int[] hitProbTable;
    int total;
    [HideInInspector] public int criticalHitProb;
    
    //end

    [SerializeField] LayerMask obstacleLayer;
    [HideInInspector]public float enemyHealingAmount;

    [HideInInspector] public Vector2 direction, myFacingDirection, otherCharacterFacingDirection,  targetForDirection; // target for enemy direction;
    [HideInInspector] public bool isCharacterControllable = true, isHealer = false, healing = false;
    [HideInInspector] public float speedFactor = 1;
    int cnt = 0;
    bool enemyPlayerDistanceAdjustment = false;
    Coroutine healingCoroutine;

    [SerializeField] bool isCutsceneModeOn;
    [SerializeField] bool cutsceneFixedFaceMode;
    bool isReadyToSavePreviousState;
    bool isReadyToGetNPCRandomDirection = true;
    float mainEndDistance, angle = 0, npcWaitTime;
    [Tooltip("NPC wait time at the self position after destination reached")]
    [SerializeField] float npcStartWaitTime;
    public Transform minX;
    public Transform maxX;
    public Transform minY;
    public Transform maxY;
    [SerializeField] float npcMoveSpeed;


    //movement states..
    public  enum MovementControls
    {
        walk, dash, roll, attack, block, chargedAttack, knockedOff, none
    }
    [HideInInspector]public  MovementControls MovementControl;

    private void Start()
    {
        previousCharacter = character;
        player = (GameObject.FindGameObjectWithTag("Player")).transform;

        aIDestinationSetter = GetComponent<AIDestinationSetter>();
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        myCombatManager = GetComponent<CombatManager>();
        path = GetComponent<AIPath>();
        animator = GetComponent<Animator>();

        if(path != null)
            mainEndDistance = path.endReachedDistance;

        //Call reset session method to reset things
        RestSession();

        //critical hit section
        hitProbTable = new int[2];
        hitProbTable[0] = 100 - criticalHitProb;  //non critical hit prob
        hitProbTable[1] = criticalHitProb; //critical hit prob
        total = hitProbTable[0] + hitProbTable[1];

        npcWaitTime = npcStartWaitTime;
        if (minX != null && minY != null && maxX != null && maxY != null)
        {
            targetForDirection = new Vector2(UnityEngine.Random.Range(minX.position.x, maxX.position.x), UnityEngine.Random.Range(minY.position.y, maxY.position.y));  
        }
        else
            Debug.LogError("Set boundary for npc movement " + gameObject.name);


    }

    void Update()
    {
       
        if (!myCombatManager.isDead)
        {
            if (character == characters.player)
            {

                DoPlayerStuffsInUpdate();
            }

            else if (character == characters.npc)
            {
                DoNPCStuffsInUpdate();
            }

            if (isCharacterControllable || Vector2.Distance(transform.position, player.transform.position) <= chargeAndLookoutArea)
                AnimateMovement();

            if (isCutsceneModeOn)
            {
                if(isReadyToSavePreviousState)
                    previousCharacter = character;
                isReadyToSavePreviousState = false;

                character = characters.none;
                //set end reach distance to 1
                path.endReachedDistance = 1;
                CutsceneMode(cutsceneFixedFaceMode, cutsceneDestinationTarget);
            }
            else
            {

                if (!isReadyToSavePreviousState)
                {
                    character = previousCharacter;
                    isReadyToGetNPCRandomDirection = true;
                }
                isReadyToSavePreviousState = true;

                if (character == characters.enemy)
                {
                    //set end distance to previous one for enemy
                    path.endReachedDistance = mainEndDistance;
                    aIDestinationSetter.target = player;
                    targetForDirection = player.position;
                }
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

       
    }

    /// <summary>
    /// Actions will perfoem if character is player selected in fixed update to handle physics system properly..
    /// </summary>

    private void DoPlayerStuffsInFixedUpdate()
    {
        path.enabled = false;
        aIDestinationSetter.enabled = false;
        seeker.enabled = false;

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
    /// Moves the player..
    /// </summary>
    private void MovePlayer()
    {
        rb.velocity = direction * characterMoveSpeed * speedFactor * moveSpeedMultiplierDuringAttack;

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
                moveSpeedMultiplierDuringAttack = speedMultiplierDuringAttack;
                //OnFreezeInputEnable();
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

    #region npc

    private void DoNPCStuffsInUpdate()
    {
        if (path.enabled && aIDestinationSetter.enabled && seeker.enabled)
        {
            path.enabled = false;
            aIDestinationSetter.enabled = false;
            seeker.enabled = false;
        }
        NPCRandomMove();
    }
    /// <summary>
    /// randomly move within the boundary
    /// </summary>
    private void NPCRandomMove()
    {
        path.enabled = false;
        aIDestinationSetter.enabled = false;
        seeker.enabled = false;

        if (isReadyToGetNPCRandomDirection)
        {
            targetForDirection = new Vector2(UnityEngine.Random.Range(minX.position.x, maxX.position.x), UnityEngine.Random.Range(minY.position.y, maxY.position.y));
            isReadyToGetNPCRandomDirection = false;
        }


        transform.position = Vector2.MoveTowards(transform.position, targetForDirection, npcMoveSpeed * Time.deltaTime);

        animator.SetFloat("Speed", 1f);


        if (Vector2.Distance(transform.position, targetForDirection) <= 0.2f)
        {
            if (npcWaitTime <= 0)
            {

                isReadyToGetNPCRandomDirection = true;
                npcWaitTime = npcStartWaitTime;
            }
            else
            {
                npcWaitTime -= Time.deltaTime;
                animator.SetFloat("Speed", 0f);
            }
        }
    }

    #endregion

    #region enemy

    private void DoEnemyStuffsInFixedUpdate()
    {

        path.enabled = true;
        aIDestinationSetter.enabled = true;
        seeker.enabled = true;


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
    /// This checks if enemy has reached destination or not..
    /// </summary>
    private void EnemyPathCheck()
    {
       
            //if destination reached enemy will play attack animation otherwise play move animation
            if (!path.reachedDestination && !player.GetComponent<CombatManager>().isDead)
            {

                if (gameObject.tag == "ChargedEnemy" &&
                        !Physics2D.Raycast(transform.position, direction,
                        Vector2.Distance(transform.position, player.transform.position), obstacleLayer) &&
                        Vector2.Distance(transform.position, player.transform.position) <= chargedDistance)
                {
                    AttackEnemySettings();
                    animator.SetTrigger("GetCharged");
                }
                else
                {
                    MoveEnemy();
                }
            }
            else
            {
                if (!player.GetComponent<CombatManager>().isDead)
                {
                    if (gameObject.tag == "ChargedEnemy")
                    {
                        AttackEnemySettings();
                        animator.SetTrigger("GetCharged");
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
                            path.endReachedDistance = 5f;
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
    /// This method makes the enemy to go to charged attack state and do the attack.
    /// </summary>
    private void OnChargedAttackEnable()
    {
        MovementControl = MovementControls.chargedAttack;
        
    }

    /// <summary>
    /// Enemy heals slowly (it will wait half a second each time)...
    /// </summary>
    /// <returns></returns>
    IEnumerator SlowHeal()
    {
        while (healing && myCombatManager.currentHealth > 0f && 
            myCombatManager.currentHealth < myCombatManager.maxHealth)
        {
            yield return new WaitForSeconds(0.8f);

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
    /// Method to Animate player sprites during movement.
    /// </summary>
    private void AnimateMovement()
    {
        if (character == characters.player)
        {

            animator.SetFloat("Speed", moveSpeed);
        }

        else
        {
            // aro better kisu paile replace kore nite hobe..(need optimization).. 
            //direction = new Vector2(rb.velocity.x, rb.velocity.y);
            direction = (targetForDirection - (Vector2)transform.position);

            direction.Normalize();

            // for making animation not to overlap
            if (direction.x > 0)
            {
                if (direction.y > 0.5f)
                {
                    direction.y = 1;
                    direction.x = 0;
                }
                else if (direction.y < -0.5f)
                {
                    direction.y = -1;
                    direction.x = 0;
                }
                else
                {
                    direction.y = 0;
                    direction.x = 1;
                }
            }
            else if (direction.x < 0)
            {
                if (direction.y > 0.5f)
                {
                    direction.y = 1;
                    direction.x = 0;
                }
                else if (direction.y < -0.5f)
                {
                    direction.y = -1;
                    direction.x = 0;
                }
                else
                {
                    direction.y = 0;
                    direction.x = -1;
                }
            }
        }

        if (direction != Vector2.zero)
        {
            animator.SetFloat("Horizontal", direction.x);
            animator.SetFloat("Vertical", direction.y);
            myFacingDirection = direction;

        }

        

    }

    /// <summary>
    /// This is for cutscene mode 
    /// </summary>
    /// <param name="fixedFaceMode"></param>
    /// <param name="aiTarget"></param>
    void CutsceneMode(bool fixedFaceMode, Transform aiTarget)
    {
        if (!path.enabled && !aIDestinationSetter.enabled && !seeker.enabled)
        {
            path.enabled = true;
            aIDestinationSetter.enabled = true;
            seeker.enabled = true;
        }

        if (GetComponent<AIPath>().reachedDestination)
        {
            animator.SetFloat("Speed", 0f);

            if (!fixedFaceMode)
            {

                float speed = (2 * Mathf.PI) / 15;  //2*PI in degress is 360, so 15 seconds to complete a circle
                float radius = 50;

                angle += speed * Time.deltaTime; // to switch direction, use -= instead of +=
                float x = Mathf.Cos(angle) * radius;
                float y = Mathf.Sin(angle) * radius;

                targetForDirection = new Vector2(x, y);

            }
            else
            {
                targetForDirection = player.position;
            }

        }
        else
        {
            MoveEnemy();
            aIDestinationSetter.target = aiTarget;
            targetForDirection = aiTarget.position;
        }

    }

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
                animator.SetBool("Attack", false);
                characterHit = false;
                chargedAttackTime = startAttackTime;
                rb.velocity = Vector2.zero;

                cnt = 0;
                stamina -= staminaDecreaseRate;

                StartCoroutine(WaitAfterAttack());
            }
            else
            {

                if (!characterHit)
                {
                    
                    myCombatManager.ChargeAttack();
                }

                chargedAttackTime -= Time.deltaTime;

                if (cnt == 0)
                {
                    animator.SetBool("Attack", true);
                    animator.SetFloat("AttackIndex", attackIndex);
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
        OnFreezeInputDisable();
        MovementControl = MovementControls.walk;
        waiting = false;

        
    }

    /// <summary>
    /// knockback characters.
    /// </summary>
    public void OnHitPush(bool isHitCritical)
    {
        if (myCombatManager.currentHealth >= 1f)
        {
            float hitMultiplier = 1f;

            if (isHitCritical)
            {
                hitMultiplier = 5f;
                CinemachineShake.Instance.ShakeCamera(5, .2f); // parameter (intensity, shake time)
            }
            else
                hitMultiplier = 1f;

            OnFreezeInputEnable();

            rb.AddForce(otherCharacterFacingDirection * getPushedForce * hitMultiplier);

            OnFreezeInputDisable();
        }
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

    /// <summary>
    /// returns the probability of critical hit
    /// </summary>
    public bool GetHitProb()
    {
        int randomNumber = UnityEngine.Random.Range(0, total);

        for (int i = 0; i < hitProbTable.Length; i++)
        {
            if (randomNumber <= hitProbTable[i])
            {
                if (i == 0)
                {
                    Debug.Log("Non critical hit occured!!");
                }

                else
                {
                    Debug.Log("Critical hit occcured!!");
                    return true;
                }

            }
            else
                randomNumber -= hitProbTable[i];
        }
        return false;
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
        rb.velocity = Vector2.zero;
        if (path != null)
            path.enabled = false;
    }
    #endregion
    
    /// <summary>
    /// resets the stats of character
    /// </summary>
    public void RestSession()
    {
        OnFreezeInputDisable();

        dashTime = startDashTime;
        rollTime = startRollTime;
        knockTime = startKnockTime;
        chargedAttackTime = startAttackTime;

        MovementControl = MovementControls.walk;
        animator = GetComponent<Animator>();
        animator.SetBool("IsDead", false);

        //reset associated scripts
        myCombatManager.ResetSession();
    }
}
