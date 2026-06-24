using UnityEngine;
using UnityEngine.Events;
using Core.Interface;
using Core.MasterData;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TPSRoguelite.InGame.Enemy 
{
    public class EnemyState : MonoBehaviour, IDamageable
    {
        // 点滅する時間
        private const float kFlashDuration = 0.1f;

        [SerializeField] private Renderer[] modelRenderers;

        private Color[] defaultColors;
        private CancellationTokenSource flashCts;

        public event UnityAction OnDamageAction;

        public event UnityAction<EnemyState> OnReturnToPoolAction;

        public EnemyDataRecord EnemyDataAsset { get; private set; }

        /// <summary>
        /// 現在の体力
        /// </summary>
        public int CurrentHP { get; private set; }

        public void Initialize(ulong id)
        {
            EnemyDataAsset = MasterDataAccessor.Instance.GetById<EnemyDataRecord>(id);

            if (modelRenderers != null)
            {
                defaultColors = new Color[modelRenderers.Length];
                for (int i = 0; i < modelRenderers.Length; i++)
                {
                    if (modelRenderers[i] != null)
                    {
                        defaultColors[i] = modelRenderers[i].material.color;
                    }
                }
            }
        }

        public void Setup()
        {
            ResetColor();
            CurrentHP = EnemyDataAsset.MaxHp;
            gameObject.SetActive(true);
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
                flashCts = null;
                flashCts = new CancellationTokenSource();
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(flashCts.Token, this.GetCancellationTokenOnDestroy());

                DamageFlashAsync(linkedCts.Token).Forget();
            }
            else
            {
                Die();
            }
        }

        private async UniTaskVoid DamageFlashAsync(CancellationToken token)
        {
            if (modelRenderers == null)
            {
                return;
            }

            foreach (var r in modelRenderers)
            {
                if (r != null)
                {
                    r.material.color = Color.red;
                }

                bool isCanceled = await UniTask.Delay(System.TimeSpan.FromSeconds(kFlashDuration), cancellationToken: token).SuppressCancellationThrow();

                if (!isCanceled)
                {
                    ResetColor();
                }
            }
        }

        private void ResetColor()
        {
            if (modelRenderers == null || defaultColors == null)
            {
                return;
            }

            for (int i = 0; i < modelRenderers.Length; i++)
            {
                if (modelRenderers[i] != null)
                {
                    modelRenderers[i].material.color = defaultColors[i];
                }
            }
        }

        private void Die() 
        {
            Debug.Log($"{EnemyDataAsset.EnemyName}を倒しました");
            gameObject.SetActive(false);
            OnReturnToPoolAction?.Invoke(this);
        }
    }
}
