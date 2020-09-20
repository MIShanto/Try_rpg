using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class NpcMovement : MonoBehaviour
{
    public float speed;
    float waitTime, angle = 0;
    [Tooltip("NPC wait time at the self position after destination reached")]
    public float startWaitTime;
    public Transform minX;
    public Transform maxX;
    public Transform minY;
    public Transform maxY;
    public bool isCutsceneModeOn;
    public bool cutsceneFixedFaceMode;

    Transform player;
    AIDestinationSetter aIDestinationSetter;
    [SerializeField] Transform destinationTarget;


    Vector2 targetForDirection;
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        player = (GameObject.FindGameObjectWithTag("Player")).transform;
        aIDestinationSetter = GetComponent<AIDestinationSetter>();
        aIDestinationSetter.target = null;
        animator = GetComponent<Animator>();
        waitTime = startWaitTime;
        targetForDirection = new Vector2(UnityEngine.Random.Range(minX.position.x, maxX.position.x), UnityEngine.Random.Range(minY.position.y, maxY.position.y));
    }

    // Update is called once per frame
    void Update()
    {
        AnimateMovement();

        if (isCutsceneModeOn)
            CutsceneMode(cutsceneFixedFaceMode, destinationTarget);
        else
            NPCRandomMove();
    }

    /// <summary>
    /// This is for cutscene mode 
    /// </summary>
    /// <param name="fixedFaceMode"></param>
    /// <param name="aiTarget"></param>
    void CutsceneMode(bool fixedFaceMode, Transform aiTarget)
    {
        GetComponent<AIPath>().enabled = true;
        GetComponent<AIDestinationSetter>().enabled = true;
        GetComponent<Seeker>().enabled = true;

        if (GetComponent<AIPath>().reachedDestination)
        {
            animator.SetFloat("Speed", 0f);

            if (!fixedFaceMode)
            {

                float speed = (2 * Mathf.PI) / 15;  //2*PI in degress is 360, so you get 5 seconds to complete a circle
                float radius = 50;

                angle += speed * Time.deltaTime; //if you want to switch direction, use -= instead of +=
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
            animator.SetFloat("Speed", 1f);

            aIDestinationSetter.target = aiTarget;
            targetForDirection = aiTarget.position;
        }

    }

    private void NPCRandomMove()
    {
        GetComponent<AIPath>().enabled = false;
        GetComponent<AIDestinationSetter>().enabled = false;
        GetComponent<Seeker>().enabled = false;


        transform.position = Vector2.MoveTowards(transform.position, targetForDirection, speed * Time.deltaTime);

        animator.SetFloat("Speed", 1f);


        if (Vector2.Distance(transform.position, targetForDirection) <= 0.2f)
        {
            if (waitTime <= 0)
            {

                targetForDirection = new Vector2(UnityEngine.Random.Range(minX.position.x, maxX.position.x), UnityEngine.Random.Range(minY.position.y, maxY.position.y));
                waitTime = startWaitTime;
            }
            else
            {
                waitTime -= Time.deltaTime;
                animator.SetFloat("Speed", 0f);
            }
        }
    }

    private void AnimateMovement()
    {
        Vector2 direction = (targetForDirection - (Vector2)transform.position);

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


        // used to set enemy face to the last direction
        if (direction != Vector2.zero)
        {
            animator.SetFloat("Horizontal", direction.x);
            animator.SetFloat("Vertical", direction.y);
        }
    }
}
