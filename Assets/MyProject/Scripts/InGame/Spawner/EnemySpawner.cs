using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TPSRoguelite.InGame.Enemy;
using UnityEngine;
using UnityEngine.AI;

namespace TPSRoguelite.InGame.Spawner
{
    public class EnemySpawner : MonoBehaviour
    {
        /// <summary>
        /// 出現時間
        /// </summary>
        private const float SPAWN_INTERVAL = 3.0f;

        /// <summary>
        /// 道を探す最大距離
        /// </summary>
        private const float MAX_SPAWN_DISTANCE = 2.0f;

        /// <summary>
        /// 最初に用意する敵の数
        /// </summary>
        private const int POOL_SIZE = 20;

        /// <summary>
        /// 敵のプレハブ
        /// </summary>
        [SerializeField] private GameObject enemyPrefab;

        /// <summary>
        /// 敵の親コンポーネント
        /// </summary>
        [SerializeField] private Transform enemyParent;

        /// <summary>
        /// 出現ポイント
        /// </summary>
        [SerializeField] private Transform[] spawnPoints;

        private Queue<EnemyState> enemyPool = new Queue<EnemyState>();

        private void Awake ()
        {
            for (int i = 0; i < POOL_SIZE; i++)
            {
                GameObject enemyObj = Instantiate(enemyPrefab, enemyParent);
                enemyObj.SetActive(false);
                EnemyState enemyState = enemyObj.GetComponent<EnemyState>();
                if (enemyState != null)
                {
                    enemyPool.Enqueue(enemyState);
                }
            }
        }

        private void Start ()
        {
            SpawnLoopAsync().Forget();
        }

        /// <summary>
        /// UniTaskを用いた非同期の生成ループ
        /// </summary>
        private async UniTaskVoid SpawnLoopAsync ()
        {
            // 発生装置が壊された時にタイマーを安全に止めるための切符（トークン）を取得
            var token = this.GetCancellationTokenOnDestroy();

            // 無限ループ（awaitがあるためフリーズしません）
            while (true)
            {
                // 指定時間待機する
                await UniTask.Delay(System.TimeSpan.FromSeconds(SPAWN_INTERVAL), cancellationToken: token);
                SpawnEnemyFromPool();
            }
        }

        /// <summary>
        /// 敵の生成
        /// </summary>
        private void SpawnEnemyFromPool()
        {
            if (enemyPrefab == null || spawnPoints.Length == 0)
            {
                return;
            }

            // ランダムな出現場所を選ぶ
            int randomIndex = Random.Range(0, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[randomIndex];

            // --- 安全な座標を探す ---
            Vector3 safePosition = spawnPoint.position;

            // 選んだポイントの周囲にNavMesh（歩ける道）があるか探す
            if (NavMesh.SamplePosition(spawnPoint.position, out NavMeshHit hit, MAX_SPAWN_DISTANCE, NavMesh.AllAreas))
            {
                // 見つかったら、その安全な座標で上書きする
                safePosition = hit.position;
            }
            else
            {
                // 見つからなければ今回は生成を諦めてスキップする
                Debug.LogWarning("近くに安全なスポーン位置が見つかりませんでした。");
                return;
            }

            EnemyState enemyState = null;
            if (enemyPool.Count > 0)
            {
                enemyState = enemyPool.Dequeue();
            }
            else
            {
                GameObject enemyObj = Instantiate(enemyPrefab, enemyParent);
                enemyState = enemyObj.GetComponent<EnemyState>();
                if (enemyState == null)
                {
                    return;
                }

                Debug.LogWarning("プールに空きがなたったため、敵を生成(Instantiate)しました。POOL_SIZEを調整するか、生成数を制限してください。");
            }

            enemyState.OnReturnToPoolAction -= ReturnToPool;
            enemyState.OnReturnToPoolAction += ReturnToPool;
            enemyState.transform.position = safePosition;
            enemyState.transform.rotation = spawnPoint.rotation;

            enemyState.gameObject.SetActive(true);
        }

        private void ReturnToPool (EnemyState enemy)
        {
            enemyPool.Enqueue(enemy);
            enemy.OnReturnToPoolAction -= ReturnToPool;
        }
    }
}
