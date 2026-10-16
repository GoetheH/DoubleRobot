using System;
using UnityEngine;

public partial class NeonRun
{
    NeonCampaign campaign;
    NeonBalance balance;
    StageLayout activeLayout;
    StageWorkspace workspace;
    int missionItemTotal;
    float Floor => activeLayout?activeLayout.floorHeight:1.4f;
    NeonBalance.Role CurrentRole => balance.roles[profession];
    NeonBalance.Gun CurrentGun => balance.guns[weapon];
    int[] CircuitOrder => activeLayout.switchOrder;
    bool CircuitComplete => sequence>=CircuitOrder.Length;
    void LoadAuthoring()
    {
        workspace=FindObjectOfType<StageWorkspace>();
        campaign=workspace&&workspace.campaign?workspace.campaign:Resources.Load<NeonCampaign>("Authoring/Campaign");
        if(!campaign||!campaign.balance||campaign.levels.Length!=8) throw new InvalidOperationException("缺少战役配置：Resources/Authoring/Campaign。");
        balance=campaign.balance;balance.ValidateData();
        for(int i=0;i<5;i++) {var g=balance.guns[i];weaponNames[i]=g.name;GunColors[i]=g.color;GunReach[i]=g.muzzleDistance;GunWidth[i]=g.displayWidth;}
        for(int i=0;i<4;i++) {jobNames[i]=balance.roles[i].name;ClassColors[i]=balance.roles[i].color;}
        for(int i=0;i<3;i++)difficultyNames[i]=balance.difficulties[i].name;
        for(int i=1;i<=8;i++)stageNames[i]=i.ToString("00")+" / "+campaign.levels[i-1].displayName;
    }
    int InitialSector()
    {
#if UNITY_EDITOR
        if(workspace&&workspace.testEditedLevel&&workspace.editedLevel)return workspace.editedLevel.sector;
#endif
        return 1;
    }
    void ReadAuthoredLevel(int sector)
    {
        if(activeLayout) {activeLayout.gameObject.SetActive(false);Destroy(activeLayout.gameObject);}
        StageLayout source=campaign.levels[sector-1];
#if UNITY_EDITOR
        if(workspace&&workspace.editedLevel&&workspace.editedLevel.sector==sector)source=workspace.editedLevel;
#endif
        source.ValidateData();
        activeLayout=Instantiate(source);activeLayout.name="运行关卡 / "+source.displayName;
        // The gameplay presentation reads these actual objects, while Scene view keeps
        // their SpriteRenderers and colliders available for placement and inspection.
        foreach(var r in activeLayout.GetComponentsInChildren<Renderer>())r.enabled=false;
        foreach(var c in activeLayout.GetComponentsInChildren<Collider2D>())c.enabled=false;
        mapWidth=source.mapWidth;mapHeight=source.mapHeight;quota=source.enemyQuota;mission=source.mission;
        spawnAt=clock+source.firstWaveDelay;
        var terminals=new System.Collections.Generic.SortedDictionary<int,Vector2>();
        foreach(var e in activeLayout.Elements)
        {
            Vector2 p=e.transform.position;
            switch(e.kind)
            {
                case StageElement.Kind.Platform:ledges.Add(e.PlatformRect);break;
                case StageElement.Kind.PlayerSpawn:player=p;break;
                case StageElement.Kind.Exit:exit=p;break;
                case StageElement.Kind.MissionItem:missionItems.Add(p);break;
                case StageElement.Kind.Terminal:terminals.Add(e.order,p);break;
                case StageElement.Kind.BossSpawn:
                    if(source.hasBoss) {float hp=source.bossHealth*EnemyHealthScale;enemies.Add(new Foe{p=p,hp=hp,max=hp,kind=3,fire=clock+2*EnemyAttackIntervalScale});spawned++;}break;
                case StageElement.Kind.EnemySpawn:
                    float health=((e.enemyKind==2?95:48)+sector*12)*EnemyHealthScale;
                    enemies.Add(new Foe{p=p,hp=health,max=health,kind=e.enemyKind,fire=clock+1.3f*EnemyAttackIntervalScale});spawned++;break;
            }
        }
        foreach(var p in terminals.Values)switches.Add(p);
        missionItemTotal=missionItems.Count;
    }
    string RoleSkillText(int role)
    {
        var r=balance.roles[role];string duration=r.duration.ToString("0.#"),cooldown=r.cooldown.ToString("0.#");
        string body=role==0?"双枪过载：幻化副枪，同步开火"+duration+"秒":role==1?"急救：回复"+r.healing+"装甲，无敌"+duration+"秒":role==2?"炮台：自动索敌"+duration+"秒":"弱点扫描：标记"+r.scanRadius.ToString("0.#")+"米内敌人"+duration+"秒，伤害+"+((r.scanMultiplier-1)*100).ToString("0")+"%";
        return body+" / 冷却"+cooldown+"秒";
    }
    void ApplyArtOverrides()
    {
        for(int i=0;i<5;i++)if(balance.guns[i].artwork)gunModels[i]=balance.guns[i].artwork;
        for(int i=0;i<4;i++) {if(balance.roles[i].torso)sliceTorso[i]=balance.roles[i].torso;if(balance.roles[i].head)sliceHead[i]=balance.roles[i].head;}
        var scout=balance.roles[3];roleDescriptions[3]="紫色侦察机体。移动更快，扫描施放时"+scout.scanRadius.ToString("0.#")+"米内的敌人并追踪其弱点，持续"+scout.duration.ToString("0.#")+"秒；枪械伤害提高"+((scout.scanMultiplier-1)*100).ToString("0")+"%，可标记Boss。";
        gunTraits[1]=balance.guns[1].pellets+"弹丸扇形 / 近战爆发 / 距离衰减";
        weaponDescriptions[1]="双管枪口与前置泵柄，一次射出"+balance.guns[1].pellets+"枚弹丸，近距离集中命中可形成爆发。";
    }
    void LoadEditableArt()
    {
        for(int i=0;i<4;i++)
        {
            classModels[i]=campaign.portraitModels[i];sliceTorso[i]=balance.roles[i].torso;sliceHead[i]=balance.roles[i].head;
            if(!classModels[i]||!sliceTorso[i]||!sliceHead[i])throw new InvalidOperationException("缺少职业图片，请检查战斗/战役配置。");
        }
        for(int i=0;i<5;i++) {gunModels[i]=balance.guns[i].artwork;if(!gunModels[i])throw new InvalidOperationException("缺少枪械图片。");}
        streetTile=campaign.street;catwalkTile=campaign.catwalk;sliceDrone=campaign.drone;sentinelModel=campaign.boss;
        guardTorso=campaign.guardTorso;guardHead=campaign.guardHead;
        if(!streetTile||!catwalkTile||!sliceDrone||!sentinelModel||!guardTorso||!guardHead)throw new InvalidOperationException("缺少场景/敌人美术资源。");
        nightCity=campaign.levels[0].background;foundryScene=campaign.levels[1].background;spireScene=campaign.levels[3].background;archiveScene=campaign.levels[4].background;reactorScene=campaign.levels[7].background;
        robotArt=classModels[0];
    }
    string TerminalName(int index) {return index<3?new[]{"I","II","III"}[index]:(index+1).ToString();}
    string OrderText() {return string.Join(" → ",Array.ConvertAll(CircuitOrder,TerminalName));}
    string GunStatsText(int gun)
    {
        var g=balance.guns[gun];return "基础伤害 "+g.damage+(g.pellets>1?" × "+g.pellets+"弹丸":"")+" / 间隔 "+g.interval.ToString("0.###")+"秒"+(g.penetration?" / 穿透":"")+(g.usesHeat?" / 过热冷却":"");
    }
    void DrawAuthoredProps()
    {
        if(!activeLayout)return;
        foreach(var e in activeLayout.Elements)
        {
            if(e.kind==StageElement.Kind.Platform&&e.platformArtwork)
            {
                Rect r=e.PlatformRect;Vector2 p=ScreenPoint(new Vector2(r.x,r.yMax));
                GUI.DrawTexture(new Rect(p.x,p.y,r.width*60,r.height*60+8),e.platformArtwork,ScaleMode.StretchToFill);
            }
            if(e.kind!=StageElement.Kind.Decoration)continue;
            var renderer=e.GetComponent<SpriteRenderer>();if(!renderer||!renderer.sprite)continue;
            var sprite=renderer.sprite;Vector3 scale=e.transform.lossyScale;
            Vector2 center=ScreenPoint(e.transform.TransformPoint(sprite.bounds.center));
            Vector2 size=Vector2.Scale(sprite.bounds.size,new Vector2(Mathf.Abs(scale.x),Mathf.Abs(scale.y)))*60;
            Rect uv=sprite.rect;uv=new Rect(uv.x/sprite.texture.width,uv.y/sprite.texture.height,uv.width/sprite.texture.width,uv.height/sprite.texture.height);
            if(renderer.flipX^(scale.x<0)){uv.x+=uv.width;uv.width=-uv.width;}if(renderer.flipY^(scale.y<0)){uv.y+=uv.height;uv.height=-uv.height;}
            Matrix4x4 saved=GUI.matrix;GUI.matrix=saved*LocalRotation(center,-e.transform.eulerAngles.z);
            GUI.color=renderer.color;GUI.DrawTextureWithTexCoords(new Rect(-size.x/2,-size.y/2,size.x,size.y),sprite.texture,uv);GUI.color=Color.white;GUI.matrix=saved;
        }
    }
}
