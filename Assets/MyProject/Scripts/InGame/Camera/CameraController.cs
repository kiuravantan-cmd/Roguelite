using UnityEngine;

namespace TPSRoguelite.InGame.Camera 
{
    public class CameraController : MonoBehaviour 
    {
        [Header("カメラの基本設定")]
        [SerializeField] private float lookSensitivity = 0.2f;
        [SerializeField] private float minPitch = -10f;
        [SerializeField] private float maxPitch = 60f;

        /// <summary>
        /// 追従するターゲット
        /// </summary>
        [SerializeField] private Transform target;

        [Header("通常時の視点")]
        [SerializeField] private float normalDistance = 3.0f; // 後ろに下がる距離
        [SerializeField] private float normalHeightOffset = 1.2f; // 高さ
        [SerializeField] private float normalShoulderOffset = 0.8f; // 【重要】右にずらす距離

        [Header("エイム（ADS）時の視点")]
        [SerializeField] private float aimDistance = 1.0f; // キャラに近づく
        [SerializeField] private float aimHeightOffset = 1.2f;
        [SerializeField] private float aimShoulderOffset = 0.5f; // 少し中央に寄せる
        [SerializeField] private float zoomSpeed = 10f; // カメラが移動する滑らかさ

        /// <summary>
        /// 自動生成されたクラス
        /// </summary>
        private PlayerInputActions inputActions;

        /// <summary>
        /// マウスの移動量
        /// </summary>
        private Vector2 lookInput = Vector2.zero;

        /// <summary>
        /// 横の回転角度(Y軸回転)
        /// </summary>
        private float currentYaw = 0f;

        /// <summary>
        /// 縦の回転角度（X軸回転）
        /// </summary>
        private float currentPitch = 20f;

        // エイム中かどうかを判定するフラグ
        private bool isAiming = false;

        // 現在のカメラの位置情報（滑らかに変化させるための変数）
        private float currentDistance;
        private float currentHeightOffset;
        private float currentShoulderOffset;

        private void Awake() 
        {
            inputActions = new PlayerInputActions();

            // マウスカーソルを画面中央にロックして非表示にする
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // 最初は通常時の視点をセットしておく
            currentDistance = normalDistance;
            currentHeightOffset = normalHeightOffset;
            currentShoulderOffset = normalShoulderOffset;
        }

        private void OnEnable() 
        {
            inputActions.Enable();    
        }

        private void OnDisable() 
        {
            inputActions.Disable();
        }

        private void Update() 
        {
            // マウスの移動量を取得
            lookInput = inputActions.Player.Look.ReadValue<Vector2>();

            // 感度を掛けて現在の角度に足し引きする
            currentYaw += lookInput.x * lookSensitivity;
            currentPitch -= lookInput.y * lookSensitivity;

            currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

            // ※将来、ここにInputSystemのエイムボタン（右クリック等）の処理を書きます
            // 【テスト用】右クリックを押している間だけエイム状態にする
            if (UnityEngine.InputSystem.Mouse.current.rightButton.isPressed)
            {
                isAiming = true;
            }
            else
            {
                isAiming = false;
            }
        }

        private void LateUpdate()
        {
            // カメラの移動は、プレイヤーの移動が終わった後に行う
            
            // ターゲットが設定されてない場合はエラー回避
            if (target == null) 
            {
                return;
            }

            // 1. 目標となる数値を決定する（エイム中ならエイム用の数値、そうでないなら通常用の数値）
            float targetDistance = isAiming ? aimDistance : normalDistance;
            float targetHeight = isAiming ? aimHeightOffset : normalHeightOffset;
            float targetShoulder = isAiming ? aimShoulderOffset : normalShoulderOffset;

            // 2. 現在の数値を、目標の数値に向かって滑らかに変化させる（Mathf.Lerp）
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomSpeed);
            currentHeightOffset = Mathf.Lerp(currentHeightOffset, targetHeight, Time.deltaTime * zoomSpeed);
            currentShoulderOffset = Mathf.Lerp(currentShoulderOffset, targetShoulder, Time.deltaTime * zoomSpeed);

            // 3. カメラの回転を計算
            Quaternion rotate = Quaternion.Euler(currentPitch, currentYaw, 0f);

            // 4. 注視点の計算（プレイヤーの高さ）
            Vector3 basePosition = target.position + Vector3.up * currentHeightOffset;

            // 5. 肩越し視点にするため、カメラにとっての「右方向」へずらす
            Vector3 shoulderPosition = basePosition + (rotate * Vector3.right * currentShoulderOffset);

            // 6. そこから、カメラにとっての「後ろ方向」へ距離分だけ離す
            Vector3 cameraPosition = shoulderPosition - (rotate * Vector3.forward * currentDistance);

            // 7. 最終的な位置と回転を適用
            transform.position = cameraPosition;
            transform.rotation = rotate;
        }
    }
}
