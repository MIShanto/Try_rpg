using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System;

public class Movement : MonoBehaviour
{
    /// Need to polish FOV.. TODO:.. 
    #region enums

    //character states..
    public enum characters
    {
        player, enemy, npc, boss, cutsceneCharacter, none
    }
    public characters character;
    [HideInInspector] public characters previousCharacter;

    //movement states..
    public enum MovementControls
    {
        walk, patrol, dash, roll, attack, block, chargedAttack, knockedOff, none
    }
    public MovementControls MovementControl;
    [HideInInspector] public MovementControls previousState;

    #endregion

    #region Primitives

    //[HideInInspector] public float playerInteractionDistance; //TODO:..
    public float characterMoveSpeed; // used for player speed

    private int[] hitProbTable;
    private int destPoint = 0, total, cnt = 0; // for patrol enemies.. // total for critical hit
    private bool isReadyToGetNPCRandomDirection = true, waiting = false, enemyPlayerDistanceAdjustment = false, isReadyToSavePreviousState,
                 isReadyToThrow = true; // waiting is for charged attack waiting..
    private float tmpMoveSpeed, moveSpeed, angle = 0, npcWaitTime, mainEndDistance, mainSlowDownDistance, patrolWaitTime, waitBeforeGoingToDisObject,
                  waitAfterGoingToDisObject, dashTime, rollTime, knockTime; // mainEndDistance is the distance fixed at the start via inspector.. // movespeed used for player animation..

    [SerializeField] bool isCutsceneModeOn, cutsceneFixedFaceMode;
    [Tooltip("NPC wait time at the self position after destination reached")]
    [SerializeField] float npcStartWaitTime; // how much seconds npc will wait..

    [HideInInspector] public int criticalHitProb;
    [HideInInspector] public bool isCharacterControllable = true, isHealer = false, healing = false, characterHit = false, doInvestigation; // character hit for charged attack hit..
    [HideInInspector]
    public float callGangRadius, speedMultiplierDuringAttack, moveSpeedMultiplierDuringAttack = 1, nextAttackTime, attackIndex, getPushedForce,
                 dashSpeed, startDashTime, rollSpeed, startRollTime, waitTimeForThrow, knockSpeed, startKnockTime, chargedAttackSpeed, startAttackTime,
                 chargedAttackTime, chargedDistance, waitAfterAttackDuration, chargeAndLookoutArea, stamina, staminaDecreaseRate, speedFactor = 1,
                 enemyHealingAmount, fov, viewDistanceBack, viewDistanceFront;

    #endregion

    #region Non Primitives

    [HideInInspector] public Vector2 direction, myFacingDirection, otherCharacterFacingDirection, targetForDirection; // target for enemy direction;
    [SerializeField] LayerMask obstacleLayer;
    Vector3 PrevLocation; // character previous location on previous frame (used for enemy animation direction)..
    #endregion

    #region Components

    [SerializeField] Transform cutsceneDestinationTarget;
    [SerializeField] Transform[] patrolPoints;
    [Header("Diagonal 2 Points for NPC")]
    [Tooltip("For rectangle ABCD 1st point is A and 2nd point is D")]
    [SerializeField] Transform[] diagonalPoints;
    Transform targetForDirection_Transform;
    [HideInInspector]
    public Transform player, distractionObjPosition;



    #endregion

    #region Classes

    Coroutine healingCoroutine;
    CombatManager myCombatManager;
    AIDestinationSetter aIDestinationSetter;
    Seeker seeker;
    Rigidbody2D rb;
    Animator animator;
    [HideInInspector] public AIPath path;
    [SerializeField] FieldOfView m_Fov;

    #endregion

