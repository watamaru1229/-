using PoolScene;
using UnityEngine;

/// <summary>
/// オブジェクトプールを使わず、寿命切れや命中時にDestroyされる比較用弾クラス。
/// </summary>
public class Bullet3D_NoPool : MonoBehaviour
{
    // 弾の移動速度。
    public float speed = 10f;

    // 弾が自動で消えるまでの秒数。
    public float lifetime = 3f;

    // 生成されてからの経過時間。
    private float _lifeTimer;

    // 毎フレーム前進し、寿命を超えたら破棄する。
    void Update()
    {
        _lifeTimer += Time.deltaTime;

        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        if (_lifeTimer > lifetime)
        {
            Destroy(gameObject); // ←プール版との違い
        }
    }

    // 敵に当たったらスコアを加算し、敵と弾を破棄する。
    // other: 接触したCollider。
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            GameManager3D.Instance.AddScore(10);

            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}
