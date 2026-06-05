using UnityEngine;
using UnityEngine.Events;
using Core.Interface;

namespace TPSRoguelite.InGame.Enemy 
{
    public class EnemyState : MonoBehaviour, IDamageable
    {
        /// <summary>
        /// 体力の最大値
        /// </summary>
        private const int MAX_HP = 100;

        /// <summary>
        /// 現在の体力
        /// </summary>
        public int CurrentHP { get; private set; }

        public event UnityAction<EnemyState> OnReturnToPoolAction;

        private void Awake() 
        {
            CurrentHP = MAX_HP;
        }

        private void OnEnable ()
        {
            // オブジェクトプールで再利用される時、表示された瞬間にHPを元に戻す
            CurrentHP = MAX_HP;
        }

        public void TakeDamage(int damageAmount) 
        {
            // マイナスのダメージ（回復）を防ぐ
            if (damageAmount <= 0)
            {
                return;
            }

            CurrentHP -= damageAmount;
            Debug.Log($"敵に{damageAmount}のダメージ！残りHP:{CurrentHP}");

            if (CurrentHP <= 0)
            {
                Die();
            }
        }

        private void Die() 
        {
            gameObject.SetActive(false);
            OnReturnToPoolAction?.Invoke(this);
        }
    }
}
