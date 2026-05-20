using UnityEngine;

/// <summary>
/// 弾Prefabと基本性能を定義するScriptableObject。
/// </summary>
[CreateAssetMenu(menuName = "Bullet Data")]
public class BulletData : ScriptableObject
{
    // 生成またはプール化する弾Prefab。
    public GameObject bulletPrefab;

    // 弾の基本移動速度。
    public float speed = 10f;

    // 弾の基本ダメージ。
    public int damage = 1;

    // 弾の表示色。
    public Color color = Color.white;

    // 弾が自動で消えるまでの秒数。
    public float lifetime = 3f;
}
