using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MyProject.Scripts.InGame.Camera
{
    public class CameraController : MonoBehaviour
    {
        /// <summary>
        /// マウス感度
        /// </summary>
        private const float LOOK_SENSITIVITY = 0.2f;

        /// <summary>
        /// プレイヤーからの距離
        /// </summary>
        private const float DISTANCE = 5.0f;

        /// <summary>
        /// プレイヤーの少し上を狙う高さ
        /// </summary>
        private const float HEIGHT_OFFSET = 1.5f;

        /// <summary>
        /// 縦の最小角度
        /// </summary>
        private const float MIN_PITCH = -10f;

        /// <summary>
        /// 縦の最大角度
        /// </summary>
        private const float MAX_PITCH = 60f;

        /// <summary>
        /// 追従するターゲット
        /// </summary>
        [SerializeField] private Transform cameraTarget = null;

        /// <summary>
        /// 自動生成されたクラス
        /// </summary>
        private PlayerInputActions inputActions;

        /// <summary>
        /// 移動量
        /// </summary>
        private Vector2 lookInput = Vector2.zero;

        /// <summary>
        /// 横の回転角度（Y軸回転）
        /// </summary>
        private float currentYaw = 0f;

        /// <summary>
        /// 縦の回転角度（X軸回転）
        /// 初期値を20にしておく
        /// </summary>
        private float currentPitch = 20f;

        private void Awake ()
        {
            inputActions = new PlayerInputActions();

            // マウスカーソルを画面中央にロックして非表示にする
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
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
            // マウスの移動量を取得
            lookInput = inputActions.Player.Look.ReadValue<Vector2>();

            // 感度を掛けて現在の角度に足し引きする
            currentYaw += lookInput.x * LOOK_SENSITIVITY;
            currentPitch -= lookInput.y * LOOK_SENSITIVITY;

            // 縦の角度を制限（真上や真下を向きすぎてカメラが反転するのを防ぐ）
            currentPitch = Mathf.Clamp(currentPitch, MIN_PITCH, MAX_PITCH);
        }

        private void LateUpdate ()
        {
            if (cameraTarget == null)
            {
                return;
            }

            // 注視点を計算（足元ではなく頭のあたり）
            Vector3 targetPosition = cameraTarget.position + Vector3.up * HEIGHT_OFFSET;

            // 角度（ピッチとヨー）をQuaternionに変換
            Quaternion targetRotation = Quaternion.Euler(currentPitch, currentYaw, 0f);

            // 注視点から、計算した角度の後ろ方向へ距離分だけ離した位置を計算
            Vector3 cameraPosition = targetPosition - (targetRotation * Vector3.forward * DISTANCE);

            // カメラの位置と回転を確定
            transform.position = cameraPosition;
            transform.rotation = targetRotation;
        }
    }
}
