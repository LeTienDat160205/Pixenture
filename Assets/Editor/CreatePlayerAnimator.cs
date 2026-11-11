using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Creates the player animator and its single-frame Jump/Fall clips.</summary>
public static class CreatePlayerAnimator
{
    private const string Folder = "Assets/Animation/VirtualGuy";
    private const string ControllerPath = "Assets/Animation/PlayerAnimator.controller";

    [InitializeOnLoadMethod]
    private static void CreateOnImport()
    {
        EditorApplication.delayCall += EnsureCreated;
    }

    [MenuItem("Tools/Pixenture/Create or Repair Player Animator")]
    public static void EnsureCreated()
    {
        var jump = CreateSingleFrameClip("Jump", $"{Folder}/Jump (32x32).png");
        var fall = CreateSingleFrameClip("Fall", $"{Folder}/Fall (32x32).png");
        var idle = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{Folder}/Idle.anim");
        var run = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{Folder}/Run.anim");

        if (idle == null || run == null || jump == null || fall == null)
        {
            Debug.LogError("Player animation assets could not be found.");
            return;
        }

        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath)
            ?? AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        var layers = controller.layers;
        layers[0].defaultWeight = 1f;
        controller.layers = layers;
        AddParameterIfMissing(controller, "Speed", AnimatorControllerParameterType.Float);
        AddParameterIfMissing(controller, "IsGrounded", AnimatorControllerParameterType.Bool);
        AddParameterIfMissing(controller, "VerticalVelocity", AnimatorControllerParameterType.Float);

        var machine = controller.layers[0].stateMachine;
        var idleState = GetOrCreateState(machine, "Idle", new Vector3(250, 100));
        var runState = GetOrCreateState(machine, "Run", new Vector3(500, 100));
        var jumpState = GetOrCreateState(machine, "Jump", new Vector3(250, 300));
        var fallState = GetOrCreateState(machine, "Fall", new Vector3(500, 300));
        idleState.motion = idle;
        runState.motion = run;
        jumpState.motion = jump;
        fallState.motion = fall;
        machine.defaultState = idleState;

        AddTransitionIfMissing(idleState, runState, AnimatorConditionMode.Greater, 0.01f, "Speed");
        AddTransitionIfMissing(runState, idleState, AnimatorConditionMode.Less, 0.01f, "Speed");

        if (!machine.anyStateTransitions.Any(t => t.destinationState == jumpState))
        {
            var toJump = machine.AddAnyStateTransition(jumpState);
            toJump.hasExitTime = false;
            toJump.duration = 0.05f;
            toJump.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsGrounded");
            toJump.AddCondition(AnimatorConditionMode.Greater, 0.01f, "VerticalVelocity");
        }

        AddTransitionIfMissing(jumpState, fallState, AnimatorConditionMode.Less, 0f, "VerticalVelocity");

        if (!fallState.transitions.Any(t => t.destinationState == idleState))
        {
            var fallToIdle = fallState.AddTransition(idleState);
            fallToIdle.hasExitTime = false;
            fallToIdle.duration = 0.05f;
            fallToIdle.AddCondition(AnimatorConditionMode.If, 0f, "IsGrounded");
            fallToIdle.AddCondition(AnimatorConditionMode.Less, 0.01f, "Speed");
        }

        if (!fallState.transitions.Any(t => t.destinationState == runState))
        {
            var fallToRun = fallState.AddTransition(runState);
            fallToRun.hasExitTime = false;
            fallToRun.duration = 0.05f;
            fallToRun.AddCondition(AnimatorConditionMode.If, 0f, "IsGrounded");
            fallToRun.AddCondition(AnimatorConditionMode.Greater, 0.01f, "Speed");
        }

        AssetDatabase.SaveAssets();
        AssignToPlayer();
        Debug.Log("Created PlayerAnimator with Idle, Run, Jump and Fall states.");
    }

    private static AnimationClip CreateSingleFrameClip(string name, string spritePath)
    {
        var path = $"{Folder}/{name}.anim";
        var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (existing != null)
        {
            return existing;
        }

        var sprite = AssetDatabase.LoadAllAssetsAtPath(spritePath).OfType<Sprite>().FirstOrDefault();
        if (sprite == null)
        {
            return null;
        }

        var clip = new AnimationClip { name = name, frameRate = 12f };
        var binding = EditorCurveBinding.PPtrCurve(string.Empty, typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(
            clip,
            binding,
            new[] { new ObjectReferenceKeyframe { time = 0f, value = sprite } });
        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    private static void AddTransitionIfMissing(
        AnimatorState from,
        AnimatorState to,
        AnimatorConditionMode condition,
        float threshold,
        string parameter)
    {
        if (from.transitions.Any(t => t.destinationState == to))
        {
            return;
        }

        var transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.duration = 0.05f;
        transition.AddCondition(condition, threshold, parameter);
    }

    private static AnimatorState GetOrCreateState(AnimatorStateMachine machine, string name, Vector3 position)
    {
        foreach (var child in machine.states)
        {
            if (child.state.name == name)
            {
                return child.state;
            }
        }

        return machine.AddState(name, position);
    }

    private static void AddParameterIfMissing(
        AnimatorController controller,
        string name,
        AnimatorControllerParameterType type)
    {
        if (!controller.parameters.Any(parameter => parameter.name == name))
        {
            controller.AddParameter(name, type);
        }
    }

    private static void AssignToPlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (player == null || controller == null)
        {
            return;
        }

        var animator = player.GetComponent<Animator>();
        if (animator == null)
        {
            animator = Undo.AddComponent<Animator>(player);
        }

        if (animator.runtimeAnimatorController != controller)
        {
            animator.runtimeAnimatorController = controller;
            EditorUtility.SetDirty(player);
            EditorSceneManager.MarkSceneDirty(player.scene);
        }
    }
}
