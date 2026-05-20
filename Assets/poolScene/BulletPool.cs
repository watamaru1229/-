using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace poolScene
{
    /// <summary>
    /// 弾Prefabを再利用し、生成と破棄の負荷を減らすための弾プール。
    /// </summary>
    public class BulletPool : MonoBehaviour
    {
        // 種類ごとに事前確保するときの目安数。
        public int poolSize = 20;

        // Inspectorから確認できる全弾エントリの一覧。
        [FormerlySerializedAs("_pool")] [SerializeField] private List<BulletPoolEntry> pool =
            new List<BulletPoolEntry>(20);

        // 弾データごとに使用可能な弾を管理するキュー。
        private readonly Dictionary<BulletData, Queue<BulletPoolEntry>> _availableByData =
            new Dictionary<BulletData, Queue<BulletPoolEntry>>(8);

        // プール一覧の容量を設定値に合わせて確保する。
        private void Awake()
        {
            if (pool.Capacity < poolSize)
            {
                pool.Capacity = poolSize;
            }
        }

        /// <summary>
        /// 指定された弾データ用の使用可能な弾を取得し、足りなければ新しく作る。
        /// </summary>
        /// <param name="data">取得したい弾のデータ。</param>
        /// <returns>利用する弾エントリ。データが不正な場合はnull。</returns>
        public BulletPoolEntry GetBullet(BulletData data)
        {
            if (data == null)
            {
                return null;
            }

            var available = GetAvailableQueue(data);

            while (available.Count > 0)
            {
                var entry = available.Dequeue();
                entry.isAvailable = false;

                if (entry.gameObject != null)
                {
                    return entry;
                }
            }

            return CreateEntry(data);
        }

        /// <summary>
        /// 使用済みの弾を再利用できる状態としてプールへ戻す。
        /// </summary>
        /// <param name="entry">返却する弾エントリ。</param>
        public void ReturnBullet(BulletPoolEntry entry)
        {
            if (entry == null ||
                entry.isAvailable ||
                entry.bullet3D == null ||
                entry.bullet3D.data == null)
            {
                return;
            }

            entry.isAvailable = true;
            GetAvailableQueue(entry.bullet3D.data).Enqueue(entry);
        }

        // 指定された弾データから新しい弾オブジェクトとプールエントリを作成する。
        // data: 生成に使う弾データ。
        // 戻り値: 作成した弾エントリ。
        private BulletPoolEntry CreateEntry(BulletData data)
        {
            GameObject obj =
                Instantiate(data.bulletPrefab);

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
                    isAvailable = false
                };

            bulletComponent.InitializePool(this, poolEntry);

            pool.Add(poolEntry);

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
