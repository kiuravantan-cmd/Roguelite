# 第10週：吸い込む快感とレベルアップ！「成長」の手触りを極める

## 本日の目標
今日は、ゲームの醍醐味である「成長」のシステムを作り、さらに「魔法のツール」を使って画面の演出を劇的に強化します！
1. 吸い込む快感：近づくと「シュバッ！」と飛んでくる経験値オーブを作る。
2. 成長のロジック：経験値ゲージとレベルアップのシステムを構築する。
3. UIの魔術（グラデーション）：プログラムの力だけでUIを綺麗なグラデーションに染め上げる。

## 1. 経験値オーブを作ろう（マグネット吸引）

ただ触れるだけではなく、近づくと自動でプレイヤーに吸い寄せられる気持ちいいオーブを作ります。
Scripts/InGame/Item フォルダを作成し、ExperienceOrb.cs を作ります。
ファイル名： ExperienceOrb.cs
``` cs
using TPSRoguelite.InGame.Player;
using UnityEngine;

namespace TPSRoguelite.InGame.Item
{
    public class ExperienceOrb : MonoBehaviour
    {
        /// <summary>
        /// プレイヤーに引き寄せられる範囲
        /// </summary>
        private const float MAGNET_RANGE = 5f;

        /// <summary>
        /// プレイヤーに引き寄せられる速度
        /// </summary>
        private const float MOVE_SPEED = 15f;

        /// <summary>
        /// プレイヤーのタグ
        /// </summary>
        private const string PLAYER_TAG = "Player";

        /// <summary>
        /// プレイヤーのTransform
        /// </summary>
        private Transform targetPlayer = null;

        /// <summary>
        /// プレイヤーに引き寄せられているかどうか
        /// </summary>
        private bool isFollowing = false;

        private void Start ()
        {
            // 出現時にプレイヤーのTransformを取得する
            GameObject playerObj = GameObject.FindGameObjectWithTag(PLAYER_TAG);
            if (playerObj != null)
            {
                targetPlayer = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("プレイヤーが見つかりませんでした。");
            }
        }

        private void Update ()
        {
            if (targetPlayer == null)
            {
                return;
            }

            if (isFollowing)
            {
                // プレイヤーに向かって移動する
                transform.position = Vector3.MoveTowards(transform.position, targetPlayer.position, MOVE_SPEED * Time.deltaTime);
            }
            else
            {
                // プレイヤーとの距離を計算し、引き寄せ範囲内であれば引き寄せを開始する
                float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);
                if (distanceToPlayer <= MAGNET_RANGE)
                {
                    // プレイヤーに引き寄せられるようになる
                    isFollowing = true;
                }
            }
        }

        /// <summary>
        /// プレイヤーに触れたときの処理（コライダーの Is Trigger がオンになっている必要があります）
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(PLAYER_TAG))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null)
                {
                    // プレイヤーに触れたら経験値を付与する処理をここに追加
                }
                else
                {
                    Debug.LogWarning("PlayerController コンポーネントが見つかりませんでした。");
                }

                // 経験値オーブを破棄する
                Destroy(gameObject);
            }
        }
    }
}
```

エディタでの作業手順
1. Hierarchyウィンドウの何もない場所で右クリックし、3D Object > Sphere（球体）を作成します。
2. 名前を ExperienceOrb に変更し、Inspectorの上部にある Transform の Scale をすべて 0.3 くらいに小さくします。（色が欲しい場合は黄色いマテリアルなどを作って適用してください）
3. 今作った ExperienceOrb に、先ほど書いたスクリプト ExperienceOrb.cs をドラッグ＆ドロップでアタッチします。
4. Inspectorにある Sphere Collider コンポーネントを探し、Is Trigger の左にあるチェックボックスに必ずチェックを入れてください。（入れないとプレイヤーにぶつかって弾き飛ばされてしまいます！）
5. Projectウィンドウに Prefabs（または Items）フォルダを用意し、そこへ ExperienceOrb をドラッグ＆ドロップして Prefab（プレハブ）化 します。青い箱のアイコンになったら、Hierarchy上にある元のオーブは Delete キーで削除してOKです。

