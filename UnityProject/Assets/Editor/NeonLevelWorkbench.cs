using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class NeonLevelWorkbench : EditorWindow
{
    NeonCampaign campaign;int levelIndex;StageElement.Kind addKind=StageElement.Kind.Platform;Vector2 scroll;
    [MenuItem("霓虹突围/关卡工作台")]
    public static void Open(){GetWindow<NeonLevelWorkbench>("霓虹突围 · 关卡工作台").Show();}
    void OnEnable(){campaign=Resources.Load<NeonCampaign>("Authoring/Campaign");SceneView.duringSceneGui+=DrawLabels;}
    void OnDisable(){SceneView.duringSceneGui-=DrawLabels;}
    void OnGUI()
    {
        scroll=EditorGUILayout.BeginScrollView(scroll);
        EditorGUILayout.LabelField("可视化关卡编辑",EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("在Scene视图用W拖动对象、R缩放平台，或在Inspector修改Transform。修改完成后点击“保存关卡”。关卡数据来自Prefab，战斗数值与贴图来自配置资产。",MessageType.Info);
        campaign=(NeonCampaign)EditorGUILayout.ObjectField("战役配置",campaign,typeof(NeonCampaign),false);
        if(!campaign){EditorGUILayout.HelpBox("缺少Campaign.asset，请从版本库恢复配置资产。",MessageType.Error);EditorGUILayout.EndScrollView();return;}
        if(GUILayout.Button("打开战斗数值与职业/枪械贴图配置")){Selection.activeObject=campaign.balance;EditorGUIUtility.PingObject(campaign.balance);}
        if(GUILayout.Button("打开战役美术配置")){Selection.activeObject=campaign;EditorGUIUtility.PingObject(campaign);}
        string[] names=Array.ConvertAll(campaign.levels,l=>l?l.sector.ToString("00")+" / "+l.displayName:"缺少关卡");
        levelIndex=EditorGUILayout.Popup("编辑关卡",levelIndex,names);
        using(new EditorGUI.DisabledScope(EditorApplication.isPlaying))
        {
            if(GUILayout.Button("载入到Scene视图"))LoadLevel(levelIndex);
            var w=UnityEngine.Object.FindObjectOfType<StageWorkspace>();
            if(w&&w.editedLevel)
            {
                EditorGUILayout.Space();EditorGUILayout.LabelField("当前："+w.editedLevel.displayName,EditorStyles.boldLabel);
                if(GUILayout.Button("选中关卡根对象（地图/任务/背景/刷怪）"))Selection.activeObject=w.editedLevel.gameObject;
                if(GUILayout.Button("保存关卡：应用到Prefab并保存场景"))SaveLevel(w);
                addKind=(StageElement.Kind)EditorGUILayout.EnumPopup("添加对象类型",addKind);
                if(GUILayout.Button("添加对象（Scene视图中心）"))AddPart(w,addKind);
                if(GUILayout.Button("检查布局与任务配置"))
                {
                    try{w.editedLevel.ValidateData();campaign.balance.ValidateData();EditorUtility.DisplayDialog("检查通过","出生点、出口、平台和任务配置有效。修改跳跃参数或平台高度后仍需试玩路线。","确定");}
                    catch(Exception e){EditorUtility.DisplayDialog("配置需要修正",e.Message,"确定");}
                }
                if(GUILayout.Button("试玩当前编辑关（仅编辑器）"))
                {
                    try{w.editedLevel.ValidateData();Undo.RecordObject(w,"切换关卡测试");w.testEditedLevel=true;EditorSceneManager.MarkSceneDirty(w.gameObject.scene);if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())EditorApplication.isPlaying=true;}
                    catch(Exception e){EditorUtility.DisplayDialog("无法试玩",e.Message,"确定");}
                }
                if(w.testEditedLevel&&GUILayout.Button("恢复从第一关开始测试")){Undo.RecordObject(w,"恢复战役入口");w.testEditedLevel=false;EditorSceneManager.MarkSceneDirty(w.gameObject.scene);}
            }
        }
        EditorGUILayout.Space();EditorGUILayout.HelpBox("平台：青色；出生点：绿色；出口：蓝绿；任务道具：金色；机关：紫色；敌人/Boss：红色。\n复制对象：Ctrl+D；删除：Delete；撤销：Ctrl+Z。\n装饰物可添加SpriteRenderer，再指定Sprite和Color。平台可指定platformArtwork；背景在关卡根对象的background中更换。",MessageType.None);
        EditorGUILayout.EndScrollView();
    }
    void LoadLevel(int index)
    {
        var w=UnityEngine.Object.FindObjectOfType<StageWorkspace>();
        if(w&&w.editedLevel&&PrefabUtility.HasPrefabInstanceAnyOverrides(w.editedLevel.gameObject,false))
        {
            int choice=EditorUtility.DisplayDialogComplex("当前关卡有未应用的布局修改","切换前可保存到该关卡Prefab。","保存关卡","取消","放弃布局修改");
            if(choice==1)return;if(choice==0&&!SaveLevel(w))return;
        }
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");w=UnityEngine.Object.FindObjectOfType<StageWorkspace>();
        campaign=Resources.Load<NeonCampaign>("Authoring/Campaign");
        if(!w)throw new InvalidOperationException("Main场景缺少StageWorkspace。");
        if(w.editedLevel)Undo.DestroyObjectImmediate(w.editedLevel.gameObject);
        var instance=(GameObject)PrefabUtility.InstantiatePrefab(campaign.levels[index].gameObject,w.transform);Undo.RegisterCreatedObjectUndo(instance,"载入关卡");
        Undo.RecordObject(w,"切换编辑关卡");w.editedLevel=instance.GetComponent<StageLayout>();w.testEditedLevel=false;EditorSceneManager.MarkSceneDirty(w.gameObject.scene);
        Selection.activeGameObject=instance;SceneView.lastActiveSceneView?.Frame(new Bounds(new Vector3(w.editedLevel.mapWidth/2,w.editedLevel.mapHeight/2,0),new Vector3(w.editedLevel.mapWidth,w.editedLevel.mapHeight,1)),false);
        if(SceneView.lastActiveSceneView)SceneView.lastActiveSceneView.in2DMode=true;
    }
    static bool SaveLevel(StageWorkspace w)
    {
        try{w.editedLevel.ValidateData();PrefabUtility.ApplyPrefabInstance(w.editedLevel.gameObject,InteractionMode.UserAction);EditorSceneManager.SaveScene(w.gameObject.scene);AssetDatabase.SaveAssets();return true;}
        catch(Exception e){EditorUtility.DisplayDialog("保存失败",e.Message,"确定");return false;}
    }
    static void AddPart(StageWorkspace w,StageElement.Kind kind)
    {
        Vector3 p=SceneView.lastActiveSceneView?SceneView.lastActiveSceneView.pivot:new Vector3(10,4,0);p.z=0;p.x=Mathf.Clamp(p.x,1,w.editedLevel.mapWidth-1);p.y=Mathf.Clamp(p.y,w.editedLevel.floorHeight,w.editedLevel.mapHeight-1);
        var square=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Authoring/Art/PlacementSquare.png");
        var e=NeonAuthoringMigration.AddElement(w.editedLevel,kind,kind+"_新增",p,kind==StageElement.Kind.Platform?new Vector2(4,.32f):Vector2.one*.6f,square);
        if(kind==StageElement.Kind.Terminal){int next=0;foreach(var t in w.editedLevel.Elements)if(t!=e&&t.kind==kind)next=Mathf.Max(next,t.order+1);e.order=next;}
        if(kind==StageElement.Kind.Decoration)
        {
            foreach(Transform child in e.transform)UnityEngine.Object.DestroyImmediate(child.gameObject);
            e.gameObject.AddComponent<SpriteRenderer>().sprite=square;
        }
        Undo.RegisterCreatedObjectUndo(e.gameObject,"添加关卡部件");Selection.activeGameObject=e.gameObject;EditorSceneManager.MarkSceneDirty(w.gameObject.scene);
    }
    static void DrawLabels(SceneView view)
    {
        if(EditorApplication.isPlaying)return;
        foreach(var root in UnityEngine.Object.FindObjectsOfType<StageLayout>())
        {
            Handles.color=new Color(.2f,.8f,.9f,.3f);Handles.DrawWireCube(new Vector3(root.mapWidth/2,root.mapHeight/2,0),new Vector3(root.mapWidth,root.mapHeight,0));
            foreach(var e in root.Elements)
            {
                if(e.kind==StageElement.Kind.Platform){Rect r=e.PlatformRect;Handles.color=Color.cyan;Handles.DrawLine(new Vector3(r.x,r.yMax,0),new Vector3(r.xMax,r.yMax,0));}
                Handles.Label(e.transform.position+Vector3.up*.4f,e.name+(e.kind==StageElement.Kind.Terminal?" [编号 "+e.order+"]":""));
            }
        }
    }
}

[CustomEditor(typeof(StageWorkspace))]
public class StageWorkspaceInspector : Editor
{
    public override void OnInspectorGUI(){DrawDefaultInspector();if(GUILayout.Button("打开关卡工作台"))NeonLevelWorkbench.Open();EditorGUILayout.HelpBox("保存场景覆盖可用于本机测试；点击工作台的“保存关卡”才能将布局写入共享Prefab。发布构建读取Prefab，不读取临时场景覆盖。",MessageType.Info);}
}
