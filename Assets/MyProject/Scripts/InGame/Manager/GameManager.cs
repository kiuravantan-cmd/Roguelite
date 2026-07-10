using Core.MasterData;
using Cysharp.Threading.Tasks;
using TPSRoguelite.InGame.Player;
using TPSRoguelite.InGame.Spawner;
using UnityEngine;

namespace TPSRoguelite.InGame.Manager
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private PlayerController player = null;
        [SerializeField] private EnemySpawner enemySpawner = null;

        private void Awake ()
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

        private void Start ()
        {
            // 非同期でセットアップを開始する
            Setup().Forget();
        }

        private async UniTaskVoid Setup ()
        {
            // 【重要】ここでマスターデータの読み込みが完了するまで「待つ（await）」！
            await MasterDataAccessor.Instance.InitializeAsync();

            // 読み込みが完了したら、プレイヤーと敵発生装置の準備を始める
            if (player != null)
            {
                player.Setup();
            }

            if (enemySpawner != null)
            {
                enemySpawner.Setup();
            }
        }
    }
}
