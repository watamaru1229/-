using UnityEngine;

namespace PoolScene
{
    /// <summary>
    /// 生成された爆発エフェクトを一定時間後に自動削除するクラス。
    /// </summary>
    public class ExplosionAutoDestroy : MonoBehaviour
    {
        // エフェクト生成から1秒後に自身を破棄する。
        void Start()
        {
            Destroy(gameObject, 1f);
        }
    }
}
