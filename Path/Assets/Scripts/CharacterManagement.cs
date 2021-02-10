using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class CharacterManagement : MonoBehaviour
{
    [Header("Player Section")]
    [SerializeField] GameObject playerPrefab;
    [SerializeField] float stamina, staminaDecreaseRate;
    [SerializeField] float speedMultiplierDuringAttack;
    [SerializeField] float attackDuration_player;
    [SerializeField] float getPushedForce_player;
    [SerializeField] float waitDurationToThrowObject;
    [SerializeField] float dashDuration;
    [SerializeField] float dashVelocity;
    [SerializeField] float rollDuration;
    [SerializeField] float rollVelocity;
    [SerializeField] float chargedAttackDuration_player;
    [SerializeField] float chargedAttackVelocity_player;
    [Tooltip("Duration of staying in knocked stage")]
    [SerializeField] float knockDuration;
    [Tooltip("moving velocity during knockedoff")]
    [SerializeField] float knockVelocity;
    [SerializeField] float health_player;
    [Range(0, 100)]
    [SerializeField] int criticalHitProbability;
    [SerializeField] int playerSwordDamage;
    
    [Header("Distraction Object Section")]
    [SerializeField] GameObject distractibleObject;
    [SerializeField] float primaryRange, secondaryRange;
    GameObject[] m_players;


    [Header("Archer Section")]
    [SerializeField] GameObject archerPrefab;
    [SerializeField] float callGangRadius_archer;
    [SerializeField] float attackDuration_archer;
    [SerializeField] float fovAngleFront_archer;
    [SerializeField] float fovAngleBack_archer;
    [SerializeField] float viewDistanceFront_archer;
    [SerializeField] float viewDistanceBack_archer;
    [SerializeField] float getPushedForce_archer;
    [SerializeField] float movementSpeed_archer;
    [SerializeField] float health_archer;
    [Range(0,100)][Tooltip("increase/decrease in every 0.2 second..")]
    [SerializeField] float alertIncreaseRate_archer, alertDecreaseRate_archer;
    GameObject[] m_archers;

    [Header("missile thrower Section")]
    [SerializeField] GameObject MTPrefab;
    [SerializeField] float callGangRadius_MT;
    [SerializeField] float fovAngleFront_MT;
    [SerializeField] float fovAngleBack_MT;
    [SerializeField] float viewDistanceFront_MT;
    [SerializeField] float viewDistanceBack_MT;
    [SerializeField] float attackDuration_MT;
    [SerializeField] float getPushedForce_MT;
    [SerializeField] float movementSpeed_MT;
    [SerializeField] float health_MT;
    [Range(0, 100)]
    [Tooltip("increase/decrease in every 0.2 second..")]
    [SerializeField] float alertIncreaseRate_MT, alertDecreaseRate_MT;
    GameObject[] m_missileThrowers;

    [Header("Swordsman Section")]
    [SerializeField] GameObject SUnitPrefab;
    [SerializeField] float callGangRadius_swordsman;
    [SerializeField] float fovAngleFront_swordsman;
    [SerializeField] float fovAngleBack_swordsman;
    [SerializeField] float viewDistanceFront_swordsman;
    [SerializeField] float viewDistanceBack_swordsman;
    [SerializeField] float attackDuration_swordsman;
    [SerializeField] float getPushedForce_swordsman;
    [SerializeField] float movementSpeed_swordsman;
    [SerializeField] float health_swordsman;
    [Range(0, 100)]
    [Tooltip("increase/decrease in every 0.2 second..")]
    [SerializeField] float alertIncreaseRate_swordsman, alertDecreaseRate_swordsman;
    [Tooltip("Applicable for healer only")]
    [SerializeField] float healingAmount;
    [SerializeField] int swordDamage;
    GameObject[] m_swordsmans;


    [Header("Charged enemy Section")]
    [SerializeField] GameObject CEPrefab;
    [SerializeField] float callGangRadius_CE;
    [SerializeField] float fovAngleFront_CE;
    [SerializeField] float fovAngleBack_CE;
    [SerializeField] float viewDistanceFront_CE;
    [SerializeField] float viewDistanceBack_CE;
    [SerializeField] float attackDuration_chargedEnemy;
    [SerializeField] float getPushedForce_CE;
    [SerializeField] float chargedAttackDuration_enemy;
    [SerializeField] float chargedAttackVelocity_enemy;
    [SerializeField] float chargedAttackDistance_enemy;
    [SerializeField] float waitAfterAttackDuration;
    [SerializeField] float lookingRangeDuringCharge;
    [SerializeField] float movementSpeed_CE;
    [SerializeField] float health_CE;
    [Range(0, 100)]
    [Tooltip("increase/decrease in every 0.2 second..")]
    [SerializeField] float alertIncreaseRate_CE, alertDecreaseRate_CE;
    GameObject[] m_chargedEnemies;
    //charged enemy bug fix korte hobe...

    [Header("poison damage")]
    [SerializeField] int poisonDamage;

    [Header("Projectile Section")]
    [SerializeField] GameObject arrowPrefab;
    [SerializeField] int arrowDamage;
    [SerializeField] bool SelfDestroyableArrow;
    [SerializeField] float selfDestroyTimeArrow, flightTime;
    [Space]
    [SerializeField] GameObject missilePrefab;
    [SerializeField] int missileDamage, missileLimit;
    [SerializeField] float missileSpeed, missileRotationSpeed;
    [SerializeField] bool SelfDestroyableMissile;
    [SerializeField] float selfDestroyTimeMissile;

    private void Awake()
    {
        #region player

        if (m_players == null)
            m_players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject m_player in m_players)
        {
            m_player.GetComponent<Movement>().stamina = stamina;
            m_player.GetComponent<Movement>().staminaDecreaseRate = staminaDecreaseRate;
            m_player.GetComponent<Movement>().speedMultiplierDuringAttack = speedMultiplierDuringAttack;
            m_player.GetComponent<Movement>().nextAttackTime = attackDuration_player;
            m_player.GetComponent<Movement>().getPushedForce = getPushedForce_player;
            m_player.GetComponent<Movement>().waitTimeForThrow = waitDurationToThrowObject;
            m_player.GetComponent<Movement>().startDashTime = dashDuration;
            m_player.GetComponent<Movement>().dashSpeed = dashVelocity;
            m_player.GetComponent<Movement>().startRollTime = rollDuration;
            m_player.GetComponent<Movement>().rollSpeed = rollVelocity;
            m_player.GetComponent<Movement>().startKnockTime = knockDuration;
            m_player.GetComponent<Movement>().knockSpeed = knockVelocity;
            m_player.GetComponent<Movement>().startAttackTime = chargedAttackDuration_player;
            m_player.GetComponent<Movement>().chargedAttackSpeed = chargedAttackVelocity_player;
            m_player.GetComponent<Movement>().criticalHitProb = criticalHitProbability;
            m_player.GetComponent<CombatManager>().maxHealth = health_player;
            m_player.GetComponent<CombatManager>().distractibleObject = distractibleObject;
        }

        #endregion

        #region archer

        if (m_archers == null)
            m_archers = GameObject.FindGameObjectsWithTag("Archer");

        foreach (GameObject m_archer in m_archers)
        {
            m_archer.GetComponent<AIPath>().maxSpeed = movementSpeed_archer;
            m_archer.GetComponent<Movement>().nextAttackTime = attackDuration_archer;
            m_archer.GetComponent<Movement>().callGangRadius = callGangRadius_archer;
            m_archer.GetComponent<Movement>().fovAngleFront = fovAngleFront_archer;
            m_archer.GetComponent<Movement>().fovAngleBack = fovAngleBack_archer;
            m_archer.GetComponent<Movement>().viewDistanceFront = viewDistanceFront_archer;
            m_archer.GetComponent<Movement>().viewDistanceBack = viewDistanceBack_archer;
            m_archer.GetComponent<CombatManager>().maxHealth = health_archer;
            m_archer.GetComponent<CombatManager>().alertIncreaseRate = alertIncreaseRate_archer;
            m_archer.GetComponent<CombatManager>().alertDecreaseRate = alertDecreaseRate_archer;
            m_archer.GetComponent<CombatManager>().arrowFlightTime = flightTime;
            m_archer.GetComponent<Movement>().getPushedForce = getPushedForce_archer;
        }

        #endregion

        #region missile thrower

        if(m_missileThrowers == null)
            m_missileThrowers = GameObject.FindGameObjectsWithTag("MissileThrower");

        foreach (GameObject m_missileThrower in m_missileThrowers)
        {
            m_missileThrower.GetComponent<AIPath>().maxSpeed = movementSpeed_MT;
            m_missileThrower.GetComponent<Movement>().nextAttackTime = attackDuration_MT;
            m_missileThrower.GetComponent<Movement>().callGangRadius = callGangRadius_MT;
            m_missileThrower.GetComponent<Movement>().fovAngleFront = fovAngleFront_MT;
            m_missileThrower.GetComponent<Movement>().fovAngleBack = fovAngleBack_MT;
            m_missileThrower.GetComponent<Movement>().viewDistanceFront = viewDistanceFront_MT;
            m_missileThrower.GetComponent<Movement>().viewDistanceBack = viewDistanceBack_MT;
            m_missileThrower.GetComponent<CombatManager>().maxHealth = health_MT;
            m_missileThrower.GetComponent<CombatManager>().alertIncreaseRate = alertIncreaseRate_MT;
            m_missileThrower.GetComponent<CombatManager>().alertDecreaseRate = alertDecreaseRate_MT;
            m_missileThrower.GetComponent<CombatManager>().missileLimit = missileLimit;
            m_missileThrower.GetComponent<Movement>().getPushedForce = getPushedForce_MT;
        }

        #endregion

        #region sworsman
        
        if (m_swordsmans == null)
            m_swordsmans = GameObject.FindGameObjectsWithTag("SwordsmanUnit");
        
        foreach (GameObject m_swordsman in m_swordsmans)
        {
            foreach (Transform s_man in m_swordsman.transform)
            {
                s_man.GetComponent<AIPath>().maxSpeed = movementSpeed_swordsman;
                s_man.GetComponent<Movement>().nextAttackTime = attackDuration_swordsman;
                s_man.GetComponent<Movement>().callGangRadius = callGangRadius_swordsman;
                s_man.GetComponent<Movement>().fovAngleFront = fovAngleFront_swordsman;
                s_man.GetComponent<Movement>().fovAngleBack = fovAngleBack_swordsman;
                s_man.GetComponent<Movement>().viewDistanceFront = viewDistanceFront_swordsman;
                s_man.GetComponent<Movement>().viewDistanceBack = viewDistanceBack_swordsman;
                s_man.GetComponent<Movement>().getPushedForce = getPushedForce_swordsman;
                s_man.GetComponent<Movement>().enemyHealingAmount = healingAmount;
                s_man.GetComponent<Movement>().criticalHitProb = criticalHitProbability;
                s_man.GetComponent<CombatManager>().maxHealth = health_swordsman;
                s_man.GetComponent<CombatManager>().alertIncreaseRate = alertIncreaseRate_swordsman;
                s_man.GetComponent<CombatManager>().alertDecreaseRate = alertDecreaseRate_swordsman;

            }
        }

        #endregion

        #region chargedEnemy

        if (m_chargedEnemies == null)
            m_chargedEnemies = GameObject.FindGameObjectsWithTag("ChargedEnemy");

        foreach (GameObject m_chargedEnemie in m_chargedEnemies)
        {
            m_chargedEnemie.GetComponent<Movement>().callGangRadius = callGangRadius_CE;
            m_chargedEnemie.GetComponent<Movement>().fovAngleFront = fovAngleFront_CE;
            m_chargedEnemie.GetComponent<Movement>().fovAngleBack = fovAngleBack_CE;
            m_chargedEnemie.GetComponent<Movement>().viewDistanceFront = viewDistanceFront_CE;
            m_chargedEnemie.GetComponent<Movement>().viewDistanceBack = viewDistanceBack_CE;
            m_chargedEnemie.GetComponent<Movement>().nextAttackTime = attackDuration_chargedEnemy;
            m_chargedEnemie.GetComponent<Movement>().getPushedForce = getPushedForce_CE;
            m_chargedEnemie.GetComponent<Movement>().startAttackTime = chargedAttackDuration_enemy;
            m_chargedEnemie.GetComponent<Movement>().chargedAttackSpeed = chargedAttackVelocity_enemy;
            m_chargedEnemie.GetComponent<Movement>().chargedDistance = chargedAttackDistance_enemy;
            m_chargedEnemie.GetComponent<Movement>().waitAfterAttackDuration = waitAfterAttackDuration;
            m_chargedEnemie.GetComponent<AIPath>().maxSpeed = movementSpeed_CE;
            m_chargedEnemie.GetComponent<Movement>().chargeAndLookoutArea = lookingRangeDuringCharge;
            m_chargedEnemie.GetComponent<CombatManager>().maxHealth = health_CE;
            m_chargedEnemie.GetComponent<CombatManager>().alertIncreaseRate = alertIncreaseRate_CE;
            m_chargedEnemie.GetComponent<CombatManager>().alertDecreaseRate = alertDecreaseRate_CE;
            m_chargedEnemie.GetComponent<Movement>().criticalHitProb = criticalHitProbability;

            if (lookingRangeDuringCharge > chargedAttackDistance_enemy)
            {
                Debug.LogError("!!!(looking Range During Charge) this distance must be less than charged Distance!!!");
            }
        }

        #endregion

        #region arrow

        if (arrowPrefab != null)
        {
            arrowPrefab.GetComponent<Projectile>().isSelfDestroyable = SelfDestroyableArrow;
            arrowPrefab.GetComponent<Projectile>().selfDestroyTime = selfDestroyTimeArrow;

        }

        #endregion

        #region missile

        if (missilePrefab != null)
        {
            missilePrefab.GetComponent<Projectile>().isSelfDestroyable = SelfDestroyableMissile;
            missilePrefab.GetComponent<Projectile>().selfDestroyTime = selfDestroyTimeMissile;
            missilePrefab.GetComponent<Projectile>().missileRotationSpeed = missileRotationSpeed;
            missilePrefab.GetComponent<Projectile>().missileSpeed = missileSpeed;

        }

        #endregion
        
        #region distraction object

        if (distractibleObject != null)
        {
            distractibleObject.GetComponent<Projectile>().dis_ObjPrimaryRange = primaryRange;
            distractibleObject.GetComponent<Projectile>().dis_ObjSecondaryRange = secondaryRange;


        }

        #endregion

    }

    private void Start()
    {
        #region Damage Handler Class

        DamageHandler.Instance.swordDamage = swordDamage;
        DamageHandler.Instance.arrowDamage = arrowDamage;
        DamageHandler.Instance.poisonDamage = poisonDamage;
        DamageHandler.Instance.missileDamage = missileDamage;
        DamageHandler.Instance.playerSwordDamage = playerSwordDamage;

        #endregion
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(archerPrefab.transform.position, callGangRadius_archer);
        Gizmos.DrawWireSphere(MTPrefab.transform.position, callGangRadius_MT);
        Gizmos.DrawWireSphere(SUnitPrefab.transform.position, callGangRadius_swordsman);
        Gizmos.DrawWireSphere(CEPrefab.transform.position, callGangRadius_CE);
        Gizmos.DrawWireSphere(distractibleObject.transform.position, primaryRange);
        Gizmos.DrawWireSphere(distractibleObject.transform.position, secondaryRange);
    }
}
