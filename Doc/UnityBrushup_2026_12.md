# 第12週：生き残れ！「ゲームループ」と「シーン遷移」
これまで作ってきたシステムに「ルール（勝利と敗北）」を追加し、一つのゲームとして完成（ループ）させます！<br>
さらに、プロの現場で使われている「Gitの除外設定」と「MVPパターン（設計手法）」という、ワンランク上の技術に挑戦します。

## 本日の目標
1. **Git運用（.gitignore）**：：配布されたフォントなど、重いデータや権利物をGitの保存対象から外す。
2. **タイムサバイバルと死（ゲームループ）**：制限時間のタイマーと、プレイヤーのHP（ゲームオーバー判定）を作る。
3. **リザルト画面とMVPパターン**：：UIとロジックを綺麗に分ける「MVPアーキテクチャ」で結果発表画面を作る。

## 1. プロのGit運用（.gitignoreの設定）
ゲームの見栄えを良くするために、外部サイトからダウンロードしたカッコいい「フォント（文字の形）」を使うことがあります。<br>
しかし、フォントファイルはデータ容量が非常に重く、また「他人に勝手に再配布してはいけない（著作権）」ルールがあることが多いため、そのままGit（GitHubなど）にアップロード（コミット）してしまうと大問題になることがあります。<br>
そこで、Gitに「このフォルダの中身は無視してね」と指示を出すファイル（.gitignore）を編集します。

💡 **作業手順：特定のフォルダをGitから除外する**<br>
[ ] 自分のゲームのプロジェクトフォルダ（`Assets` などがある場所）を開きます。<br>
[ ] その中にある `.gitignore` というファイルを、VSCodeなどのテキストエディタで開きます。<br>
[ ] ファイルの一番下（または分かりやすい場所）に、以下の2行を追記して保存します。
```
# --- フォントファイルをGitの管理から除外する ---
/[Aa]ssets/Fonts/
/[Aa]ssets/Fonts.meta
```
【注意】<br>
もし**すでにフォントをコミット（保存）してしまった後**にこれを書いた場合は、手遅れです。<br>
その場合は、ターミナル（コマンドプロンプト）で `git rm --cached -r Assets/Fonts` というコマンドを打って、Gitの記憶から強制的に消し去る必要があります。

## 2. ゲームループ（タイムサバイバルとゲームオーバー）
ゲーム全体の進行を管理している GameManager を改修し、「制限時間を生き残ったらクリア」「プレイヤーのHPが0になったらゲームオーバー」になる仕組みを作りましょう。

### 2.1 タイマー機能の実装
**💡 準備：タイマーUIの配置**<br>
[ ] Hierarchyの `Canvas` の中に、`UI > Text - TextMeshPro` を作成し、名前を `TimerText` にします。<br>
[ ] 画面の上部中央に大きく配置します。

**プログラムの追加**<br>
`GameManager.cs` にタイマー機能と、シーンをまたいでデータを保存する機能を追加します。

**ファイル名： `GameManager.cs`（追加・変更部分のみ）**
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
+       [SerializeField, Tooltip("ゲームタイマーのテキスト")]  private TextMeshProUGUI timerText;
+       [SerializeField, Header("クリア時間（秒）"), Tooltip("クリアに必要な時間")] private float gameClearTime = 180f;

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
                DontDestroyOnLoad(gameObject);
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

