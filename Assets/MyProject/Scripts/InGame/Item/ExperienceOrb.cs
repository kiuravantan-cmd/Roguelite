using TPSRoguelite.InGame.Player;
using UnityEngine;

namespace TPSRoguelite.InGame.Item
{
    public class ExperienceOrb : MonoBehaviour
    {
        /// <summary>
        /// プレイヤーに引き寄せられる範囲
        /// </summary>
        private const float MAGNET_RANGE = 5f;

        /// <summary>
        /// プレイヤーに引き寄せられる速度
        /// </summary>
        private const float MOVE_SPEED = 15f;

        /// <summary>
        /// プレイヤーのタグ
        /// </summary>
        private const string PLAYER_TAG = "Player";

        /// <summary>
        /// プレイヤーのTransform
        /// </summary>
        private Transform targetPlayer = null;

        /// <summary>
        /// プレイヤーに引き寄せられているかどうか
        /// </summary>
        private bool isFollowing = false;

        private void Start ()
        {
            // 出現時にプレイヤーのTransformを取得する
            GameObject playerObj = GameObject.FindGameObjectWithTag(PLAYER_TAG);
            if (playerObj != null)
            {
                targetPlayer = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("プレイヤーが見つかりませんでした。");
            }
        }

        private void Update ()
        {
            if (targetPlayer == null)
            {
                return;
            }

            if (isFollowing)
            {
                // プレイヤーに向かって移動する
                transform.position = Vector3.MoveTowards(transform.position, targetPlayer.position, MOVE_SPEED * Time.deltaTime);
            }
            else
            {
                // プレイヤーとの距離を計算し、引き寄せ範囲内であれば引き寄せを開始する
                float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);
                if (distanceToPlayer <= MAGNET_RANGE)
                {
                    // プレイヤーに引き寄せられるようになる
                    isFollowing = true;
                }
            }
        }

        /// <summary>
        /// プレイヤーに触れたときの処理（コライダーの Is Trigger がオンになっている必要があります）
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(PLAYER_TAG))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null)
                {
                    // プレイヤーに触れたら経験値を付与する処理をここに追加
                    player.AddExperience(1);
                }
                else
                {
                    Debug.LogWarning("PlayerController コンポーネントが見つかりませんでした。");
                }

                // 経験値オーブを破棄する
                Destroy(gameObject);
            }
        }
    }
}
