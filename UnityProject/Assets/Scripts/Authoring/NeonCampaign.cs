using UnityEngine;

[CreateAssetMenu(menuName="霓虹突围/战役配置",fileName="Campaign")]
public class NeonCampaign : ScriptableObject
{
    public NeonBalance balance;
    [Tooltip("八关预制体；双击对应Prefab即可拖动平台、出生点、出口、任务道具和机关。")]
    public StageLayout[] levels=new StageLayout[8];
    [Header("可替换的像素美术资产")]
    public Texture2D street, catwalk, drone, boss, guardTorso, guardHead;
    public Texture2D[] portraitModels=new Texture2D[4];
}
