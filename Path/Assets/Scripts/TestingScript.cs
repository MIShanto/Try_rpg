using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingScript : MonoBehaviour
{
    public Movement m;
    public Transform t;

    private void Update()
    {
         if (Input.GetKeyDown(KeyCode.M))
         {
             m.CutsceneModeSettings(true, false, t);
         }
         if (Input.GetKeyDown(KeyCode.N))
         {
             m.CutsceneModeSettings(false, true, t);
         }
         /*
         if (Input.GetKeyDown(KeyCode.J))
         {
             m.isCharacterControllable = false;
         }
         if (Input.GetKeyDown(KeyCode.H))
         {
             m.isCharacterControllable = true;

         }*/
        if (Input.GetKeyDown(KeyCode.L))
        {
            
            m.MoveCharacterWithAISettings(true, "test_anim1_for_move_with_AI", "test_anim2_for_move_with_AI", t);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            m.MoveCharacterWithAISettings(false, "test_anim1_for_move_with_AI", "test_anim2_for_move_with_AI", t);
        }
    }
}
