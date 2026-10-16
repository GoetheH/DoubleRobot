using UnityEngine;

[DisallowMultipleComponent]
public class StageElement : MonoBehaviour
{
    public enum Kind { Platform, PlayerSpawn, Exit, MissionItem, Terminal, BossSpawn, EnemySpawn, Decoration }
    public Kind kind;
    [Tooltip("机关编号从0开始；通过关卡根对象的switchOrder决定操作顺序。")]
    [Min(0)] public int order;
    [Tooltip("额外布置敌人的类型：0守卫、1无人机、2重装。")]
    [Range(0,2)] public int enemyKind;
    [Tooltip("平台使用此贴图替换默认材质；装饰物使用SpriteRenderer的Sprite和Color。")]
    public Texture2D platformArtwork;
    public Rect PlatformRect
    {
        get
        {
            var box=GetComponent<BoxCollider2D>();
            Vector2 center=box?transform.TransformPoint(box.offset):transform.position;
            Vector3 scale=transform.lossyScale;
            Vector2 size=Vector2.Scale(box?box.size:Vector2.one,new Vector2(Mathf.Abs(scale.x),Mathf.Abs(scale.y)));
            return new Rect(center-size/2,size);
        }
    }
}
