using UnityEngine;
using UnityEngine.Events;
using Core.Interface;
using TPSRoguelite.InGame.Data;

namespace TPSRoguelite.InGame.Enemy 
{
    public class EnemyState : MonoBehaviour, IDamageable
    {
        [field: SerializeField] public EnemyData EnemyDataAsset { get; private set; }

        /// <summary>
        /// 現在の体力
        /// </summary>
        public int CurrentHP { get; private set; }

        public event UnityAction<EnemyState> OnReturnToPoolAction;


        private void OnEnable()
        {
            if (EnemyDataAsset != null)
            {
                CurrentHP = EnemyDataAsset.MaxHp;
            }
            else
            {
                Debug.LogError("EnemyDataがセットされていません");
            }
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

            if (CurrentHP <= 0)
            {
                Die();
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
