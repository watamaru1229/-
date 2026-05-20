using UnityEngine;

namespace poolScene
{
    /// <summary>
    /// 敵の移動、射撃、被ダメージ、死亡時のスコア加算とアイテムドロップを管理するクラス。
    /// </summary>
    public class Enemy3D : MonoBehaviour
    {
        // この敵に適用されている敵データ。
        private EnemyData.EnemyData _data;

        // 現在のHP。
        private int _currentHp;

        // 敵の移動速度。
        public float speed;

        // 撃破時に再生する効果音。
        public AudioClip hitSound;

        // 敵が発射する弾のPrefab。
        public GameObject enemyBulletPrefab;

        // 敵弾の発射間隔を測るためのタイマー。
        private float _shotTimer;

        // 死亡時に生成する爆発エフェクト。
        public GameObject explosionEffect;

        // ドロップアイテム用のPrefab。
        public GameObject dropPrefab;

        // この敵を管理する敵プール。
        private EnemyPool _pool;

        // 追跡対象のプレイヤーTransform。
        private Transform _target;


        // ゲーム中のみプレイヤーを追跡し、一定間隔で弾を発射する。
        private void Update()
        {
            if (GameManager3D.Instance.isGameOver) return;

            MoveTowardPlayer();

            _shotTimer += Time.deltaTime;

            if (_shotTimer > 2f)

            {
                Instantiate(enemyBulletPrefab, transform.position, Quaternion.identity);

                _shotTimer = 0f;
            }

            if (transform.position.z < -10)
            {
                ReturnToPool();
            }
        }

        // プレイヤーに接触したらダメージを与えてプールへ戻る。
        // other: 接触したCollider。
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var player =
                    other.GetComponent<Player3D>();

                if (player != null)
                {
                    player.TakeDamage(1);
                }

                ReturnToPool();
            }
        }

        // 敵をプールへ返却し、シーン上では非表示にする。
        private void ReturnToPool()
        {
            _pool?.ReturnEnemy(this);
            gameObject.SetActive(false);
        }

        // 外部から無効化された場合もプールへ返却する。
        private void OnDisable()
        {
            _pool?.ReturnEnemy(this);
        }

        /// <summary>
        /// 敵にダメージを与え、HPが0以下になったら死亡処理を行う。
        /// </summary>
        /// <param name="damage">受けるダメージ量。</param>
        public void TakeDamage(int damage)
        {
            _currentHp -= damage;

            if (_currentHp <= 0)
            {
                Die();
            }
        }

        // 敵データのドロップ設定から重み付き抽選でアイテムを生成する。
        private void DropItem()
        {
            if (_data == null ||
                _data.drops == null ||
                _data.drops.Length == 0 ||
                dropPrefab == null)
            {
                return;
            }

            var totalWeight = 0;

            for (var i = 0; i < _data.drops.Length; i++)
            {
                var drop = _data.drops[i];

                if (drop.itemData != null)
                {
                    totalWeight += drop.weight;
                }
            }

            if (totalWeight <= 0)
            {
                return;
            }

            var randomValue =
                Random.Range(0, totalWeight);

            var currentWeight = 0;

            for (var i = 0; i < _data.drops.Length; i++)
            {
                var drop = _data.drops[i];

                if (drop.itemData == null)
                {
                    continue;
                }

                currentWeight += drop.weight;

                if (randomValue >= currentWeight)
                {
                    continue;
                }

                var item =
                    Instantiate(
                        dropPrefab,
                        transform.position,
                        Quaternion.identity);

                if (!item.TryGetComponent(
                        out ItemPickup itemPickup))
                {
                    return;
                }

                Debug.Log(drop.itemData.itemName);

                itemPickup.Initialize(drop.itemData);

                return;
            }
        }

        // 効果音、爆発、ドロップ、スコア加算を行って敵をプールへ戻す。
        private void Die()
        {
            if (AudioManager.Instance != null &&
                hitSound != null)
            {
                AudioManager.Instance.PlaySe(hitSound);
            }

            if (explosionEffect != null)
            {
                Instantiate(
                    explosionEffect,
                    transform.position,
                    Quaternion.identity);
            }

            DropItem();

            if (GameManager3D.Instance != null &&
                _data != null)
            {
                GameManager3D.Instance
                    .AddScore(_data.score);
            }

            ReturnToPool();
        }

        /// <summary>
        /// この敵を管理するプールを登録する。
        /// </summary>
        /// <param name="pool">返却先の敵プール。</param>
        public void InitializePool(EnemyPool pool)
        {
            _pool = pool;
        }

        /// <summary>
        /// 敵データを適用し、HP、速度、色、追跡対象を初期化する。
        /// </summary>
        /// <param name="data">適用する敵データ。</param>
        public void Initialize(EnemyData.EnemyData data)
        {
            _data = data;

            _currentHp = data.hp;

            speed = data.speed;

            var r =
                GetComponentInChildren<Renderer>();

            r.material.color = data.color;

            if (_target == null)
            {
                var player =
                    GameObject.FindGameObjectWithTag("Player");

                if (player != null)
                {
                    _target = player.transform;
                }
            }
        }

        // プレイヤーが見つかっていれば追跡し、見つからなければ後方へ直進する。
        private void MoveTowardPlayer()
        {
            if (_target == null)
            {
                transform.Translate(Vector3.back * (speed * Time.deltaTime));
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
}
