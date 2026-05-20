using UnityEngine;

/// <summary>
/// 素材アイテムの所持上限数を定義するScriptableObject。
/// </summary>
[CreateAssetMenu(menuName = "Material Data")]
public class MaterialData : ItemData
{
    // インベントリで重ねて持てる最大数。
    public int stackLimit = 99;
}
