using TPSRoguelite.InGame.Player;
using UnityEngine;

namespace TPSRoguelite.InGame.Item
{
    public class ExperienceOrb : MonoBehaviour
    {
        /// <summary>
        /// プレイヤーに吸い寄せられ始める距離
        /// </summary>
        private const float MAGNET_RANGE = 5.0f;

        /// <summary>
        /// 吸い寄せられるスピード（プレイヤーより速くする）
        /// </summary>
        private const float MOVE_SPEED = 15.0f;

        private const string PLAYER_TAG_NAME = "Player";

        private Transform targetPlayer;
        private bool isFollowing = false;

        private void Start ()
        {
            // 出現時にシーン上のプレイヤーを探してターゲットにする
            GameObject playerObj = GameObject.FindGameObjectWithTag(PLAYER_TAG_NAME);
            if (playerObj != null)
            {
                targetPlayer = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("ExperienceOrb: プレイヤーが見つかりませんでした。");
            }
        }

        private void Update ()
        {
            if (targetPlayer == null)
            {
                return;
            }

            if (!isFollowing)
            {
                // まだ吸い寄せられていない場合、距離を測る
                float distance = Vector3.Distance(transform.position, targetPlayer.position);
                if (distance <= MAGNET_RANGE)
                {
                    // 範囲内に入ったら吸い寄せ開始
                    isFollowing = true;
                }
            }
            else
            {
                // 吸い寄せ中の場合、ターゲットに向かって高速で移動する
                transform.position = Vector3.MoveTowards(transform.position, targetPlayer.position, MOVE_SPEED * Time.deltaTime);
            }
        }

        /// <summary>
        /// プレイヤーに触れた瞬間の処理（コライダーの Is Trigger がオンになっている必要があります）
        /// </summary>
        private void OnTriggerEnter (Collider other)
        {
            if (other.CompareTag(PLAYER_TAG_NAME))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null)
                {
                    // プレイヤーの経験値を増やすメソッドを呼ぶ
                    player.AddExperience(1);

                    // 吸い込まれたら自分自身は消滅する
                    // ※将来的にオブジェクトプールにする場合はここを改修します
                    Destroy(gameObject);
                }
            }
        }
    }
}
