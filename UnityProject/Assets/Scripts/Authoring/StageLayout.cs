using System;
using UnityEngine;

[ExecuteAlways,DisallowMultipleComponent]
public class StageLayout : MonoBehaviour
{
    public enum Goal { ClearEnemies, CollectItems, Circuit, BossAndCircuit }
    [Range(1,8)] public int sector=1;
    public string displayName;
    [TextArea(2,4)] public string mission;
    public Goal goal;
    public bool autoFinish;
    [Min(1)] public float mapWidth=44, mapHeight=12;
    public float floorHeight=1.4f;
    [Min(0)] public int enemyQuota=8;
    [Min(.1f)] public float firstWaveDelay=1.2f, waveInterval=1.8f;
    public bool hasBoss;
    [Min(1)] public float bossHealth=1250;
    [Tooltip("按机关的order编号填写；例如 1,0,2 表示 II→I→III。")]
    public int[] switchOrder={1,0,2};
    public Texture2D background;
    public Color accent=new Color(.22f,.95f,.88f);

    public StageElement[] Elements => GetComponentsInChildren<StageElement>(false);
    void Update()
    {
        if(Application.isPlaying)return;
        Transform ground=transform.Find("地面预览（高度在关卡根对象配置）");
        if(!ground)return;
        Vector3 p=new Vector3(mapWidth/2,floorHeight-.3f,0),s=new Vector3(mapWidth,.6f,1);
        if(ground.localPosition!=p)ground.localPosition=p;if(ground.localScale!=s)ground.localScale=s;
    }
    public void ValidateData()
    {
        if(transform.position.sqrMagnitude>.0001f||Vector3.Distance(transform.lossyScale,Vector3.one)>.001f||Quaternion.Angle(transform.rotation,Quaternion.identity)>.01f)throw new InvalidOperationException("关卡根对象应保持原点、单位缩放和零旋转；请移动/缩放内部部件。");
        int spawn=0,exit=0,items=0,boss=0;var terminals=new System.Collections.Generic.HashSet<int>();
        foreach(var e in Elements)
        {
            if(e.kind==StageElement.Kind.PlayerSpawn) spawn++;
            if(e.kind==StageElement.Kind.Exit)exit++;
            if(e.kind==StageElement.Kind.MissionItem)items++;
            if(e.kind==StageElement.Kind.BossSpawn)boss++;
            if(e.kind==StageElement.Kind.Terminal&&!terminals.Add(e.order))throw new InvalidOperationException("机关编号重复："+e.order);
            if(e.kind==StageElement.Kind.Platform&&(Mathf.Abs(e.transform.eulerAngles.z)>.01f||e.PlatformRect.width<=0||e.PlatformRect.height<=0))throw new InvalidOperationException("平台必须轴对齐且有正尺寸。");
        }
        if(spawn!=1||(!autoFinish&&exit!=1)||hasBoss&&boss!=1)throw new InvalidOperationException("关卡需要一个出生点、一个出口（自动结算除外）及Boss出生点（启用Boss时）。");
        if(goal==Goal.CollectItems&&items==0)throw new InvalidOperationException("收集关必须放置任务道具。");
        if(goal==Goal.Circuit||goal==Goal.BossAndCircuit)
        {
            if(switchOrder==null||switchOrder.Length==0||switchOrder.Length!=terminals.Count)throw new InvalidOperationException("机关顺序必须包含所有机关且无重复。");
            var seen=new System.Collections.Generic.HashSet<int>();
            foreach(int i in switchOrder)if(!terminals.Contains(i)||!seen.Add(i)||i<0||i>=terminals.Count)throw new InvalidOperationException("机关编号须为0至数量-1，顺序不可重复或缺失。");
        }
        if(mapWidth<=0||mapHeight<=floorHeight)throw new InvalidOperationException("地图尺寸无效。");
    }
}
