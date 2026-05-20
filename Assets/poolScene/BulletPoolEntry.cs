using UnityEngine;

namespace poolScene
{
    /// <summary>
    /// 弾プール内で1つの弾オブジェクトと利用状態を保持する設定クラス。
    /// </summary>
    [System.Serializable]
    public class BulletPoolEntry
    {
        // プールで管理している弾のGameObject。
        public GameObject gameObject;

        // 弾の動作コンポーネント。
        public Bullet3D bullet3D;

        // 現在この弾が再利用可能かどうか。
        public bool isAvailable;
    }
}
