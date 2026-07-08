using Core.Interface;
using Core.MasterData;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using TMPro;
using TPSRoguelite.InGame.Enums;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TPSRoguelite.InGame.Player
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
        private const float ROTATE_SPEED = 10f;

        /// <summary>
        /// レーザーポインターの描画距離
        /// </summary>
        private const float LASER_MAX_DISTANCE = 50f;

        /// <summary>
        /// 攻撃距離（射撃範囲）
        /// </summary>
        private const float ATTACK_RANGE = 50f;

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
        /// マズルフラッシュ（銃口の火花）のエフェクト
        /// </summary>
        [SerializeField] private ParticleSystem muzzleFlash;

        [Header("UI & Effects")]
        [SerializeField] private Slider expSlider;
        [SerializeField] private TextMeshProUGUI levelUpText;
        [SerializeField] private ParticleSystem levelUpEffect;

        [Header("Weapon UI")]
        [SerializeField] private TextMeshProUGUI fireModeText;
        [SerializeField] private TextMeshProUGUI ammoText;

        [Header("Reload UI")]
        [SerializeField] private GameObject reloadUI;
        [SerializeField] private Image reloadCircleImage;

        /// <summary>
        /// 武器のデータ
        /// </summary>
        private WeaponDataRecord currentWeapon;

        /// <summary>
        /// 射撃のキャンセルトークン
        /// </summary>
        private CancellationTokenSource fireCts;

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
        /// 射撃可能か
        /// </summary>
        private bool canShoot = true;

        /// <summary>
        /// 次のレベルに必要な経験値
        /// </summary>
        private int requiredExp;

        /// <summary>
        /// 外部（アニメーションやUIなど）に現在の速度を教えるために保持するVelocity
        /// </summary>
        public Vector3 CurrentVelocity { get; private set; }

        /// <summary>
        /// 現在の弾数
        /// </summary>
        public int CurrentAmmo { get; private set; }

        /// <summary>
        /// 現在の経験値
        /// </summary>
        public int CurrentExp { get; private set; }

        /// <summary>
        /// 現在のレベル
        /// </summary>
        public int CurrentLevel { get; private set; }

        private void Start ()
        {
            gameObject.SetActive(false);
        }

        public void Setup ()
        {
            currentWeapon = MasterDataAccessor.Instance.GetById<WeaponDataRecord>(1);
            if (currentWeapon != null)
            {
                CurrentAmmo = currentWeapon.MaxAmmo;
            }
            else
            {
                Debug.LogError("currentWeaponが見つかりませんでした");
            }

            CurrentLevel = 1;
            CurrentExp = 0;

            // 最初はオーブ5個でレベルアップ
            requiredExp = 5;

            // ゲージを空にする
            UpdateExpUI();

            if (levelUpText != null)
            {
                // 文字は最初は隠しておく
                levelUpText.enabled = false;
            }

            inputActions = new PlayerInputActions();
            inputActions.Player.Fire.started += OnFire;
            inputActions.Player.Fire.canceled += OnFire;
            inputActions.Player.Reload.performed += OnReload;

            if (UnityEngine.Camera.main != null)
            {
                mainCameraTransform = UnityEngine.Camera.main.transform;
            }
            else
            {
                Debug.LogError("Main Cameraが見つかりません。");
            }

            UpdateWeaponUI();

            if (reloadUI != null)
            {
                reloadUI.SetActive(false);
            }

            gameObject.SetActive(true);
        }

        private void OnEnable ()
        {
            if (inputActions != null)
            {
                inputActions.Enable();
            }
        }

        private void OnDisable ()
        {
            if (inputActions != null)
            {
                inputActions.Disable();
            }
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
            if (rigidbody == null || mainCameraTransform == null)
            {
                return;
            }

            Vector3 cameraForward = mainCameraTransform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            if (cameraForward != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
                rigidbody.rotation = Quaternion.Slerp(rigidbody.rotation, targetRotation, ROTATE_SPEED * Time.fixedDeltaTime);
            }

            // 入力がない場合はピタッと止める
            if (moveInput == Vector2.zero)
            {
                rigidbody.linearVelocity = new Vector3(0f, rigidbody.linearVelocity.y, 0f);
                CurrentVelocity = Vector3.zero;
                return;
            }

            // カメラ基準の計算に変更
            Vector3 cameraRight = mainCameraTransform.right;
            cameraRight.y = 0f;
            cameraRight.Normalize();

            Vector3 moveDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;

            Vector3 targetVelocity = moveDirection * MOVE_SPEED;
            rigidbody.linearVelocity = new Vector3(targetVelocity.x, rigidbody.linearVelocity.y, targetVelocity.z);

            // 外部（アニメーションやUIなど）に現在の速度を教えるためにプロパティを更新
            CurrentVelocity = rigidbody.linearVelocity;
        }

        private void OnFire (InputAction.CallbackContext context)
        {
            if (context.started)
            {
                // クールダウン中やリロード中（撃てない状態）なら、連打されても完全に無視する！
                if (!canShoot || isReloading || currentWeapon == null)
                {
                    return;
                }

                // 押された瞬間に、新しいキャンセルスイッチを作成
                fireCts = new CancellationTokenSource();

                if ((FireType)currentWeapon.FireType == FireType.SemiAuto)
                {
                    // セミオートは指を離しても中断しないので、消滅トークンだけ渡す
                    ShootSemiAutoAsync(this.GetCancellationTokenOnDestroy()).Forget();
                }
                else if ((FireType)currentWeapon.FireType == FireType.Burst)
                {
                    // バーストも途中で止まらないように消滅トークンだけ渡す
                    ShootBurstAsync(this.GetCancellationTokenOnDestroy()).Forget();
                }
                else if ((FireType)currentWeapon.FireType == FireType.FullAuto)
                {
                    // フルオートは指を離した時に止めるため、合体させたトークンを渡す
                    ShootFullAutoAsync(fireCts.Token).Forget();
                }
            }

            if (context.canceled)
            {
                if (fireCts != null)
                {
                    fireCts.Cancel();
                    fireCts.Dispose();
                    fireCts = null;
                }
            }
        }

        private async UniTaskVoid ShootSemiAutoAsync (CancellationToken token)
        {
            canShoot = false;

            if (CurrentAmmo <= 0)
            {
                Reload();
                return;
            }

            CurrentAmmo--;
            UpdateCurrentAmmoUI();
            Debug.Log($"バン！ 残弾: {CurrentAmmo}");
            Shoot();

            await UniTask.Delay(TimeSpan.FromSeconds(currentWeapon.FireInterval), cancellationToken: token);

            canShoot = true;
        }

        private async UniTaskVoid ShootFullAutoAsync (CancellationToken token)
        {
            canShoot = false;

            while (!token.IsCancellationRequested)
            {
                if (CurrentAmmo <= 0)
                {
                    Reload();
                    break;
                }

                CurrentAmmo--;
                UpdateCurrentAmmoUI();
                Debug.Log($"フルオート発射！ 残弾: {CurrentAmmo}");
                Shoot();

                bool isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(currentWeapon.FireRate), cancellationToken: token).SuppressCancellationThrow();

                if (isCanceled)
                {
                    break;
                }
            }

            await UniTask.Delay(TimeSpan.FromSeconds(currentWeapon.FireInterval), cancellationToken: this.GetCancellationTokenOnDestroy());

            canShoot = true;
        }

        private async UniTaskVoid ShootBurstAsync (CancellationToken token)
        {
            canShoot = false;
            for (int i = 0; i < 3; i++)
            {
                if (CurrentAmmo <= 0)
                {
                    Reload();
                    return;
                }

                CurrentAmmo--;
                UpdateCurrentAmmoUI();
                Shoot();
                Debug.Log($"バースト！ 残弾: {CurrentAmmo}");

                await UniTask.Delay(TimeSpan.FromSeconds(currentWeapon.FireRate), cancellationToken: token);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(currentWeapon.FireInterval), cancellationToken: token);
            canShoot = true;
        }

        private void Shoot ()
        {
            if (muzzleFlash != null)
            {
                muzzleFlash.Play();
            }

            Ray ray = new Ray(mainCameraTransform.position, mainCameraTransform.forward);

            // 光線に何かが当たったか判定
            if (Physics.Raycast(ray, out RaycastHit hitInfo, ATTACK_RANGE))
            {
                Debug.Log($"{hitInfo.collider.name}に命中！");

                // 当たった相手が IDamageable を持っているか確認
                IDamageable target = hitInfo.collider.GetComponent<IDamageable>();

                // ダメージを受ける性質を持ったオブジェクトであればダメージを与える
                if (target != null)
                {
                    target.TakeDamage(currentWeapon.AttackPower);
                }
            }
        }

        private void OnReload (InputAction.CallbackContext context)
        {
            if (isReloading || CurrentAmmo == currentWeapon.MaxAmmo)
            {
                return;
            }

            Reload();
        }

        private void Reload()
        {
            isReloading = true;

            if (reloadUI != null)
            {
                reloadUI.SetActive(true);
            }

            if (reloadCircleImage != null)
            {
                reloadCircleImage.fillAmount = 0f;
            }

            DOVirtual.Float(0f, 1f, currentWeapon.ReloadTime, UpdateReloadUI).SetEase(Ease.Linear).OnComplete(FinishReload);
        }

        private void UpdateReloadUI(float value)
        {
            if (reloadCircleImage != null)
            {
                reloadCircleImage.fillAmount = value;
            }
        }

        private void FinishReload()
        {
            if (reloadUI != null)
            {
                reloadUI.SetActive(false);
            }

            CurrentAmmo = currentWeapon.MaxAmmo;
            UpdateCurrentAmmoUI();
            isReloading = false;
        }

        /// <summary>
        /// レーザーポインターの描画
        /// </summary>
        private void DrawLaserPointer ()
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

        /// <summary>
        /// 経験値を追加する
        /// </summary>
        public void AddExperience (int amount)
        {
            CurrentExp += amount;
            Debug.Log($"経験値を{amount}獲得！ 現在の経験値: {CurrentExp}");

            // 経験値が満タンになったか判定（レベルアップ）
            if (CurrentExp >= requiredExp)
            {
                LevelUp();
            }

            // UIのゲージを更新
            UpdateExpUI();
        }

        /// <summary>
        /// レベルアップの処理
        /// </summary>
        private void LevelUp ()
        {
            CurrentLevel++;

            // 余った経験値を消さずに、次のレベルに持ち越す（重要！）
            CurrentExp -= requiredExp;

            // 次のレベルに必要な経験値を再計算（例：今のレベル × 5）
            requiredExp = CurrentLevel * 5;

            Debug.Log($"レベルアップ！ レベル {CurrentLevel} になった！");

            // レベルアップエフェクトの再生
            if (levelUpEffect != null)
            {
                levelUpEffect.Play();
            }

            // 画面にド派手な文字を出す
            ShowLevelUpTextAsync().Forget();
        }

        /// <summary>
        /// UIゲージの長さを更新する
        /// </summary>
        private void UpdateExpUI ()
        {
            if (expSlider != null)
            {
                // 0.0（空） ～ 1.0（満タン） の割合を計算してSliderにセットする
                expSlider.value = (float)CurrentExp / requiredExp;
            }
        }

        /// <summary>
        /// レベルアップの文字を数秒間だけ表示して自動で消す
        /// </summary>
        private async UniTaskVoid ShowLevelUpTextAsync ()
        {
            if (levelUpText == null)
            {
                return;
            }

            // テキストの中身を書き換えて表示
            levelUpText.text = $"LEVEL UP!\n<size=50%>Lv.{CurrentLevel}</size>";
            levelUpText.enabled = true;

            // 2秒間待つ
            await UniTask.Delay(TimeSpan.FromSeconds(2.0f), cancellationToken: this.GetCancellationTokenOnDestroy());

            // 2秒後に自動で非表示にする
            levelUpText.enabled = false;
        }

        private void UpdateWeaponUI()
        {
            if (fireModeText == null || ammoText == null)
            {
                return;
            }

            FireType fireType = (FireType)currentWeapon.FireType;
            switch (fireType)
            {
                case FireType.SemiAuto:
                    fireModeText.text = "Semi-Auto";
                    fireModeText.color = Color.white;
                    break;
                case FireType.Burst:
                    fireModeText.text = "Burst";
                    fireModeText.color = Color.yellow;
                    break;
                case FireType.FullAuto:
                    fireModeText.text = "Full-Auto";
                    fireModeText.color = Color.red;
                    break;
                default:
                    fireModeText.text = "Unknown";
                    break;
            }

            UpdateCurrentAmmoUI();
        }

        private void UpdateCurrentAmmoUI()
        {
            if (ammoText != null)
            {
                ammoText.SetText($"{CurrentAmmo}/{currentWeapon.MaxAmmo}");
            }
        }
    }
}