+           IsGameClear = false;
+           currentTime = gameClearTime; // タイマーをセット
+           isGameActive = true; // ゲーム開始！
        }

        private void Update()
        {
            // ゲームがアクティブでない場合は何もしない
            if (!isGameActive)
            {
                return;
            }

            // ゲームが一時停止中の場合はタイマーを更新しない
            if (Time.timeScale == 0f)
            {
                return;
            }

            // タイマーの更新
            currentTime -= Time.deltaTime;
            SurvivedTime = gameClearTime - currentTime;

            // UIを更新（分：秒 の形式で表示）
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(currentTime / 60f);
                int seconds = Mathf.FloorToInt(currentTime - minutes * 60f);
                timerText.SetText($"{minutes:00}:{seconds:00}");
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

        /// <summary>
        /// リザルトシーンへ遷移する処理
        /// </summary>
        private void GoToResultScene()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            SceneManager.LoadScene("ResultScene");
        }
    }
}
```
【エディタでの作業】<br>
`GameManager` オブジェクトの `Timer Text` の枠に、先ほど作ったテキストをセットします。

# 2.2 プレイヤーのHPと死（ゲームオーバー）
敵に触れた（または撃たれた）時にダメージを受け、HPが0になったらゲームオーバーになるようにします。

💡 **準備：HPバーの配置**<br>
[ ] Canvasの中に、UIの `Slider` を作り、名前を `HpSlider` にします。（Interactableのチェックは外す）<br>
[ ] 画面の左上などに配置し、色を赤や緑にします。<br><br>
**ファイル名： `PlayerController.cs`（追加・変更部分のみ）**
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
**【エディタでの作業】**<br>
Player オブジェクトの Hp Slider の枠に、今作ったスライダーをセットします。

## 3. リザルト画面とMVPパターン（アーキテクチャ）
「ボタンを押したらタイトルに戻る」という処理を書く際、今までのように1つのスクリプトに全部書くこともできます。
しかし今回は、プロの現場で必ず使われる MVPパターン（Model-View-Presenter） という設計方法を使って作ってみましょう！

### なぜMVPパターンを使うの？
1つのスクリプトに「見た目（UI）の操作」と「ゲームのルール（データ）」を混ぜて書くと、後で「デザインを変えたい」時にルールの部分まで壊してしまう（バグが起きる）からです。<br>
MVPパターンでは、役割を**3つのスクリプト**に完全に分けます。<br>
・**Model（モデル / データ係）**：スコアやHPなどの「データ」だけを持つ。画面のことは一切知らない。<br>
・**View（ビュー / 見た目係）**：ボタンや文字などの「UI」だけをいじる。ルールのことは一切知らない。<br>
・**Presenter（プレゼンター / 司会者）**：Modelからデータをもらい、Viewに「こう表示して」と命令する司令塔。

**💡 準備：シーンの作成と登録**
1. メニューの `File > New Scene` で新しいシーンを作り、名前を `ResultScene` にして保存します。
2. メニューの `File > Build Settings` を開きます。
3. `Scenes In Build` という上の広い枠に、元のメインゲームのシーンと、今作った `ResultScene` を両方ともドラッグ＆ドロップして入れます。（これをやらないと `SceneManager` で移動できません！）
4. `ResultScene` を開き、Canvasの中に結果を表示する `TextMeshPro` を作ります。（名前を `ResultText` にします）
5. 「タイトルへ戻る」ボタン（Button - TextMeshPro）を作ります。

### 3-1. Model（データ係）の作成
GameManager（前のシーンから生き残っているシングルトン）からデータを受け取るだけのシンプルなクラスを作ります。

**ファイル名： `ResultModel.cs`**
``` cs
using InGame.Manager; // GameManagerにアクセスするため

namespace TPSRoguelite.Result
{
    public class ResultModel
    {
        public bool IsClear { get; private set; }
        public int Level { get; private set; }
        public float SurvivedTime { get; private set; }