    private void Start()
    {
        m_Fov = Instantiate(m_Fov, Vector3.zero, Quaternion.identity);
        m_Fov.SetFoV(fov);
        m_Fov.SetFrontViewDistance(viewDistanceFront);
        m_Fov.SetBackViewDistance(viewDistanceBack);
        GetComponentsAtStart();

        targetForDirection_Transform = new GameObject().transform;

        player = (GameObject.FindGameObjectWithTag("Player")).transform;

        //previousCharacter = character;
        //previousState = MovementControl;

        if (path != null)
        {
            mainEndDistance = path.endReachedDistance;
        }

        //critical hit section
        hitProbTable = new int[2];
        hitProbTable[0] = 100 - criticalHitProb;  //non critical hit prob
        hitProbTable[1] = criticalHitProb; //critical hit prob
        total = hitProbTable[0] + hitProbTable[1];

        if (character == characters.enemy)
        {
            MovementControl = MovementControls.patrol;
        }

        if (character == characters.npc)
        {
            npcWaitTime = npcStartWaitTime;
            GetRandomPointToMoveNPC();
        }

        ResetSession();

    }


    private void Update()
    {
        if (!myCombatManager.isDead)
        {

            if (isCharacterControllable || Vector2.Distance(transform.position, player.transform.position) <= chargeAndLookoutArea)
                AnimateMovement();

            if (!isCutsceneModeOn)
            {

                if (character == characters.player)
                {
                    DoPlayerStuffsInUpdate();
                }

                else if (character == characters.npc)
                {
                    DoNPCStuffsInUpdate();

                }

                else if (character == characters.enemy)
                {
                    DoEnemyStuffsInUpdate();

                }

            }
            else
            {
                CutsceneMode(cutsceneFixedFaceMode, cutsceneDestinationTarget);
            }

        }
        if (character == characters.enemy && MovementControl == MovementControls.patrol && !isCutsceneModeOn && !myCombatManager.isDead)
            m_Fov.gameObject.SetActive(true);
        else
            m_Fov.gameObject.SetActive(false);


    }

    private void FixedUpdate()
    {
        if (!myCombatManager.isDead)
        {
            if (!isCutsceneModeOn)
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
    }

    #region Player

    /// <summary>
    /// Actions will perfoem if character is player selected in update..
    /// </summary>
    private void DoPlayerStuffsInUpdate()
    {

        if (isCharacterControllable)
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
                attackIndex = 0;
                moveSpeedMultiplierDuringAttack = speedMultiplierDuringAttack;
                MovementControl = MovementControls.attack;
                //OnFreezeInputEnable();
            }

            //Input for doing charged attack
            else if (Input.GetKeyDown(KeyCode.P) && stamina >= staminaDecreaseRate)
            {
                attackIndex = 1;
                MovementControl = MovementControls.chargedAttack;
                OnFreezeInputEnable();
            }

            //Input for distraction mode.. 
            else if (Input.GetKey(KeyCode.Z) && isReadyToThrow)
            {
                //hold player on spot..
                if (characterMoveSpeed != 0)
                    tmpMoveSpeed = characterMoveSpeed;
                characterMoveSpeed = 0;

                //face player toward mouse position..
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                direction = (mousePos - (Vector2)transform.position);

                direction.Normalize();

                //click to fire..
                if (Input.GetMouseButtonDown(0))
                {
                    myCombatManager.LaunchDistractibleObject(mousePos);
                    StartCoroutine(WaitForAnotherThrow(tmpMoveSpeed));
                }

            }
            else if (Input.GetKeyUp(KeyCode.Z))
            {
                characterMoveSpeed = tmpMoveSpeed;
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
            if (myCombatManager.isBlocked)
                myCombatManager.OnBlockDisable();
        }
        //Input for change speed
        if (Input.GetKeyDown(KeyCode.T))
        {
            ChangeSpeed();
        }

    }

    /// <summary>
    /// Moves the player..
    /// </summary>
    private void MovePlayer()
    {
        rb.MovePosition((Vector2)transform.position +
            (direction * characterMoveSpeed * speedFactor * moveSpeedMultiplierDuringAttack * Time.deltaTime));

    }


    #endregion

    #region enemy


