using UnityEngine;

//Initial script by Foulco
//Initial viable version : 2020-01-20
//
//INFORMATIONS
//A script that will attach itself to a game object to follow the specified collider (Position, Scaling, Rotation)
//
//MODIFICATIONS
//2020-XX-XX    <Informations>
//

public class CollisionViewerFollower : MonoBehaviour
{
  //Params
  CollisionViewerParam m_params;
  //The collider it follow
  Collider m_collider;
  //The parent renderer potentialy attached (May be null)
  MeshRenderer m_parentRenderer;

  //Extra for Capsule
  GameObject m_childCylinder;
  GameObject m_childSphereUp;
  GameObject m_childSphereDown;

  //TODO : Keep track of the renderer state (enable or not)
  //Instead of simply disabling it then re-enabling it
  //It could keep track if the collider it follow if it is disable-enable
  //We could set the reverse state for the renderer at the first script that run an Update
  //Then at the last script that run an Update, chech the state, and store it.
  //This way it could prevent some problem when entering shop/houses, etc

  //Create and initialize a CollisionFollower to a GameObject
  public static CollisionViewerFollower CreateAndAttachTo(GameObject p_gameObject, CollisionViewerParam p_params, Collider p_collider, MeshRenderer p_parentRenderer, CollisionViewerParam p_visibilityParams)
  {
    CollisionViewerFollower t_follower = p_gameObject.AddComponent<CollisionViewerFollower>();
    t_follower.m_params = p_params;
    t_follower.m_collider = p_collider;
    t_follower.m_parentRenderer = p_parentRenderer;

    //Extra for Capsule
    if (p_collider is CapsuleCollider)
    {
      t_follower.m_childCylinder = p_gameObject.transform.GetChild(0).gameObject;
      t_follower.m_childSphereUp = p_gameObject.transform.GetChild(1).gameObject;
      t_follower.m_childSphereDown = p_gameObject.transform.GetChild(2).gameObject;
    }

    t_follower.SetVisibility(p_visibilityParams);
    return t_follower;
  }

  public void SetVisibility(CollisionViewerParam p_visibilityParams)
  {
    if (m_collider != null)
    {
      bool t_isVisible = (p_visibilityParams & m_params) == m_params;
      gameObject.SetActive(t_isVisible);
      if (m_parentRenderer)
      {
        m_parentRenderer.enabled = !t_isVisible;
      }
    }
  }

  //Know if it follow the requested collider
  public bool FollowThisCollider(Collider p_collider)
  {
    return m_collider == p_collider;
  }

  //Know if this can be destroyed, since the collider it was following no longer exist for "X" unknown reason
  public bool CanBeDestroy()
  {
    return m_collider == null;
  }

  //Update the transform to correctly follow the object attached to
  public void ManualUpdate()
  {
    //Check if it need to be updated
    if (gameObject.activeInHierarchy && m_collider != null)
    {
      //Box Collider
      if (m_collider is BoxCollider)
      {
        BoxCollider t_boxCollider = (BoxCollider)m_collider;
        transform.localPosition = t_boxCollider.center;
        transform.localScale = new Vector3(t_boxCollider.size.x, t_boxCollider.size.y, t_boxCollider.size.z);
      }
      //Capsule Collider
      else if (m_collider is CapsuleCollider)
      {
        //TODO : Fix the weird (rotation?) problem when this is inside an objet with rotation that is inside another objet with rotation, that affect some specific axes.
        //Not a big problem for now thought.
        //Might be because of the rotation dilema

        CapsuleCollider t_capsuleCollider = (CapsuleCollider)m_collider;
        transform.localPosition = t_capsuleCollider.center;

        //Rotation
        switch (t_capsuleCollider.direction)
        {
          //X Direction
          case 0:
            {
              transform.localEulerAngles = new Vector3(0f, 0f, 90f);
              break;
            }
          //Y Direction
          case 1:
            {
              transform.localEulerAngles = new Vector3(0f, 0f, 0f);
              break;
            }
          //Z Direction
          default:
            {
              transform.localEulerAngles = new Vector3(90f, 0f, 0f);
              break;
            }
        }

        //Values
        Vector3 t_lossyScale = transform.lossyScale;
        bool t_simulateSphere;

        float t_targetHeight;
        float t_targetRadius;

        float t_cyliderScaleX;
        float t_cyliderScaleY;
        float t_cyliderScaleZ;

        float t_halfSphereDistanceY;
        float t_halfSphereScaleXYZ;

        //Height
        t_targetHeight = Mathf.Abs(t_lossyScale.y * t_capsuleCollider.height);
        //Radius : Biggest X or Z
        t_targetRadius = (Mathf.Abs(t_lossyScale.x) > Mathf.Abs(t_lossyScale.z) ? Mathf.Abs(t_lossyScale.x) : Mathf.Abs(t_lossyScale.z)) * Mathf.Abs(t_capsuleCollider.radius);
        //Half-Spheres Scale
        t_halfSphereScaleXYZ = t_targetRadius * 2f;

        //Evaluate if must simulate a Sphere
        if (t_targetRadius * 2f >= t_targetHeight)
        {
          t_simulateSphere = true;

          t_cyliderScaleX = 0f;
          t_cyliderScaleY = 0f;
          t_cyliderScaleZ = 0f;

          t_halfSphereDistanceY = 0f;
        }
        else
        {
          t_simulateSphere = false;

          //Calculate Cylinder Scale
          t_cyliderScaleX = t_targetRadius * 2f;
          t_cyliderScaleY = t_targetHeight - t_targetRadius * 2f;
          t_cyliderScaleZ = t_targetRadius * 2f;

          //Calculate Half-Sphere Distance
          t_halfSphereDistanceY = (t_cyliderScaleY * 0.5f) / t_lossyScale.y;
        }

        //Check if need to simulate a sphere
        if (t_simulateSphere)
        {
          //Cylinder
          m_childCylinder.transform.localScale = new Vector3();
          //Half-Spheres
          m_childSphereUp.transform.localPosition = new Vector3();
          m_childSphereDown.transform.localPosition = new Vector3();
        }
        else
        {
          //Cylinder
          m_childCylinder.transform.parent = null;
          m_childCylinder.transform.localScale = new Vector3(t_cyliderScaleX, t_cyliderScaleY, t_cyliderScaleZ);
          m_childCylinder.transform.parent = transform;

          //Half-Spheres
          m_childSphereUp.transform.localPosition = new Vector3(0f, t_halfSphereDistanceY, 0f);
          m_childSphereDown.transform.localPosition = new Vector3(0f, -t_halfSphereDistanceY, 0f);
        }

        //Half-Spheres Scale
        m_childSphereUp.transform.parent = null;
        m_childSphereUp.transform.localScale = new Vector3(t_halfSphereScaleXYZ, t_halfSphereScaleXYZ, t_halfSphereScaleXYZ);
        m_childSphereUp.transform.parent = transform;

        m_childSphereDown.transform.parent = null;
        m_childSphereDown.transform.localScale = new Vector3(t_halfSphereScaleXYZ, t_halfSphereScaleXYZ, t_halfSphereScaleXYZ);
        m_childSphereDown.transform.parent = transform;
      }
      //Sphere Collider
      else if (m_collider is SphereCollider)
      {
        SphereCollider t_sphereCollider = (SphereCollider)m_collider;
        transform.localPosition = t_sphereCollider.center;
        transform.localScale = new Vector3(1f, 1f, 1f) * t_sphereCollider.radius * 2;
      }
      //Mesh Collider
      else if (m_collider is MeshCollider)
      {
        //Do nothing
      }
    }
  }
}