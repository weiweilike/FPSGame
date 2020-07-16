using System;
using System.Linq;
using UnityEngine;


public class FPSPlayer : MonoBehaviour
{
    public Transform m_transform;//当前的玩家

    [Header("Movement Settings")]
    [SerializeField]
    private float walkingSpeed = 5f;

    [SerializeField]
    private float jumpForce = 35f;

    [Header("Look Settings")]
    [SerializeField]
    private float mouseSensitivity = 7f;
    [SerializeField]
    private float smooth_time = 0.05f;

    private float current_x;
    private float current_y;
    private float current_xx = 0f;
    private float current_yy = 0f;
    private float _currentVelocity = 0f;

    public int life = 5;//玩家的生命值

    [SerializeField]
    private float minVerticalAngle = -90f;

    [SerializeField]
    private float maxVerticalAngle = 90f;

    private Rigidbody _rigidbody;
    private CapsuleCollider _collider;

    private bool _isGrounded;

    private readonly RaycastHit[] _groundCastResults = new RaycastHit[8];
    private readonly RaycastHit[] _wallCastResults = new RaycastHit[8];

    // 初始化
    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;//锁定刚体的旋转状态
        _collider = GetComponent<CapsuleCollider>();
        m_transform.SetPositionAndRotation(transform.position, transform.rotation);//游戏开始同步手臂与游戏对象

        current_x = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        current_y = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        Cursor.lockState = CursorLockMode.Locked;//锁定光标
    }

    // 持续碰撞函数（？）
    private void OnCollisionStay()
    {
        var bounds = _collider.bounds;
        var extents = bounds.extents;
        var radius = extents.x - 0.01f;
        Physics.SphereCastNonAlloc(bounds.center, radius, Vector3.down,
            _groundCastResults, extents.y - radius * 0.5f, ~0, QueryTriggerInteraction.Ignore);
        if (!_groundCastResults.Any(hit => hit.collider != null && hit.collider != _collider)) return;
        for (var i = 0; i < _groundCastResults.Length; i++)
        {
            _groundCastResults[i] = new RaycastHit();
        }

        _isGrounded = true;
    }

    // Processes the character movement and the camera rotation every fixed framerate frame.
    private void FixedUpdate()
    {
        RotateCameraAndCharacter();
        MoveCharacter();
        _isGrounded = false;
    }

    // 相机与对象位置一致，判断跳跃
    private void Update()
    {
        m_transform.position = transform.position;
        //判断跳跃
        if (_isGrounded && Input.GetButtonDown("space"))
        {
            _isGrounded = false;
            _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void RotateCameraAndCharacter()
    {
        current_x = Mathf.SmoothDampAngle(current_x, Input.GetAxisRaw("Mouse X") * mouseSensitivity, ref _currentVelocity, smooth_time);
        current_y = Mathf.SmoothDampAngle(current_y, Input.GetAxisRaw("Mouse Y") * mouseSensitivity, ref _currentVelocity, smooth_time);
        //限制相机垂直方向上的角度范围
        var currentAngle = m_transform.eulerAngles.x;
        if (currentAngle > 180f)
            currentAngle -= 360f;
        else if (currentAngle <= -180f)
            currentAngle += 360f;
        current_y = Mathf.Clamp(current_y, minVerticalAngle + currentAngle + 0.01f, maxVerticalAngle + currentAngle - 0.01f);

        var worldUp = m_transform.InverseTransformDirection(Vector3.up);//转换到世界坐标系
        var rotation = m_transform.rotation *
                       Quaternion.AngleAxis(current_x, worldUp) *
                       Quaternion.AngleAxis(current_y, Vector3.left);//一个好用的旋转的工具（还没有很弄懂原理）
        transform.eulerAngles = new Vector3(0f, rotation.eulerAngles.y, 0f);
        m_transform.rotation = rotation;
    }

    private void MoveCharacter()
    {
        var direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;//单位化
        var worldDirection = transform.TransformDirection(direction);
        var velocity = worldDirection * walkingSpeed;

        //判断下一步是否会撞到墙
        if (CheckCollisionsWithWalls(velocity))
        {
            current_xx = current_yy = 0f;
            return;
        }

        current_xx = Mathf.SmoothDamp(current_xx, velocity.x, ref _currentVelocity, smooth_time);
        current_yy = Mathf.SmoothDamp(current_yy, velocity.z, ref _currentVelocity, smooth_time);
        var rigidbodyVelocity = _rigidbody.velocity;
        var force = new Vector3(current_xx - rigidbodyVelocity.x, 0f, current_yy - rigidbodyVelocity.z);
        _rigidbody.AddForce(force, ForceMode.VelocityChange);
    }

    private bool CheckCollisionsWithWalls(Vector3 velocity)
    {
        if (_isGrounded) return false;
        var bounds = _collider.bounds;
        var radius = _collider.radius;
        var halfHeight = _collider.height * 0.5f - radius * 1.0f;
        var point1 = bounds.center;
        point1.y += halfHeight;
        var point2 = bounds.center;
        point2.y -= halfHeight;
        Physics.CapsuleCastNonAlloc(point1, point2, radius, velocity.normalized, _wallCastResults,
            radius * 0.04f, ~0, QueryTriggerInteraction.Ignore);
        var collides = _wallCastResults.Any(hit => hit.collider != null && hit.collider != _collider);
        if (!collides) return false;
        for (var i = 0; i < _wallCastResults.Length; i++)
        {
            _wallCastResults[i] = new RaycastHit();
        }

        return true;
    }

    //玩家受到攻击
    public void OnDamage(int damage)
    {
        life -= damage;
        //UI_Manager.SetLife(life);//调用管理器实例的方法
        //如果没命了，释放鼠标
        if (life <= 0) Screen.lockCursor = false;
    }
}