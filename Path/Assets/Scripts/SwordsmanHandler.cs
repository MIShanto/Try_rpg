using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class SwordsmanHandler : MonoBehaviour
{
    [Tooltip("Enemy in 3rd index is the Healer")]
    [SerializeField] GameObject[] swordsman;

    GameObject tmp;
    private void Start()
    {
        if (swordsman[2] != null)
        {
            swordsman[2].GetComponent<Movement>().isHealer = true;
            swordsman[2].GetComponent<AIPath>().endReachedDistance = 5f;
        }
    }

    private void Update()
    {
        if (swordsman[2] != null && swordsman[2].activeSelf)
        {
            if (swordsman[0].GetComponent<CombatManager>().currentHealth <
                swordsman[0].GetComponent<CombatManager>().maxHealth * 0.5f &&
                swordsman[2].GetComponent<CombatManager>().currentHealth == swordsman[2].GetComponent<CombatManager>().maxHealth)
            {
                MakeSwordsman1Healer();

            }
            else if (swordsman[1].GetComponent<CombatManager>().currentHealth <
                swordsman[1].GetComponent<CombatManager>().maxHealth * 0.5f &&
                swordsman[2].GetComponent<CombatManager>().currentHealth == swordsman[2].GetComponent<CombatManager>().maxHealth)
            {
                MakeSwordsman2Healer();
            }
        }
    }

    /// <summary>
    /// make attacker2 healer and
    /// swap attacker2 and healers position
    /// </summary>
    private void MakeSwordsman2Healer()
    {
        swordsman[1].GetComponent<Movement>().Reposition();

        tmp = swordsman[2];
        swordsman[2] = swordsman[1];
        swordsman[1] = tmp;

        //swordsman[1].GetComponent<Movement>().healing = false;
        swordsman[1].GetComponent<Movement>().isHealer = false;
        swordsman[2].GetComponent<Movement>().isHealer = true;

        swordsman[2].GetComponent<AIPath>().endReachedDistance = 5f;
        swordsman[2].GetComponent<Movement>().Reposition();
    }

    /// <summary>
    /// make attacker1 healer and
    /// swap attacker1 and healers position
    /// </summary>
    private void MakeSwordsman1Healer()
    {
        tmp = swordsman[2];
        swordsman[2] = swordsman[0];
        swordsman[0] = tmp;

        //swordsman[0].GetComponent<Movement>().healing = false;
        swordsman[0].GetComponent<Movement>().isHealer = false;
        swordsman[2].GetComponent<Movement>().isHealer = true;

        swordsman[2].GetComponent<AIPath>().endReachedDistance = 5f;
        swordsman[2].GetComponent<Movement>().Reposition();
    }
}