## 2. 敵からのドロップ処理
敵が死んだときに、さっき作ったオーブを落とすようにします。
ファイル名： EnemyState.cs
``` diff
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TPSRoguelite.InGame.Enemy 
{
    public class EnemyState : MonoBehaviour, IDamageable
    {
        /// <summary>
        /// 点滅する時間
        /// </summary>
        private const float FLASH_DURATION = 0.1f;
+
+       /// <summary>
+       /// オーブのドロップ位置の高さのオフセット
+       /// </summary>
+       private const float ORB_DROP_HEIGHT_OFFSET = 0.5f;

        /// <summary>
        /// キャラクターのレンダラー
        /// </summary>
        [SerializeField] private Renderer[] modelRenderers;
+
+       [Header("ドロップアイテム")]
+
+       /// <summary>
+       /// ドロップするオーブ
+       /// </summary>
+       [SerializeField] private GameObject experienceOrbPrefab;

        /// <summary>
        /// キャラクターの元々の色
        /// </summary>
        private Color[] defaultColors;

        /// <summary>
        /// 点滅するアニメーションのキャンセルトークン
        /// </summary>
        private CancellationTokenSource flashCts;

        /// <summary>
        /// 敵のデータ
        /// </summary>
        public EnemyDataRecord EnemyDataAsset { get; private set; }

        /// <summary>
        /// 現在の体力
        /// </summary>
        public int CurrentHP { get; private set; }

        public event UnityAction<EnemyState> OnReturnToPoolAction;

        /// <summary>
        /// ダメージを受けたときに受け取るイベント
        /// </summary>
        public event UnityAction OnDamageAction;

        public void Initialize(ulong id)
        {
            EnemyDataAsset = MasterDataAccessor.Instance.GetById<EnemyDataRecord>(id);

            if (modelRenderers != null)
            {
                defaultColors = new Color[modelRenderers.Length];
                for (int i = 0; i < modelRenderers.Length; i++)
                {
                    defaultColors[i] = modelRenderers[i].material.color;
                }
            }
        }

        public void Setup()
        {
            if (EnemyDataAsset == null) 
            {
                Debug.LogError("EnemyDataがセットされていません。");
                return;
            }

            CurrentHP = EnemyDataAsset.MaxHP;
            gameObject.SetActive(true);
            ResetColor();
        }

        public void TakeDamage(int damageAmount) 
        {
            // マイナスのダメージ（回復）を防ぐ
            if (damageAmount <= 0)
            {
                return;
            }

            CurrentHP -= damageAmount;
            Debug.Log($"{EnemyDataAsset.EnemyName}に{damageAmount}のダメージ！残りHP:{CurrentHP}");

            if (CurrentHP > 0)
            {
                OnDamageAction?.Invoke();

                flashCts?.Cancel();
                flashCts?.Dispose();
                flashCts = new CancellationTokenSource();
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(flashCts.Token, this.GetCancellationTokenOnDestroy());

                DamageFlashAsync(linkedCts.Token).Forget();
            }
            else
            {
                Die();
            }
        }

        private void Die() 
        {
            Debug.Log($"{EnemyDataAsset.EnemyName}を倒しました");
+
+           // 経験値オーブをドロップする
+           if (experienceOrbPrefab != null)
+           {
+               Vector3 spawnPosition = transform.position + Vector3.up * ORB_DROP_HEIGHT_OFFSET;
+               Instantiate(experienceOrbPrefab, spawnPosition, Quaternion.identity);
+           }

            gameObject.SetActive(false);
            OnReturnToPoolAction?.Invoke(this);
        }

        /// <summary>
        /// ダメージを受けたときの点滅処理
        /// </summary>
        private async UniTaskVoid DamageFlashAsync(CancellationToken token)
        {
            if (modelRenderers == null || defaultColors == null)
            {
                return;
            }

            foreach (var renderer in modelRenderers)
            {
                if (renderer != null)
                {
                    renderer.material.color = Color.red;
                }

                bool isCancelled = await UniTask.Delay(TimeSpan.FromSeconds(FLASH_DURATION), cancellationToken: token).SuppressCancellationThrow();

                if (!isCancelled)
                {
                    ResetColor();
                }
            }
        }
    }
}
```
【エディタでの作業】 Enemyプレハブの Experience Orb Prefab 枠に、先ほど作ったオーブのプレハブをセットします。

## 3. プレイヤーの経験値とレベルアップ処理
PlayerController.cs を開き、経験値の受け皿と、レベルアップの仕組みを追加します。

