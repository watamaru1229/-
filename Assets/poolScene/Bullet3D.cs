using UnityEngine;

namespace PoolScene
{
    /// <summary>
    /// プレイヤー弾の移動、寿命管理、敵へのダメージ、プールへの返却を行うクラス。
    /// </summary>
    public class Bullet3D : MonoBehaviour
    {
        // この弾に使われている基本データ。
        public BulletData data;

        // この弾を管理している弾プール。
        private BulletPool _pool;

        // プール内でこの弾を表すエントリ。
        private BulletPoolEntry _poolEntry;

        // 有効化されてからの経過時間。
        private float _lifeTimer;

        // 敵に与えるダメージ量。
        private int _damage;

        // 弾の移動速度。
        private float _speed;

        // 自動で消えるまでの秒数。
        private float _lifetime;

        // ReturnToPoolから非アクティブ化している最中かどうか。
        private bool _isReturningToPool;

        /// <summary>
        /// 弾データの標準値で弾を初期化する。
        /// </summary>
        /// <param name="bulletData">弾の基本データ。</param>
        public void Initialize(BulletData bulletData)
        {
            data = bulletData;
            _damage = bulletData.damage;
            _speed = bulletData.speed;
            _lifetime = bulletData.lifetime;
        }

        /// <summary>
        /// 武器補正後の性能で弾を初期化する。
        /// </summary>
        /// <param name="bulletData">弾の基本データ。</param>
        /// <param name="damage">補正後のダメージ。</param>
        /// <param name="speed">補正後の速度。</param>
        /// <param name="lifetime">補正後の寿命。</param>
        public void Initialize(
            BulletData bulletData,
            int damage,
            float speed,
            float lifetime)
        {
            data = bulletData;
            _damage = Mathf.Max(1, damage);
            _speed = Mathf.Max(0.1f, speed);
            _lifetime = Mathf.Max(0.1f, lifetime);
        }

        /// <summary>
        /// この弾が戻る先のプールとエントリを登録する。
        /// </summary>
        /// <param name="pool">弾プール。</param>
        /// <param name="poolEntry">この弾のプールエントリ。</param>
        public void InitializePool(BulletPool pool, BulletPoolEntry poolEntry)
        {
            _pool = pool;
            _poolEntry = poolEntry;
        }

        /// <summary>
        /// プール管理側から呼ばれ、1フレーム分の移動と寿命判定を行う。
        /// </summary>
        /// <param name="deltaTime">前フレームからの経過秒数。</param>
        public void Tick(float deltaTime)
        {
            _lifeTimer += deltaTime;

            transform.Translate(
                Vector3.forward *
                (_speed * deltaTime));

            if (_lifeTimer > _lifetime)
            {
                ReturnToPool();
            }
        }

        // 再利用されるたびに寿命タイマーをリセットする。
        private void OnEnable()
        {
            _lifeTimer = 0f;
        }

        // 敵に当たったらダメージを与えてプールへ戻す。
        // other: 接触したCollider。
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Enemy")) return;

            if (other.TryGetComponent(
                    out Enemy3D enemy))
            {
                enemy.TakeDamage(_damage);
            }

            ReturnToPool();
        }

        // 弾を使用可能状態としてプールに返し、非表示にする。
        private void ReturnToPool()
        {
            if (_isReturningToPool ||
                _poolEntry == null ||
                _poolEntry.isAvailable)
            {
                return;
            }

            _isReturningToPool = true;
            _pool?.ReturnBullet(_poolEntry);
            gameObject.SetActive(false);
            _isReturningToPool = false;
        }

        // 外部から直接非表示にされた場合だけ、返却経路へ流す。
        private void OnDisable()
        {
            if (_isReturningToPool ||
                _poolEntry == null ||
                _poolEntry.isAvailable)
            {
                return;
            }

            ReturnToPool();
        }

        // 外部から破棄された場合、プール側で再利用候補から外せるようにする。
        private void OnDestroy()
        {
            _pool?.MarkBulletDestroyed(_poolEntry);
        }
    }
}
