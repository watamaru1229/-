using UnityEngine;

/// <summary>
/// 武器アイテムの基本性能と、取得時にランダム補正する範囲を定義するScriptableObject。
/// </summary>
[CreateAssetMenu(menuName = "Weapon Data")]
public class WeaponData : ItemData
{
    // この武器が発射する弾のデータ。
    public BulletData bulletData;

    // 基本の射撃間隔。
    public float fireInterval = 0.3f;

    // 基本の同時発射数。
    public int projectileCount = 1;

    // 射撃間隔に掛けるランダム倍率の範囲。
    public Vector2 fireIntervalMultiplierRange =
        new Vector2(0.85f, 1.2f);

    // ダメージに掛けるランダム倍率の範囲。
    public Vector2 damageMultiplierRange =
        new Vector2(0.8f, 1.35f);

    // 弾速に掛けるランダム倍率の範囲。
    public Vector2 bulletSpeedMultiplierRange =
        new Vector2(0.9f, 1.25f);

    // 弾の寿命に掛けるランダム倍率の範囲。
    public Vector2 lifetimeMultiplierRange =
        new Vector2(0.9f, 1.15f);

    // 取得時に追加される弾数のランダム範囲。
    public Vector2Int additionalProjectileRange =
        new Vector2Int(0, 2);

    // 複数弾を撃つときの拡散角度のランダム範囲。
    public Vector2 spreadAngleRange =
        new Vector2(0f, 8f);
}
