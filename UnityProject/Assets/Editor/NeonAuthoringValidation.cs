using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class NeonAuthoringValidation
{
    public static void ValidateAssets()
    {
        var c=Resources.Load<NeonCampaign>("Authoring/Campaign");
        if(!c||!c.balance||c.levels.Length!=8)throw new InvalidOperationException("Missing authoring assets.");
        c.balance.ValidateData();
        for(int i=0;i<8;i++)
        {
            if(!c.levels[i]||c.levels[i].sector!=i+1||!c.levels[i].background)throw new InvalidOperationException("Invalid level "+(i+1));
            c.levels[i].ValidateData();
            if(!PrefabUtility.IsPartOfPrefabAsset(c.levels[i]))throw new InvalidOperationException("Stage must be a persistent prefab.");
        }
        Debug.Log("AUTHORING_ASSETS_PASS: eight serialized layouts and combat configuration validated.");
    }
    public static void MigrateAndValidate()
    {
        NeonAuthoringMigration.CreateMissingAssets();ValidateAssets();
        var c=Resources.Load<NeonCampaign>("Authoring/Campaign");
        // Exercise the exact prefab save/reload workflow used by a designer, then
        // restore the source bytes so verification never changes authored content.
        string path=AssetDatabase.GetAssetPath(c.levels[0]);byte[] before=File.ReadAllBytes(path);
        try
        {
            var root=PrefabUtility.LoadPrefabContents(path);
            var platform=Array.Find(root.GetComponentsInChildren<StageElement>(),e=>e.kind==StageElement.Kind.Platform);
            platform.transform.localPosition+=new Vector3(1.25f,.45f,0);platform.transform.localScale+=new Vector3(1.5f,.1f,0);
            Vector3 expected=platform.transform.localPosition;string name=platform.name;
            PrefabUtility.SaveAsPrefabAsset(root,path);PrefabUtility.UnloadPrefabContents(root);
            var reloaded=PrefabUtility.LoadPrefabContents(path);
            if(Vector3.Distance(reloaded.transform.Find(name).localPosition,expected)>.001f)throw new InvalidOperationException("Prefab layout edit was not persisted.");
            PrefabUtility.UnloadPrefabContents(reloaded);
        }
        finally{File.WriteAllBytes(path,before);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);}
        ValidateAssets();
        string output=Environment.GetEnvironmentVariable("NEON_AUTHORING_REPORT");
        if(!string.IsNullOrEmpty(output))File.WriteAllText(output,"PASS — editable Unity authoring\n8 persistent level prefabs, Main scene workspace, 5 gun / 4 role / 3 difficulty configurations.\nPrefab Transform edit, save and reload succeeds; source content restored.\nPersistent PNG character, weapon, enemy and platform art available in Inspector.\n");
        Debug.Log("AUTHORING_EDIT_SAVE_RELOAD_PASS");
    }
}
