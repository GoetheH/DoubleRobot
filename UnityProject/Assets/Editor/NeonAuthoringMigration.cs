using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// One-time migration only. Existing layouts/configurations/images are never regenerated.
public static class NeonAuthoringMigration
{
    const string Root="Assets/Resources/Authoring";
    public static void CreateMissingAssets()
    {
        Directory.CreateDirectory(Root+"/Levels");Directory.CreateDirectory(Root+"/Art");AssetDatabase.Refresh();
        var balance=AssetDatabase.LoadAssetAtPath<NeonBalance>(Root+"/Balance.asset");
        if(!balance){balance=CreateBalance();AssetDatabase.CreateAsset(balance,Root+"/Balance.asset");}
        var campaign=AssetDatabase.LoadAssetAtPath<NeonCampaign>(Root+"/Campaign.asset");
        if(!campaign){campaign=ScriptableObject.CreateInstance<NeonCampaign>();campaign.balance=balance;AssetDatabase.CreateAsset(campaign,Root+"/Campaign.asset");}
        Sprite square=CreateSquare();
        for(int i=1;i<=8;i++)
        {
            string path=Root+"/Levels/Sector"+i.ToString("00")+".prefab";
            var level=AssetDatabase.LoadAssetAtPath<StageLayout>(path);
            if(!level)level=CreateLevel(i,path,square);
            if(!campaign.levels[i-1])campaign.levels[i-1]=level;
        }
        EditorUtility.SetDirty(campaign);AssetDatabase.SaveAssets();AssetDatabase.Refresh();
        BakeMissingArt(campaign);
        foreach(string image in Directory.GetFiles(Root+"/Art","*.png"))
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(image.Replace('\\','/'));
            if(importer.textureType==TextureImporterType.Default&&importer.npotScale!=TextureImporterNPOTScale.None){importer.npotScale=TextureImporterNPOTScale.None;importer.SaveAndReimport();}
        }
        campaign=EnsureMainWorkspace();
        foreach(var level in campaign.levels)level.ValidateData();campaign.balance.ValidateData();
        AssetDatabase.SaveAssets();
        Debug.Log("AUTHORING_READY: 8 editable prefabs, balance/campaign assets, persistent PNG art and Main workspace.");
    }
    static NeonBalance CreateBalance()
    {
        var b=ScriptableObject.CreateInstance<NeonBalance>();
        string[] names={"脉冲步枪","裂片霰弹","磁轨狙击枪","重型机枪","蜂群冲锋枪"};
        int[] damages={16,13,100,11,8};float[] rates={.145f,.65f,1.05f,.085f,.065f},speed={28,22,55,30,23},life={1.5f,.55f,1.6f,1.4f,.72f},reach={.95f,1.05f,1.45f,1.2f,.72f},width={1.35f,1.48f,1.98f,1.6f,1.02f};
        Color[] guns={new Color(.22f,.95f,.88f),new Color(1,.78f,.25f),new Color(1,.25f,.48f),new Color(.65f,.86f,1),new Color(.72f,.55f,1)};
        for(int i=0;i<5;i++)b.guns[i]=new NeonBalance.Gun{name=names[i],damage=damages[i],interval=rates[i],speed=speed[i],lifetime=life[i],pellets=i==1?7:1,pelletAngle=i==1?7:4,randomSpread=i==3?4:i==4?9:0,penetration=i==2,distanceFalloff=i==1,usesHeat=i==3,muzzleDistance=reach[i],displayWidth=width[i],color=guns[i]};
        string[] roles={"突击兵","医疗兵","工程师","侦察兵"};float[] duration={5,2,8,6},cooldown={12,14,15,14};
        Color[] colors={new Color(1,.56f,.18f),new Color(.25f,1,.55f),new Color(1,.8f,.2f),new Color(.72f,.42f,1)};
        for(int i=0;i<4;i++)b.roles[i]=new NeonBalance.Role{name=roles[i],duration=duration[i],cooldown=cooldown[i],moveSpeed=i==3?8.6f:7.4f,color=colors[i]};
        b.difficulties[0]=new NeonBalance.Difficulty{name="简单",health=.75f,damage=.7f,attackInterval=1.25f};
        b.difficulties[1]=new NeonBalance.Difficulty{name="中等"};
        b.difficulties[2]=new NeonBalance.Difficulty{name="困难",health=1.35f,damage=1.35f,attackInterval=.8f};
        return b;
    }
    static Sprite CreateSquare()
    {
        string path=Root+"/Art/PlacementSquare.png";
        if(!File.Exists(path)) {var t=new Texture2D(8,8);var p=new Color[64];for(int i=0;i<p.Length;i++)p[i]=Color.white;t.SetPixels(p);t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);AssetDatabase.ImportAsset(path);}
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spritePixelsPerUnit=8;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    public static StageElement AddElement(StageLayout root,StageElement.Kind kind,string name,Vector2 position,Vector2 size,Sprite square)
    {
        var go=new GameObject(name);go.transform.SetParent(root.transform,false);go.transform.localPosition=position;
        var e=go.AddComponent<StageElement>();e.kind=kind;
        Color c=kind==StageElement.Kind.Platform?new Color(.16f,.48f,.54f):kind==StageElement.Kind.PlayerSpawn?Color.green:kind==StageElement.Kind.Exit?Color.cyan:kind==StageElement.Kind.MissionItem?new Color(1,.64f,.1f):kind==StageElement.Kind.Terminal?new Color(.8f,.4f,1):Color.red;
        if(kind==StageElement.Kind.Platform) {go.transform.localScale=new Vector3(size.x,size.y,1);var box=go.AddComponent<BoxCollider2D>();box.size=Vector2.one;var r=go.AddComponent<SpriteRenderer>();r.sprite=square;r.color=c;r.sortingOrder=1;}
        else {var visual=new GameObject("摆放标识");visual.transform.SetParent(go.transform,false);visual.transform.localScale=new Vector3(size.x,size.y,1);if(kind==StageElement.Kind.Exit||kind==StageElement.Kind.PlayerSpawn)visual.transform.localPosition=Vector2.up*size.y/2;var r=visual.AddComponent<SpriteRenderer>();r.sprite=square;r.color=c;r.sortingOrder=2;}
        return e;
    }
    static StageLayout CreateLevel(int sector,string path,Sprite square)
    {
        string[] names={"","霓雨下城","熔炉生产线","核心守卫","天线高塔","数据回收","配电迷阵","地下档案","中枢停机"};
        var go=new GameObject("第"+sector+"关_"+names[sector]);var l=go.AddComponent<StageLayout>();l.sector=sector;l.displayName=names[sector];
        l.enemyQuota=sector==3||sector==8?1:6+sector*2;l.hasBoss=sector==3||sector==8;l.bossHealth=sector==8?2200:1250;l.autoFinish=sector==8;
        string[] images={"","NightCity","Foundry","Reactor","Spire","Archive","Foundry","Archive","Reactor"};
        l.background=Resources.Load<Texture2D>("ArtSlice/"+images[sector]);
        l.accent=sector==2?new Color(1,.48f,.17f):sector==4?new Color(.36f,.76f,1):sector==5?new Color(.3f,1,.77f):sector==6?new Color(1,.76f,.24f):sector==7?new Color(.46f,.8f,.68f):sector==8?new Color(1,.25f,.55f):new Color(.22f,.95f,.88f);
        Vector2 spawn=new Vector2(4,1.4f),exit=new Vector2(42,1.4f);
        l.mission="清除敌军后进入传送门";
        var platforms=new System.Collections.Generic.List<Rect>{new Rect(8,3.3f,4.4f,.32f),new Rect(16,4.9f,4,.32f),new Rect(25,3.5f,5,.32f),new Rect(35,5,4,.32f)};
        var items=new System.Collections.Generic.List<Vector2>();
        if(sector==4){l.mapWidth=24;l.mapHeight=32;l.goal=StageLayout.Goal.CollectItems;platforms.Clear();for(int i=0;i<10;i++)platforms.Add(new Rect(i%2==0?7:12,3.5f+i*2.5f,7,.32f));exit=new Vector2(15,26.32f);items.Add(new Vector2(9,24.4f));l.mission="攀登高塔，领取天线钥匙后到达顶部";}
        if(sector==5){l.goal=StageLayout.Goal.CollectItems;exit=new Vector2(3,1.4f);items.AddRange(new[]{new Vector2(10,4.2f),new Vector2(18,5.8f),new Vector2(37,5.9f)});l.mission="取得任务数据，再返回左侧传送门";}
        if(sector==6||sector==8){l.goal=sector==8?StageLayout.Goal.BossAndCircuit:StageLayout.Goal.Circuit;exit=new Vector2(22,sector==8?5.22f:1.4f);l.mission=sector==8?"击败守卫＋按提示顺序关闭反应堆，即可通关（靠近按E）":"按提示顺序接通配电（靠近按E，错误会重置）";Vector2[] points={new Vector2(10,4.1f),new Vector2(18,5.7f),new Vector2(37,5.8f)};for(int i=0;i<points.Length;i++)AddElement(l,StageElement.Kind.Terminal,"机关_"+(i+1),points[i],new Vector2(.7f,.8f),square).order=i;if(sector==8)platforms.Add(new Rect(20,4.9f,5,.32f));}
        if(sector==7){l.goal=StageLayout.Goal.CollectItems;l.mapWidth=26;l.mapHeight=30;platforms.Clear();for(int i=0;i<9;i++)platforms.Add(new Rect(i%2==0?4:13,3.5f+i*2.5f,8,.32f));spawn=new Vector2(8,23.82f);exit=new Vector2(5,1.4f);items.AddRange(new[]{new Vector2(7,14.1f),new Vector2(17,6.6f)});l.mission="下行收集档案，进入底层左侧传送门（S穿过平台）";}
        int n=0;foreach(var r in platforms)AddElement(l,StageElement.Kind.Platform,"平台_"+(++n).ToString("00"),r.center,r.size,square);
        AddElement(l,StageElement.Kind.PlayerSpawn,"玩家出生点",spawn,new Vector2(.5f,1.6f),square);
        AddElement(l,StageElement.Kind.Exit,"关卡出口",exit,new Vector2(1.2f,3.1f),square);
        for(int i=0;i<items.Count;i++)AddElement(l,StageElement.Kind.MissionItem,"任务道具_"+(i+1),items[i],Vector2.one*.4f,square);
        if(l.hasBoss)AddElement(l,StageElement.Kind.BossSpawn,"Boss出生点",new Vector2(28,5.8f),new Vector2(3,2.7f),square);
        var ground=new GameObject("地面预览（高度在关卡根对象配置）");ground.transform.SetParent(go.transform,false);ground.transform.localPosition=new Vector2(l.mapWidth/2,l.floorHeight-.3f);ground.transform.localScale=new Vector3(l.mapWidth,.6f,1);var renderer=ground.AddComponent<SpriteRenderer>();renderer.sprite=square;renderer.color=new Color(.1f,.18f,.24f);
        l.ValidateData();PrefabUtility.SaveAsPrefabAsset(go,path);UnityEngine.Object.DestroyImmediate(go);return AssetDatabase.LoadAssetAtPath<StageLayout>(path);
    }
    static void BakeMissingArt(NeonCampaign c)
    {
        var host=new GameObject("OneTimeArtMigration");host.hideFlags=HideFlags.HideAndDontSave;
        try
        {
            var run=host.AddComponent<NeonRun>();Type type=typeof(NeonRun);BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
            type.GetMethod("LoadAuthoring",flags).Invoke(run,null);type.GetMethod("MakeModels",flags).Invoke(run,null);
            Texture2D Get(string field)=>(Texture2D)type.GetField(field,flags).GetValue(run);
            Texture2D[] GetArray(string field)=>(Texture2D[])type.GetField(field,flags).GetValue(run);
            for(int i=0;i<4;i++) {if(!c.balance.roles[i].torso)c.balance.roles[i].torso=SaveImage(GetArray("sliceTorso")[i],"Role"+i+"-Torso");if(!c.balance.roles[i].head)c.balance.roles[i].head=SaveImage(GetArray("sliceHead")[i],"Role"+i+"-Head");if(!c.portraitModels[i])c.portraitModels[i]=SaveImage(GetArray("classModels")[i],"Role"+i+"-Preview");}
            for(int i=0;i<5;i++)if(!c.balance.guns[i].artwork)c.balance.guns[i].artwork=SaveImage(GetArray("gunModels")[i],"Gun"+i);
            if(!c.street)c.street=SaveImage(Get("streetTile"),"Street");if(!c.catwalk)c.catwalk=SaveImage(Get("catwalkTile"),"Catwalk");if(!c.drone)c.drone=SaveImage(Get("sliceDrone"),"Drone");if(!c.boss)c.boss=SaveImage(Get("sentinelModel"),"Sentinel");if(!c.guardTorso)c.guardTorso=SaveImage(Get("guardTorso"),"Guard-Torso");if(!c.guardHead)c.guardHead=SaveImage(Get("guardHead"),"Guard-Head");
            EditorUtility.SetDirty(c);EditorUtility.SetDirty(c.balance);AssetDatabase.SaveAssets();
        }
        finally{UnityEngine.Object.DestroyImmediate(host);}
    }
    static Texture2D SaveImage(Texture2D texture,string name)
    {
        string path=Root+"/Art/"+name+".png";if(!File.Exists(path))File.WriteAllBytes(path,texture.EncodeToPNG());AssetDatabase.ImportAsset(path);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Default;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.npotScale=TextureImporterNPOTScale.None;importer.isReadable=true;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.wrapMode=TextureWrapMode.Repeat;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
    static NeonCampaign EnsureMainWorkspace()
    {
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
        // OpenScene unloads unreferenced native prefab objects; reacquire them after it.
        var c=AssetDatabase.LoadAssetAtPath<NeonCampaign>(Root+"/Campaign.asset");
        if(!UnityEngine.Object.FindObjectOfType<StageWorkspace>())
        {
            var go=new GameObject("关卡编辑工作台");var w=go.AddComponent<StageWorkspace>();w.campaign=c;
            var instance=(GameObject)PrefabUtility.InstantiatePrefab(c.levels[0].gameObject,go.transform);w.editedLevel=instance.GetComponent<StageLayout>();
            EditorSceneManager.SaveScene(scene);
        }
        return c;
    }
}
