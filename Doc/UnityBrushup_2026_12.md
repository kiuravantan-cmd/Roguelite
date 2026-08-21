# 第12週：生き残れ！「ゲームループ」と「シーン遷移」
## 本日の目標
これまで作ってきたシステムに「ルール（勝利と敗北）」を追加し、一つのゲームとして完成（ループ）させます！
1. **タイムサバイバル（勝利条件）**：制限時間を生き残ったらクリアにする「タイマー」を作る。
2. **プレイヤーのHP（敗北条件）**：敵にやられたらゲームオーバーになる仕組みを作る。
3. **シーン遷移とデータ引継ぎ**：タイトルやリザルト（結果発表）画面を作り、到達レベルやタイムを引き継ぐ。

## 1. タイムサバイバル（タイマー）の実装ヴァンサバ系ゲームのクリア条件は「時間」です。
ゲーム全体の進行を管理している GameManager に、カウントダウンタイマーを実装しましょう。
💡 **準備：タイマーUIの配置**
[ ] Hierarchyの Canvas の中に、UI > Text - TextMeshPro を作成し、名前を TimerText にします。
[ ] 画面の上部中央に大きく配置します。

**プログラムの追加**
GameManager.cs にタイマー機能と、シーンをまたいでデータを保存する機能を追加します。（第7週で作成した GameManager を改修します）
**ファイル名： GameManager.cs（追加・変更部分のみ）**
``` diff
using UnityEngine;
+ using UnityEngine.SceneManagement;
+ using TMPro;

namespace InGame.Manager
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private PlayerController player = null;
        [SerializeField] private EnemySpawner enemySpawner = null;
        
+       [Header("Game Rules")]
+       [SerializeField] private TextMeshProUGUI timerText;
+       [SerializeField] private float gameClearTime = 180f; // 3分間（180秒）でクリア

        // --- シーン間（リザルト画面）で引き継ぐデータ ---
+       public bool IsGameClear { get; private set; }
+       public float SurvivedTime { get; private set; }
+       public int FinalLevel { get; private set; }

        private float currentTime;
        private bool isGameActive = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
+               DontDestroyOnLoad(gameObject); // シーンが変わってもGameManagerを破壊しない！
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            SetupAsync().Forget();
        }

        private async UniTaskVoid SetupAsync()
        {
            await MasterDataAccessor.Instance.InitializeAsync();

            currentTime = gameClearTime; // タイマーをセット
            IsGameClear = false;

            if (player != null) player.Setup();
            if (enemySpawner != null) enemySpawner.Setup();

            isGameActive = true; // ゲーム開始！
        }

        private void Update()
        {
            // ゲーム中以外はタイマーを動かさない
            if (!isGameActive) return;

            // タイマーを減らす
            currentTime -= Time.deltaTime;
            SurvivedTime = gameClearTime - currentTime; // 生存時間を記録

            // UIを更新（分：秒 の形式で表示）
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(currentTime / 60F);
                int seconds = Mathf.FloorToInt(currentTime - minutes * 60);
                timerText.text = $"{minutes:00}:{seconds:00}";
            }

            // 0秒になったらゲームクリア！
            if (currentTime <= 0f)
            {
                GameClear();
            }
        }

        /// <summary>
        /// ゲームクリア処理
        /// </summary>
        private void GameClear()
        {
            isGameActive = false;
            IsGameClear = true;
            FinalLevel = player != null ? player.CurrentLevel : 1;
            
            Debug.Log("ゲームクリア！リザルトへ移行します。");
            GoToResultScene();
        }

        /// <summary>
        /// ゲームオーバー処理（プレイヤーから呼ばれる）
        /// </summary>
        public void GameOver()
        {
            isGameActive = false;
            IsGameClear = false;
            FinalLevel = player != null ? player.CurrentLevel : 1;
            
            Debug.Log("ゲームオーバー...");
            GoToResultScene();
        }

        private void GoToResultScene()
        {
            // 時間停止などの呪いを解く（非常に重要！）
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // リザルトシーンへ遷移
            SceneManager.LoadScene("ResultScene");
        }
    }
}
```
【エディタでの作業】
GameManager オブジェクトの Timer Text の枠に、先ほど作ったテキストをセットします。

