using UnityEngine;
using System.Collections.Generic;

//Initial script by Foulco
//Initial viable version : 2020-01-20
//
//INFORMATIONS
//The manager that manage the CollisionViewer (Initialization, Inputs, Update, ...)
//
//MODIFICATIONS
//2020-XX-XX    <Informations>
//

public class CollisionViewerManager : MonoBehaviour {
    //Constant
    const string PREFIX_COLLISION_VIEWER_OBJECT_NAME = "MOD_CollisionViewer_";

    //Singleton
    public static CollisionViewerManager Instance() { return m_instance; }
    static CollisionViewerManager m_instance;

    //Settings Keys
    KeyCode m_keyCodeEnable;

    KeyCode m_keyCodeShowSolid;
    KeyCode m_keyCodeShowTrigger;

    KeyCode m_keyCodeShowCube;
    KeyCode m_keyCodeShowCapsule;
    KeyCode m_keyCodeShowSphere;

    KeyCode m_keyCodeOpacityUp;
    KeyCode m_keyCodeOpacityDown;

    //Settings Colors
    float m_colorAlpha;
    Color m_colorCollisionSolid;
    Color m_colorCollisionTrigger;
    Color m_colorCollisionTriggerRespawn;

    //Toggles
    bool m_isEnable;
    CollisionViewerParam m_visibilityParams;

    //Shaders
    Shader m_shaderTransparant;

    //Materials
    Material m_materialCollisionSolid;
    Material m_materialCollisionTrigger;
    Material m_materialCollisionTriggerRespawn;

    //Prefabs (Manualy created)
    GameObject m_prefCube;
    GameObject m_prefCapsule;
    GameObject m_prefCylinder;
    GameObject m_prefSphere;
    GameObject m_prefPolygon;

    //CollisionViewerFollower List
    List<CollisionViewerFollower> m_followers;

    public static GameObject ClonePrefabCude() {
        if(m_instance != null && m_instance.m_prefCube != null) {
            return Instantiate(m_instance.m_prefCube);
        }

        return null;
    }

