using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    FPSPlayer m_player;//玩家

    Text text_Life;      //玩家生命值

    // Start is called before the first frame update
    void Start()
    {
        m_player = GameObject.FindGameObjectWithTag("Player").GetComponent<FPSPlayer>();
        text_Life = this.transform.Find("text_Life").GetComponent<Text>();
        text_Life.text = " 生命：" + m_player.life.ToString();
    }

    //更新生命
    public void SetLife(int life)
    {
        text_Life.text = "生命：" + life.ToString();
    }

    void OnGUI()
    {
        if (m_player.life <= 0)
        {//居中显示玩家已挂
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.skin.label.fontSize = 40;
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "玩家已挂");

            //重新游戏
            GUI.skin.label.fontSize = 50;
            if (GUI.Button(new Rect(Screen.width * 0.5f - 150, Screen.height * 0.75f, 300, 40), "再来一次"))
            {
                Application.LoadLevel(Application.loadedLevelName);
            }
        }
    }
}
