using UnityEngine;

public class StageWorkspace : MonoBehaviour
{
    public NeonCampaign campaign;
    [Tooltip("主场景中可编辑的关卡预制体实例。未应用的场景覆盖也会用于本机Play测试。")]
    public StageLayout editedLevel;
    [Tooltip("仅Unity编辑器中生效：从当前编辑关开始测试。独立试玩始终从第一关开始。")]
    public bool testEditedLevel;
}