    private void Start() {
        GameObject _CreatePrefab(string p_name, Mesh p_mesh) {
            GameObject t_gameObject = new GameObject(p_name);

            MeshRenderer t_renderer = t_gameObject.AddComponent<MeshRenderer>();
            t_renderer.enabled = true;
            t_renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            t_renderer.receiveShadows = false;
            t_renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            t_renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            t_renderer.allowOcclusionWhenDynamic = false;

            MeshFilter t_filter = t_gameObject.AddComponent<MeshFilter>();
            t_filter.mesh = p_mesh;

            return t_gameObject;
        }

        //Initialize
        if (m_instance == null) {
            //This is now the singleton instance
            m_instance = this;

            //Settings Keys
            m_keyCodeEnable = KeyCode.R;

            m_keyCodeShowSolid = KeyCode.T;
            m_keyCodeShowTrigger = KeyCode.Y;

            m_keyCodeShowCube = KeyCode.None;       //None by default for now (but is supported)
            m_keyCodeShowCapsule = KeyCode.None;    //None by default for now (but is supported)
            m_keyCodeShowSphere = KeyCode.None;     //None by default for now (but is supported)

            m_keyCodeOpacityUp = KeyCode.F;
            m_keyCodeOpacityDown = KeyCode.G;

            //Settings Colors
            m_colorAlpha = 0.5f;
            m_colorCollisionSolid = new Color(0f, 1f, 0f, m_colorAlpha);
            m_colorCollisionTrigger = new Color(1f, 0f, 0f, m_colorAlpha);
            m_colorCollisionTriggerRespawn = new Color(0f, 0f, 1f, m_colorAlpha);

            //Toggles
            m_isEnable = false;
            m_visibilityParams = CollisionViewerParam.ALL;

            //Shaders
            m_shaderTransparant = Shader.Find("Transparent/Diffuse");
            
            //Materials
            m_materialCollisionSolid = new Material(m_shaderTransparant);
            m_materialCollisionSolid.color = m_colorCollisionSolid;
            m_materialCollisionTrigger = new Material(m_shaderTransparant);
            m_materialCollisionTrigger.color = m_colorCollisionTrigger;
            m_materialCollisionTriggerRespawn = new Material(m_shaderTransparant);
            m_materialCollisionTriggerRespawn.color = m_colorCollisionTriggerRespawn;

            //Meshes
            //Retreive Primitive Meshes (From Primitive Type)
            //NOTE : Not retreiving the Capsule mesh, since it ues 1 Cylinder and 1 Sphere instead
            //       Because if it use Capsule, when the object is scaled, it is wrong
            MeshFilter t_tempMeshFilter;
            GameObject t_cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            t_tempMeshFilter = t_cube.GetComponent<MeshFilter>();
            Mesh t_meshCube = (t_tempMeshFilter) ? t_tempMeshFilter.sharedMesh : null;

            GameObject t_cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            t_tempMeshFilter = t_cylinder.GetComponent<MeshFilter>();
            Mesh t_meshCylinder = (t_tempMeshFilter) ? t_tempMeshFilter.sharedMesh : null;

            GameObject t_sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            t_tempMeshFilter = t_sphere.GetComponent<MeshFilter>();
            Mesh t_meshSphere = (t_tempMeshFilter) ? t_tempMeshFilter.sharedMesh : null;

            Mesh t_meshCylinderWithoutTopBot = CreateCylinderMesh(20, 0.5f, 1f);
            Mesh t_meshShpereHalfUp = CreateHalfSphereMesh(20, 10, 0.5f, true);
            Mesh t_meshShpereHalfDown = CreateHalfSphereMesh(20, 10, 0.5f, false);

            //Destroy Primitive, no longer have a need to for those
            Destroy(t_cube);
            Destroy(t_cylinder);
            Destroy(t_sphere);

            //Prefabs
            GameObject t_gameObject = null;
            m_prefCube = _CreatePrefab(PREFIX_COLLISION_VIEWER_OBJECT_NAME + "Cube", t_meshCube);

            //Special Case for Capsule (Do 1 Cyllidner + 2 Sphere)
            m_prefCapsule = _CreatePrefab(PREFIX_COLLISION_VIEWER_OBJECT_NAME + "Capsule", null);
            t_gameObject = _CreatePrefab(PREFIX_COLLISION_VIEWER_OBJECT_NAME + "CapsuleSubCylinder", t_meshCylinderWithoutTopBot);
            t_gameObject.transform.parent = m_prefCapsule.transform;
            t_gameObject = _CreatePrefab(PREFIX_COLLISION_VIEWER_OBJECT_NAME + "CapsuleSubSphereUp", t_meshShpereHalfUp);
            t_gameObject.transform.parent = m_prefCapsule.transform;
            t_gameObject = _CreatePrefab(PREFIX_COLLISION_VIEWER_OBJECT_NAME + "CapsuleSubSphereDown", t_meshShpereHalfDown);
            t_gameObject.transform.parent = m_prefCapsule.transform;

            m_prefCylinder = _CreatePrefab(PREFIX_COLLISION_VIEWER_OBJECT_NAME + "Cylinder", t_meshCylinder);
            m_prefSphere = _CreatePrefab(PREFIX_COLLISION_VIEWER_OBJECT_NAME + "Sphere", t_meshSphere);

            //Special case, the mesh will be assigned depending of the mesh of the collider retreived
            m_prefPolygon = _CreatePrefab(PREFIX_COLLISION_VIEWER_OBJECT_NAME + "_Polygon", null);

            //Prefabs : Prevent destruction on load of new scene and etc
            DontDestroyOnLoad(m_prefCube);
            DontDestroyOnLoad(m_prefCapsule);
            DontDestroyOnLoad(m_prefCylinder);
            DontDestroyOnLoad(m_prefSphere);
            DontDestroyOnLoad(m_prefPolygon);

            //Prefabs : Disable
            m_prefCube.SetActive(false);
            m_prefCapsule.SetActive(false);
            m_prefCylinder.SetActive(false);
            m_prefSphere.SetActive(false);
            m_prefPolygon.SetActive(false);

            //CollisionViewerFollower List
            m_followers = new List<CollisionViewerFollower>();
        }
        else if(this != m_instance) {
            Destroy(this);
        }
    }

