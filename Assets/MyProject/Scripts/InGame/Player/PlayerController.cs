using Core.Interface;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using System;

namespace TPSRoguelite.InGame.Player {

    public class PlayerController : MonoBehaviour {
        /// <summary>
        /// 移動速度
        /// </summary>
        private const float MOVE_SPEED = 5.0f;

        /// <summary>
        /// 回転速度
        /// </summary>
        private const float ROTATE_SPEED = 10f;

        /// <summary>
        /// レーザーポインターの描画距離
        /// </summary>
        private const float LASER_MAX_DISTANCE = 50f;

        /// <summary>
        /// 相手に与えるダメージ量
        /// </summary>
        private const int ATTACK_DAMAGE = 20;

        /// <summary>
        /// 攻撃距離（射撃範囲）
        /// </summary>
        private const float ATTACK_RANGE = 50f;

        /// <summary>
        /// 最大弾数
        /// </summary>
        private const int MAX_AMMO = 30;

        /// <summary>
        /// リロード時間
        /// </summary>
        private const float RELOAD_TIME = 1.5f;

        /// <summary>
        /// 物理演算コンポーネント
        /// </summary>
        [SerializeField] private Rigidbody rigidbody;

        /// <summary>
        /// 銃口のトランスフォーム
        /// </summary>
        [SerializeField] private Transform weponOrigin;

        /// <summary>
        /// レーザーポインターの描画コンポーネント
        /// </summary>
        [SerializeField] private LineRenderer laserLineRenderer;

        /// <summary>
        /// 自動生成されたInputクラス
        /// </summary>
        private PlayerInputActions inputActions;

        /// <summary>
        /// 入力方向
        /// </summary>
        private Vector2 moveInput = Vector2.zero;

        /// <summary>
        /// 移動方向のベクトル
        /// </summary>
        private Vector3 moveDirection;

        /// <summary>
        /// カメラのトランスフォーム
        /// </summary>
        private Transform mainCameraTransform;

        /// <summary>
        /// リロードしているか
        /// </summary>
        private bool isReloading;

        /// <summary>
        /// 外部（アニメーションやUIなど）に現在の速度を教えるために保持するVelocity
        /// </summary>
        public Vector3 CurrentVelocity { get; private set; }

        /// <summary>
        /// 現在の弾数
        /// </summary>
        public int CurrentAmmo { get; private set; }

        private void Awake() {
            CurrentAmmo = MAX_AMMO;

            inputActions = new PlayerInputActions();
            inputActions.Player.Fire.performed += OnFire;
            inputActions.Player.Reload.performed += OnReload;

            if (UnityEngine.Camera.main != null)
            {
                mainCameraTransform = UnityEngine.Camera.main.transform;
            }
            else
            {
                Debug.LogError("Main Cameraが見つかりません。");
            }
        }

        private void OnEnable() {
            inputActions.Enable();
        }

        private void OnDisable() {
            inputActions.Disable();
        }

        private void Update() {
            moveInput = inputActions.Player.Move.ReadValue<Vector2>();
            DrawLaserPointer();
        }

        private void FixedUpdate() {
            // 物理演算に関わる移動処理になるため、FixedUpdateで行う
            Move();
        }


        private void Move() {
            if (rigidbody == null) {
                return;
            }

            // 入力がない場合はピタッと止める
            if (moveInput == Vector2.zero) {
                rigidbody.linearVelocity = new Vector3(0f, rigidbody.linearVelocity.y, 0f);
                CurrentVelocity = Vector3.zero;
                return;
            }

            // カメラ基準の計算に変更
            Vector3 cameraForward = mainCameraTransform.forward;
            Vector3 cameraRight = mainCameraTransform.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 moveDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;

            // キャラクターを進行方向へ滑らかに振り向かせる
            Quaternion targeRotation = Quaternion.LookRotation(moveDirection);
            rigidbody.rotation = Quaternion.Slerp(rigidbody.rotation, targeRotation, ROTATE_SPEED * Time.fixedDeltaTime);

            Vector3 targetVelocity = moveDirection * MOVE_SPEED;
            rigidbody.linearVelocity = new Vector3(targetVelocity.x, rigidbody.linearVelocity.y, targetVelocity.z);

            // 外部（アニメーションやUIなど）に現在の速度を教えるためにプロパティを更新
            CurrentVelocity = rigidbody.linearVelocity;
        }

        private void OnFire(InputAction.CallbackContext context)
        {
            if (isReloading)
            {
                return;
            }

            if (CurrentAmmo <= 0)
            {
                ReloadAsync().Forget();
                return;
            }

            CurrentAmmo--;
            Debug.Log($"発砲！残り弾数：{CurrentAmmo}");

            Ray ray = new Ray(mainCameraTransform.position, mainCameraTransform.forward);

            // 光線に何かが当たったか判定
            if (Physics.Raycast(ray, out RaycastHit hitInfo, ATTACK_RANGE)) {
                Debug.Log($"{hitInfo.collider.name}に命中！");

                // 当たった相手が IDamageable を持っているか確認
                IDamageable target = hitInfo.collider.GetComponent<IDamageable>();

                // ダメージを受ける性質を持ったオブジェクトであればダメージを与える
                if (target != null) {
                    target.TakeDamage(ATTACK_DAMAGE);
                }
            }
        }

        private void OnReload(InputAction.CallbackContext context)
        {
            if (isReloading || CurrentAmmo == MAX_AMMO) {
                return;
            }

            ReloadAsync().Forget();
        }

        private async UniTaskVoid ReloadAsync()
        {
            isReloading = true;
            Debug.Log("リロード中");

            await UniTask.Delay(TimeSpan.FromSeconds(RELOAD_TIME), cancellationToken: this.GetCancellationTokenOnDestroy());

            CurrentAmmo = MAX_AMMO;
            isReloading = false;
            Debug.Log("リロード完了");
        }

        /// <summary>
        /// レーザーポインターの描画
        /// </summary>
        private void DrawLaserPointer()
        {
            if (laserLineRenderer == null || weponOrigin == null || mainCameraTransform == null) 
            {
                return;
            }

            laserLineRenderer.SetPosition(0, weponOrigin.position);

            Ray ray = new Ray(mainCameraTransform.position, mainCameraTransform.forward);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, LASER_MAX_DISTANCE))
            {
                laserLineRenderer.SetPosition(1, hitInfo.point);
            }
            else
            {
                laserLineRenderer.SetPosition(1, ray.GetPoint(LASER_MAX_DISTANCE));
            }
        }
    }
}
