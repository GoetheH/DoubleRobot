using UnityEngine;

public partial class NeonRun
{
    static readonly Color[] ClassColors = { new Color(1,.56f,.18f), new Color(.25f,1,.55f), new Color(1,.8f,.2f), new Color(.72f,.42f,1) };
    static readonly Color[] GunColors = { new Color(.22f,.95f,.88f), new Color(1,.78f,.25f), new Color(1,.25f,.48f), new Color(.65f,.86f,1), new Color(.72f,.55f,1) };
    readonly Texture2D[] classModels = new Texture2D[4];
    readonly Texture2D[] gunModels = new Texture2D[5];
    static readonly float[] GunReach = { .95f, 1.05f, 1.45f, 1.2f, .72f };
    static readonly float[] GunWidth = { 1.35f, 1.48f, 1.98f, 1.6f, 1.02f };
    float flashUntil, skillFxUntil, skillFxStarted;
    int skillFxKind;
    Vector2 turretAim = Vector2.right;

#if UNITY_EDITOR
    // Legacy procedural artwork is used only by the one-time asset migration.
    Texture2D NewModel(int width, int height)
    {
        var t = new Texture2D(width, height, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
        t.SetPixels(new Color[width * height]); ownedTextures.Add(t); return t;
    }
    void Pixels(Texture2D t, int x, int y, int width, int height, Color color)
    {
        for (int py = y; py < y + height; py++) for (int px = x; px < x + width; px++)
            if (px >= 0 && px < t.width && py >= 0 && py < t.height) t.SetPixel(px, t.height - 1 - py, color);
    }
    void Plate(Texture2D t, int x, int y, int width, int height, Color color)
    {
        Pixels(t,x,y,width,height,Ink); Pixels(t,x+1,y+1,width-2,height-2,color);
        if (width > 4) Pixels(t,x+2,y+1,width-4,1,Color.Lerp(color,Color.white,.4f));
    }
    void MakeModels()
    {
        Color steel = new Color(.48f,.59f,.68f), white = new Color(.87f,.94f,.95f), dark = new Color(.14f,.22f,.29f);
        for (int role = 0; role < 4; role++)
        {
            Texture2D t = NewModel(32,40); Color accent = ClassColors[role];
            if (role == 3)
                for (int y = 9; y < 34; y++) Pixels(t, 4 - Mathf.Min(3,(y-9)/7), y, 10 + (y-9)/3, 1, new Color(.18f,.11f,.29f));
            Plate(t,10,27,5,11,role == 1 ? white : dark); Plate(t,18,27,5,11,role == 1 ? white : dark);
            Plate(t,8,35,8,4,steel); Plate(t,17,35,8,4,steel);
            Plate(t,10,14,14,15,role == 1 ? white : role == 2 ? new Color(.64f,.44f,.19f) : steel);
            Plate(t,12,4,12,12,role == 1 ? white : dark);
            Pixels(t,14,8,9,3,accent); Pixels(t,20,8,2,1,Color.white);
            Plate(t,7,17,5,12,role == 1 ? white : steel); Plate(t,22,17,5,12,steel);
            Pixels(t,14,19,6,4,accent);
            if (role == 0)
            {
                Plate(t,3,13,10,9,steel); Plate(t,23,13,8,9,steel);
                Plate(t,8,3,18,5,dark); Pixels(t,5,15,6,2,accent); Pixels(t,25,15,4,2,accent);
                Plate(t,13,19,8,7,dark); Pixels(t,15,21,4,3,accent);
                Pixels(t,11,30,3,3,accent); Pixels(t,19,30,3,3,accent);
            }
            else if (role == 1)
            {
                Plate(t,4,12,7,18,accent); Plate(t,10,2,15,6,white);
                Pixels(t,16,2,3,6,accent); Pixels(t,13,4,9,2,accent);
                Pixels(t,16,18,3,8,accent); Pixels(t,13,21,9,2,accent);
                Plate(t,24,23,5,5,white); Pixels(t,25,24,3,3,accent);
            }
            else if (role == 2)
            {
                Plate(t,3,12,7,20,dark); Pixels(t,4,14,5,10,accent);
                Plate(t,9,2,17,6,accent); Plate(t,7,7,21,3,accent);
                Pixels(t,26,3,2,10,steel); Pixels(t,25,2,4,2,Cyan);
                Plate(t,23,24,8,7,accent); Pixels(t,28,23,3,3,dark); Pixels(t,28,29,3,3,dark);
                Pixels(t,12,26,12,2,accent); Pixels(t,14,26,2,2,Ink); Pixels(t,20,26,2,2,Ink);
            }
            else
            {
                Plate(t,9,1,17,6,new Color(.26f,.16f,.39f)); Plate(t,8,5,5,13,new Color(.26f,.16f,.39f));
                Pixels(t,13,13,12,3,accent); Pixels(t,15,17,5,2,accent);
                Plate(t,24,20,6,6,dark); Pixels(t,11,32,3,3,accent); Pixels(t,19,32,3,3,accent);
            }
            t.Apply(); classModels[role] = t;
        }
        for (int gun = 0; gun < 5; gun++)
        {
            Texture2D t = NewModel(gun == 2 ? 80 : gun == 4 ? 44 : 64, 28); Color accent = GunColors[gun];
            Plate(t,4,10,12,9,dark); Plate(t,14,8,25,12,steel); Plate(t,19,17,5,9,dark);
            if (gun == 0)
            {
                Plate(t,38,10,23,5,steel); Plate(t,25,18,8,9,accent); Plate(t,22,4,12,5,dark);
                Pixels(t,17,11,16,2,accent); Pixels(t,48,10,7,1,Cyan);
            }
            else if (gun == 1)
            {
                Plate(t,35,7,28,5,steel); Plate(t,35,13,28,5,dark); Plate(t,37,17,15,5,accent);
                Pixels(t,17,11,3,3,accent); Pixels(t,22,11,3,3,accent); Pixels(t,27,11,3,3,accent);
            }
            else if (gun == 2)
            {
                Plate(t,38,10,40,5,steel); Plate(t,25,1,24,6,dark); Pixels(t,44,2,4,3,accent);
                Pixels(t,42,11,31,2,accent); Plate(t,45,15,3,10,steel); Plate(t,53,15,3,10,steel);
                Plate(t,25,18,9,8,dark);
            }
            else if (gun == 3)
            {
                Plate(t,12,5,31,16,dark); Plate(t,39,8,24,8,steel); Plate(t,26,18,17,10,accent);
                for (int x = 42; x < 60; x += 4) Pixels(t,x,9,2,6,Ink);
                Pixels(t,15,9,20,3,accent); Plate(t,19,1,14,5,steel);
            }
            else
            {
                Pixels(t,38,0,6,28,Color.clear); Plate(t,31,10,11,5,dark); Plate(t,23,18,6,10,accent);
                Pixels(t,16,10,14,2,accent); Plate(t,1,15,11,3,steel); Plate(t,16,3,11,5,dark);
            }
            t.Apply(); gunModels[gun] = t;
        }
        robotArt = classModels[0];
        MakeSliceArt();
    }
#endif
    void DrawModelPreview(int role, Rect r)
    {
        if(sliceTorso[role]!=null) DrawSlicePreview(role,r);
        else GUI.DrawTexture(r,classModels[role],ScaleMode.ScaleToFit);
    }
    bool IsDualWield => profession == 0 && clock < skillUntil;
    Vector2 GunHand(int barrel)
    {
        Vector2 side = new Vector2(-aim.y,aim.x);
        return player + Vector2.up * .98f + (IsDualWield ? side * (barrel == 0 ? .22f : -.22f) : Vector2.zero);
    }
    Vector2 GunMuzzle(int barrel) { return GunHand(barrel) + aim * GunReach[weapon]; }
    Vector2 PlayerMuzzle() { return GunMuzzle(0); }
    static Matrix4x4 LocalRotation(Vector2 center, float angle)
    {
        return Matrix4x4.TRS(new Vector3(center.x,center.y,0),Quaternion.Euler(0,0,angle),Vector3.one);
    }
    void Line(Vector2 a, Vector2 b, Color color, float thickness = 3)
    {
        Matrix4x4 saved = GUI.matrix; Vector2 d = b-a;
        GUI.matrix = saved * LocalRotation(a,Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg);
        Box(0,-thickness/2,d.magnitude,thickness,color); GUI.matrix = saved;
    }
    void Ring(Vector2 center, float radius, Color color, int segments = 24, float thickness = 3)
    {
        for (int i = 0; i < segments; i++)
        {
            float a=i*Mathf.PI*2/segments, b=(i+1)*Mathf.PI*2/segments;
            Line(center + new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,center + new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius,color,thickness);
        }
    }
    void DrawGun(Vector2 center, Vector2 direction, int gun, float width, float alpha = 1)
    {
        Matrix4x4 saved=GUI.matrix;
        float height=width * gunModels[gun].height / gunModels[gun].width;
        // Compose the weapon's local transform inside the scaled/letterboxed canvas.
        GUI.matrix = saved * LocalRotation(center,Mathf.Atan2(-direction.y,direction.x)*Mathf.Rad2Deg);
        GUI.color=new Color(1,1,1,alpha);
        GUI.DrawTextureWithTexCoords(new Rect(-width*.3f,-height*.48f,width,height),gunModels[gun],new Rect(0,direction.x<0?1:0,1,direction.x<0?-1:1));
        GUI.color=Color.white; GUI.matrix=saved;
    }
    void DrawPlayerModel(bool blink)
    {
        DrawSliceHero(blink);
        Vector2 anchor=ScreenPoint(GunHand(0));
        float recoil=clock<flashUntil ? 5 : 0;
        DrawGun(anchor-new Vector2(aim.x,-aim.y)*recoil,aim,weapon,GunWidth[weapon]*60,blink?.35f:1);
        if (IsDualWield) DrawGun(ScreenPoint(GunHand(1))-new Vector2(aim.x,-aim.y)*recoil,aim,weapon,GunWidth[weapon]*60,.7f);
        if (clock < flashUntil)
        {
            for (int barrel=0;barrel<(IsDualWield?2:1);barrel++)
            {
            Vector2 p=ScreenPoint(GunMuzzle(barrel)), d=new Vector2(aim.x,-aim.y);
            Color c=GunColors[weapon];
            if (weapon==1) for(int i=-1;i<=1;i++) Line(p,p+(Vector2)(Quaternion.Euler(0,0,i*18)*d)*24,c,4);
            else if(weapon==2) { Line(p,p+d*65,c,3); Ring(p,12,c,12,2); }
            else { Line(p,p+d*(weapon==3?28:18),c,5); Line(p+d*5-new Vector2(d.y,-d.x)*8,p+d*5+new Vector2(d.y,-d.x)*8,Color.white,3); }
            }
        }
        DrawClassEffects();
    }
    void DrawSkillScreen(int role, Vector2 center, float progress, float scale)
    {
        Color c=ClassColors[role]; float pulse=.6f+.4f*Mathf.Sin(progress*12);
        if(role==0)
        {
            Ring(center,52*scale,c,12,4*scale);
            for(int i=0;i<8;i++) { float a=progress*6+i*Mathf.PI/4; Vector2 d=new Vector2(Mathf.Cos(a),Mathf.Sin(a)); Line(center+d*58*scale,center+d*72*scale,c,3*scale); }
        }
        else if(role==1)
        {
            Ring(center,(40+progress*35)*scale,new Color(c.r,c.g,c.b,1-progress*.5f),24,3*scale);
            for(int i=0;i<4;i++) { float x=(i-1.5f)*27*scale, y=-Mathf.Repeat(progress*90+i*20,90)*scale; Box(center.x+x-7*scale,center.y+y-2*scale,14*scale,4*scale,c); Box(center.x+x-2*scale,center.y+y-7*scale,4*scale,14*scale,c); }
        }
        else if(role==2)
        {
            Color blue=new Color(.3f,.8f,1,.6f);
            for(int i=-2;i<=2;i++) { Line(center+new Vector2(i*24,-50)*scale,center+new Vector2(i*24,50)*scale,blue,scale); Line(center+new Vector2(-50,i*24)*scale,center+new Vector2(50,i*24)*scale,blue,scale); }
            Ring(center,61*scale,Cyan,6,3*scale);
        }
        else
        {
            Ring(center,62*scale,new Color(c.r,c.g,c.b,.5f),32,2*scale);
            Ring(center,31*scale,new Color(c.r,c.g,c.b,.3f),24,scale);
            float angle=progress*Mathf.PI*6;
            Line(center,center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*60*scale,c,3*scale);
            for(int i=0;i<3;i++) {float a=i*2.1f;Vector2 p=center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*43*scale;Box(p.x-3*scale,p.y-3*scale,6*scale,6*scale,c);}
        }
    }
    void DrawClassEffects()
    {
        if (clock < skillFxUntil)
        {
            float duration=skillFxUntil-skillFxStarted;
            float progress=Mathf.Clamp01((clock-skillFxStarted)/Mathf.Max(.01f,duration));
            Vector2 center=ScreenPoint(player+Vector2.up*.8f);
            if(skillFxKind==3) DrawScanTargets();
            DrawSkillScreen(skillFxKind,skillFxKind==2?ScreenPoint(turret):center,progress,.7f);
        }
        if(clock < turretUntil)
        {
            Vector2 p=ScreenPoint(turret);
            Line(p+new Vector2(-15,26),p+new Vector2(0,5),ClassColors[2],5);
            Line(p+new Vector2(15,26),p+new Vector2(0,5),ClassColors[2],5);
            Box(p.x-14,p.y-8,28,17,Ink); Box(p.x-10,p.y-5,20,11,ClassColors[2]);
            DrawGun(p,turretAim,3,48);
            Ring(p,21,new Color(.3f,.8f,1,.3f),16,2);
        }
    }
    void DrawScanTargets()
    {
        Color c=ClassColors[3];
        foreach(Foe foe in enemies)
        {
            if(!IsScanned(foe))continue;
            Vector2 p=ScreenPoint(foe.p+Vector2.up*(foe.kind==3?0:.7f));
            float w=foe.kind==3?118:36,h=foe.kind==3?103:foe.kind==1?30:64;
            if(p.x+w<0||p.x-w>1600||p.y+h<104||p.y-h>805)continue;
            for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)
            {
                Vector2 corner=p+new Vector2(x*w,y*h);
                Line(corner,corner-new Vector2(x*14,0),c,3);
                Line(corner,corner-new Vector2(0,y*14),c,3);
            }
            Text("弱点 +"+((balance.roles[3].scanMultiplier-1)*100).ToString("0")+"% / "+(foe.scannedUntil-clock).ToString("0.0")+"s",p.x-w,p.y-h-26,220,14,c,true);
        }
    }
}