    void SetColorAlpha(float p_alpha) {
        m_colorAlpha = p_alpha < 0.1f ? 0.1f : p_alpha > 1f ? 1f : p_alpha;
        m_colorCollisionSolid.a = m_colorAlpha;
        m_colorCollisionTrigger.a = m_colorAlpha;
        m_colorCollisionTriggerRespawn.a = m_colorAlpha;
        m_materialCollisionSolid.color = m_colorCollisionSolid;
        m_materialCollisionTrigger.color = m_colorCollisionTrigger;
        m_materialCollisionTriggerRespawn.color = m_colorCollisionTriggerRespawn;
    }

    void Update() {
        //Toggle for enable/disable CollisionViewer
        if (Input.GetKeyDown(m_keyCodeEnable)) {
            m_isEnable = !m_isEnable;
            CollisionViewerParam t_params = m_isEnable ? m_visibilityParams : CollisionViewerParam.NONE;
            //Cleanup null entry just in case
            m_followers.RemoveAll(x => x == null);
            for (int i = 0; i < m_followers.Count; ++i) {
                m_followers[i].SetVisibility(t_params);
                m_followers[i].gameObject.SetActive(m_isEnable);
            }
        }

        if (m_isEnable) {
            //Cleanup Follower to be safe
            //Since I do not know how ALL the game code works.
            //It is to prevent having millions of objects no longer used
            for(int i = 0; i < m_followers.Count; ++i) {
                if(m_followers[i] != null && m_followers[i].CanBeDestroy()) {
                    Destroy(m_followers[i].gameObject);
                    m_followers[i] = null;
                }
            }
            m_followers.RemoveAll(x => x == null);

            //Check if colliders need a CollisionFollower
            Collider[] t_colliders = GameObject.FindObjectsOfType<Collider>();
            for (int i = 0; i < t_colliders.Length; ++i) {
                //Current collider
                Collider t_collider = t_colliders[i];

                //Check if there is already a CollisionFollower for this Collider
                if (m_followers.Find(x => x.FollowThisCollider(t_collider)) != null) {
                    continue;
                }

                //Reach here => Current Collider not already retreived, prepare it to add a CollisionFollower
                //Setup values
                GameObject t_prefabUsed = null;
                Mesh t_meshFound = null;
                CollisionViewerParam t_param = CollisionViewerParam.NONE;

                //Retreive the Collider type
                //NOTE : Bug Fables does not seems to use any TerrainCollider, since it does not found the Class for it in dnSpy...?
                //Box Collider
                if (t_collider is BoxCollider) {
                    t_param += (int)CollisionViewerParam.CUBE;
                    t_prefabUsed = m_prefCube;
                }
                //Capsule Collider
                else if (t_collider is CapsuleCollider) {
                    t_param += (int)CollisionViewerParam.CAPSULE;
                    t_prefabUsed = m_prefCapsule;
                }
                //Sphere Collider
                else if (t_collider is SphereCollider) {
                    t_param += (int)CollisionViewerParam.SPHERE;
                    t_prefabUsed = m_prefSphere;
                }
                //Mesh-Polygon Collider
                else if (t_collider is MeshCollider) {
                    t_param += (int)CollisionViewerParam.POLYGON;
                    t_meshFound = ((MeshCollider)t_collider).sharedMesh;
                    if(t_meshFound != null) {
                        t_prefabUsed = m_prefPolygon;
                    }
                }

                //If found, create a GameObject, and attach it a CollisionFollower for it
                if (t_prefabUsed != null) {
                    //Prepare the new object
                    GameObject t_child = Instantiate(t_prefabUsed);
                    t_child.SetActive(true);

                    //Extra instructions for Polygon type
                    if (t_prefabUsed == m_prefPolygon) {
                        t_child.GetComponent<MeshFilter>().mesh = t_meshFound;
                    }

                    //Ajust transform values
                    {
                        t_child.transform.position = t_collider.transform.position;
                        t_child.transform.rotation = t_collider.transform.rotation;
                        t_child.transform.parent = t_collider.transform;
                        //Correcting scale (You CAN'T simply do t_child.transform.localScale = new Vector3(1f,1f,1f);
                        Vector3 scale = t_child.transform.localScale;
                        scale.Set(1f, 1f, 1f);
                        t_child.transform.localScale = scale;
                    }

                    //Change Material depending if is a Trigger or a Solid Collider
                    {
                        Material t_material = null;
                        if (t_collider.isTrigger) {
                            if(t_collider.CompareTag("Respawn")) {
                                t_material = m_materialCollisionTriggerRespawn;
                            }
                            else {
                                t_material = m_materialCollisionTrigger;
                            }
                            t_param += (int)CollisionViewerParam.TRIGGER;
                        }
                        else {
                            t_material = m_materialCollisionSolid;
                            t_param += (int)CollisionViewerParam.SOLID;
                        }
                        t_child.GetComponent<MeshRenderer>().material = t_material;
                        //Extra instructions for Capsule (Because 1 Cylinder, 2 Spheres)
                        if (t_prefabUsed == m_prefCapsule) {
                            t_child.transform.GetChild(0).GetComponent<MeshRenderer>().material = t_material;
                            t_child.transform.GetChild(1).GetComponent<MeshRenderer>().material = t_material;
                            t_child.transform.GetChild(2).GetComponent<MeshRenderer>().material = t_material;
                        }
                    }

                    //Attempt to evaluate what category this collider is used for
                    //Checking stuff like : tag name, object name, MonoBehaviour script type attached to it, etc!
                    {
                        //TODO : TEST
                        //Camera Limiter
                        if (t_collider.GetComponent<CamLimiter>() != null) {
                            t_param += (int)CollisionViewerParam.CAMERA_MANIPULATION;
                        }
                        //TODO : TEST
                        //Fake Wall
                        else if (t_collider.GetComponent<FakeWall>() != null) {
                            t_param += (int)CollisionViewerParam.FAKE_WALL;
                        }
                        //TODO : TEST
                        //Stealth (Thief / Wasp stealth?)
                        else if (t_collider.GetComponent<StealthCheck>() != null) {
                            t_param += (int)CollisionViewerParam.STEALTH_CHECK;
                        }
                        //TODO : TEST
                        //Respawn location
                        //Reference : Is used in the script : PlayerControl
                        else if (t_collider.CompareTag("Respawn")) {
                            t_param += (int)CollisionViewerParam.RESPAWN_LOCATION;
                        }
                        //Default
                        else {
                            t_param += (int)CollisionViewerParam.UNKNOWN_OR_DEFAULT_CATEGORY;
                        }
                    }

                    //Attempt to get parent Renderer (So we can disable the graphics related to this collision)
                    //TODO : To fix for some objects, maybe later, it is good enough for now
                    MeshRenderer t_parentRenderer = null;
                    {
                        t_parentRenderer = t_colliders[i].GetComponent<MeshRenderer>();
                    }

                    //Create a CollisionViewerFollower and Add to the list
                    m_followers.Add(CollisionViewerFollower.CreateAndAttachTo(t_child, t_param, t_collider, t_parentRenderer, m_visibilityParams));
                }
            }

            //Check for Visibility Change
            CollisionViewerParam t_visibilityParams = CollisionViewerParam.NONE;
            t_visibilityParams ^= Input.GetKeyDown(m_keyCodeShowSolid) ? CollisionViewerParam.SOLID : CollisionViewerParam.NONE;
            t_visibilityParams ^= Input.GetKeyDown(m_keyCodeShowTrigger) ? CollisionViewerParam.TRIGGER : CollisionViewerParam.NONE;
            t_visibilityParams ^= Input.GetKeyDown(m_keyCodeShowCube) ? CollisionViewerParam.CUBE : CollisionViewerParam.NONE;
            t_visibilityParams ^= Input.GetKeyDown(m_keyCodeShowCapsule) ? CollisionViewerParam.CAPSULE : CollisionViewerParam.NONE;
            t_visibilityParams ^= Input.GetKeyDown(m_keyCodeShowSphere) ? CollisionViewerParam.SPHERE : CollisionViewerParam.NONE;
            //If it changed, update all Follower visibility
            if (t_visibilityParams != CollisionViewerParam.NONE) {
                m_visibilityParams ^= t_visibilityParams;
                for (int i = 0; i < m_followers.Count; ++i) {
                    m_followers[i].SetVisibility(m_visibilityParams);
                }
            }

            //Check for Opacity Change
            int t_opacityChange = 0;
            if(Input.GetKey(m_keyCodeOpacityUp)) {
                ++t_opacityChange;
            }
            if(Input.GetKey(m_keyCodeOpacityDown)) {
                --t_opacityChange;
            }
            //If it changed, update alpha
            if (t_opacityChange != 0) {
                SetColorAlpha(m_colorAlpha + (t_opacityChange * 0.02f));
            }

            //Update the CollisionFollower
            for (int i = 0; i < m_followers.Count; ++i) {
                m_followers[i].ManualUpdate();
            }
        }
    }

