using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour
{
    public Transform m_transform;//僵尸
    private FPSPlayer m_player;//玩家实例
    private NavMeshAgent m_agent;//寻路器

    private float move_speed = 3f;//僵尸的移动速度
    private float max_distance = 30f;//僵尸能感应到人的最大范围
    private float time = 0;//计时器，更新寻路目标点

    Animator anim;//动画控制器

    float m_rotspeed;//旋转速度
    public int m_life = 5;              // 生命值

    // Start is called before the first frame update
    void Start()
    {
        m_transform = this.transform;
        m_player = GameObject.FindGameObjectWithTag("Player").GetComponent<FPSPlayer>();
        m_agent = this.GetComponent<NavMeshAgent>();
        m_agent.SetDestination(m_player.transform.position);//设置僵尸寻路的目标点

        anim = GetComponent<Animator>();//设置动画组件
    }

    // Update is called once per frame
    void Update()
    {
        if (m_player.life == 0) return;//如果主角的血量为0，那僵尸什么也不做

        AnimatorStateInfo anim_info = anim.GetCurrentAnimatorStateInfo(0);//获取僵尸的所有动画状态
        //如果僵尸处于静止状态且不在过渡状态
        if (anim_info.fullPathHash == Animator.StringToHash("Base Layer.idle") && !anim.IsInTransition(0))
        {
            //如果玩家进入僵尸的最大感知范围，僵尸解除静止状态
            if (Vector3.Distance(m_transform.position, m_player.m_transform.position) < max_distance)
            {
                anim.SetBool("idle", false);
            }
            //如果玩家和僵尸的距离小于8，僵尸进入攻击状态，否则，走向玩家
            if (Vector3.Distance(m_transform.position, m_player.m_transform.position) < 8.0f)
            {
                anim.SetBool("attack", true);
            }
            else
            {
                m_agent.SetDestination(m_player.m_transform.position);//设置僵尸寻路的目标点
                anim.SetBool("walk", true);
            }
        }

        //如果僵尸处于行走状态且不在过渡状态
        if (anim_info.fullPathHash == Animator.StringToHash("Base Layer.walk") && !anim.IsInTransition(0))
        {
            anim.SetBool("walk", false);
            time -= Time.deltaTime;
            if (time < 0)
            {
                m_agent.SetDestination(m_player.m_transform.position);//设置僵尸寻路的目标点
                time = 1;//每隔一秒定位一次玩家的位置
            }
            m_agent.Move(m_transform.TransformDirection(new Vector3(0, 0, move_speed * Time.deltaTime)));//寻路位置更新

            //如果玩家和僵尸的距离小于8，僵尸进入攻击状态
            if (Vector3.Distance(m_transform.position, m_player.m_transform.position) < 8.0f)
            {
                m_agent.ResetPath();//停止寻路状态
                anim.SetBool("attack", true);
            }
        }

        //如果僵尸处于攻击状态且不在过渡状态
        if (anim_info.fullPathHash == Animator.StringToHash("Base Layer.attack") && !anim.IsInTransition(0))
        {
            //先让僵尸面对玩家进行攻击
            Vector3 last_angle = m_transform.eulerAngles;//先存储僵尸的当前朝向，作为旋转的起点
            m_transform.LookAt(m_player.m_transform);//将僵尸的forward锁定在玩家身上
            Vector3 recent_angle = m_transform.eulerAngles;//存储僵尸面对玩家的朝向，作为旋转的终点

            //基于旋转的速度，计算两个角度之间的旋转角度
            float angle = Mathf.MoveTowardsAngle(last_angle.y, recent_angle.y, m_rotspeed * Time.deltaTime);
            m_transform.eulerAngles = new Vector3(0, angle, 0);//僵尸转向

            anim.SetBool("attack", true);
            if (anim_info.normalizedTime >= 1.0f)//动画已经播放完一次了，待机
            {
                anim.SetBool("idle", true);
                time = 1;//重置计时器

                m_player.OnDamage(1);
            }
        }

        //如果僵尸处于死亡状态且不在过渡状态
        if (anim_info.fullPathHash == Animator.StringToHash("Base Layer.fallForward") && !anim.IsInTransition(0))
        {
            if (anim_info.normalizedTime >= 1.0f)//动画已经播放完一次了，僵尸死亡
            {
                Destroy(this.gameObject);
            }
        }
    }

    //僵尸受到伤害
    public void OnDamage(int damage)
    {
        m_life -= damage;
        //如果没命了，释放鼠标
        if (m_life <= 0)
        {
            anim.SetBool("fallForward", true);
        }
    }
}
