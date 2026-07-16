using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace TPSRoguelite.InGame.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        private const string PLAYER_TAG_NAME = "Player";

        /// <summary>
        /// ノックバックの強さ
        /// </summary>
        private const float KNOCKBACK_FORCE = 2.0f;

        /// <summary>
        /// ノックバックの持続時間
        /// </summary>
        private const float KNOCKBACK_DURATION = 0.15f;

        /// <summary>
        /// 敵の本体
        /// </summary>
        [SerializeField] private EnemyState enemyState = null;

        /// <summary>
        /// NavMeshAgent
        /// </summary>
        [SerializeField] private NavMeshAgent navMeshAgent = null;

        /// <summary>
        /// 目的地となるPlayerのTransform
        /// </summary>
        private Transform targetPlayer = null;

        /// <summary>
        /// ノックバック動作のキャンセルトークン
        /// </summary>
        private CancellationTokenSource hitCts;

        private void Awake() 
        {
            // シーンから"Player"というタグが付いたオブジェクトを探す
            GameObject player = GameObject.FindGameObjectWithTag(PLAYER_TAG_NAME);
            if (player != null) 
            {
                targetPlayer = player.transform;
            }
            else
            {
                Debug.LogError($"{PLAYER_TAG_NAME}というタグのついたオブジェクトが見つかりませんでした。");
            }

            if (navMeshAgent != null && enemyState != null && enemyState.EnemyDataAsset != null)
            {
                navMeshAgent.speed = enemyState.EnemyDataAsset.MoveSpeed;
            }
        }

        private void Update()
        {
            // ターゲット（プレイヤー）とナビが存在しているか
            if (targetPlayer != null && navMeshAgent != null) 
            {
                // プレイヤーの現在位置を毎フレーム目的地として設定する
                navMeshAgent.SetDestination(targetPlayer.position);
            }
        }

        private void OnEnable ()
        {
            enemyState.OnDamageAction -= HandleDamage;
            enemyState.OnDamageAction += HandleDamage;
        }

        private void OnDisable ()
        {
            if (enemyState != null)
            {
                enemyState.OnDamageAction -= HandleDamage;
            }

            if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
            {
                navMeshAgent.isStopped = false;
            }
        }

        /// <summary>
        /// ダメージを受けたときの処理
        /// </summary>
        private void HandleDamage()
        {
            hitCts?.Cancel();
            hitCts?.Dispose();
            hitCts = new CancellationTokenSource();
            var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(hitCts.Token, this.GetCancellationTokenOnDestroy());
            KnockbackAsync(linkedToken.Token).Forget();
        }

        /// <summary>
        /// ノックバック
        /// </summary>
        private async UniTaskVoid KnockbackAsync(CancellationToken token)
        {
            if (navMeshAgent == null)
            {
                return;
            }

            // 追跡を一時停止
            bool wasStopped = navMeshAgent.isStopped;
            navMeshAgent.isStopped = true;

            // プレイヤーの逆方向にノックバックするため、プレイヤーの位置を基準に方向を計算
            if (targetPlayer != null)
            {
                Vector3 dir = (transform.position - targetPlayer.position).normalized;
                // 上下にはノックバックしないようにY軸を0にする
                dir.y = 0f;
                transform.position += dir * KNOCKBACK_FORCE;
            }

            // ノックバックの持続時間待機
            bool isCanceled = await UniTask.Delay(System.TimeSpan.FromSeconds(KNOCKBACK_DURATION), cancellationToken: token).SuppressCancellationThrow();

            // 追跡を再開
            if (!isCanceled && navMeshAgent.isActiveAndEnabled)
            {
                navMeshAgent.isStopped = wasStopped;
            }
        }
    }
}
