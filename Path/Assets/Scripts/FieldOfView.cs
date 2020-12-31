

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour {

    [SerializeField] private LayerMask layerMask;
    [SerializeField] private LayerMask enemyLayerForCallingGang;
    GameObject characterWhichHasThisFOV;
    private Mesh mesh;
    private float fovAngleFront;
    private float fovAngleBack;
    private float viewDistanceFront;
    private float viewDistanceBack;
    private Vector3 origin;
    private float startingAngleFront;
    private float startingAngleBack;

    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        origin = Vector3.zero;
    }

    private void LateUpdate() 
    {
        int rayCount = 50;
        float angleFront = startingAngleFront;
        float angleBack = startingAngleBack;
        float angleIncreaseFront = fovAngleFront / rayCount;
        float angleIncreaseBack = fovAngleBack / rayCount;

        Vector3[] vertices = new Vector3[rayCount*2 + 2 + 2];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount *2 * 3];

        vertices[0] = origin;

        int vertexIndex = 1;
        int triangleIndex = 0;
        
        //for front section..
        for (int i = 0; i <= rayCount; i++)
        {
            Vector3 vertex;
            RaycastHit2D raycastHit2D = Physics2D.Raycast(origin, GetVectorFromAngle(angleFront), viewDistanceFront, layerMask);
            if (raycastHit2D.collider == null)
            {
                
                // No hit
                vertex = origin + GetVectorFromAngle(angleFront) * viewDistanceFront;
            }
            else
            {
                //if get player follow and attack
                if (raycastHit2D.collider.tag == "Player")
                {
                    vertex = origin + GetVectorFromAngle(angleFront) * viewDistanceFront;
                    characterWhichHasThisFOV.GetComponent<Movement>().MovementControl = Movement.MovementControls.walk;
                    Collider2D[] hitArea = Physics2D.OverlapCircleAll(characterWhichHasThisFOV.transform.position,
                                                                     characterWhichHasThisFOV.GetComponent<Movement>().callGangRadius, enemyLayerForCallingGang);
                    foreach (Collider2D hitObject in hitArea)
                    {
                        if(hitObject.GetComponent<Movement>().character == Movement.characters.enemy)
                        {
                            hitObject.GetComponent<Movement>().MovementControl = Movement.MovementControls.walk;
                        }
                        

                    }

                }
                else // Hit object
                    vertex = raycastHit2D.point;
            }
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

        //for back section..
        for (int i = 0; i <= rayCount; i++)
        {
            Vector3 vertex;
            RaycastHit2D raycastHit2D = Physics2D.Raycast(origin, GetVectorFromAngle(angleBack), viewDistanceBack, layerMask);
            if (raycastHit2D.collider == null)
            {
                // No hit
                vertex = origin + GetVectorFromAngle(angleBack) * viewDistanceBack;
            }
            else
            {

                //if get player follow and attack
                if (raycastHit2D.collider.tag == "Player")
                {
                    vertex = origin + GetVectorFromAngle(angleBack) * viewDistanceBack;
                    characterWhichHasThisFOV.GetComponent<Movement>().MovementControl = Movement.MovementControls.walk;
                    Collider2D[] hitArea = Physics2D.OverlapCircleAll(characterWhichHasThisFOV.transform.position,
                                                                     characterWhichHasThisFOV.GetComponent<Movement>().callGangRadius, enemyLayerForCallingGang);
                    foreach (Collider2D hitObject in hitArea)
                    {
                        if (hitObject.GetComponent<Movement>().character == Movement.characters.enemy)
                        {
                            hitObject.GetComponent<Movement>().MovementControl = Movement.MovementControls.walk;
                        }


                    }
                }
                else // Hit object
                    vertex = raycastHit2D.point;
            }
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


        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.bounds = new Bounds(origin, Vector3.one * 1000f);
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
