using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mouse_Behaviour : MonoBehaviour
{
    SpriteRenderer rend;
    [SerializeField] Sprite cursorNormal;
    [SerializeField] Sprite cursorDistraction;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        rend = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 cursorPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = cursorPosition;

        if (Input.GetKey(KeyCode.Z))
        {
            rend.sprite = cursorDistraction;
        }
        else
            rend.sprite = cursorNormal;
    }
}
