using UnityEngine.SceneManagement;
using UnityEngine;

public class DemoController : MonoBehaviour
{
    public TMPro.TMP_Dropdown m_sceneDropDown;
    private bool m_intialise = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    void LateUpdate()
    {
        m_intialise = true;
    }

    public void ResetLevel()
    {
        SceneData.s_instance.SpawnSelected();
    }

    public void ChangeScene()
    {
        if (m_intialise)
        {
            m_intialise = false;
            SceneManager.LoadScene(m_sceneDropDown.value);
        }
    }
}
