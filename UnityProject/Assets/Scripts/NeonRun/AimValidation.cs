using System;
using System.Collections;
using System.IO;
using UnityEngine;

public partial class NeonRun
{
    string ValidateAimTransforms()
    {
        Vector2[] sizes = { new Vector2(1600,900), new Vector2(1280,720), new Vector2(1024,768), new Vector2(1920,1080), new Vector2(1000,500), new Vector2(2560,1080) };
        Vector2[] anchors = { new Vector2(480,678), new Vector2(1270,440) };
        int checks = 0;
        foreach (Vector2 size in sizes)
        {
            Matrix4x4 canvas = CanvasMatrix(size.x,size.y);
            foreach (Vector2 anchor in anchors)
            {
                Vector3 expected = canvas.MultiplyPoint3x4(anchor);
                for (int degree = -180; degree <= 180; degree += 15)
                {
                    Matrix4x4 transform = canvas * LocalRotation(anchor,degree);
                    Require(Vector3.Distance(transform.MultiplyPoint3x4(Vector3.zero),expected) < .001f, "gun grip stays anchored at " + size + "/" + degree);
                    Vector2 delta = (Vector2)(Quaternion.Euler(0,0,degree)*Vector2.right)*100;
                    Require(Vector3.Distance(transform.MultiplyPoint3x4(new Vector3(100,0,0)),canvas.MultiplyPoint3x4(anchor+delta)) < .001f, "line endpoint respects canvas scale");
                    for (int gun = 0; gun < gunModels.Length; gun++)
                    {
                        float width = GunWidth[gun]*60, height = width*gunModels[gun].height/gunModels[gun].width;
                        foreach (Vector2 corner in new[] { new Vector2(-width*.3f,-height*.48f), new Vector2(width*.7f,-height*.48f), new Vector2(width*.7f,height*.52f), new Vector2(-width*.3f,height*.52f) })
                            Require(Vector3.Distance(transform.MultiplyPoint3x4(corner),expected) < width*Mathf.Min(size.x/1600,size.y/900)+.01f, "gun bounds remain near grip");
                    }
                    checks++;
                }
            }
        }
        return checks + " rotation poses: fixed grip, transformed line endpoints, all five gun bounds; six viewport sizes including letterboxing.";
    }

    IEnumerator RunAimCheck()
    {
        string[] args = Environment.GetCommandLineArgs(); int index = Array.IndexOf(args,"--neon-aim-check");
        string output = index+1 < args.Length ? args[index+1] : Application.persistentDataPath;
        Directory.CreateDirectory(output);
        yield return new WaitForSeconds(.5f);
        string result;
        try { result = "PASS — Aim pivot regression\n" + ValidateAimTransforms() + "\nActual viewport: " + Screen.width + "x" + Screen.height; }
        catch (Exception error) { result = "FAIL\n" + error; }
        File.WriteAllText(Path.Combine(output,"aim-validation.txt"),result); Debug.Log(result);
        Vector2[] directions = { Vector2.right,Vector2.left,Vector2.up,Vector2.down,new Vector2(-1,1).normalized,new Vector2(-1,-1).normalized,new Vector2(1,1).normalized,new Vector2(1,-1).normalized };
        string[] names = { "right","left","up","down","upper-left","lower-left","upper-right","lower-right" };
        for(int gun=0;gun<gunModels.Length;gun++)
        {
            ReturnToMenu(); SelectWeapon(gun); SelectProfession(gun%4); StartRun();
            player=new Vector2(11,Floor); cameraX=3;
            for(int pose=0;pose<directions.Length;pose++)
            {
                if(pose>=4 && gun!=2) continue;
                State=Phase.Playing; aim=directions[pose]; shots.Clear(); sparks.Clear(); clock+=.25f;
                FirePlayer(); State=Phase.Paused; smokeCapture=true;
                yield return new WaitForSeconds(.07f);
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(Path.Combine(output,"gun-"+gun+"-"+names[pose]+".png"));
                yield return null;
            }
        }
        Application.Quit(result.StartsWith("PASS")?0:1);
    }
}
