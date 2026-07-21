using UnityEngine;
using UnityEngine.Events;
using Core.Interface;
using Core.MasterData;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TPSRoguelite.InGame.Enemy 
{
    public class EnemyState : MonoBehaviour, IDamageable
    {
        /// <summary>
        /// 点滅する時間
        /// </summary>
        private const float FLASH_DURATION = 0.1f;

        /// <summary>
        /// オーブのドロップ位置の高さのオフセット
        /// </summary>
        private const float ORB_DROP_HEIGHT_OFFSET = 0.5f;

        /// <summary>
        /// キャラクターのレンダラー
        /// </summary>
        [SerializeField] private Renderer[] modelRenderers;

        [Header("ドロップアイテム")]

        /// <summary>
        /// ドロップするオーブ
        /// </summary>
        [SerializeField] private GameObject experienceOrbPrefab;

        /// <summary>
        /// キャラクターの元々の色
        /// </summary>
        private Color[] defaultColors;

        /// <summary>
        /// 点滅するアニメーションのキャンセルトークン
        /// </summary>
        private CancellationTokenSource flashCts;

        /// <summary>
        /// 敵のデータ
        /// </summary>
        public EnemyDataRecord EnemyDataAsset { get; private set; }

        /// <summary>
        /// 現在の体力
        /// </summary>
        public int CurrentHP { get; private set; }

        public event UnityAction<EnemyState> OnReturnToPoolAction;

        /// <summary>
        /// ダメージを受けたときに受け取るイベント
        /// </summary>
        public event UnityAction OnDamageAction;

        public void Initialize(ulong id)
        {
            EnemyDataAsset = MasterDataAccessor.Instance.GetById<EnemyDataRecord>(id);

            if (modelRenderers != null)
            {
                defaultColors = new Color[modelRenderers.Length];
                for (int i = 0; i < modelRenderers.Length; i++)
                {
                    defaultColors[i] = modelRenderers[i].material.color;
                }
            }
        }

        public void Setup()
        {
            if (EnemyDataAsset == null) 
            {
                Debug.LogError("EnemyDataがセットされていません。");
                return;
            }

            CurrentHP = EnemyDataAsset.MaxHP;
            gameObject.SetActive(true);
            ResetColor();
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
                flashCts = new CancellationTokenSource();
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(flashCts.Token, this.GetCancellationTokenOnDestroy());

                DamageFlashAsync(linkedCts.Token).Forget();
            }
            else
            {
                Die();
            }
        }

        private void Die() 
        {
            Debug.Log($"{EnemyDataAsset.EnemyName}を倒しました");

            // 経験値オーブをドロップする
            if (experienceOrbPrefab != null)
            {
                Vector3 spawnPosition = transform.position + Vector3.up * ORB_DROP_HEIGHT_OFFSET;
                Instantiate(experienceOrbPrefab, spawnPosition, Quaternion.identity);
            }

            gameObject.SetActive(false);
            OnReturnToPoolAction?.Invoke(this);
        }

        /// <summary>
        /// ダメージを受けたときの点滅処理
        /// </summary>
        private async UniTaskVoid DamageFlashAsync(CancellationToken token)
        {
            if (modelRenderers == null || defaultColors == null)
            {
                return;
            }

            foreach (var renderer in modelRenderers)
            {
                if (renderer != null)
                {
                    renderer.material.color = Color.red;
                }

                bool isCancelled = await UniTask.Delay(TimeSpan.FromSeconds(FLASH_DURATION), cancellationToken: token).SuppressCancellationThrow();

                if (!isCancelled)
                {
                    ResetColor();
                }
            }
        }

        /// <summary>
        /// 色をリセット
        /// </summary>
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
    }
}
