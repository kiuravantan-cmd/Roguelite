using MyProject.Scripts.InGame.Interface;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MyProject.Scripts.InGame.Camera
{

    public class PlayerController : MonoBehaviour
    {
        /// <summary>
        /// 移動速度
        /// </summary>
        private const float MOVE_SPEED = 5.0f;

        /// <summary>
        /// 回転速度
        /// </summary>
        private const float ROTATION_SPEED = 10.0f;

        /// <summary>
        /// 相手に与えるダメージ量
        /// </summary>
        private const int ATTACK_DAMAGE = 20;

        /// <summary>
        /// 攻撃距離（射撃範囲）
        /// </summary>
        private const float ATTACK_RANGE = 50f;

        /// <summary>
        /// レーザーポインターの描画距離
        /// </summary>
        private const float LASER_MAX_DISTANCE = 50f;

        /// <summary>
        /// 物理演算コンポーネント
        /// </summary>
        [SerializeField] private Rigidbody rigidbody;

        /// <summary>
        /// レーザーポインターの描画コンポーネント
        /// </summary>
        [SerializeField] private LineRenderer laserLineRenderer;

        /// <summary>
        /// 銃口の位置
        /// </summary>
        [SerializeField] private Transform weaponOrigin;

        /// <summary>
        /// 自動生成されたクラス
        /// </summary>
        private PlayerInputActions inputActions;

        /// <summary>
        /// 入力方向
        /// </summary>
        private Vector2 moveInput;

        /// <summary>
        /// カメラのトランスフォーム
        /// </summary>
        private Transform mainCameraTransform;

        /// <summary>
        /// 外部（アニメーションやUIなど）に現在の速度を教えるために保持するVelocity
        /// </summary>
        public Vector3 CurrentVelocity { get; private set; }

        private void Awake ()
        {
            if (rigidbody == null)
            {
                Debug.LogError("PlayerにRigidbodyがアタッチされていません！");
            }

            if (UnityEngine.Camera.main != null)
            {
                mainCameraTransform = UnityEngine.Camera.main.transform;
            }
            else
            {
                Debug.LogError("Main Cameraが見つかりません！");
            }

            inputActions = new PlayerInputActions();
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

        private void Update ()
        {
            moveInput = inputActions.Player.Move.ReadValue<Vector2>();

            DrawLaserPointer();
        }

        private void FixedUpdate ()
        {
            // 物理演算に関わる移動処理になるため、FixedUpdateで行う
            Move();
        }

        private void Move ()
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

            // --- カメラ基準の移動方向計算 ---
            Vector3 cameraForward = mainCameraTransform.forward;
            Vector3 cameraRight = mainCameraTransform.right;

            // Y軸（高さ）を0にして、水平なベクトルにする
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            // 入力値とカメラの向きを掛け合わせて、進むべき方向を決定
            Vector3 moveDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x);
            moveDirection.Normalize();

            // --- キャラクターの振り向き ---
            // 進む方向を向く角度（Quaternion）を作り、Slerpで滑らかに回転させる
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rigidbody.rotation = Quaternion.Slerp(rigidbody.rotation, targetRotation, ROTATION_SPEED * Time.fixedDeltaTime);

            // Y軸の速度（落下など）は現在の物理演算の値を維持し、XとZのみ上書きする
            rigidbody.linearVelocity = moveDirection * MOVE_SPEED;

            // 外部（アニメーションやUIなど）に現在の速度を教えるためにプロパティを更新
            CurrentVelocity = rigidbody.linearVelocity;
        }

        private void OnFire(InputAction.CallbackContext context)
        {
            // カメラの中央から真っ直ぐ前へ光線を飛ばす
            Ray ray = new Ray(mainCameraTransform.position, mainCameraTransform.forward);

            // 光線が何かに当たったか判定
            if (Physics.Raycast(ray, out RaycastHit hitInfo, ATTACK_RANGE))
            {
                Debug.Log($"{hitInfo.collider.name} に命中！");

                // 当たった相手が IDamageable (ダメージを受けられる性質) を持っているか確認
                IDamageable target = hitInfo.collider.GetComponent<IDamageable>();

                // ダメージを受けられる性質を持っていればダメージ処理を行う
                if (target != null)
                {
                    target.TakeDamage(ATTACK_DAMAGE);
                }
            }
        }

        /// <summary>
        /// レーザーを描画
        /// </summary>
        private void DrawLaserPointer()
        {
            if (laserLineRenderer == null || weaponOrigin == null || mainCameraTransform == null)
            {
                return;
            }

            laserLineRenderer.SetPosition(0, weaponOrigin.position);

            // カメラの中央から真っ直ぐ前へ光線を飛ばす
            Ray ray = new Ray(mainCameraTransform.position, mainCameraTransform.forward);

            // 光線が何かに当たったか判定
            if (Physics.Raycast(ray, out RaycastHit hitInfo, LASER_MAX_DISTANCE))
            {
                // 当たった場所をレーザーの終点にする
                laserLineRenderer.SetPosition(1, hitInfo.point);
            }
            else
            {
                // 何も当たらなかったら、最大距離の場所を終点にする
                laserLineRenderer.SetPosition(1, ray.GetPoint(LASER_MAX_DISTANCE));
            }
        }
    }
}
