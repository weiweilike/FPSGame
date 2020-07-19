using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Robot : MonoBehaviour
{
    public Transform m_transform;//机器人
    private FPSPlayer m_player;//玩家实例

    Slider slider;//滑块表示机器人的血条，血量为10

    Animator anim;//动画控制器

    // Start is called before the first frame update
    void Start()
    {

        anim = GetComponent<Animator>();//设置动画组件
        slider = GameObject.Find("Robot/Robot_Base/Canvas/Slider").GetComponent<Slider>();
        slider.value = 10;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDamage(int demage)
    {
        slider.value -= demage;

        //如果没命了,怪物死亡
        if (slider.value <= 0)
        {
            Destroy(this.gameObject);
        }
    }
}
