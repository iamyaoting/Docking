using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputGUI : MonoBehaviour
{
    public UnityEngine.UI.Button m_upBtn;
    public UnityEngine.UI.Button m_downBtn;
    public UnityEngine.UI.Button m_leftBtn;
    public UnityEngine.UI.Button m_rightBtn;
    public UnityEngine.UI.Button m_dockBtn;

    private UnityEngine.UI.Button[] m_btns;
    private KeyCode[][] m_keys;
    // Start is called before the first frame update
    void Start()
    {
        m_upBtn = transform.Find("UpButton").GetComponent<UnityEngine.UI.Button>();
        m_downBtn = transform.Find("DownButton").GetComponent<UnityEngine.UI.Button>();
        m_leftBtn = transform.Find("LeftButton").GetComponent<UnityEngine.UI.Button>();
        m_rightBtn = transform.Find("RightButton").GetComponent<UnityEngine.UI.Button>();
        m_dockBtn = transform.Find("DockButton").GetComponent<UnityEngine.UI.Button>();

        m_btns = new UnityEngine.UI.Button[4] { m_upBtn, m_downBtn, m_leftBtn, m_rightBtn };

        m_keys = new KeyCode[4][];
        m_keys[0] = new KeyCode[] { KeyCode.W, KeyCode.UpArrow };
        m_keys[1] = new KeyCode[] { KeyCode.S, KeyCode.DownArrow };
        m_keys[2] = new KeyCode[] { KeyCode.A, KeyCode.LeftArrow };
        m_keys[3] = new KeyCode[] { KeyCode.D, KeyCode.RightArrow };
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < 4; ++i)
        {
            var btn = m_btns[i];
            for (int j = 0; j < m_keys[i].Length; ++j)
            {
                if (Input.GetKeyDown(m_keys[i][j]))
                {
                    btn.onClick.Invoke();
                    btn.OnSubmit(null);
                }
            }
        }

        if(Controller.HasEnvInteractiveActionUserInput())
        {
            m_dockBtn.onClick.Invoke();
            m_dockBtn.OnSubmit(null);
        }
    }
}
