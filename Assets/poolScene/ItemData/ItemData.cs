using UnityEngine;

/// <summary>
/// アイテムの希少度を表す列挙型。
/// </summary>
public enum Rarity
{
    // 通常アイテム。
    Common,

    // やや希少なアイテム。
    Rare,

    // 高レアアイテム。
    Epic,

    // 最高レアアイテム。
    Legendary
}


/// <summary>
/// すべてのアイテムデータに共通する名前、アイコン、説明、レアリティを持つ基底ScriptableObject。
/// </summary>
public class ItemData : ScriptableObject
{
    // 画面表示やログに使うアイテム名。
    public string itemName;

    // UIに表示するアイコン画像。
    public Sprite icon;

    // アイテムの説明文。
    [TextArea] public string description;

    // アイテムの希少度。
    public Rarity rarity;
}
