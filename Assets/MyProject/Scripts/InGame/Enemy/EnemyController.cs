using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

namespace TPSRoguelite.InGame.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        private const string PLAYER_TAG_NAME = "Player";
        private const float STUN_DURATION = 0.5f;

        /// <summary>
        /// NavMeshAgent
        /// </summary>
        [SerializeField] private NavMeshAgent navMeshAgent = null;

        [SerializeField] private EnemyState enemyState = null;

        /// <summary>
        /// 目的地となるPlayerのTransform
        /// </summary>
        private Transform targetPlayer = null;

        private CancellationTokenSource stunCts;

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

            if (enemyState != null && enemyState.EnemyDataAsset != null)
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

        private void HandleDamage()
        {
            stunCts?.Cancel();
            stunCts?.Dispose();
            stunCts = null;
            stunCts = new CancellationTokenSource();
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(stunCts.Token, this.GetCancellationTokenOnDestroy());

            StunAsync(linkedCts.Token).Forget();
        }

        private async UniTaskVoid StunAsync (CancellationToken token)
        {
            if (navMeshAgent == null)
            {
                return;
            }

            navMeshAgent.isStopped = true;

            bool isCanceled = await UniTask.Delay(System.TimeSpan.FromSeconds(STUN_DURATION)).SuppressCancellationThrow();

            if (!isCanceled && navMeshAgent.isActiveAndEnabled)
            {
                navMeshAgent.isStopped = false;
            }
        }
    }
}
