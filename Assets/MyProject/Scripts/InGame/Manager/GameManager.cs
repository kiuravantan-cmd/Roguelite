using TPSRoguelite.InGame.Player;
using TPSRoguelite.InGame.Spawner;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Core.MasterData;
using UnityEngine.SceneManagement;
using TMPro;

namespace TPSRoguelite.InGame.Manager 
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private PlayerController player = null;
        [SerializeField] private EnemySpawner enemySpawner = null;

        [Header("ゲームルール")]
        [SerializeField, Header("ゲームタイマーのテキスト")] private TextMeshProUGUI timerText;

        [SerializeField, Header("クリア時間（秒）"), Tooltip("クリアに必要な時間")] private float gameClearTime = 180f;

        /// <summary>
        /// 現在の経過時間（秒）
        /// </summary>
        private float currentTime = 0f;

        /// <summary>
        /// ゲームがアクティブかどうかのフラグ
        /// </summary>
        private bool isGameActive = false;

        /// <summary>
        /// ゲームクリアかどうかのフラグ
        /// </summary>
        public bool IsGameClear { get; private set; } = false;

        /// <summary>
        /// 生存した時間（秒）
        /// </summary>
        public float SurvivedTime { get; private set; }

        /// <summary>
        /// ゲームオーバー時のレベル
        /// </summary>
        public int FinalLevel { get; private set; }

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
            Setup().Forget();
        }

        private async UniTaskVoid Setup()
        {
            // マスターデータの読み込み
            await MasterDataAccessor.Instance.InitializeAsync();

            // 読み込みが完了したら、プレイヤーとスポナーの準備を始める
            if (player != null)
            {
                player.Setup();
            }

            if (enemySpawner != null)
            {
                enemySpawner.Setup();
            }

            IsGameClear = false;
            currentTime = gameClearTime; // タイマーをセット
            isGameActive = true; // ゲーム開始！
        }

        private void Update ()
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
        private void GameClear ()
        {
            isGameActive = false;
            IsGameClear = true;
            FinalLevel = player != null ? player.CurrentLevel : 0;

            Debug.Log("ゲームクリア！リザルトへ移行します。");
            GoToResultScene();
        }

        /// <summary>
        /// ゲームオーバー処理（プレイヤーから呼ばれる）
        /// </summary>
        public void GameOver ()
        {
            isGameActive = false;
            IsGameClear = false;
            FinalLevel = player != null ? player.CurrentLevel : 0;

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

            // リザルトシーンへ遷移
            SceneManager.LoadScene("ResultScene");
        }
    }
}