        // データを取り出して準備する
        public void Initialize()
        {
            if (GameManager.Instance != null)
            {
                IsClear = GameManager.Instance.IsGameClear;
                Level = GameManager.Instance.FinalLevel;
                SurvivedTime = GameManager.Instance.SurvivedTime;
            }
        }
    }
}
```

### 3-2. View（見た目係）の作成
UIの文字を書き換えたり、ボタンが押されたことを「司会者（Presenter）」に知らせるだけのクラスを作ります。
ルールの計算は絶対にここには書きません。

**ファイル名： `ResultView.cs`**
``` cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace TPSRoguelite.Result
{
    public class ResultView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button returnTitleButton;

        /// <summary>
        /// ボタンが押されたときに、外部（Presenter）に知らせるためのイベント
        /// </summary>
        public event UnityAction OnReturnTitleClickedAction;

        private void Awake()
        {
            // ボタンが押されたら、イベントを発火（お知らせ）する！
            if (returnTitleButton != null)
            {
                returnTitleButton.onClick.AddListener(() => OnReturnTitleClickedAction?.Invoke());
            }
        }

        /// <summary>
        /// Presenterから命令されて、文字を画面に表示するだけのメソッド
        /// </summary>
        public void SetResultText(string text)
        {
            if (resultText != null)
            {
                resultText.text = text;
            }
        }
    }
}
```

### 3-3. Presenter（司会者）の作成
データ係（Model）と見た目係（View）を繋ぎ合わせる、一番偉いクラスを作ります。
オブジェクトにアタッチするのはこのクラス（とView）だけです。

**ファイル名： `ResultPresenter.cs`**
``` cs
using UnityEngine;
using UnityEngine.SceneManagement;
using InGame.Manager;

namespace TPSRoguelite.Result
{
    public class ResultPresenter : MonoBehaviour
    {
        [SerializeField] private ResultView resultView;
        
        // Modelは MonoBehaviour を継承していないので、ここで new して生み出す
        private ResultModel resultModel;

        private void Start()
        {
            if (resultView == null) return;

            // 1. Modelを生み出してデータを準備させる
            resultModel = new ResultModel();
            resultModel.Initialize();

            // 2. Modelのデータを使って、表示する文字を組み立てる
            string message = "";
            if (resultModel.IsClear)
            {
                message = $"GAME CLEAR!\n\n到達レベル: {resultModel.Level}";
            }
            else
            {
                int minutes = Mathf.FloorToInt(resultModel.SurvivedTime / 60F);
                int seconds = Mathf.FloorToInt(resultModel.SurvivedTime - minutes * 60);
                message = $"GAME OVER...\n\n生存時間: {minutes:00}:{seconds:00}\n到達レベル: {resultModel.Level}";
            }

            // 3. 組み立てた文字を、Viewに渡して表示させる
            resultView.SetResultText(message);

            // 4. Viewの「ボタンが押されたよイベント」を耳打ち（購読）して、遷移処理をセットする
            resultView.OnReturnTitleClickedAction += ReturnToTitle;
        }

        private void ReturnToTitle()
        {
            // 次のプレイのために、古いGameManagerを破壊してリセットする
            if (GameManager.Instance != null)
            {
                Destroy(GameManager.Instance.gameObject);
            }

            // タイトル画面（メインシーン）へ戻る
            SceneManager.LoadScene("MainGameScene"); // ※自分のシーンの名前に合わせる
        }

        private void OnDestroy()
        {
            // メモリのゴミを防ぐため、イベントの購読を解除しておく
            if (resultView != null)
            {
                resultView.OnReturnTitleClickedAction -= ReturnToTitle;
            }
        }
    }
}
```

**💡 エディタでの作業手順（仕上げ）**<br>
[ ] 空のオブジェクトを作り、`ResultManager` などの名前にします。<br>
[ ] そのオブジェクトに、**`ResultView`** と **`ResultPresenter`** の2つのスクリプトをアタッチします。<br>
[ ] `ResultView` の枠に、TextとButtonをセットします。<br>
[ ] `ResultPresenter` の `Result View` 枠に、自分自身（今アタッチしたResultView）をセットします。<br>
[ ] ※今回、ボタンの `On Click ()` イベント（＋ボタン）をインスペクターで設定する必要はありません。**すべてスクリプトの力で自動的に繋がっています！**

ゲームをプレイして、タイムアップやHPゼロでリザルト画面に移動し、正しく結果が表示されれば、あなたのゲームの「ループ」は完全に完成です！