### 3-1. UIの配置
まずは画面に経験値ゲージと文字を作ります。
1. Hierarchyウィンドウにある Canvas の上で右クリックし、UI > Slider を作成します。名前を ExpSlider に変更します。
2. ExpSlider を選択し、Inspectorにある Slider コンポーネントの Interactable のチェックを外します。（外さないと、ゲーム中にマウスでゲージを動かせてしまいます）
3. さらに Canvas の上で右クリックし、UI > Text - TextMeshPro を作成します。名前を LevelUpText に変更します。
4. 画面の真ん中に大きく配置し、Inspectorでテキストの色を黄色などに、文字を「LEVEL UP!」に変更します。

### 3-2. 経験値の取得
ファイル名： PlayerController.cs
``` diff
using Core.Interface;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TPSRoguelite.InGame.Enum;
using Core.MasterData;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

namespace TPSRoguelite.InGame.Player {

    public class PlayerController : MonoBehaviour
    {
        // 変数省略
        /// <summary>
        /// 現在の弾数
        /// </summary>
        public int CurrentAmmo { get; private set; }
+
+       /// <summary>
+       /// 現在の経験値
+       /// </summary>
+       public int CurrentExp { get; private set; }

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        public void Setup()
        {
            currentWeapon = MasterDataAccessor.Instance.GetById<WeaponDataRecord>(weaponId);

            if (currentWeapon != null)
            {
              CurrentAmmo = currentWeapon.MaxAmmo;
            }
            else
            {
                Debug.LogError("WeaponDataがありません。");
            }

+           CurrentExp = 0;

            inputActions = new PlayerInputActions();
            inputActions.Player.Fire.performed += OnFire; // 押し続けていると呼ばれる
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

            UpdateFireModeUI();

            if (reloadUI != null)
            {
              reloadUI.SetActive(false);
            }

            gameObject.SetActive(true);
        }

        //関数省略

        /// <summary>
        /// リロード完了時の処理
        /// </summary>
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
+
+       /// <summary>
+       /// 経験値を追加する
+       /// </summary>
+       public void AddExperience(int amount)
+       {
+           CurrentExp += amount;
+           Debug.Log($"経験値を{amount}獲得！現在の経験値: {CurrentExp}");
+       }
    }
}
```

ExperienceOrb.csに経験値を追加する処理を実装します。
ファイル名： ExperienceOrb.cs
``` diff
using TPSRoguelite.InGame.Player;
using UnityEngine;

namespace TPSRoguelite.InGame.Item
{
    public class ExperienceOrb : MonoBehaviour
    {
        /// <summary>
        /// プレイヤーに引き寄せられる範囲
        /// </summary>
        private const float MAGNET_RANGE = 5f;

        /// <summary>
        /// プレイヤーに引き寄せられる速度
        /// </summary>
        private const float MOVE_SPEED = 15f;

        /// <summary>
        /// プレイヤーのタグ
        /// </summary>
        private const string PLAYER_TAG = "Player";

        /// <summary>
        /// プレイヤーのTransform
        /// </summary>
        private Transform targetPlayer = null;

        /// <summary>
        /// プレイヤーに引き寄せられているかどうか
        /// </summary>
        private bool isFollowing = false;

        private void Start ()
        {
            // 出現時にプレイヤーのTransformを取得する
            GameObject playerObj = GameObject.FindGameObjectWithTag(PLAYER_TAG);
            if (playerObj != null)
            {
                targetPlayer = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("プレイヤーが見つかりませんでした。");
            }
        }

        private void Update ()
        {
            if (targetPlayer == null)
            {
                return;
            }

            if (isFollowing)
            {
                // プレイヤーに向かって移動する
                transform.position = Vector3.MoveTowards(transform.position, targetPlayer.position, MOVE_SPEED * Time.deltaTime);
            }
            else
            {
                // プレイヤーとの距離を計算し、引き寄せ範囲内であれば引き寄せを開始する
                float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);
                if (distanceToPlayer <= MAGNET_RANGE)
                {
                    // プレイヤーに引き寄せられるようになる
                    isFollowing = true;
                }
            }
        }

        /// <summary>
        /// プレイヤーに触れたときの処理（コライダーの Is Trigger がオンになっている必要があります）
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(PLAYER_TAG))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null)
                {
-                   // プレイヤーに触れたら経験値を付与する処理をここに追加
+                   player.AddExperience(1);
                }
                else
                {
                    Debug.LogWarning("PlayerController コンポーネントが見つかりませんでした。");
                }

                // 経験値オーブを破棄する
                Destroy(gameObject);
            }
        }
    }
}

```