    //p_numberOfRings count the base and the top, so minimum is always 2
    Mesh CreateHalfSphereMesh(int p_numberOfVerticesAtBase, int p_numberOfRings, float p_radius, bool p_up) {
        Mesh t_mesh = new Mesh();

        //Minimum values
        if (p_numberOfVerticesAtBase < 3) { p_numberOfVerticesAtBase = 3; }
        if (p_numberOfRings < 2) { p_numberOfRings = 2; }
        if (p_radius < 0.02f) { p_radius = 0.02f; }

        Vector3[] t_vertices = new Vector3[p_numberOfVerticesAtBase * (p_numberOfRings - 1) + 1];
        Vector3[] t_normals = new Vector3[t_vertices.Length];
        int[] t_triangles = new int[((p_numberOfVerticesAtBase * (p_numberOfRings - 2) * 2) + p_numberOfVerticesAtBase) * 3];

        float t_ringAngle = 2f * Mathf.PI / (float)p_numberOfRings * 0.25f;         //Quarter of a cricle
        float t_verticesAngle = 2f * Mathf.PI / (float)p_numberOfVerticesAtBase;
        float t_radius;
        float t_angle;
        float t_x;
        float t_y;
        float t_z;
        int t_index;

        //VERTICES and NORMALS
        //Base
        t_radius = p_radius;
        t_y = 0f;
        for (int i = 0; i < p_numberOfVerticesAtBase; ++i) {
            t_angle = t_verticesAngle * i;
            t_x = Mathf.Cos(t_angle) * t_radius;
            t_z = Mathf.Sin(t_angle) * t_radius;
            t_vertices[i] = new Vector3(t_x, t_y, t_z);
            t_normals[i] = t_vertices[i].normalized;
        }

        //Body
        for(int r = 0; r < p_numberOfRings - 2; ++r) {
            t_angle = t_ringAngle * (r + 1);
            t_y = Mathf.Sin(t_angle) * p_radius;
            t_radius = Mathf.Cos(t_angle) * p_radius;
            for (int i = 0; i < p_numberOfVerticesAtBase; ++i) {
                t_index = p_numberOfVerticesAtBase + (r * p_numberOfVerticesAtBase) + i;
                t_angle = t_verticesAngle * i;
                t_x = Mathf.Cos(t_angle) * t_radius;
                t_z = Mathf.Sin(t_angle) * t_radius;
                t_vertices[t_index] = new Vector3(t_x, t_y, t_z);
                t_normals[t_index] = t_vertices[t_index].normalized;
            }
        }

        //Last
        t_vertices[t_vertices.Length - 1] = new Vector3(0f, p_radius, 0f);
        t_normals[t_normals.Length - 1] = t_vertices[t_vertices.Length - 1].normalized;

        //TRIANGLES
        int t_indexTriangles = 0;
        int t_indexVertices = 0;
        //Body
        if (p_numberOfRings > 2) {
            for (int r = 0; r < p_numberOfRings - 2; ++r) {
                for (int i = 0; i < p_numberOfVerticesAtBase; ++i) {
                    //First
                    t_triangles[t_indexTriangles + 0] = t_indexVertices;
                    t_triangles[t_indexTriangles + 1] = t_indexVertices + p_numberOfVerticesAtBase;
                    if (i < p_numberOfVerticesAtBase - 1) {
                        t_triangles[t_indexTriangles + 2] = t_indexVertices + 1;
                    }
                    else {
                        t_triangles[t_indexTriangles + 2] = t_indexVertices + 1 - p_numberOfVerticesAtBase;
                    }
                    //Second
                    t_triangles[t_indexTriangles + 3] = t_indexVertices + p_numberOfVerticesAtBase;
                    if (i < p_numberOfVerticesAtBase - 1) {
                        t_triangles[t_indexTriangles + 4] = t_indexVertices + 1 + p_numberOfVerticesAtBase;
                        t_triangles[t_indexTriangles + 5] = t_indexVertices + 1;
                    }
                    else {
                        t_triangles[t_indexTriangles + 4] = t_indexVertices + 1;
                        t_triangles[t_indexTriangles + 5] = t_indexVertices + 1 - p_numberOfVerticesAtBase;
                    }
                    ++t_indexVertices;
                    t_indexTriangles += 6;
                }
            }
        }

        //Last Ring
        int t_last = t_vertices.Length - 1;
        for (int i = 0; i < p_numberOfVerticesAtBase; ++i) {
            t_triangles[t_indexTriangles + 0] = t_indexVertices;
            t_triangles[t_indexTriangles + 1] = t_last;
            if (i < p_numberOfVerticesAtBase - 1) {
                t_triangles[t_indexTriangles + 2] = t_indexVertices + 1;
            }
            else {
                t_triangles[t_indexTriangles + 2] = t_indexVertices + 1 - p_numberOfVerticesAtBase;
            }
            ++t_indexVertices;
            t_indexTriangles += 3;
        }

        //Is Down Half, flip values
        if(!p_up) {
            for(int i = 0; i < t_vertices.Length; ++i) {
                t_vertices[i].y = -t_vertices[i].y;
                t_normals[i].y = -t_normals[i].y;
            }

            //Swap triangle order
            int t_temp;
            for(int i= 0; i < t_triangles.Length; i+=3) {
                t_temp = t_triangles[i + 1];
                t_triangles[i + 1] = t_triangles[i + 2];
                t_triangles[i + 2] = t_temp;
            }
        }

        t_mesh.vertices = t_vertices;
        t_mesh.triangles = t_triangles;
        t_mesh.normals = t_normals;

        return t_mesh;
    }

