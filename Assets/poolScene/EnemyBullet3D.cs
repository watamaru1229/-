using UnityEngine;

namespace PoolScene
{
    /// <summary>
    /// 敵が発射する弾の移動、プレイヤーへのダメージ、画面外での削除を行うクラス。
    /// </summary>
    public class EnemyBullet3D : MonoBehaviour
    {
        // 敵弾の移動速度。
        public float speed = 10f;

        // ゲーム中のみ後方へ移動し、画面外に出たら破棄する。
        void Update()
        {
            if (GameManager3D.Instance.isGameOver) return;

            transform.Translate(Vector3.back * (speed * Time.deltaTime));

            if (transform.position.z < -10)
            {
                Destroy(gameObject);
            }
        }

        // プレイヤーに当たったらダメージを与えて弾を非表示にする。
        // other: 接触したCollider。
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Player3D player =
                    other.GetComponent<Player3D>();

                if (player != null)
                {
                    player.TakeDamage(1);
                }

                gameObject.SetActive(false);
            }
        }

        // 物理衝突のデバッグ用ログを出力する。
        // collision: 衝突情報。
        void OnCollisionEnter(Collision collision)
        {
            Debug.Log("COLLISION: " + collision.gameObject.name);
        }
    }
}
