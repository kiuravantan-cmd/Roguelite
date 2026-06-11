using System;
using System.Collections.Generic;
using System.Text;
using TPSRoguelite.InGame.Enums;
using UnityEngine;

namespace TPSRoguelite.InGame.Data
{
    [CreateAssetMenu(fileName = "NewWeaponData", menuName = "ScriptableObjects/WeaponData")]
    public class WeponData : ScriptableObject
    {
        /// <summary>
        /// 武器名
        /// </summary>
        [field: SerializeField] public string WeaponName { get; private set; }

        /// <summary>
        /// 射撃タイプ
        /// </summary>
        [field: SerializeField] public FireType FireType { get; private set; }

        /// <summary>
        /// 攻撃力
        /// </summary>
        [field: SerializeField] public int AttackPower { get; private set; }

        /// <summary>
        /// 撃ち終わった後のクールダウン
        /// </summary>
        [field: SerializeField] public float FireInterval { get; private set; }

        /// <summary>
        /// フルオートやバースト時の連射間隔
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