    //Without filling top or bottom
    Mesh CreateCylinderMesh(int p_numberOfVerticesAtBase, float p_radius, float p_heightTotal) {
        Mesh t_mesh = new Mesh();

        //Minimum values
        if (p_numberOfVerticesAtBase < 3) { p_numberOfVerticesAtBase = 3; }
        if (p_radius < 0.02f) { p_radius = 0.02f; }
        if (p_heightTotal < 0.02f) { p_heightTotal = 0.02f; }

        Vector3[] t_vertices = new Vector3[p_numberOfVerticesAtBase * 2];
        Vector3[] t_normals = new Vector3[t_vertices.Length];
        int[] t_triangles = new int[t_vertices.Length * 3];

        float t_verticesAngle = 2f * Mathf.PI / (float)p_numberOfVerticesAtBase;
        float t_heightHalf = p_heightTotal * 0.5f;
        float t_radius;
        float t_angle;
        float t_x;
        float t_y;
        float t_z;

        t_radius = p_radius;
        t_y = t_heightHalf;
        for (int i = 0; i < p_numberOfVerticesAtBase; ++i) {
            t_angle = t_verticesAngle * i;
            t_x = Mathf.Cos(t_angle) * t_radius;
            t_z = Mathf.Sin(t_angle) * t_radius;

            //Vertices Down
            t_vertices[i] = new Vector3(t_x, -t_y, t_z);
            t_normals[i] = t_vertices[i].normalized;
            t_normals[i].y = 0f;

            //Vertices Up
            t_vertices[i + p_numberOfVerticesAtBase] = new Vector3(t_x, t_y, t_z);
            t_normals[i + p_numberOfVerticesAtBase] = t_vertices[i + p_numberOfVerticesAtBase].normalized;
            t_normals[i + p_numberOfVerticesAtBase].y = 0f;

            //Triangle First
            t_triangles[i * 6 + 0] = i;
            t_triangles[i * 6 + 1] = i + p_numberOfVerticesAtBase;
            if(i < p_numberOfVerticesAtBase - 1) {
                t_triangles[i * 6 + 2] = i + 1;
            }
            else {
                t_triangles[i * 6 + 2] = i + 1 - p_numberOfVerticesAtBase;
            }

            //Triangle Second
            t_triangles[i * 6 + 3] = i + p_numberOfVerticesAtBase;
            if (i < p_numberOfVerticesAtBase - 1) {
                t_triangles[i * 6 + 4] = i + 1 + p_numberOfVerticesAtBase;
                t_triangles[i * 6 + 5] = i + 1;
            }
            else {
                t_triangles[i * 6 + 4] = i + 1;
                t_triangles[i * 6 + 5] = i + 1 - p_numberOfVerticesAtBase;
            }
        }

        t_mesh.vertices = t_vertices;
        t_mesh.triangles = t_triangles;
        t_mesh.normals = t_normals;

        return t_mesh;
    }
}