    void DoEnemyStuffsInUpdate()
    {
        path.enabled = true;
        aIDestinationSetter.enabled = true;
        seeker.enabled = true;



        if (MovementControl == MovementControls.patrol)
        {
            m_Fov.SetAimDirection(myFacingDirection);
            m_Fov.SetOrigin(transform.position);
            m_Fov.SetCharacter(this.gameObject);

            PerfomPatrol();
        }

        if (isCharacterControllable && MovementControl == MovementControls.walk) // target maramari kora.. :3
        {
            EnemyPathCheckForPlayer();
        }
        if (MovementControl == MovementControls.block)
            myCombatManager.OnBlockEnable();
        if (MovementControl == MovementControls.attack)
        {
            myCombatManager.performAttack(attackIndex, nextAttackTime);
        }

        if (enemyPlayerDistanceAdjustment)
            HandlePath();

        //for animation purpose get direction..
        //targetForDirection = aIDestinationSetter.target.transform.position;
    }
    private void DoEnemyStuffsInFixedUpdate()
    {


        if (MovementControl == MovementControls.chargedAttack)
        {
            PerfomChargedAttack();
        }

        if (MovementControl == MovementControls.knockedOff)
        {
            PerfomKnockOff(otherCharacterFacingDirection);
        }

    }

    private void PerfomPatrol()
    {
        if (patrolPoints != null && !doInvestigation)
        {
            path.endReachedDistance = 0.01f;

            if (Array.IndexOf(patrolPoints, aIDestinationSetter.target) < 0 || aIDestinationSetter.target == null)
                GotoNextPoint();
            // Choose the next destination point when the enemy gets
            // close to the current one.                
            if (path.reachedDestination && Vector2.Distance(transform.position, aIDestinationSetter.target.position) < 0.01f)
            {
                animator.SetFloat("Speed", 0);
                //isCutsceneModeOn = true;

                //wait for 1 sec brfore moving o next patrol point
                if (patrolWaitTime <= 0)
                {
                    patrolWaitTime = 1;
                    GotoNextPoint();
                }
                else
                {
                    patrolWaitTime -= Time.deltaTime;

                }
            }
            else
                animator.SetFloat("Speed", 1);

            // play move animation if more than one point..
            if (patrolPoints.Length == 1)
                animator.SetFloat("Speed", 0);


        }
        else if(doInvestigation)
        {
            if (path.remainingDistance < 100f)
            {
                

                if (waitBeforeGoingToDisObject <= 0)
                {
                    aIDestinationSetter.target = distractionObjPosition;

                    path.canMove = true;

                    waitBeforeGoingToDisObject = 2;
                    
                    MoveEnemy();
                }
                else if(aIDestinationSetter.target == null)
                {
                    path.canMove = false;

                    animator.SetFloat("Speed", 0f); // stop moving
                    //TODO: play alert animation..
                    waitBeforeGoingToDisObject -= Time.deltaTime;
                }

                if (aIDestinationSetter.target !=null && path.reachedDestination && Vector2.Distance(transform.position, aIDestinationSetter.target.position) < 0.01f)
                {
                    if (waitAfterGoingToDisObject <= 0)
                    {
                        doInvestigation = false;
                        waitAfterGoingToDisObject = 2;
                    }
                    else
                    {

                        animator.SetFloat("Speed", 0f); // stop moving
                                                        //TODO: play search animation..
                        waitAfterGoingToDisObject -= Time.deltaTime;
                    }
                    
                }

            }
            else
                doInvestigation = false;
        }
    }
    void GotoNextPoint()
    {
        // Returns if no points have been set up
        if (patrolPoints.Length == 0)
            return;


        // Set the enemy to go to the currently selected destination.
        aIDestinationSetter.target = patrolPoints[destPoint];

        // Choose the next point in the array as the destination,
        // cycling to the start if necessary.
        if (patrolPoints.Length > 1)
            destPoint = (destPoint + 1) % patrolPoints.Length;

    }


    /// <summary>
    /// This checks if enemy has reached destination or not..
    /// </summary>
    private void EnemyPathCheckForPlayer()
    {
        aIDestinationSetter.target = player;
        path.endReachedDistance = mainEndDistance;

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
                if (gameObject.tag == "Archer" &&
                           Vector2.Distance(transform.position, player.transform.position) <= 2f)
                {
                    Reposition();
                }

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

        animator.SetFloat("Speed", 1f);
    }

