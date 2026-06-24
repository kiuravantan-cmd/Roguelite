using Core.MasterData;
using Cysharp.Threading.Tasks;
using TPSRoguelite.InGame.Player;
using TPSRoguelite.InGame.Spawner;
using UnityEngine;

namespace InGame.Manager
{
    public class GameManager : MonoBehaviour
    {
        /// <summary>
        /// 外部からアクセスするためのインスタンス
        /// </summary>
        public static GameManager Instance { get; private set; }

        [SerializeField] private PlayerController player = null;
        [SerializeField] private EnemySpawner enemySpawner = null;

        new void Awake ()
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
            await MasterDataAccessor.Instance.InitializeAsync();

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
