using Cysharp.Threading.Tasks;
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
        /// 敵のプレハブ
        /// </summary>
        [SerializeField] private GameObject enemyPrefab;

        /// <summary>
        /// 出現ポイント
        /// </summary>
        [SerializeField] private Transform[] spawnPoints;

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
                SpawnEnemy();
            }
        }

        /// <summary>
        /// 敵の生成
        /// </summary>
        private void SpawnEnemy ()
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

            // 敵のクローンを生成する
            GameObject enemy = Instantiate(enemyPrefab, safePosition, spawnPoint.rotation);
            Debug.Log("敵を生成(Instantiate)しました！");
        }
    }
}
