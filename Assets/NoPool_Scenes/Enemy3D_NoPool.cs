using PoolScene;
using UnityEngine;

/// <summary>
/// オブジェクトプールを使わず、画面外や被弾時にDestroyされる比較用敵クラス。
/// </summary>
public class Enemy3D_NoPool : MonoBehaviour
{
    // 敵の移動速度。
    public float speed = 3f;

    // 追跡対象のプレイヤーTransform。
    private Transform _target;

    // 開始時にPlayerタグのオブジェクトを探して追跡対象にする。
    private void Start()
    {
        var player =
            GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            _target = player.transform;
        }
    }

    // 毎フレームプレイヤーへ移動し、画面外に出たら破棄する。
    void Update()
    {
        MoveTowardPlayer();

        if (transform.position.z < -10)
        {
            Destroy(gameObject); // ←ここが違い
        }
    }

    // 弾に当たったらスコアを加算し、弾と敵を破棄する。
    // other: 接触したCollider。
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            GameManager3D.Instance.AddScore(10);

            Destroy(other.gameObject); // 弾も消す
            Destroy(gameObject);       // 自分も消す
        }
    }

    // プレイヤーが見つかっていれば追跡し、見つからなければ後方へ直進する。
    private void MoveTowardPlayer()
    {
        if (_target == null)
        {
            transform.Translate(Vector3.back * speed * Time.deltaTime);
            return;
        }

        var direction =
            _target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        direction.Normalize();
        transform.position += direction * (speed * Time.deltaTime);
        transform.rotation =
            Quaternion.LookRotation(direction, Vector3.up);
    }
}
