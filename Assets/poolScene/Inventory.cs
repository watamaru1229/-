using System.Collections.Generic;
using PoolScene.ItemData.WeaponData;
using UnityEngine;

namespace PoolScene
{
    /// <summary>
    /// 取得した通常アイテムと武器インスタンスを保持する簡易インベントリ。
    /// </summary>
    public class Inventory : MonoBehaviour
    {
        // 他のクラスからインベントリへ追加するための共有インスタンス。
        public static Inventory Instance;

        // 取得済みの通常アイテム一覧。
        public List<global::ItemData> items =
            new List<global::ItemData>(32);

        // 取得済みの武器インスタンス一覧。
        public List<WeaponInstance> weapons =
            new List<WeaponInstance>(32);

        // シーン内のInventoryを共有インスタンスとして登録する。
        void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// 通常アイテムをインベントリに追加し、取得ポップアップを表示する。
        /// </summary>
        /// <param name="item">追加するアイテムデータ。</param>
        public void AddItem(global::ItemData item)
        {
            items.Add(item);

            ItemPickupPopup.ShowItem(item);

            Debug.Log(item.itemName +
                      " added to inventory");
        }

        /// <summary>
        /// 武器インスタンスをインベントリに追加し、取得ポップアップを表示する。
        /// </summary>
        /// <param name="weapon">追加する武器インスタンス。</param>
        public void AddWeapon(WeaponInstance weapon)
        {
            if (weapon == null)
            {
                return;
            }

            weapons.Add(weapon);

            ItemPickupPopup.ShowWeapon(weapon);

            Debug.Log(weapon.displayName +
                      " added to inventory");
        }
    }
}
