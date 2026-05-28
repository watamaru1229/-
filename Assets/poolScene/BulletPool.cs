using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace PoolScene
{
    /// <summary>
    /// 弾Prefabを再利用し、生成と破棄の負荷を減らすための弾プール。
    /// </summary>
    public class BulletPool : MonoBehaviour
    {
        // 種類ごとに事前確保するときの目安数。
        public int poolSize = 20;

        // 起動時に事前生成しておく弾データ。
        [SerializeField] private List<BulletData> prewarmBulletData =
            new List<BulletData>(4);

        // 生成した弾をまとめる親。未指定ならこのオブジェクトの配下に作る。
        [SerializeField] private Transform bulletContainer;

        // Inspectorから確認できる全弾エントリの一覧。
        [FormerlySerializedAs("_pool")] [SerializeField]
        private List<BulletPoolEntry> pool =
            new List<BulletPoolEntry>(20);

        // 弾データごとに使用可能な弾を管理するキュー。
        private readonly Dictionary<BulletData, Queue<BulletPoolEntry>> _availableByData =
            new Dictionary<BulletData, Queue<BulletPoolEntry>>(8);

        // プール一覧の容量を設定値に合わせ、指定データ分の弾を事前生成する。
        private void Awake()
        {
            int prewarmCapacity =
                poolSize * Mathf.Max(1, prewarmBulletData.Count);

            if (pool.Capacity < prewarmCapacity)
            {
                pool.Capacity = prewarmCapacity;
            }

            Prewarm();
        }

        // Inspectorで指定された弾データごとに、使用可能な弾を先に確保する。
        private void Prewarm()
        {
            for (int i = 0; i < prewarmBulletData.Count; i++)
            {
                BulletData data = prewarmBulletData[i];

                if (ReferenceEquals(data, null))
                {
                    continue;
                }

                Queue<BulletPoolEntry> available =
                    GetAvailableQueue(data);

                for (int j = available.Count; j < poolSize; j++)
                {
                    CreateEntry(data, true);
                }
            }
        }

        // すべての使用中の弾を1つの更新ループで進める。
        private void Update()
        {
            float deltaTime = Time.deltaTime;

            for (int i = 0; i < pool.Count; i++)
            {
                BulletPoolEntry entry = pool[i];

                if (ReferenceEquals(entry, null) ||
                    !entry.isAlive ||
                    entry.isAvailable ||
                    ReferenceEquals(entry.bullet3D, null) ||
                    ReferenceEquals(entry.gameObject, null) ||
                    !entry.gameObject.activeInHierarchy)
                {
                    continue;
                }

                entry.bullet3D.Tick(deltaTime);
            }
        }

        /// <summary>
        /// 指定された弾データ用の使用可能な弾を取得し、足りなければ新しく作る。
        /// </summary>
        /// <param name="data">取得したい弾のデータ。</param>
        /// <returns>利用する弾エントリ。データが不正な場合はnull。</returns>
        public BulletPoolEntry GetBullet(BulletData data)
        {
            if (ReferenceEquals(data, null))
            {
                return null;
            }

            var available = GetAvailableQueue(data);

            while (available.Count > 0)
            {
                var entry = available.Dequeue();
                entry.isAvailable = false;

                if (entry.isAlive &&
                    !ReferenceEquals(entry.gameObject, null))
                {
                    return entry;
                }
            }

            return CreateEntry(data, false);
        }

        /// <summary>
        /// 使用済みの弾を再利用できる状態としてプールへ戻す。
        /// </summary>
        /// <param name="entry">返却する弾エントリ。</param>
        public void ReturnBullet(BulletPoolEntry entry)
        {
            if (ReferenceEquals(entry, null) ||
                !entry.isAlive ||
                entry.isAvailable ||
                ReferenceEquals(entry.bullet3D, null) ||
                ReferenceEquals(entry.bullet3D.data, null))
            {
                return;
            }

            entry.isAvailable = true;
            GetAvailableQueue(entry.bullet3D.data).Enqueue(entry);
        }

        /// <summary>
        /// 管理中の弾が外部から破棄されたことを記録する。
        /// </summary>
        /// <param name="entry">破棄された弾のエントリ。</param>
        public void MarkBulletDestroyed(BulletPoolEntry entry)
        {
            if (ReferenceEquals(entry, null))
            {
                return;
            }

            entry.isAlive = false;
            entry.isAvailable = false;
        }

        // 指定された弾データから新しい弾オブジェクトとプールエントリを作成する。
        // data: 生成に使う弾データ。
        // 戻り値: 作成した弾エントリ。
        private BulletPoolEntry CreateEntry(BulletData data, bool isAvailable)
        {
            GameObject obj =
                Instantiate(
                    data.bulletPrefab,
                    bulletContainer != null ? bulletContainer : transform);

            if (!obj.TryGetComponent(
                    out Bullet3D bulletComponent))
            {
                return null;
            }

            bulletComponent.Initialize(data);

            obj.SetActive(false);

            BulletPoolEntry poolEntry =
                new BulletPoolEntry
                {
                    gameObject = obj,
                    bullet3D = bulletComponent,
                    isAvailable = isAvailable,
                    isAlive = true
                };

            bulletComponent.InitializePool(this, poolEntry);

            pool.Add(poolEntry);

            if (isAvailable)
            {
                GetAvailableQueue(data).Enqueue(poolEntry);
            }

            return poolEntry;
        }

        // 弾データに対応する使用可能キューを取得し、なければ新しく作る。
        // data: キューを取得する弾データ。
        // 戻り値: 使用可能な弾を入れるキュー。
        private Queue<BulletPoolEntry> GetAvailableQueue(BulletData data)
        {
            if (_availableByData.TryGetValue(data, out var available))
            {
                return available;
            }

            available = new Queue<BulletPoolEntry>(poolSize);
            _availableByData.Add(data, available);
            return available;
        }
    }
}
