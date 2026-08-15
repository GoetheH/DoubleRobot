using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class AnimatorSetup : EditorWindow
{
    [MenuItem("Tools/Setup Mech Animator")]
    public static void SetupAnimator()
    {
        string savePath = "Assets/Animations/MechAnimator.controller";
        
        // Create directory if needed
        string dirPath = System.IO.Path.GetDirectoryName(savePath);
        if (!AssetDatabase.IsValidFolder(dirPath))
        {
            System.IO.Directory.CreateDirectory(Application.dataPath + "/Animations");
            AssetDatabase.Refresh();
        }

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(savePath);

        // Add parameters
        controller.AddParameter("Walk", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Idle", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Defend", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);

        // Create states
        AnimatorState idleState = controller.layers[0].stateMachine.AddState("Idle");
        AnimatorState walkState = controller.layers[0].stateMachine.AddState("Walk");
        AnimatorState attackState = controller.layers[0].stateMachine.AddState("Attack");
        AnimatorState defendState = controller.layers[0].stateMachine.AddState("Defend");
        AnimatorState hitState = controller.layers[0].stateMachine.AddState("Hit");
        AnimatorState dieState = controller.layers[0].stateMachine.AddState("Die");

        // Set default state
        controller.layers[0].stateMachine.defaultState = idleState;

        // Create transitions
        // Idle <-> Walk
        AnimatorStateTransition idleToWalk = idleState.AddTransition(walkState);
        idleToWalk.AddCondition(AnimatorConditionMode.If, 0, "Walk");
        idleToWalk.hasExitTime = false;

        AnimatorStateTransition walkToIdle = walkState.AddTransition(idleState);
        walkToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "Walk");
        walkToIdle.hasExitTime = false;

        // Attack -> Idle
        AnimatorStateTransition attackToIdle = attackState.AddTransition(idleState);
        attackToIdle.hasExitTime = true;
        attackToIdle.exitTime = 0.5f;

        // Triggered states -> Idle
        AnimatorStateTransition defendToIdle = defendState.AddTransition(idleState);
        defendToIdle.hasExitTime = true;
        defendToIdle.exitTime = 1f;

        AnimatorStateTransition hitToIdle = hitState.AddTransition(idleState);
        hitToIdle.hasExitTime = true;
        hitToIdle.exitTime = 0.3f;

        // Any State -> triggered states
        AddTriggerTransition(controller, "Attack", attackState);
        AddTriggerTransition(controller, "Defend", defendState);
        AddTriggerTransition(controller, "Hit", hitState);
        AddTriggerTransition(controller, "Die", dieState);

        AssetDatabase.SaveAssets();
        Debug.Log("Animator Controller created at: " + savePath);
    }

    private static void AddTriggerTransition(AnimatorController controller, string paramName, AnimatorState targetState)
    {
        AnimatorStateTransition transition = controller.layers[0].stateMachine.AddAnyStateTransition(targetState);
        transition.AddCondition(AnimatorConditionMode.If, 0, paramName);
        transition.hasExitTime = false;
    }
}