## 2. プレイヤーのHPと死（ゲームオーバー）敵に触れた（または撃たれた）時にダメージを受け、HPが0になったらゲームオーバーになるようにします。
💡 **準備：HPバーの配置**
[ ] Canvasの中に、UIの Slider を作り、名前を HpSlider にします。（Interactableのチェックは外す）<br>
[ ] 画面の左上などに配置し、色を赤や緑にします。<br>
**ファイル名： PlayerController.cs（追加・変更部分のみ）**
``` diff
namespace TPSRoguelite.InGame.Player 
{
    // IDamageable インターフェースを追加して、ダメージを受けられるようにする
    public class PlayerController : MonoBehaviour, IDamageable
    {
        // --- 変数に追加 ---
        [Header("Status")]
        public int MaxHP { get; private set; } = 100;
        public int CurrentHP { get; private set; }

        [Header("HP UI")]
        [SerializeField] private Slider hpSlider;

        public void Setup()
        {
            // ... 既存の処理 ...
            
            CurrentHP = MaxHP;
            UpdateHpUI();
            
            gameObject.SetActive(true);
        }

        // --- 新しく追加するメソッド ---
        public void TakeDamage(int damageAmount)
        {
            if (damageAmount <= 0 || CurrentHP <= 0) return;

            CurrentHP -= damageAmount;
            Debug.Log($"プレイヤーがダメージを受けた！ 残りHP: {CurrentHP}");

            UpdateHpUI();

            if (CurrentHP <= 0)
            {
                Die();
            }
        }

        private void UpdateHpUI()
        {
            if (hpSlider != null)
            {
                hpSlider.value = (float)CurrentHP / MaxHP;
            }
        }

        private void Die()
        {
            Debug.Log("プレイヤーが倒れました。");
            gameObject.SetActive(false); // プレイヤーを消す

            // GameManagerにゲームオーバーを知らせる
            if (InGame.Manager.GameManager.Instance != null)
            {
                InGame.Manager.GameManager.Instance.GameOver();
            }
        }
```
【エディタでの作業】<br>
Player オブジェクトの Hp Slider の枠に、今作ったスライダーをセットします。

## 3. リザルトシーンとデータの引継ぎゲームの成績（クリアしたか？ 何レベルまでいったか？）を表示する画面を作ります。
💡 **準備：シーンの作成と登録**
1. メニューの File > New Scene で新しいシーンを作り、名前を ResultScene にして保存します。
2. メニューの File > Build Settings を開きます。
3. Scenes In Build という上の広い枠に、元のメインゲームのシーンと、今作った ResultScene を両方ともドラッグ＆ドロップして入れます。（これをやらないと SceneManager で移動できません！）

**リザルト画面の作成**
1. ResultScene を開き、Canvasの中に結果を表示する TextMeshPro を作ります。（名前を ResultText にします）
2. 「タイトルへ戻る」ボタン（Button - TextMeshPro）を作ります。
3. 空のオブジェクトを作り、ResultManager.cs というスクリプトを作ってアタッチします。
**ファイル名： ResultManager.cs**
``` cs
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using InGame.Manager;

namespace TPSRoguelite.Result
{
    public class ResultManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI resultText;

        private void Start()
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            bool isClear = GameManager.Instance.IsGameClear;
            int level = GameManager.Instance.FinalLevel;
            float time = GameManager.Instance.SurvivedTime;

            // 時間を 分:秒 に直す
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time - minutes * 60f);

            // 文字を組み立てて表示する
            if (isClear)
            {
                resultText.text = $"GAME CLEAR!\n\n到達レベル: {level}";
            }
            else
            {
                resultText.text = $"GAME OVER...\n\n生存時間: {minutes:00}:{seconds:00}\n到達レベル: {level}";
            }
        }

        // 「タイトルに戻る」ボタンにセットするメソッド
        public void OnClickReturnTitle()
        {
            // GameManagerはもう用済みなので破壊する（次のプレイで新しく作り直すため）
            if (GameManager.Instance != null)
            {
                Destroy(GameManager.Instance.gameObject);
            }

            // タイトル画面（またはメインゲーム）へ戻る
            SceneManager.LoadScene("MainGameScene"); // ※自分のメインシーンの名前に合わせる
        }
    }
}
```
【エディタでの作業】
[ ] ResultManager の枠に ResultText をセットします。
[ ] 「タイトルへ戻るボタン」の On Click () に ResultManager を入れ、OnClickReturnTitle() を設定します。
**これで、あなたのゲームの「ループ（始まりから終わり）」が完全に繋がりました！**