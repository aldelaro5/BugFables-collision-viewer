//Draw a line in the direction the player is facing
//Using a LineRenderer, might be better to use something else (like a long cube instead)
//Because of how LineRenderer are drawn

using UnityEngine;

public class DisplayPlayerDirection : MonoBehaviour {
	//constants
	const float KABBU_DASH_DISTANCE = 3.9f; // Is 3.5f but is ajusted to 3.9f following a function call in the original game

	//Singleton
	public static DisplayPlayerDirection Instance() { return m_instance; }
	static DisplayPlayerDirection m_instance;

	//Variables
	Shader m_shaderSpritesDefault;
	Material m_material;
	GameObject m_directionAction;
	LineRenderer m_lineRenderer;

    private void Start() {
        //Initialize
        if (m_instance == null) {
            //This is now the singleton instance
            m_instance = this;

			//Shader
			m_shaderSpritesDefault = Shader.Find("Sprites/Default");

			//Materials
			if (m_shaderSpritesDefault != null) {
				m_material = new Material(m_shaderSpritesDefault);
				m_material.color = Color.yellow;
			}

			//GameObejct
			m_directionAction = new GameObject("MOD_DirectionAction");
			m_directionAction.transform.parent = this.transform;
			m_lineRenderer = m_directionAction.AddComponent<LineRenderer>();
			m_lineRenderer.enabled = true;
			m_lineRenderer.alignment = LineAlignment.TransformZ;
			m_lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			m_lineRenderer.receiveShadows = false;
			m_lineRenderer.allowOcclusionWhenDynamic = false;
			m_lineRenderer.useWorldSpace = true;
			m_lineRenderer.startWidth = 0.1f;
			m_lineRenderer.endWidth = 0.1f;
			m_lineRenderer.material = m_material;
		}
        else if (this != m_instance) {
            Destroy(this);
        }
    }

    //Update
    void Update() {
		///Check if initilization was fine
		if (m_lineRenderer != null) {
			PlayerControl t_player = MainManager.player;
			//Check if find the player
			if (t_player == null) {
				//Reset the line positions
				m_lineRenderer.SetPositions(new Vector3[2] { new Vector3(), new Vector3() });
			}
			else {
				//Direction you are facing
				Vector3 t_fromPos = t_player.entity.transform.position + new Vector3(0f, 0.5f, 0f);
				Vector3 t_toPos = t_fromPos + new Vector3(t_player.lastdelta.x, t_player.lastdelta.y, t_player.lastdelta.z).normalized * KABBU_DASH_DISTANCE;
				m_lineRenderer.SetPositions(new Vector3[] { t_fromPos, t_toPos });
			}
		}
	}
}
