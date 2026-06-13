using UnityEngine;
using TPSRoguelite.InGame.Enum;

namespace TPSRoguelite.InGame.Data 
{
    [CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
    public class WeaponData : ScriptableObject 
    {
        /// <summary>
        /// 武器の名前
        /// </summary>
        [field: SerializeField] public string WeaponName { get; private set; }

        /// <summary>
        /// 射撃タイプ
        /// </summary>
        [field: SerializeField] public FireType WeaponFireType { get; private set; }

        /// <summary>
        /// 攻撃力
        /// </summary>
        [field: SerializeField] public int AttackPower { get; private set; }

        /// <summary>
        /// 射撃のインターバル時間（バーストやフルオートの連射間隔）
        /// </summary>
        [field: SerializeField] public float FireInteval { get; private set; }

        /// <summary>
        /// 次の弾が撃てるまでの待機時間
        /// </summary>
        [field: SerializeField] public float FireRate { get; private set; }

        /// <summary>
        /// 最大弾数
        /// </summary>
        [field: SerializeField] public int MaxAmmo { get; private set; }

        /// <summary>
        /// リロード時間
        /// </summary>
        [field: SerializeField] public float ReloadTime { get; private set; }
    }
}