### 3-3. レベルアップ処理
ファイル名： PlayerController.cs
``` diff
using Core.Interface;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TPSRoguelite.InGame.Enum;
using Core.MasterData;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

namespace TPSRoguelite.InGame.Player {

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
        /// レベルアップ時のエフェクト表示時間
        /// </summary>
        private const float LEVEL_UP_EFFECT_DURATION = 2f;

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
        /// 武器のID（デフォルトは1）
        /// </summary>
        [SerializeField] private ulong weaponId = 1;

        /// <summary>
        /// マズルフラッシュ（銃口の火花）のエフェクト
        /// </summary>
        [SerializeField] private ParticleSystem muzzleFlash;

        [Header("Weapon UI")]

        [SerializeField] private TextMeshProUGUI fireModeText;

        [SerializeField] private TextMeshProUGUI ammoText;

        [Header("Relaod UI")]

        [SerializeField] private GameObject reloadUI;
        [SerializeField] private Image reloadCircleImage;

        [Header("経験値＆レベルアップのUI")]

        /// <summary>
        /// 経験値を表示するスライダーUI
        /// </summary>
        [SerializeField] private Slider expSlider;

        /// <summary>
        /// レベルアップ時に表示するテキストUI
        /// </summary>
        [SerializeField] private TextMeshProUGUI levelUpText;

        /// <summary>
        /// レベルアップ時のエフェクト
        /// </summary>
        [SerializeField] private ParticleSystem levelUpEffect;

        /// <summary>
        /// 武器のデータ
        /// </summary>
        private WeaponDataRecord currentWeapon;

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
        /// 射撃のキャンセルトークン
        /// </summary>
        private CancellationTokenSource fireCts;

        /// <summary>
        /// 次のレベルに必要な経験値
        /// </summary>
        private int RequiredExp => CurrentLevel * 5; // 例: レベル1なら5、レベル2なら10、レベル3なら15...

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
        public int CurrentLevel { get; private set; } = 1;

        private void Awake() {
            gameObject.SetActive(false);
        }

        public void Setup()
        {
            currentWeapon = MasterDataAccessor.Instance.GetById<WeaponDataRecord>(weaponId);

            if (currentWeapon != null) {
                CurrentAmmo = currentWeapon.MaxAmmo;
            } else {
                Debug.LogError("WeaponDataがありません。");
            }

            CurrentExp = 0;
            CurrentLevel = 1;

            if (levelUpText != null)
            {
                // レベルアップ時のテキストを非表示にする
                levelUpText.enabled = false;
            }

            UpdateExpUI();


            inputActions = new PlayerInputActions();
            inputActions.Player.Fire.performed += OnFire; // 押し続けていると呼ばれる
            inputActions.Player.Fire.canceled += OnFire;
            inputActions.Player.Reload.performed += OnReload;

            if (UnityEngine.Camera.main != null) {
                mainCameraTransform = UnityEngine.Camera.main.transform;
            } else {
                Debug.LogError("Main Cameraが見つかりません。");
            }

            UpdateFireModeUI();

            if (reloadUI != null)
            {
                reloadUI.SetActive(false);
            }

            gameObject.SetActive(true);
        }

        private void OnEnable() {
            inputActions?.Enable();
        }

        private void OnDisable() {
            inputActions?.Disable();
        }

        private void Update() {
            moveInput = inputActions.Player.Move.ReadValue<Vector2>();
            DrawLaserPointer();
        }

        private void FixedUpdate() {
            // 物理演算に関わる移動処理になるため、FixedUpdateで行う
            Move();
        }


        private void Move()
        {
            if (rigidbody == null || mainCameraTransform == null)
            {
                return;
            }

            // カメラの水平方向の前方を計算 (入力の有無に関わらず常に計算する)
            Vector3 cameraForward = mainCameraTransform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            // キャラクターを常に「カメラの向いている方向」へ振り向かせる
            if (cameraForward != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
                rigidbody.rotation = Quaternion.Slerp(rigidbody.rotation, targetRotation, ROTATE_SPEED * Time.fixedDeltaTime);
            }

            // 入力がない場合はピタッと止める
            if (moveInput == Vector2.zero) {
                rigidbody.linearVelocity = new Vector3(0f, rigidbody.linearVelocity.y, 0f);
                CurrentVelocity = Vector3.zero;
                return;
            }

            // カメラ基準の計算に変更
            Vector3 cameraRight = mainCameraTransform.right;
            cameraRight.y = 0f;
            cameraRight.Normalize();

            Vector3 moveDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;

            // 物理演算で移動させる
            Vector3 targetVelocity = moveDirection * MOVE_SPEED;
            rigidbody.linearVelocity = new Vector3(targetVelocity.x, rigidbody.linearVelocity.y, targetVelocity.z);

            // 外部（アニメーションやUIなど）に現在の速度を教えるためにプロパティを更新
            CurrentVelocity = rigidbody.linearVelocity;
        }

        private void OnFire(InputAction.CallbackContext context)
        {
            if (context.performed) 
            {
                if (!canShoot || isReloading || currentWeapon == null) 
                {
                    return;
                }

                fireCts = new CancellationTokenSource();
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(fireCts.Token, this.GetCancellationTokenOnDestroy());
                
                switch ((FireType)currentWeapon.WeaponFireType)
                {
                    case Enum.FireType.SemiAuto:
                        ShootSemiAutoAsync(this.GetCancellationTokenOnDestroy()).Forget();
                        break;

                    case Enum.FireType.Burst:
                        ShootBurstAsync(this.GetCancellationTokenOnDestroy()).Forget();
                        break;

                    case Enum.FireType.FullAuto:
                        ShootFullAutoAsync(linkedCts.Token).Forget();
                        break;

                    default:
                        Debug.LogWarning($"割り当てていない射撃タイプがあります。{currentWeapon.WeaponFireType}");
                        break;
                }
            }

            if (context.canceled) 
            {
                fireCts?.Cancel();
                fireCts?.Dispose();
                fireCts = null;
            }
        }

        /// <summary>
        /// セミオートの射撃処理
        /// </summary>
        private async UniTaskVoid ShootSemiAutoAsync(CancellationToken token) 
        {
            if (CurrentAmmo == 0) 
            {
                Reload();
                return;
            }

            canShoot = false;

            CurrentAmmo--;
            UpdateCurrentAmmoUI();
            Debug.Log($"セミオートで撃った！弾数：{CurrentAmmo}");
            Shoot();

            await UniTask.Delay(System.TimeSpan.FromSeconds(currentWeapon.FireRate), cancellationToken: token);

            canShoot = true;
        }

        /// <summary>
        /// バーストの射撃処理
        /// </summary>
        private async UniTaskVoid ShootBurstAsync(CancellationToken token) 
        {
            canShoot = false;

            for (int i = 0; i < 3; i++) 
            {
                if (CurrentAmmo <= 0)
                {
                    Reload();
                    break;
                }

                CurrentAmmo--;
                UpdateCurrentAmmoUI();
                Shoot();
                Debug.Log($"バースト！残弾数：{CurrentAmmo}");

                await UniTask.Delay(TimeSpan.FromSeconds(currentWeapon.FireInteval), cancellationToken: token);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(currentWeapon.FireRate), cancellationToken: token);
            canShoot = true;
        }

        private async UniTaskVoid ShootFullAutoAsync(CancellationToken token)
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
                Debug.Log($"フルオート！残弾数：{CurrentAmmo}");
                Shoot();

                bool isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(currentWeapon.FireInteval), cancellationToken: token).SuppressCancellationThrow();
                if (isCanceled)
                {
                    break;
                }
            }

            await UniTask.Delay(TimeSpan.FromSeconds(currentWeapon.FireRate), cancellationToken: this.GetCancellationTokenOnDestroy());

            canShoot = true;
        }

        /// <summary>
        /// 共通の射撃処理
        /// </summary>
        private void Shoot() 
        {
            if (muzzleFlash != null)
            {
                muzzleFlash.Play();
            }

            Ray ray = new Ray(mainCameraTransform.position, mainCameraTransform.forward);

            // 光線に何かが当たったか判定
            if (Physics.Raycast(ray, out RaycastHit hitInfo, ATTACK_RANGE)) {
                Debug.Log($"{hitInfo.collider.name}に命中！");

                // 当たった相手が IDamageable を持っているか確認
                IDamageable target = hitInfo.collider.GetComponent<IDamageable>();

                // ダメージを受ける性質を持ったオブジェクトであればダメージを与える
                if (target != null) {
                    target.TakeDamage(currentWeapon.AttackPower);
                }
            }
        }

        private void OnReload(InputAction.CallbackContext context)
        {
            if (isReloading || CurrentAmmo == currentWeapon.MaxAmmo) {
                return;
            }

            Reload();
        }

        private void Reload()
        {
            isReloading = true;

            if (reloadUI != null)
            {
                reloadUI.gameObject.SetActive(true);
            }

            if (reloadCircleImage != null)
            {
                reloadCircleImage.fillAmount = 0f;
            }

            DOVirtual.Float(0f, 1f, currentWeapon.ReloadTime, UpdateReloadUI).SetEase(Ease.Linear).OnComplete(FinishReload);
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

        private void UpdateFireModeUI ()
        {
            if (fireModeText == null || currentWeapon == null)
            {
                return;
            }

            FireType fireType = (FireType)currentWeapon.WeaponFireType;
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
            if (ammoText != null && currentWeapon != null)
            {
                ammoText.text = $"{CurrentAmmo}/{currentWeapon.MaxAmmo}";
            }
        }

        /// <summary>
        /// リロードUIの更新
        /// </summary>
        private void UpdateReloadUI(float value)
        {
            if (reloadCircleImage != null)
            {
                reloadCircleImage.fillAmount = value;
            }
        }

        /// <summary>
        /// リロード完了時の処理
        /// </summary>
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
        /// 経験値を追加する
        /// </summary>
        public void AddExperience(int amount)
        {
            CurrentExp += amount;
            Debug.Log($"経験値を{amount}獲得！現在の経験値: {CurrentExp}");

            // レベルアップ判定
            if (CurrentExp >= RequiredExp)
            {
                LevelUp();
            }

            // UIゲージの長さを更新
            UpdateExpUI();
        }

        /// <summary>
        /// レベルアップ処理
        /// </summary>
        private void LevelUp()
        {
            CurrentLevel++;

            // 余った経験値を消さずに、次のレベルに持ち越す
            CurrentExp -= RequiredExp;

            Debug.Log($"レベルアップ！現在のレベル: {CurrentLevel}, 次のレベルまでの経験値: {RequiredExp - CurrentExp}");

            // レベルアップのエフェクトを再生
            if (levelUpEffect != null)
            {
                levelUpEffect.Play();
            }

            ShowLevelUpTextAsync().Forget();
        }

        /// <summary>
        /// UIゲージの長さを更新する
        /// </summary>
        private void UpdateExpUI()
        {
            if (expSlider != null)
            {
                // 0.0（空） ～ 1.0（満タン） の割合を計算してSliderにセットする
                expSlider.value = (float)CurrentExp / RequiredExp;
            }
        }

        /// <summary>
        /// レベルアップの文字を表示する非同期処理
        /// </summary>
        private async UniTaskVoid ShowLevelUpTextAsync()
        {
            if (levelUpText == null)
            {
                return;
            }

            levelUpText.enabled = true;
            levelUpText.SetText($"Level Up!\n<size=50%>Lv.{CurrentLevel}</size>");

            // 2秒間表示した後に非表示にする
            await UniTask.Delay(TimeSpan.FromSeconds(LEVEL_UP_EFFECT_DURATION), cancellationToken: this.GetCancellationTokenOnDestroy());

            levelUpText.enabled = false;
        }
    }
}
```
【エディタでの作業手順】
1. Hierarchyウィンドウの Player をクリックして選択します。
2. Inspectorウィンドウを下へスクロールし、PlayerController コンポーネントに追加された Exp Slider と Level Up Text の枠を見つけます。
3. その2つの枠に、先ほどCanvasの中に作ったUIをそれぞれドラッグ＆ドロップで割り当てます。

## 4. 演出強化①：素材不要！プログラムで作るグラデーションUI
経験値バーを綺麗なグラデーションにしたいですが、画像素材がありません。
そこで、UIの頂点色をプログラムで塗り替える魔法のスクリプトを作ります！
Scripts/InGame/UI フォルダを作成し、UIGradient.cs を作ります。
ファイル名： UIGradient.cs
``` cs
```
【エディタでの作業】
Hierarchyで経験値Sliderの中の Fill Area > Fill を選択します。
今作った UIGradient.cs をアタッチし、Top Color と Bottom Color を好きな色（黄色とオレンジなど）に変更します。これだけでグラデーションになります！
