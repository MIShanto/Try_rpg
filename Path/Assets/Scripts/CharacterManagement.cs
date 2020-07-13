using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class CharacterManagement : MonoBehaviour
{
    [Header("Player Section")]
    [SerializeField] GameObject playerPrefab;

    [SerializeField] float movementSpeed_player;
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


    [Header("Archer Section")]
    [SerializeField] GameObject archerPrefab;
    [SerializeField] float attackDuration_archer;
    [SerializeField] float getPushedForce_archer;
    [SerializeField] float movementSpeed_archer;


    [Header("Swordsman Section")]
    [SerializeField] GameObject swordsmanPrefab;
    [SerializeField] float attackDuration_swordsman;
    [SerializeField] float getPushedForce_swordsman;
    [SerializeField] float movementSpeed_swordsman;
    [Tooltip("Applicable for healer only")]
    [SerializeField] float healingAmount;


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
    //charged enemy bug fix korte hobe...
    private void Awake()
    {
        #region player

        playerPrefab.GetComponent<Movement>().characterMoveSpeed = movementSpeed_player;
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

        #endregion

        #region archer

        archerPrefab.GetComponent<AIPath>().maxAcceleration = movementSpeed_archer;
        archerPrefab.GetComponent<Movement>().nextAttackTime = attackDuration_archer;
        archerPrefab.GetComponent<Movement>().getPushedForce = getPushedForce_archer;


        #endregion

        #region sworsman

        foreach (Transform s_man in swordsmanPrefab.transform)
        {
            s_man.GetComponent<AIPath>().maxAcceleration = movementSpeed_swordsman;
            s_man.GetComponent<Movement>().nextAttackTime = attackDuration_swordsman;
            s_man.GetComponent<Movement>().getPushedForce = getPushedForce_swordsman;
            s_man.GetComponent<Movement>().enemyHealingAmount = healingAmount;
        }

        #endregion

        #region chargedEnemy

        chargedEnemyPrefab.GetComponent<Movement>().nextAttackTime = attackDuration_chargedEnemy;
        chargedEnemyPrefab.GetComponent<Movement>().getPushedForce = getPushedForce_CE;
        chargedEnemyPrefab.GetComponent<Movement>().startAttackTime = chargedAttackDuration_enemy;
        chargedEnemyPrefab.GetComponent<Movement>().chargedAttackSpeed = chargedAttackVelocity_enemy;
        chargedEnemyPrefab.GetComponent<Movement>().chargedDistance = chargedAttackDistance_enemy;
        chargedEnemyPrefab.GetComponent<Movement>().waitAfterAttackDuration = waitAfterAttackDuration;
        chargedEnemyPrefab.GetComponent<AIPath>().maxAcceleration = movementSpeed_CE;
        chargedEnemyPrefab.GetComponent<Movement>().chargeAndLookoutArea = lookingRangeDuringCharge;

        if(lookingRangeDuringCharge > chargedAttackDistance_enemy)
        {
            Debug.LogError("!!!(looking Range During Charge) this distance must be less than charged Distance!!!");
        }


        #endregion
    }
}
