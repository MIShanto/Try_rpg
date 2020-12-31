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

    [Header("Swordsman Section")]
    [SerializeField] GameObject swordsmanPrefab;
    [SerializeField] float callGangRadius_swordsman;
    [SerializeField] float fovAngleFront_swordsman;
    [SerializeField] float fovAngleBack_swordsman;
    [SerializeField] float viewDistanceFront_swordsman;
    [SerializeField] float viewDistanceBack_swordsman;
    [SerializeField] float attackDuration_swordsman;
    [SerializeField] float getPushedForce_swordsman;
    [SerializeField] float movementSpeed_swordsman;
    [SerializeField] float health_swordsman;
    [Tooltip("Applicable for healer only")]
    [SerializeField] float healingAmount;
    [SerializeField] int swordDamage;


    [Header("Charged enemy Section")]
    [SerializeField] GameObject chargedEnemyPrefab;
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
        if (playerPrefab != null)
        {
            playerPrefab.GetComponent<Movement>().stamina = stamina;
            playerPrefab.GetComponent<Movement>().staminaDecreaseRate = staminaDecreaseRate;
            playerPrefab.GetComponent<Movement>().speedMultiplierDuringAttack = speedMultiplierDuringAttack;
            playerPrefab.GetComponent<Movement>().nextAttackTime = attackDuration_player;
            playerPrefab.GetComponent<Movement>().getPushedForce = getPushedForce_player;
            playerPrefab.GetComponent<Movement>().waitTimeForThrow = waitDurationToThrowObject;
            playerPrefab.GetComponent<Movement>().startDashTime = dashDuration;
            playerPrefab.GetComponent<Movement>().dashSpeed = dashVelocity;
            playerPrefab.GetComponent<Movement>().startRollTime = rollDuration;
            playerPrefab.GetComponent<Movement>().rollSpeed = rollVelocity;
            playerPrefab.GetComponent<Movement>().startKnockTime = knockDuration;
            playerPrefab.GetComponent<Movement>().knockSpeed = knockVelocity;
            playerPrefab.GetComponent<Movement>().startAttackTime = chargedAttackDuration_player;
            playerPrefab.GetComponent<Movement>().chargedAttackSpeed = chargedAttackVelocity_player;
            playerPrefab.GetComponent<Movement>().criticalHitProb = criticalHitProbability;
            playerPrefab.GetComponent<CombatManager>().maxHealth = health_player;
            playerPrefab.GetComponent<CombatManager>().distractibleObject = distractibleObject;


        }

        #endregion

        #region archer
        if (archerPrefab != null)
        {
            archerPrefab.GetComponent<AIPath>().maxAcceleration = movementSpeed_archer;
            archerPrefab.GetComponent<Movement>().nextAttackTime = attackDuration_archer;
            archerPrefab.GetComponent<Movement>().callGangRadius = callGangRadius_archer;
            archerPrefab.GetComponent<Movement>().fovAngleFront = fovAngleFront_archer;
            archerPrefab.GetComponent<Movement>().fovAngleBack = fovAngleBack_archer;
            archerPrefab.GetComponent<Movement>().viewDistanceFront = viewDistanceFront_archer;
            archerPrefab.GetComponent<Movement>().viewDistanceBack = viewDistanceBack_archer;
            archerPrefab.GetComponent<CombatManager>().maxHealth = health_archer;
            archerPrefab.GetComponent<CombatManager>().arrowFlightTime = flightTime;
            archerPrefab.GetComponent<Movement>().getPushedForce = getPushedForce_archer;
        }


        #endregion

        #region missile thrower
        if (MTPrefab != null)
        {
            MTPrefab.GetComponent<AIPath>().maxAcceleration = movementSpeed_MT;
            MTPrefab.GetComponent<Movement>().nextAttackTime = attackDuration_MT;
            MTPrefab.GetComponent<Movement>().callGangRadius = callGangRadius_MT;
            MTPrefab.GetComponent<Movement>().fovAngleFront = fovAngleFront_MT;
            MTPrefab.GetComponent<Movement>().fovAngleBack = fovAngleBack_MT;
            MTPrefab.GetComponent<Movement>().viewDistanceFront = viewDistanceFront_MT;
            MTPrefab.GetComponent<Movement>().viewDistanceBack = viewDistanceBack_MT;
            MTPrefab.GetComponent<CombatManager>().maxHealth = health_MT;
            MTPrefab.GetComponent<CombatManager>().missileLimit = missileLimit;
            MTPrefab.GetComponent<Movement>().getPushedForce = getPushedForce_MT;
        }


        #endregion

        #region sworsman
        if (swordsmanPrefab != null)
        {
            foreach (Transform s_man in swordsmanPrefab.transform)
            {
                s_man.GetComponent<AIPath>().maxAcceleration = movementSpeed_swordsman;
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
            }
        }

        #endregion

        #region chargedEnemy
        if (chargedEnemyPrefab != null)
        {
            chargedEnemyPrefab.GetComponent<Movement>().callGangRadius = callGangRadius_CE;
            chargedEnemyPrefab.GetComponent<Movement>().fovAngleFront = fovAngleFront_CE;
            chargedEnemyPrefab.GetComponent<Movement>().fovAngleBack = fovAngleBack_CE;
            chargedEnemyPrefab.GetComponent<Movement>().viewDistanceFront = viewDistanceFront_CE;
            chargedEnemyPrefab.GetComponent<Movement>().viewDistanceBack = viewDistanceBack_CE;
            chargedEnemyPrefab.GetComponent<Movement>().nextAttackTime = attackDuration_chargedEnemy;
            chargedEnemyPrefab.GetComponent<Movement>().getPushedForce = getPushedForce_CE;
            chargedEnemyPrefab.GetComponent<Movement>().startAttackTime = chargedAttackDuration_enemy;
            chargedEnemyPrefab.GetComponent<Movement>().chargedAttackSpeed = chargedAttackVelocity_enemy;
            chargedEnemyPrefab.GetComponent<Movement>().chargedDistance = chargedAttackDistance_enemy;
            chargedEnemyPrefab.GetComponent<Movement>().waitAfterAttackDuration = waitAfterAttackDuration;
            chargedEnemyPrefab.GetComponent<AIPath>().maxAcceleration = movementSpeed_CE;
            chargedEnemyPrefab.GetComponent<Movement>().chargeAndLookoutArea = lookingRangeDuringCharge;
            chargedEnemyPrefab.GetComponent<CombatManager>().maxHealth = health_CE;
            chargedEnemyPrefab.GetComponent<Movement>().criticalHitProb = criticalHitProbability;

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
        Gizmos.DrawWireSphere(swordsmanPrefab.transform.position, callGangRadius_swordsman);
        Gizmos.DrawWireSphere(chargedEnemyPrefab.transform.position, callGangRadius_CE);
        Gizmos.DrawWireSphere(distractibleObject.transform.position, primaryRange);
        Gizmos.DrawWireSphere(distractibleObject.transform.position, secondaryRange);
    }
}
