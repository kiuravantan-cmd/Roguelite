using UnityEngine;

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
    /// 移動方向のベクトル
    /// </summary>
    private Vector3 moveDirection;

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
    }

    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        // // 入力値から移動方向のベクトルを作成し、斜め移動が速くならないよう正規化(normalized)する
        moveDirection = new Vector3(x, 0f, z).normalized;
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
        if (moveDirection == Vector3.zero)
        {
            rigidbody.linearVelocity = new Vector3(0f, rigidbody.linearVelocity.y, 0f);
            CurrentVelocity = Vector3.zero;
            return;
        }

        // 実際の速度を計算
        Vector3 targetVelocity = moveDirection * MOVE_SPRRD;

        // Y軸の速度（落下など）は現在の物理演算の値を維持し、XとZのみ上書きする
        rigidbody.linearVelocity = new Vector3(targetVelocity.x, rigidbody.linearVelocity.y, targetVelocity.z);

        // 外部（アニメーションやUIなど）に現在の速度を教えるためにプロパティを更新
        CurrentVelocity = rigidbody.linearVelocity;
    }
}
