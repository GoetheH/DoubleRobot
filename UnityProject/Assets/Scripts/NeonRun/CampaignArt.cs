using UnityEngine;

public partial class NeonRun
{
    Texture2D foundryScene, spireScene, archiveScene, reactorScene, sentinelModel;
    Color SectorAccent => activeLayout?activeLayout.accent:Cyan;
    Texture2D SectorBackdrop(int sector)
    {
        if(campaign&&sector>0&&sector<=campaign.levels.Length)return campaign.levels[sector-1].background;
        switch(sector)
        {
            case 2:return foundryScene;
            case 3:case 8:return reactorScene;
            case 4:return spireScene;
            case 5:case 7:return archiveScene;
            case 6:return foundryScene;
            default:return nightCity;
        }
    }
#if UNITY_EDITOR
    void MakeCampaignArt()
    {
        foundryScene=Resources.Load<Texture2D>("ArtSlice/Foundry");
        spireScene=Resources.Load<Texture2D>("ArtSlice/Spire");
        archiveScene=Resources.Load<Texture2D>("ArtSlice/Archive");
        reactorScene=Resources.Load<Texture2D>("ArtSlice/Reactor");
        // Original hovering armored sentinel. The luminous lens is the real collision center.
        sentinelModel=NewModel(128,112);
        Color armor=new Color(.24f,.31f,.42f), edge=new Color(.49f,.57f,.64f);
        Poly(sentinelModel,new[]{new Vector2(17,26),new Vector2(35,13),new Vector2(48,28),new Vector2(42,78),new Vector2(15,86),new Vector2(4,63)},DeepSteel);
        Poly(sentinelModel,new[]{new Vector2(111,26),new Vector2(93,13),new Vector2(80,28),new Vector2(86,78),new Vector2(113,86),new Vector2(124,63)},DeepSteel);
        for(int side=0;side<2;side++)
        {
            int x=side==0?9:91;
            Plate(sentinelModel,x,31,28,42,armor);
            Plate(sentinelModel,x+5,39,18,9,edge);
            for(int row=0;row<4;row++) Pixels(sentinelModel,x+5,54+row*4,18,2,new Color(.08f,.12f,.19f));
            Pixels(sentinelModel,x+8,26,12,3,Pink);
            Poly(sentinelModel,new[]{new Vector2(x+4,77),new Vector2(x+24,77),new Vector2(x+18,103),new Vector2(x+10,107)},new Color(.09f,.16f,.25f));
            Pixels(sentinelModel,x+10,83,8,12,Cyan);
        }
        Poly(sentinelModel,new[]{new Vector2(44,17),new Vector2(64,8),new Vector2(84,17),new Vector2(94,48),new Vector2(83,82),new Vector2(64,98),new Vector2(45,82),new Vector2(34,48)},new Color(.025f,.045f,.075f));
        Poly(sentinelModel,new[]{new Vector2(47,21),new Vector2(64,13),new Vector2(81,21),new Vector2(88,48),new Vector2(79,77),new Vector2(64,90),new Vector2(49,77),new Vector2(40,48)},armor);
        Poly(sentinelModel,new[]{new Vector2(48,23),new Vector2(63,16),new Vector2(59,37),new Vector2(43,48)},edge);
        Plate(sentinelModel,49,34,30,35,new Color(.035f,.065f,.11f));
        for(int row=0;row<5;row++) Pixels(sentinelModel,52-row%2,38+row*5,24+row%2*2,3,new Color(.37f,.45f,.53f));
        Plate(sentinelModel,55,46,18,19,new Color(.1f,.17f,.23f)); Pixels(sentinelModel,59,50,10,11,Color.white);
        Plate(sentinelModel,53,71,22,10,DeepSteel);Pixels(sentinelModel,57,73,14,3,Pink);
        Plate(sentinelModel,57,1,14,12,armor); Pixels(sentinelModel,61,0,6,3,Cyan);
        sentinelModel.Apply();
        MakeRemainingWeapons();
    }
    void MakeRemainingWeapons()
    {
        foreach(int gun in new[]{1,3,4})
        {
            int width=gun==4?64:gun==3?96:88;
            var t=NewModel(width,32); Color accent=GunColors[gun], steel=new Color(.44f,.53f,.59f);
            Poly(t,new[]{new Vector2(1,11),new Vector2(14,9),new Vector2(23,14),new Vector2(14,22),new Vector2(2,21)},DeepSteel);
            Poly(t,new[]{new Vector2(5,13),new Vector2(14,12),new Vector2(18,15),new Vector2(12,18),new Vector2(5,18)},steel);
            Poly(t,new[]{new Vector2(17,7),new Vector2(46,6),new Vector2(53,12),new Vector2(47,21),new Vector2(18,21)},DeepSteel);
            Plate(t,20,9,27,9,steel); Pixels(t,22,9,22,1,new Color(.72f,.76f,.76f));
            Plate(t,23,20,6,11,new Color(.13f,.19f,.24f)); Pixels(t,25,22,2,6,new Color(.3f,.37f,.4f));
            if(gun==1)
            {
                Plate(t,45,8,40,6,new Color(.31f,.37f,.42f)); Plate(t,45,15,40,5,DeepSteel);
                Plate(t,49,20,22,5,new Color(.42f,.29f,.17f));
                for(int x=51;x<70;x+=4) Pixels(t,x,21,2,3,accent);
                for(int x=23;x<43;x+=6) { Plate(t,x,11,4,5,accent); Pixels(t,x+1,12,2,1,Color.white); }
                Plate(t,31,3,9,5,DeepSteel); Pixels(t,81,9,3,3,accent);
            }
            else if(gun==3)
            {
                Plate(t,17,5,39,17,DeepSteel); Plate(t,20,8,28,10,steel);
                Plate(t,51,9,42,10,new Color(.28f,.36f,.42f));
                for(int x=55;x<85;x+=5) Pixels(t,x,10,3,6,Ink);
                Plate(t,35,21,19,11,new Color(.32f,.4f,.47f));
                for(int x=39;x<52;x+=4) Pixels(t,x,24,2,6,accent);
                Plate(t,27,1,22,5,steel); Pixels(t,23,10,22,2,accent); Pixels(t,90,10,3,6,accent);
            }
            else
            {
                Plate(t,47,10,15,5,DeepSteel); Plate(t,36,19,7,13,new Color(.21f,.26f,.35f));
                Pixels(t,38,21,3,8,accent); Plate(t,25,3,13,5,DeepSteel);
                Pixels(t,22,11,22,2,accent); Pixels(t,54,10,5,1,steel);
                Pixels(t,2,17,14,3,steel); Pixels(t,3,19,2,8,steel);
            }
            t.Apply();gunModels[gun]=t;
        }
    }
#endif
    void DrawCampaignEnvironment()
    {
        Color accent=SectorAccent;
        Texture2D backdrop=SectorBackdrop(Sector);
        GUI.color=Sector==3?new Color(.6f,.86f,1):Sector==6?new Color(.6f,.75f,.9f):Sector==7?new Color(.61f,.76f,.71f):Color.white;
        if(backdrop!=null) GUI.DrawTexture(new Rect(-renderCamera*12,-cameraY*5,1900,1070),backdrop,ScaleMode.StretchToFill);
        GUI.color=Color.white;
        Box(0,104,1600,701,new Color(.012f,.025f,.05f,.2f));
        if(Sector==2) DrawFoundryMachinery(accent);
        if(Sector==3 || Sector==8) DrawReactorHall(accent);
        if(Sector==4) DrawSpireStructure(accent);
        if(Sector==5 || Sector==7) DrawArchiveStacks(accent);
        if(Sector==6) DrawPowerRouting(accent);
        DrawCampaignPlatforms(accent);
        // Outdoor rain, interior embers, data motes and descending dust share deterministic particles.
        for(int i=0;i<48;i++)
        {
            float x=Mathf.Repeat(i*117.3f-clock*(Sector==4?120:12),1640)-20;
            float y=Mathf.Repeat(i*71.7f+clock*(Sector==2?-37:Sector==4?330:16)-cameraY*6,805);
            if(Sector==4) Line(new Vector2(x,y),new Vector2(x-5,y+19),new Color(.5f,.76f,.9f,.24f),1);
            else Box(x,y,Sector==2?3:2,Sector==2?5:2,new Color(accent.r,accent.g,accent.b,.16f+.12f*(i%3)));
        }
    }
    void DrawCampaignPlatforms(Color accent)
    {
        float floor=ScreenPoint(new Vector2(0,Floor)).y;
        if(floor<805 && floor>0)
        {
            GUI.DrawTextureWithTexCoords(new Rect(-renderCamera*60,floor,mapWidth*60,805-floor),streetTile,new Rect(0,0,mapWidth*60/64,(805-floor)/32));
            Box(-renderCamera*60,floor,mapWidth*60,2,Color.Lerp(accent,Color.white,.3f));
        }
        foreach(Rect ledge in ledges)
        {
            Vector2 p=ScreenPoint(new Vector2(ledge.x,ledge.yMax));
            if(p.y<75 || p.y>815 || p.x>1620 || p.x+ledge.width*60<0) continue;
            GUI.DrawTextureWithTexCoords(new Rect(p.x,p.y,ledge.width*60,ledge.height*60+8),catwalkTile,new Rect(0,0,ledge.width*60/64,1));
            Box(p.x,p.y,ledge.width*60,3,Color.Lerp(accent,Color.white,.35f));
            for(int x=8;x<ledge.width*60-8;x+=32)
            {
                Box(p.x+x,p.y+5,12,2,new Color(accent.r,accent.g,accent.b,.6f));
                Line(p+new Vector2(x,22),p+new Vector2(x+17,39),DeepSteel,5);
            }
            Box(p.x+2,p.y+3,4,ledge.height*60+5,accent);Box(p.x+ledge.width*60-6,p.y+3,4,ledge.height*60+5,accent);
        }
    }
    void DrawFoundryMachinery(Color accent)
    {
        for(int i=0;i<6;i++)
        {
            Vector2 p=ScreenPoint(new Vector2(i*8+2,Floor));
            Box(p.x,p.y-250,88,250,new Color(.035f,.055f,.08f,.95f));
            Box(p.x+6,p.y-246,76,6,new Color(.35f,.29f,.23f));
            for(int row=0;row<6;row++) {Box(p.x+12,p.y-215+row*32,64,17,new Color(.18f,.12f,.1f));Box(p.x+17,p.y-211+row*32,54,3,new Color(1,.35f,.1f,.4f));}
            Line(p+new Vector2(30,-250),p+new Vector2(30,-470),DeepSteel,13);
            for(int rivet=0;rivet<9;rivet++) Box(p.x+3,p.y-230+rivet*26,3,3,new Color(.41f,.37f,.31f));
        }
        float y=ScreenPoint(new Vector2(0,Floor)).y-10;
        Box(0,y,1600,8,new Color(.36f,.13f,.05f,.75f));
        for(int i=0;i<40;i++) Box(i*44-Mathf.Repeat(clock*18,44),y+2,23,2,new Color(1,.47f,.11f,.45f));
    }
    void DrawSpireStructure(Color accent)
    {
        foreach(float x in new[]{2f,21f})
        {
            Vector2 bottom=ScreenPoint(new Vector2(x,0)),top=ScreenPoint(new Vector2(x,mapHeight));
            Line(bottom,top,new Color(.035f,.07f,.12f),30);Line(bottom+Vector2.right*9,top+Vector2.right*9,new Color(.19f,.29f,.37f),3);
            for(int i=0;i<16;i++) {Vector2 p=ScreenPoint(new Vector2(x,i*2));Box(p.x-13,p.y,26,8,new Color(.22f,.31f,.37f));Box(p.x+8,p.y-16,4,9,accent);}
        }
        for(int i=0;i<6;i++)
        {
            Vector2 p=ScreenPoint(new Vector2(3,3+i*5));
            Line(p,p+new Vector2(17*60,-5*60),new Color(.09f,.19f,.28f,.9f),7);
            Text("COM / "+(i+1).ToString("00"),p.x+14,p.y-48,180,14,new Color(.38f,.66f,.79f));
        }
    }
    void DrawArchiveStacks(Color accent)
    {
        int rows=Sector==7?6:2;
        for(int row=0;row<rows;row++) for(int col=0;col<8;col++)
        {
            Vector2 p=ScreenPoint(new Vector2(1+col*6,Floor+row*4.8f));
            if(p.y<80 || p.y>1100 || p.x>1650 || p.x< -180) continue;
            Box(p.x,p.y-190,90,190,new Color(.025f,.06f,.08f,.93f));
            Box(p.x+3,p.y-186,84,3,new Color(.27f,.4f,.43f));
            for(int unit=0;unit<7;unit++)
            {
                Box(p.x+8,p.y-173+unit*24,74,20,new Color(.08f,.14f,.17f));
                for(int port=0;port<4;port++) Box(p.x+14+port*12,p.y-166+unit*24,7,2,new Color(accent.r,accent.g,accent.b,.35f+((unit+port+col)%3)*.15f));
                Box(p.x+72,p.y-168+unit*24,3,5,accent*.7f);
            }
            if(Sector==7) Text("VAULT "+(rows-row).ToString("00"),p.x+6,p.y-214,120,12,new Color(.38f,.58f,.56f));
        }
    }
    void DrawPowerRouting(Color accent)
    {
        for(int i=0;i<switches.Count;i++)
        {
            Vector2 p=ScreenPoint(switches[i]);
            Vector2 conduit=p+new Vector2(0,-100-i*24);
            Color c=TerminalActive(i)?Cyan:new Color(.38f,.27f,.16f);
            Line(p,conduit,DeepSteel,11);Line(p,conduit,c,3);
            Line(conduit,new Vector2(800,conduit.y),DeepSteel,11);Line(conduit,new Vector2(800,conduit.y),c,3);
        }
        for(int i=0;i<7;i++)
        {
            Vector2 p=ScreenPoint(new Vector2(i*7,Floor));
            Box(p.x,p.y-225,125,225,new Color(.025f,.045f,.07f,.94f));
            Box(p.x+7,p.y-218,111,7,new Color(.41f,.35f,.22f));
            for(int j=0;j<6;j++) {Box(p.x+14,p.y-190+j*27,95,19,new Color(.11f,.16f,.19f));Box(p.x+19,p.y-185+j*27,8,6,accent*.7f);Box(p.x+35,p.y-185+j*27,66,2,new Color(.3f,.36f,.38f));}
        }
    }
    void DrawReactorHall(Color accent)
    {
        Vector2 p=ScreenPoint(new Vector2(25,6));
        Color core=Sector==8?Pink:Cyan;
        Ring(p,164,new Color(.06f,.11f,.17f,.95f),32,25);
        Ring(p,146,new Color(core.r,core.g,core.b,.4f),32,4);
        Ring(p,120,new Color(.27f,.34f,.42f,.6f),16,12);
        for(int i=0;i<8;i++)
        {
            float a=i*Mathf.PI/4+clock*.05f;Vector2 d=new Vector2(Mathf.Cos(a),Mathf.Sin(a));
            Line(p+d*125,p+d*157,new Color(core.r,core.g,core.b,.6f),5);
        }
        Glow(p,130,core);
        for(int i=0;i<6;i++)
        {
            Vector2 b=ScreenPoint(new Vector2(2+i*8,Floor));
            Box(b.x,b.y-400,28,400,new Color(.025f,.045f,.07f,.85f));
            Box(b.x+5,b.y-390,3,380,new Color(core.r,core.g,core.b,.3f));
            for(int j=0;j<8;j++) Box(b.x+1,b.y-370+j*45,26,4,new Color(.2f,.27f,.34f));
        }
    }
    void DrawCampaignBoss(Foe foe)
    {
        Vector2 p=ScreenPoint(foe.p);bool rage=foe.hp<foe.max/2;
        Color c=rage?Orange:Sector==8?Pink:Cyan;
        for(int side=-1;side<=1;side+=2)
        {
            Vector2 exhaust=p+new Vector2(side*80,62);
            Line(exhaust,exhaust+new Vector2(0,22+Mathf.Sin(clock*35+side)*10),new Color(c.r,c.g,c.b,.45f),9);
            Line(exhaust,exhaust+new Vector2(0,16),Color.white,3);
        }
        Part(sentinelModel,p,Mathf.Sin(clock*2)*3,232,1);
        Ring(p,23,c,12,5);Ring(p,15,Color.white,12,2);
        Glow(p,39,c);
        for(int i=0;i<3;i++) {float a=clock*(rage?2:1)+i*Mathf.PI*2/3;Vector2 d=new Vector2(Mathf.Cos(a),Mathf.Sin(a));Line(p+d*32,p+d*39,c,3);}
    }
    bool TerminalActive(int index)
    {
        // Match the existing II -> I -> III puzzle state, including partial completion.
        for(int step=0;step<Mathf.Min(sequence,CircuitOrder.Length);step++)if(CircuitOrder[step]==index)return true;
        return false;
    }
    void DrawCampaignTerminal(int index)
    {
        Vector2 p=ScreenPoint(switches[index]); bool active=TerminalActive(index);
        Color c=active?Cyan:Orange;
        Box(p.x-22,p.y-25,44,52,DeepSteel);Box(p.x-19,p.y-22,38,4,new Color(.45f,.52f,.56f));
        Box(p.x-17,p.y-15,34,25,new Color(.025f,.085f,.1f));
        for(int i=0;i<3;i++) Box(p.x-12,p.y-10+i*6,active?24:12+i*4,2,c);
        Box(p.x-14,p.y+16,18,3,c);Box(p.x+9,p.y+14,4,6,active?Cyan:Pink);
        Line(p+new Vector2(-14,27),p+new Vector2(-14,38),DeepSteel,5);Line(p+new Vector2(14,27),p+new Vector2(14,38),DeepSteel,5);
        if(active) Glow(p,28,Cyan);
        Text(TerminalName(index)+(active?" / 已接通":" [E]"),p.x-40,p.y-61,155,17,active?Cyan:Color.white,true);
    }
    void DrawCampaignItem(Vector2 item)
    {
        Vector2 p=ScreenPoint(item);float bob=Mathf.Sin(clock*3+item.x)*3;
        p.y+=bob;Glow(p,26,Orange);Ring(p,21,new Color(1,.65f,.25f,.35f),8,2);
        Box(p.x-10,p.y-13,20,26,DeepSteel);Box(p.x-7,p.y-10,14,20,new Color(.27f,.35f,.39f));
        if(Sector==4) {Line(p+new Vector2(0,-6),p+new Vector2(0,8),Orange,4);Line(p+new Vector2(0,-6),p+new Vector2(9,-6),Orange,4);Box(p.x+5,p.y-6,3,6,Orange);}
        else {Box(p.x-4,p.y-7,8,10,Orange);for(int i=0;i<3;i++)Box(p.x-6+i*5,p.y+6,2,5,Cyan);}
        Text(Sector==4?"天线钥匙":Sector==7?"封存档案":"数据芯片",p.x-42,p.y-57,155,16,Orange,true);
    }
}