    #endregion

    #region npc

    private void DoNPCStuffsInUpdate()
    {
        NPCRandomMove();
    }

    /// <summary>
    /// randomly move within the boundary
    /// </summary>
    private void NPCRandomMove()
    {
        path.enabled = true;
        aIDestinationSetter.enabled = true;
        seeker.enabled = true;
        path.endReachedDistance = 0.01f;


        if (isReadyToGetNPCRandomDirection)
        {
            GetRandomPointToMoveNPC();
            isReadyToGetNPCRandomDirection = false;
        }


        aIDestinationSetter.target = targetForDirection_Transform;
        targetForDirection = aIDestinationSetter.target.transform.position;

        animator.SetFloat("Speed", 1f);


        if (path.remainingDistance <= 0.01f)
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

    private void GetRandomPointToMoveNPC()
    {
        targetForDirection_Transform.position = new Vector2(UnityEngine.Random.Range(diagonalPoints[0].position.x, diagonalPoints[1].position.x),
            UnityEngine.Random.Range(diagonalPoints[0].position.y, diagonalPoints[1].position.y));
    }

    #endregion

    #region Other Operations

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

            direction = transform.position - PrevLocation;
            PrevLocation = transform.position;

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
    /// This method completes the dash in the last facing direction.
    /// </summary>
    void PerfomDash()
    {
        if (dashTime <= 0)
        {
            dashTime = startDashTime;
            rb.velocity = Vector2.zero;
            cnt = 0;
            MovementControl = MovementControls.walk;
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
            MovementControl = MovementControls.walk;
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
        OnFreezeInputDisable();

        //Stops enemy movement animation.

        yield return new WaitForSeconds(waitAfterAttackDuration);
        MovementControl = MovementControls.walk;
        waiting = false;


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
            MovementControl = MovementControls.walk;
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
    /// This method prevents to throw anything till time limit pass..
    /// </summary>
    IEnumerator WaitForAnotherThrow(float previousSpeed)
    {
        isReadyToThrow = false;
        characterMoveSpeed = previousSpeed;
        yield return new WaitForSeconds(waitTimeForThrow);
        isReadyToThrow = true;


    }

    /// <summary>
    /// This is for cutscene mode 
    /// </summary>
    /// <param name="fixedFaceMode"></param>
    /// <param name="aiTarget"></param>
    void CutsceneMode(bool fixedFaceMode, Transform aiTarget)
    {
        //disable fov..
        m_Fov.gameObject.SetActive(false);
        //enable AI if disabled..
        if (!path.enabled && !aIDestinationSetter.enabled && !seeker.enabled)
        {
            path.enabled = true;
            aIDestinationSetter.enabled = true;
            seeker.enabled = true;
        }
        path.endReachedDistance = 0.5f;

        aIDestinationSetter.target = aiTarget;

        if (path.reachedDestination)
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
            targetForDirection = aiTarget.position;
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
        rb.velocity = Vector2.zero;
        if (path != null)
            path.enabled = false;
    }
    #endregion

    /// <summary>
    /// resets the stats of character
    /// </summary>
    public void ResetSession()
    {
        OnFreezeInputDisable();

        waitBeforeGoingToDisObject = 2f;
        waitAfterGoingToDisObject = 2f;
        patrolWaitTime = 1f;
        dashTime = startDashTime;
        rollTime = startRollTime;
        knockTime = startKnockTime;
        chargedAttackTime = startAttackTime;

        //MovementControl = MovementControls.walk;
        animator = GetComponent<Animator>();
        animator.SetBool("IsDead", false);

        //reset associated scripts
        myCombatManager.ResetSession();
    }
    private void GetComponentsAtStart()
    {
        myCombatManager = GetComponent<CombatManager>();
        aIDestinationSetter = GetComponent<AIDestinationSetter>();
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        path = GetComponent<AIPath>();
        animator = GetComponent<Animator>();
    }

}
