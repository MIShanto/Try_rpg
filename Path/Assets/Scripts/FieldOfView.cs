

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour {

    [SerializeField] private LayerMask layerMask;
    [SerializeField] private LayerMask enemyLayerForCallingGang;

    GameObject characterWhichHasThisFOV;
    private Mesh mesh;
    Coroutine enemyAlertIncreaseOn;
    Coroutine enemyAlertDecreaseOn;

    private float fovAngleFront;
    private float fovAngleBack;

    private float viewDistanceFront;
    private float viewDistanceBack;

    private float startingAngleFront;
    private float startingAngleBack;

    private bool detectedInFront;
    private bool detectedInBack;

    private int detectionCount;

    private Vector3 origin;

    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        origin = Vector3.zero;
    }

    private void LateUpdate()
    {
        ExcecuteAlertSystem();
        ExcecuteFOV();
    }

    private void ExcecuteFOV()
    {
        int rayCount = 50;
        float angleFront = startingAngleFront;
        float angleBack = startingAngleBack;
        float angleIncreaseFront = fovAngleFront / rayCount;
        float angleIncreaseBack = fovAngleBack / rayCount;

        Vector3[] vertices = new Vector3[rayCount * 2 + 2 + 2];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 2 * 3];

        vertices[0] = origin;

        int vertexIndex = 1;
        int triangleIndex = 0;

        //for front section..
        for (int i = 0; i <= rayCount; i++)
        {
            Vector3 vertex;

            vertex = FOV_Calc_and_Detection(angleFront, viewDistanceFront, 1); // 1 for identifying front section 

            vertices[vertexIndex] = vertex;

            if (i > 0)
            {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;
            }

            vertexIndex++;
            angleFront -= angleIncreaseFront;
        }
        if (detectionCount == 0)
            detectedInFront = false;
        detectionCount = 0;

        //for back section..
        for (int i = 0; i <= rayCount; i++)
        {
            Vector3 vertex;

            vertex = FOV_Calc_and_Detection(angleBack, viewDistanceBack, 2); // 2 for identifying back section 

            vertices[vertexIndex] = vertex;

            if (i > 0)
            {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;
            }

            vertexIndex++;
            angleBack -= angleIncreaseBack;
        }
        if (detectionCount == 0)
            detectedInBack = false;
        detectionCount = 0;


        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.bounds = new Bounds(origin, Vector3.one * 1000f);
    }

    /// <summary>
    /// calculate the fov and perform detection tasks... 
    /// </summary>
    /// <param name="angle"></param>
    /// <param name="viewDistance"></param>
    /// <returns></returns>
    private Vector3 FOV_Calc_and_Detection(float angle, float viewDistance, int section_ID)
    {
        Vector3 vertex;
        RaycastHit2D raycastHit2D = Physics2D.Raycast(origin, GetVectorFromAngle(angle), viewDistance, layerMask);
        if (raycastHit2D.collider == null)
        {

            // No hit
            vertex = origin + GetVectorFromAngle(angle) * viewDistance;
        }
        else
        {
            //if get player follow and attack
            if (raycastHit2D.collider.tag == "Player")
            {
                detectionCount++;

                if(section_ID == 1)
                {
                    detectedInFront = true;
                }
                else if(section_ID == 2)
                {
                    detectedInBack = true;
                }

                vertex = origin + GetVectorFromAngle(angle) * viewDistance;

            }
            else // Hit object
            {    
                vertex = raycastHit2D.point;
            }
        }

        return vertex;
    }

    void ExcecuteAlertSystem()
    {
        if(detectedInFront || detectedInBack)
        {
            if (enemyAlertDecreaseOn != null)
                StopCoroutine(enemyAlertDecreaseOn);
            StartAlert();
        }
        else
        {
            if (enemyAlertIncreaseOn != null)
                StopCoroutine(enemyAlertIncreaseOn);
            StopAlert();
        }
    }

    /// <summary>
    /// Things to do when player is inside line of sight... 
    /// </summary>
    private void StartAlert()
    {
       
        if ( characterWhichHasThisFOV.GetComponent<CombatManager>().isAlertIncreasing == false )
        {
            enemyAlertIncreaseOn = StartCoroutine(characterWhichHasThisFOV.GetComponent<CombatManager>().EnemyAlertIncrease(enemyLayerForCallingGang));
        }

        
    }
    /// <summary>
    /// Things to do when player is outside line of sight... 
    /// </summary>
    private void StopAlert()
    {
        if (characterWhichHasThisFOV.GetComponent<CombatManager>().isAlertDecreasing == false)
        {
            enemyAlertDecreaseOn = StartCoroutine(characterWhichHasThisFOV.GetComponent<CombatManager>().EnemyAlertDecrease());
        }


    }

    public void SetOrigin(Vector3 origin)
    {
        this.origin = origin;
    }
    public void SetCharacter(GameObject m_character)
    {
        characterWhichHasThisFOV = m_character;
    }

    public void SetAimDirection(Vector3 aimDirection)
    {
        startingAngleFront = GetAngleFromVectorFloat(aimDirection) + fovAngleFront / 2f;
        startingAngleBack = 180 + GetAngleFromVectorFloat(aimDirection) + fovAngleBack / 2f;
    }

    public void SetFoVFront(float fovFront)
    {
        this.fovAngleFront = fovFront;
    }
    
    public void SetFoVBack(float fovBack)
    {
        this.fovAngleBack = fovBack;
    }

    public void SetFrontViewDistance(float viewDistance)
    {
        this.viewDistanceFront = viewDistance;
    }
    public void SetBackViewDistance(float viewDistance)
    {
        this.viewDistanceBack = viewDistance;
    }
    public Vector3 GetVectorFromAngle(float angle)
    {
        // angle = 0 means 360
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }
    public float GetAngleFromVectorFloat(Vector3 dir)
    {
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0) n += 360;

        return n;
    }

}
