using UnityEngine;

public partial class NeonRun
{
    Texture2D nightCity, streetTile, catwalkTile, sliceDrone;
    readonly Texture2D[] sliceTorso=new Texture2D[4], sliceHead=new Texture2D[4];
    Texture2D guardTorso, guardHead;
    static readonly Color DeepSteel=new Color(.09f,.14f,.2f), Rust=new Color(.57f,.2f,.24f), TealLight=new Color(.26f,.91f,.94f);
    void Poly(Texture2D texture, Vector2[] points, Color color)
    {
        for(int y=0;y<texture.height;y++) for(int x=0;x<texture.width;x++)
        {
            bool inside=false; float px=x+.5f,py=y+.5f;
            for(int i=0,j=points.Length-1;i<points.Length;j=i++)
                if((points[i].y>py)!=(points[j].y>py) && px<(points[j].x-points[i].x)*(py-points[i].y)/(points[j].y-points[i].y)+points[i].x) inside=!inside;
            if(inside) Pixels(texture,x,y,1,1,color);
        }
    }
    Texture2D Tunic(int role, bool enemy=false)
    {
        var t=NewModel(32,40);
        Color cloth=enemy?new Color(.35f,.12f,.22f):role==0?new Color(.25f,.18f,.27f):role==1?new Color(.62f,.75f,.73f):role==2?new Color(.43f,.32f,.18f):new Color(.22f,.14f,.34f);
        Poly(t,new[]{new Vector2(9,0),new Vector2(23,0),new Vector2(31,10),new Vector2(25,21),new Vector2(28,39),new Vector2(6,38),new Vector2(8,23),new Vector2(1,10)},new Color(.035f,.065f,.11f));
        Poly(t,new[]{new Vector2(10,2),new Vector2(22,2),new Vector2(28,11),new Vector2(22,22),new Vector2(25,36),new Vector2(8,35),new Vector2(11,21),new Vector2(4,11)},cloth);
        Poly(t,new[]{new Vector2(5,11),new Vector2(11,4),new Vector2(16,8),new Vector2(13,24),new Vector2(11,35),new Vector2(8,34),new Vector2(11,19)},Color.Lerp(cloth,Color.white,.22f));
        Poly(t,new[]{new Vector2(22,4),new Vector2(28,11),new Vector2(22,22),new Vector2(25,36),new Vector2(18,35),new Vector2(19,12)},Color.Lerp(cloth,Color.black,.4f));
        Pixels(t,13,4,7,3,DeepSteel); Pixels(t,15,7,3,20,new Color(.11f,.17f,.23f));
        Pixels(t,8,27,17,3,new Color(.04f,.07f,.12f)); Pixels(t,16,27,4,3,new Color(.64f,.6f,.47f));
        Pixels(t,20,10,4,2,enemy?Pink:TealLight); Pixels(t,21,12,2,4,enemy?Pink:TealLight);
        for(int i=0;i<5;i++) Pixels(t,7+i*3,31+i%2,1,2,new Color(.43f,.44f,.48f));
        if(role==1) { Pixels(t,7,13,7,2,ClassColors[1]); Pixels(t,10,10,2,8,ClassColors[1]); }
        if(role==2) {Pixels(t,7,22,17,3,ClassColors[2]*.7f); for(int i=0;i<3;i++) Plate(t,8+i*5,22,4,7,DeepSteel);}
        t.Apply(); return t;
    }
    Texture2D Helmet(int role, bool enemy=false)
    {
        var t=NewModel(24,24); Color shell=role==1?new Color(.66f,.78f,.8f):role==2?new Color(.67f,.48f,.2f):new Color(.12f,.18f,.26f);
        Poly(t,new[]{new Vector2(5,2),new Vector2(16,0),new Vector2(22,7),new Vector2(21,18),new Vector2(15,24),new Vector2(6,21),new Vector2(1,12)},new Color(.025f,.05f,.09f));
        Poly(t,new[]{new Vector2(6,4),new Vector2(15,2),new Vector2(20,8),new Vector2(19,18),new Vector2(14,21),new Vector2(7,19),new Vector2(3,12)},shell);
        Pixels(t,7,4,9,2,Color.Lerp(shell,Color.white,.35f));
        Poly(t,new[]{new Vector2(10,9),new Vector2(22,8),new Vector2(21,13),new Vector2(11,14),new Vector2(7,12)},enemy?new Color(1,.26f,.43f):TealLight);
        Pixels(t,12,9,7,1,new Color(.72f,1,1)); Pixels(t,11,16,8,2,DeepSteel); Pixels(t,13,19,4,2,new Color(.35f,.44f,.53f));
        Plate(t,2,10,5,7,new Color(.25f,.34f,.4f)); Pixels(t,3,12,2,2,enemy?Pink:Orange);
        if(role==2) { Plate(t,4,1,15,4,new Color(.83f,.6f,.2f)); Pixels(t,15,4,8,2,new Color(.87f,.75f,.4f)); }
        if(role==3) Poly(t,new[]{new Vector2(1,9),new Vector2(5,1),new Vector2(16,0),new Vector2(22,7),new Vector2(17,5),new Vector2(8,6),new Vector2(5,17)},new Color(.3f,.18f,.43f));
        t.Apply(); return t;
    }
    void MakeSliceArt()
    {
        nightCity=Resources.Load<Texture2D>("ArtSlice/NightCity");
        for(int role=0;role<4;role++) {sliceTorso[role]=Tunic(role); sliceHead[role]=Helmet(role);}
        guardTorso=Tunic(0,true); guardHead=Helmet(0,true);
        streetTile=NewModel(64,32); catwalkTile=NewModel(64,16);
        Pixels(streetTile,0,0,64,32,new Color(.055f,.075f,.11f));
        Plate(streetTile,1,4,61,26,new Color(.1f,.13f,.17f));
        Pixels(streetTile,0,0,64,2,new Color(.43f,.52f,.54f)); Pixels(streetTile,6,2,22,1,TealLight*.6f);
        Pixels(streetTile,3,12,58,1,DeepSteel); Pixels(streetTile,35,3,2,27,new Color(.035f,.05f,.07f));
        for(int i=0;i<18;i++) Pixels(streetTile,(i*17+3)%64,4+(i*7)%26,2+i%3,1,new Color(.17f,.2f,.23f));
        for(int x=3;x<64;x+=9) Pixels(streetTile,x,23,4,2,new Color(.03f,.05f,.07f));
        Pixels(catwalkTile,0,0,64,16,new Color(.045f,.065f,.1f)); Pixels(catwalkTile,0,0,64,2,new Color(.55f,.62f,.6f));
        Pixels(catwalkTile,0,3,64,2,new Color(.11f,.28f,.32f)); Pixels(catwalkTile,0,13,64,2,new Color(.32f,.2f,.16f));
        for(int x=2;x<64;x+=8) { Pixels(catwalkTile,x,6,6,5,new Color(.13f,.17f,.21f)); Pixels(catwalkTile,x+2,7,2,2,new Color(.035f,.05f,.08f)); }
        streetTile.wrapMode=catwalkTile.wrapMode=TextureWrapMode.Repeat; streetTile.Apply(); catwalkTile.Apply();
        sliceDrone=NewModel(64,28);
        Poly(sliceDrone,new[]{new Vector2(4,10),new Vector2(20,5),new Vector2(44,5),new Vector2(60,10),new Vector2(49,23),new Vector2(16,23)},DeepSteel);
        Poly(sliceDrone,new[]{new Vector2(9,11),new Vector2(24,7),new Vector2(42,7),new Vector2(55,11),new Vector2(45,18),new Vector2(19,18)},new Color(.4f,.49f,.55f));
        Plate(sliceDrone,25,11,14,11,new Color(.045f,.065f,.09f)); Pixels(sliceDrone,28,13,8,4,Orange); Pixels(sliceDrone,30,13,4,1,Color.white);
        Plate(sliceDrone,2,8,12,10,new Color(.17f,.22f,.28f)); Plate(sliceDrone,50,8,12,10,new Color(.17f,.22f,.28f));
        Pixels(sliceDrone,4,9,8,2,Pink); Pixels(sliceDrone,52,9,8,2,Pink); sliceDrone.Apply();
        MakeSliceWeapons();
        MakeCampaignArt();
    }
    void MakeSliceWeapons()
    {
        foreach(int gun in new[]{0,2})
        {
            var t=NewModel(gun==0?80:112,28); int end=t.width-2;
            Poly(t,new[]{new Vector2(1,10),new Vector2(15,9),new Vector2(21,14),new Vector2(13,20),new Vector2(2,19)},DeepSteel);
            Poly(t,new[]{new Vector2(4,11),new Vector2(14,11),new Vector2(18,14),new Vector2(12,17),new Vector2(4,17)},new Color(.33f,.39f,.43f));
            Poly(t,new[]{new Vector2(16,8),new Vector2(43,6),new Vector2(55,10),new Vector2(51,18),new Vector2(18,18)},new Color(.08f,.13f,.2f));
            Poly(t,new[]{new Vector2(20,9),new Vector2(42,8),new Vector2(51,11),new Vector2(48,15),new Vector2(20,15)},new Color(.43f,.53f,.6f));
            Pixels(t,20,9,23,1,new Color(.7f,.76f,.72f)); Plate(t,24,17,6,10,new Color(.13f,.19f,.23f));
            Plate(t,36,17,9,9,new Color(.19f,.27f,.3f)); Pixels(t,38,18,3,5,GunColors[gun]);
            Plate(t,49,10,end-49,5,DeepSteel); Pixels(t,51,11,end-53,1,GunColors[gun]);
            for(int i=0;i<5;i++) Pixels(t,32+i*4,11,2,2,new Color(.08f,.14f,.2f));
            if(gun==2) { Plate(t,33,1,26,6,DeepSteel); Pixels(t,53,2,4,3,Pink); Plate(t,71,15,3,10,new Color(.37f,.45f,.51f)); Plate(t,81,15,3,10,new Color(.37f,.45f,.51f)); }
            else {Plate(t,28,3,12,5,DeepSteel); Plate(t,55,9,15,8,new Color(.18f,.27f,.31f)); Pixels(t,59,10,7,2,TealLight);}
            t.Apply(); gunModels[gun]=t;
        }
    }
    void Glow(Vector2 p, float radius, Color c)
    {
        for(int i=4;i>=1;i--) Box(p.x-radius*i/2,p.y-radius*i/2,radius*i,radius*i,new Color(c.r,c.g,c.b,.018f*(5-i)));
    }
    void DrawSliceEnvironment()
    {
        if(nightCity!=null) GUI.DrawTexture(new Rect(-renderCamera*14,0,1900,1070),nightCity,ScaleMode.StretchToFill);
        for(int i=0;i<12;i++) Box(0,500+i*22,1600,22,new Color(.015f,.025f,.065f,i*.011f));
        float floor=ScreenPoint(new Vector2(0,Floor)).y;
        GUI.DrawTextureWithTexCoords(new Rect(-renderCamera*60,floor,mapWidth*60,805-floor),streetTile,new Rect(0,0,mapWidth*60/64,2.6f));
        for(int i=0;i<28;i++)
        {
            float x=i*103-renderCamera*35;
            Box(x,floor+5+i%4*8,30+i%3*17,2,new Color(.2f,.66f,.73f,.12f));
            if(i%3==0) Box(x+18,floor+14,18,2,new Color(1,.3f,.5f,.17f));
        }
        foreach(Rect ledge in ledges)
        {
            Vector2 p=ScreenPoint(new Vector2(ledge.x,ledge.yMax));
            GUI.DrawTextureWithTexCoords(new Rect(p.x,p.y,ledge.width*60,ledge.height*60+8),catwalkTile,new Rect(0,0,ledge.width*60/64,1));
            Line(p+new Vector2(5,22),p+new Vector2(31,43),DeepSteel,6);
            Line(p+new Vector2(ledge.width*60-5,22),p+new Vector2(ledge.width*60-31,43),DeepSteel,6);
            for(int x=12;x<ledge.width*60;x+=32) {Box(p.x+x,p.y-12,2,12,new Color(.13f,.24f,.27f)); Box(p.x+x-8,p.y-14,18,2,new Color(.17f,.28f,.3f));}
            Glow(p+new Vector2(ledge.width*30,6),20,TealLight);
        }
        for(int i=0;i<82;i++)
        {
            float x=Mathf.Repeat(i*93.7f-clock*(i%2==0?100:160),1650)-25;
            float y=Mathf.Repeat(i*61.3f+clock*(i%2==0?320:480),805);
            Line(new Vector2(x,y),new Vector2(x-5,y+16),new Color(.3f,.69f,.78f,i%2==0?.13f:.24f),1);
        }
        for(int i=0;i<8;i++)
        {
            float x=Mathf.Repeat(i*241+clock*13,1700)-80;
            Box(x,floor-12-i%2*5,110,8,new Color(.3f,.55f,.65f,.025f));
        }
    }
    void DrawSliceGate(bool open)
    {
        Vector2 p=ScreenPoint(exit); Color c=open?TealLight:Pink;
        Box(p.x-45,p.y-185,90,185,new Color(.025f,.045f,.07f));
        Box(p.x-38,p.y-178,76,170,new Color(c.r,c.g,c.b,.13f));
        for(int i=0;i<10;i++) Box(p.x-34,p.y-14-i*16,68,1,new Color(c.r,c.g,c.b,open?.45f:.13f));
        Box(p.x-46,p.y-190,9,190,DeepSteel); Box(p.x+37,p.y-190,9,190,DeepSteel);
        Box(p.x-46,p.y-190,92,10,new Color(.35f,.43f,.47f));
        Box(p.x-40,p.y-182,3,172,c); Box(p.x+37,p.y-182,3,172,c);
        Glow(p+new Vector2(0,-94),65,c);
        string[] names={"接驳电梯","接驳电梯","货运闸门","安保通道","塔顶升降舱","数据回程舱","配电检修门","档案撤离舱"};
        Text(open?names[Mathf.Clamp(Sector,0,7)]+" / 已解锁":"封锁中 / 完成本区任务",p.x-120,p.y-222,280,17,c,true);
    }
    void Part(Texture2D t,Vector2 center,float angle,float width,float alpha,bool flip=false)
    {
        Matrix4x4 saved=GUI.matrix; GUI.matrix=saved*LocalRotation(center,angle);
        float height=width*t.height/t.width; GUI.color=new Color(1,1,1,alpha);
        GUI.DrawTextureWithTexCoords(new Rect(-width/2,-height/2,width,height),t,new Rect(flip?1:0,0,flip?-1:1,1));
        GUI.color=Color.white; GUI.matrix=saved;
    }
    void ArmoredLimb(Vector2 a,Vector2 b,float width,Color c,float alpha)
    {
        Line(a,b,new Color(.025f,.045f,.075f,alpha),width+4);
        Line(a,b,new Color(c.r,c.g,c.b,alpha),width);
        Vector2 edge=new Vector2(-2,-1);
        Line(Vector2.Lerp(a,b,.12f)+edge,Vector2.Lerp(a,b,.7f)+edge,new Color(c.r+.1f,c.g+.12f,c.b+.12f,alpha),Mathf.Max(1,width*.2f));
    }
    void DrawSliceRig(int role,Vector2 p,Vector2 look,float move,float time,float scale,float alpha,bool enemy=false,bool air=false,bool held=false)
    {
        float face=look.x>=0?1:-1, stride=Mathf.Sin(time*13)*Mathf.Clamp01(Mathf.Abs(move)/5);
        Color fabric=enemy?new Color(.31f,.15f,.24f):role==1?new Color(.3f,.46f,.45f):new Color(.16f,.22f,.31f);
        Color trim=enemy?Pink:ClassColors[role];
        Vector2 hip=p+new Vector2(move*.25f,-36+Mathf.Abs(stride)*2)*scale;
        Vector2 footA=p+new Vector2(-6+stride*15,-Mathf.Max(0,stride)*9)*scale;
        Vector2 footB=p+new Vector2(6-stride*15,-Mathf.Max(0,-stride)*9)*scale;
        if(air) {footA=p+new Vector2(-13*face,-7)*scale;footB=p+new Vector2(12*face,-12)*scale;}
        Vector2 kneeA=Vector2.Lerp(hip,footA,.55f)+new Vector2(face*6,-3)*scale;
        Vector2 kneeB=Vector2.Lerp(hip,footB,.55f)+new Vector2(-face*5,-2)*scale;
        Box(p.x-21*scale,p.y+1,42*scale,4*scale,new Color(.015f,.02f,.04f,.6f*alpha));
        ArmoredLimb(hip, kneeA,9*scale,fabric,alpha*.85f); ArmoredLimb(kneeA,footA,7*scale,fabric,alpha*.85f);
        ArmoredLimb(hip,kneeB,10*scale,fabric,alpha); ArmoredLimb(kneeB,footB,8*scale,fabric,alpha);
        Line(footA,footA+new Vector2(face*8,0)*scale,new Color(.39f,.46f,.5f,alpha),6*scale);
        Line(footB,footB+new Vector2(face*9,0)*scale,new Color(.45f,.52f,.56f,alpha),6*scale);
        Vector2 neck=hip+new Vector2(0,-45)*scale;
        if(role==0 || role==3)
        {
            Color scarf=role==0?Rust:new Color(.3f,.15f,.44f); scarf.a=alpha;
            Vector2 tail=neck+new Vector2(-face*(24+Mathf.Abs(move)*1.5f),Mathf.Sin(time*8)*5-4)*scale;
            Line(neck,tail,scarf,7*scale); Line(tail,tail+new Vector2(-face*17,8+Mathf.Sin(time*10)*5)*scale,scarf,4*scale);
            if(role==3) for(int i=0;i<3;i++) Line(hip+new Vector2(-face*8,-15)*scale,hip+new Vector2(-face*(16+i*5),14+Mathf.Sin(time*7+i)*5)*scale,scarf,(6-i)*scale);
        }
        else
        {
            Vector2 pack=hip+new Vector2(-face*15,-24)*scale;
            Line(pack+Vector2.up*12*scale,pack-Vector2.up*10*scale,new Color(.035f,.06f,.09f,alpha),12*scale);
            Line(pack+Vector2.up*10*scale,pack-Vector2.up*8*scale,new Color(trim.r,trim.g,trim.b,alpha),7*scale);
        }
        Vector2 chest=hip+new Vector2(0,-24)*scale;
        Vector2 grip=held?ScreenPoint(GunHand(0)):p+new Vector2(face*19,-56)*scale;
        Vector2 support=held?ScreenPoint(IsDualWield?GunHand(1):GunHand(0)+aim*.25f):p+new Vector2(face*27,-58)*scale;
        Vector2 shoulder=chest+new Vector2(-face*11,-8)*scale;
        Vector2 elbow=Vector2.Lerp(shoulder,support,.5f)+new Vector2(-face*4,11)*scale;
        ArmoredLimb(shoulder,elbow,8*scale,fabric,alpha*.8f); ArmoredLimb(elbow,support,7*scale,fabric,alpha*.8f);
        Part(enemy?guardTorso:sliceTorso[role],chest,Mathf.Clamp(move,-6,6)*1.1f,36*scale,alpha,face<0);
        Part(enemy?guardHead:sliceHead[role],neck+new Vector2(face*2,-11)*scale,-look.y*8*face,26*scale,alpha,face<0);
        shoulder=chest+new Vector2(face*11,-8)*scale;
        elbow=Vector2.Lerp(shoulder,grip,.5f)+new Vector2(-face*5,13)*scale;
        ArmoredLimb(shoulder,elbow,9*scale,fabric,alpha); ArmoredLimb(elbow,grip,8*scale,new Color(.31f,.4f,.46f),alpha);
        Box(grip.x-3*scale,grip.y-3*scale,6*scale,6*scale,new Color(.53f,.6f,.63f,alpha));
    }
    void DrawSliceHero(bool blink)
    {
        DrawSliceRig(profession,ScreenPoint(player),aim,velocity.x,clock,1,blink?.35f:1,false,!grounded&&player.y>Floor+.12f,true);
    }
    void DrawSlicePreview(int role,Rect rect)
    {
        float scale=Mathf.Min(rect.width/70,rect.height/112);
        DrawSliceRig(role,new Vector2(rect.center.x,rect.yMax-4),new Vector2(1,.05f),0,Time.realtimeSinceStartup,scale,1);
    }
    void DrawSliceFoe(Foe foe)
    {
        if(foe.kind==3) {DrawCampaignBoss(foe);return;}
        if(foe.kind==1)
        {
            Vector2 p=ScreenPoint(foe.p+Vector2.up*.5f);
            Part(sliceDrone,p,Mathf.Sin(clock*3+foe.phase)*4,95,1);
            for(int side=-1;side<=1;side+=2) {Vector2 rotor=p+new Vector2(side*33,-14);Line(rotor+new Vector2(-13,Mathf.Sin(clock*40)*2),rotor+new Vector2(13,-Mathf.Sin(clock*40)*2),new Color(.37f,.58f,.64f),2);}
            Glow(p,16,Orange); return;
        }
        Vector2 look=(player-foe.p).normalized;
        float movement=foe.kind==0&&Mathf.Abs(player.x-foe.p.x)>4?Mathf.Sign(player.x-foe.p.x)*2.1f:0;
        DrawSliceRig(0,ScreenPoint(foe.p),look,movement,clock+foe.phase,foe.kind==2?1.12f:1,1,true);
        DrawGun(ScreenPoint(foe.p+Vector2.up*.82f),look,foe.kind==2?3:0,foe.kind==2?82:70);
    }
}
