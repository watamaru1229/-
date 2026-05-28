using UnityEngine;

namespace PoolScene.ItemData.WeaponData
{
    /// <summary>
    /// 実際に装備・所持する武器の性能を保持するクラス。WeaponDataから固定値またはランダム補正付きで作成する。
    /// </summary>
    [System.Serializable]
    public class WeaponInstance
    {
        // 元になった武器データ。
        public global::WeaponData data;

        // UIやログに表示する武器名。
        public string displayName;

        // この武器が発射する弾データ。
        public BulletData bulletData;

        // 射撃間隔。
        public float fireInterval;

        // 同時に発射する弾数。
        public int projectileCount;

        // 1発あたりのダメージ。
        public int damage;

        // 弾の移動速度。
        public float bulletSpeed;

        // 弾が消えるまでの秒数。
        public float bulletLifetime;

        // 複数弾を撃つときの弾ごとの拡散角度。
        public float spreadAngle;

        /// <summary>
        /// WeaponDataの基本値だけを使って武器インスタンスを作成する。
        /// </summary>
        /// <param name="data">元にする武器データ。</param>
        /// <returns>作成した武器インスタンス。データが不正な場合はnull。</returns>
        public static WeaponInstance CreateDefault(global::WeaponData data)
        {
            if (data == null ||
                data.bulletData == null)
            {
                return null;
            }

            return new WeaponInstance
            {
                data = data,
                displayName = data.itemName,
                bulletData = data.bulletData,
                fireInterval = data.fireInterval,
                projectileCount = Mathf.Max(1, data.projectileCount),
                damage = Mathf.Max(1, data.bulletData.damage),
                bulletSpeed = data.bulletData.speed,
                bulletLifetime = data.bulletData.lifetime,
                spreadAngle = 0f
            };
        }

        /// <summary>
        /// WeaponDataのランダム範囲を使って性能に個体差のある武器インスタンスを作成する。
        /// </summary>
        /// <param name="data">元にする武器データ。</param>
        /// <returns>ランダム補正済みの武器インスタンス。データが不正な場合はnull。</returns>
        public static WeaponInstance CreateRandom(global::WeaponData data)
        {
            if (data == null ||
                data.bulletData == null)
            {
                return null;
            }

            var fireIntervalMultiplier =
                Random.Range(
                    data.fireIntervalMultiplierRange.x,
                    data.fireIntervalMultiplierRange.y);
            var damageMultiplier =
                Random.Range(
                    data.damageMultiplierRange.x,
                    data.damageMultiplierRange.y);
            var speedMultiplier =
                Random.Range(
                    data.bulletSpeedMultiplierRange.x,
                    data.bulletSpeedMultiplierRange.y);
            var lifetimeMultiplier =
                Random.Range(
                    data.lifetimeMultiplierRange.x,
                    data.lifetimeMultiplierRange.y);
            var additionalProjectiles =
                Random.Range(
                    data.additionalProjectileRange.x,
                    data.additionalProjectileRange.y + 1);

            return new WeaponInstance
            {
                data = data,
                displayName = data.itemName,
                bulletData = data.bulletData,
                fireInterval = Mathf.Max(0.03f, data.fireInterval * fireIntervalMultiplier),
                projectileCount = Mathf.Max(1, data.projectileCount + additionalProjectiles),
                damage = Mathf.Max(1, Mathf.RoundToInt(data.bulletData.damage * damageMultiplier)),
                bulletSpeed = Mathf.Max(0.1f, data.bulletData.speed * speedMultiplier),
                bulletLifetime = Mathf.Max(0.1f, data.bulletData.lifetime * lifetimeMultiplier),
                spreadAngle = Mathf.Max(
                    0f,
                    Random.Range(data.spreadAngleRange.x, data.spreadAngleRange.y))
            };
        }
    }
}
