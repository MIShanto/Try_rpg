using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class CharacterManagement : MonoBehaviour
{
    [Header("Player Section")]
    [SerializeField] GameObject playerPrefab;

    [SerializeField] float movementSpeed_player;
    [SerializeField] float speedMultiplierDuringAttack;
    [SerializeField] float attackDuration_player;
    [SerializeField] float getPushedForce_player;
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
    [Range(0,100)]
    [SerializeField] int criticalHitProbability;
    [SerializeField] int playerSwordDamage;


    [Header("Archer Section")]
    [SerializeField] GameObject archerPrefab;
    [SerializeField] float attackDuration_archer;
    [SerializeField] float getPushedForce_archer;
    [SerializeField] float movementSpeed_archer;
    [SerializeField] float health_archer;
    
    [Header("missile thrower Section")]
    [SerializeField] GameObject MTPrefab;
    [SerializeField] float attackDuration_MT;
    [SerializeField] float getPushedForce_MT;
    [SerializeField] float movementSpeed_MT;
    [SerializeField] float health_MT;

    [Header("Swordsman Section")]
    [SerializeField] GameObject swordsmanPrefab;
    [SerializeField] float attackDuration_swordsman;
    [SerializeField] float getPushedForce_swordsman;
    [SerializeField] float movementSpeed_swordsman;
    [SerializeField] float health_swordsman;
    [Tooltip("Applicable for healer only")]
    [SerializeField] float healingAmount;
    [SerializeField] int swordDamage;


    [Header("Charged enemy Section")]
    [SerializeField] GameObject chargedEnemyPrefab;
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
    [SerializeField] float selfDestroyTimeArrow;
    [Space]
    [SerializeField] GameObject missilePrefab;
    [SerializeField] int missileDamage;
    [SerializeField] bool SelfDestroyableMissile;
    [SerializeField] float selfDestroyTimeMissile;

    private void Awake()
    {
        #region player
        if (playerPrefab != null)
        {
            playerPrefab.GetComponent<Movement>().characterMoveSpeed = movementSpeed_player;
            playerPrefab.GetComponent<Movement>().speedMultiplierDuringAttack = speedMultiplierDuringAttack;
            playerPrefab.GetComponent<Movement>().nextAttackTime = attackDuration_player;
            playerPrefab.GetComponent<Movement>().getPushedForce = getPushedForce_player;
            playerPrefab.GetComponent<Movement>().startDashTime = dashDuration;
            playerPrefab.GetComponent<Movement>().dashSpeed = dashVelocity;
            playerPrefab.GetComponent<Movement>().startRollTime = rollDuration;
            playerPrefab.GetComponent<Movement>().rollSpeed = rollVelocity;
            playerPrefab.GetComponent<Movement>().startKnockTime = knockDuration;
            playerPrefab.GetComponent<Movement>().knockSpeed = knockVelocity;
            playerPrefab.GetComponent<Movement>().startAttackTime = chargedAttackDuration_player;
            playerPrefab.GetComponent<Movement>().chargedAttackSpeed = chargedAttackVelocity_player;
            playerPrefab.GetComponent<CombatManager>().maxHealth = health_player;
            playerPrefab.GetComponent<Movement>().criticalHitProb = criticalHitProbability;
            
            
        }

        #endregion

        #region archer
        if (archerPrefab != null)
        {
            archerPrefab.GetComponent<AIPath>().maxAcceleration = movementSpeed_archer;
            archerPrefab.GetComponent<Movement>().nextAttackTime = attackDuration_archer;
            archerPrefab.GetComponent<CombatManager>().maxHealth = health_archer;
            archerPrefab.GetComponent<Movement>().getPushedForce = getPushedForce_archer;
        }


        #endregion

        #region missile thrower
        if (MTPrefab != null)
        {
            MTPrefab.GetComponent<AIPath>().maxAcceleration = movementSpeed_MT;
            MTPrefab.GetComponent<Movement>().nextAttackTime = attackDuration_MT;
            MTPrefab.GetComponent<CombatManager>().maxHealth = health_MT;
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
}
