using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// 移動速度
    /// </summary>
    private const float MOVE_SPRRD = 5.0f;

    /// <summary>
    /// 物理演算コンポーネント
    /// </summary>
    [SerializeField] private Rigidbody rigidbody;

    /// <summary>
    /// 自動生成されたクラス
    /// </summary>
    private DefaultInputActions inputActions;

    /// <summary>
    /// 入力方向
    /// </summary>
    private Vector2 moveInput;

    /// <summary>
    /// 外部（アニメーションやUIなど）に現在の速度を教えるために保持するVelocity
    /// </summary>
    public Vector3 CurrentVelocity { get; private set; }

    private void Awake()
    {
        if (rigidbody == null)
        {
            Debug.LogError("PlayerにRigidbodyがアタッチされていません！");
        }

        inputActions = new DefaultInputActions();
        inputActions.Player.Fire.performed += OnFire;
    }

    private void OnEnable ()
    {
        inputActions.Enable();
    }

    private void OnDisable ()
    {
        inputActions.Disable();
    }

    private void Update()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
    }

    private void FixedUpdate ()
    {
        // 物理演算に関わる移動処理になるため、FixedUpdateで行う
        Move();
    }

    private void Move()
    {
        if (rigidbody == null)
        {
            return;
        }

        // 入力がない場合はピタッと止める
        if (moveInput == Vector2.zero)
        {
            rigidbody.linearVelocity = new Vector3(0f, rigidbody.linearVelocity.y, 0f);
            CurrentVelocity = Vector2.zero;
            return;
        }

        // 実際の速度を計算
        Vector3 moveDirection = new Vector3(moveInput.x, rigidbody.linearVelocity.y, moveInput.y);
        moveDirection.Normalize();

        // Y軸の速度（落下など）は現在の物理演算の値を維持し、XとZのみ上書きする
        rigidbody.linearVelocity = moveDirection * MOVE_SPRRD;

        // 外部（アニメーションやUIなど）に現在の速度を教えるためにプロパティを更新
        CurrentVelocity = rigidbody.linearVelocity;
    }

    private void OnFire (InputAction.CallbackContext context)
    {
        Debug.Log("Fireボタンが押されました。");
    }
}
