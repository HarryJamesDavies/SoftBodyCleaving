using UnityEngine;

public class DebugCamera : MonoBehaviour
{
    /*
    FEATURES

        WASD/Arrows:    Movement
                  Q:    Climb
                  E:    Drop
                      Shift:    Move faster
                    Control:    Move slower
                        Tab:    Toggle cursor locking to screen(you can also press Ctrl+P to toggle play mode on and off).
                        Space: Lock Camera
	*/
 
	public float m_cameraSensitivity = 90;
    public float m_climbSpeed = 4;
    public float m_normalMoveSpeed = 10;
    public float m_slowMoveFactor = 0.25f;
    public float m_fastMoveFactor = 3;

    private float m_rotationX = 0.0f;
    private float m_rotationY = 0.0f;

    private bool m_lockCamera = true;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            m_lockCamera = !m_lockCamera;
        }

        if (!m_lockCamera)
        {
            m_rotationX += Input.GetAxis("Mouse X") * m_cameraSensitivity * Time.deltaTime;
            m_rotationY += Input.GetAxis("Mouse Y") * m_cameraSensitivity * Time.deltaTime;
            m_rotationY = Mathf.Clamp(m_rotationY, -90, 90);

            transform.localRotation = Quaternion.AngleAxis(m_rotationX, Vector3.up);
            transform.localRotation *= Quaternion.AngleAxis(m_rotationY, Vector3.left);

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                transform.position += transform.forward * (m_normalMoveSpeed * m_fastMoveFactor) * Input.GetAxis("Vertical") * Time.deltaTime;
                transform.position += transform.right * (m_normalMoveSpeed * m_fastMoveFactor) * Input.GetAxis("Horizontal") * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                transform.position += transform.forward * (m_normalMoveSpeed * m_slowMoveFactor) * Input.GetAxis("Vertical") * Time.deltaTime;
                transform.position += transform.right * (m_normalMoveSpeed * m_slowMoveFactor) * Input.GetAxis("Horizontal") * Time.deltaTime;
            }
            else
            {
                transform.position += transform.forward * m_normalMoveSpeed * Input.GetAxis("Vertical") * Time.deltaTime;
                transform.position += transform.right * m_normalMoveSpeed * Input.GetAxis("Horizontal") * Time.deltaTime;
            }


            if (Input.GetKey(KeyCode.Q)) { transform.position += transform.up * m_climbSpeed * Time.deltaTime; }
            if (Input.GetKey(KeyCode.E)) { transform.position -= transform.up * m_climbSpeed * Time.deltaTime; }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (Cursor.visible)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }
    }
}
