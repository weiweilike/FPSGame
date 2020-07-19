using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Handgun : MonoBehaviour {

    public LayerMask m_layer;           // 碰撞层
    public Transform m_fx;              // 射中目标时的特效

    Animator anim;//动画控制器

	[Header("Gun Camera")]
	public Camera gunCamera;

	[Header("UI Weapon Name")]
	public string weaponName;
	private string storedWeaponName;

	private bool isReloading;
	private bool isAiming;

	private int currentAmmo;
	public int ammo;//子弹总数
	private bool outOfAmmo;//子弹是否用完

	[Header("Audio Source")]
	public AudioSource mainAudioSource;
	public AudioSource shootAudioSource;

	[Header("UI Components")]
	public Text currentWeaponText;
	public Text currentAmmoText;
	public Text totalAmmoText;

	[System.Serializable]
	public class prefabs
	{  
		[Header("Prefabs")]
		public Transform bulletPrefab;
		public Transform casingPrefab;
		public Transform grenadePrefab;
	}
	public prefabs Prefabs;
	
	[System.Serializable]
	public class spawnpoints
	{  
		[Header("Spawnpoints")]
		public Transform bulletSpawnPoint;//子弹的发射点
	}
	public spawnpoints Spawnpoints;

	[System.Serializable]
	public class soundClips
	{
		public AudioClip shootSound;
		public AudioClip takeOutSound;
		public AudioClip holsterSound;
		public AudioClip reloadSoundOutOfAmmo;
		public AudioClip reloadSoundAmmoLeft;
		public AudioClip aimSound;
	}
	public soundClips SoundClips;

	private void Awake () 
	{
		anim = GetComponent<Animator>();//设置动画组件
		currentAmmo = ammo;//设置当前的子弹数目为子弹的总数目
	}

	private void Start () {
		storedWeaponName = weaponName;//存储当前的武器的名称
		currentWeaponText.text = weaponName;//将当前武器名称存到text中
		totalAmmoText.text = ammo.ToString();//设置总的子弹数目text

		shootAudioSource.clip = SoundClips.shootSound;//将设计的声音设置为音频源

    }
	
	private void Update () {
		//瞄准
		//当按下鼠标右键或alt时切换摄像机变成瞄准模式
		if(Input.GetButton("Fire2") && !isReloading) 
		{	
			isAiming = true;
			anim.SetBool ("Aim", true);//设置瞄准动画

			mainAudioSource.clip = SoundClips.aimSound;
			mainAudioSource.Play ();
		} 
		else 
		{
			isAiming = false;
			anim.SetBool ("Aim", false);
		}

        AnimationCheck();//判断当前是否在换弹夹
        currentAmmoText.text = currentAmmo.ToString ();//更新当前的子弹数目到text中

		//当按下Q键时，换刀进行一次攻击
		if (Input.GetKeyDown (KeyCode.Q)) 
			anim.Play ("Knife Attack 1", 0, 0f);

        outOfAmmo = (currentAmmo == 0) ? true : false;//判断子弹是否用完

		//射击
		if (Input.GetMouseButtonDown (0) && !outOfAmmo && !isReloading) 
		{
			currentAmmo -= 1;//子弹数目减一

			shootAudioSource.clip = SoundClips.shootSound;
			shootAudioSource.Play ();

			if (!isAiming)
				anim.Play ("Fire", 0, 0f);//如果不在瞄准状态，播放正常的射击动画
			else
				anim.Play ("Aim Fire", 0, 0f);//如果在瞄准状态，播放瞄准状态的射击动画
				
			//在目标点复制一个子弹实体
			var bullet = (Transform)Instantiate (
				Prefabs.bulletPrefab,
				Spawnpoints.bulletSpawnPoint.transform.position,
				Spawnpoints.bulletSpawnPoint.transform.rotation
                );

            //给子弹加上速度
            bullet.GetComponent<Rigidbody>().velocity = 500 * bullet.transform.forward;

            RaycastHit info;    // 保存射线探测结果
            // 射线只能与m_layer指定的层发生碰撞
            bool hit = Physics.Raycast(
                Spawnpoints.bulletSpawnPoint.position,                                 // 发射点--枪口位置
                gunCamera.transform.forward,     // 方向
                out info,                                               // 发生碰撞时保存光线投射信息
                1000,                                                    // 射线长度
                m_layer);                                               // 碰撞目标层
            if (hit)
            {
                // 如果射击到了Zombie
                if (info.transform.tag == "Zombie")
                {
                    // 获取游戏体实例
                    Zombie zombie = info.transform.GetComponent<Zombie>();
                    // 更新被射中游戏角色的生命
                    zombie.OnDamage(2);
                }

                // 播放一个射击效果
                //Instantiate(m_fx, info.point, info.transform.rotation);
            }
        }

        //子弹重载
        if (Input.GetKeyDown(KeyCode.R) && !isReloading)
        {
            anim.Play("Reload Ammo Left", 0, 0f);//播放换弹夹的动画

            mainAudioSource.clip = SoundClips.reloadSoundAmmoLeft;
            mainAudioSource.Play();

            //重新存储当前子弹数目
            currentAmmo = ammo;
            outOfAmmo = false;
        } 
	}

	//检查当前的动画状态
	private void AnimationCheck () 
	{
		//检查是否在换弹夹
		if (anim.GetCurrentAnimatorStateInfo (0).IsName ("Reload Ammo Left")) 
			isReloading = true;
		else 
			isReloading = false;
	}
}