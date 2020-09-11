using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageHandler : MonoBehaviour
{
    public static DamageHandler Instance { get;  set; }
    struct ArmorHandler
    {
        public int minIndex;
        public int maxIndex;
        public int damageValueToDecrease;
    }
    Dictionary<int, int> damageTypes = new Dictionary<int, int>();
    Dictionary<int, ArmorHandler> armorTypes = new Dictionary<int, ArmorHandler>();

    [HideInInspector] public int playerSwordDamage, swordDamage, arrowDamage, poisonDamage, missileDamage;

    CombatManager myCombatManager;

    int bleedingAttackCounter = 0;

    private void Awake()
    {
        // if the singleton hasn't been initialized yet
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;//Avoid doing anything else
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    /// <summary>
    /// Assigns the associated damage value and armor defence value;
    /// </summary>
    private void Start()
    {
        ArmorHandler armorHandler;

        #region damage section

        damageTypes.Add(0, 0); // nothing
        damageTypes.Add(1, swordDamage); //swordsman sword
        damageTypes.Add(2, arrowDamage); // arrow
        damageTypes.Add(3, poisonDamage); // poison
        damageTypes.Add(4, missileDamage); // missile
        damageTypes.Add(5, playerSwordDamage); // player sword

        #endregion
        #region armor section

        armorHandler.minIndex = 0;
        armorHandler.maxIndex = 0;
        armorHandler.damageValueToDecrease = 0;
        armorTypes.Add(0, armorHandler); // no armor

        armorHandler.minIndex = 1;
        armorHandler.maxIndex = 1;
        armorHandler.damageValueToDecrease = 25;
        armorTypes.Add(1, armorHandler); // closed ranged defence armor

        armorHandler.minIndex = 2;
        armorHandler.maxIndex = 2;
        armorHandler.damageValueToDecrease = 50;
        armorTypes.Add(2, armorHandler); // long ranged defence armor

        armorHandler.minIndex = 3;
        armorHandler.maxIndex = 3;
        armorHandler.damageValueToDecrease = 1; // true value
        armorTypes.Add(3, armorHandler); // poison defence armor

        #endregion

        myCombatManager = GetComponent<CombatManager>();
    }

    public int GetDamageInfo(int damageType, int armorType, bool isHitCritical)
    {

        int tmpDamageValue = damageTypes[damageType];
        int armorDefenceValue = 0;
        //check if the armor can protect from the damage type..
        if(damageType >= armorTypes[armorType].minIndex && damageType <= armorTypes[armorType].minIndex)
        {
            armorDefenceValue = armorTypes[armorType].damageValueToDecrease; 
        }

        if (damageType == 1)  // TODO: this indexes will be hard coded 
            return tmpDamageValue - armorDefenceValue;

        else if (damageType == 2)
            return tmpDamageValue - armorDefenceValue;
        
        else if (damageType == 3)
        {
            //if there is no armor do bleeding
            if (armorDefenceValue == 0)
                StartCoroutine(DoBleedingAction());
            return tmpDamageValue;
        }
        
        else if (damageType == 4)
            return tmpDamageValue - armorDefenceValue;
        
        else if (damageType == 5)
        {

            if (isHitCritical)
                return tmpDamageValue * 2 - armorDefenceValue;
            else
            {

                return tmpDamageValue - armorDefenceValue;
            }
        }

        return 0;

    }

    IEnumerator  DoBleedingAction()
    {
        yield return new WaitForSeconds(2f);//TODO: this will be hard coded
        if (bleedingAttackCounter < 3)
        {
            myCombatManager.currentHealth -= 50f; // TODO: this will be optimized near future;
            bleedingAttackCounter++;
            StartCoroutine(DoBleedingAction());
        }
        else
            bleedingAttackCounter = 0;
    }

}
