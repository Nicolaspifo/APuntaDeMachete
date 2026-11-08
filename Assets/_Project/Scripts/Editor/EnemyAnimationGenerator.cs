using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Genera los clips de animacion y el Animator Controller de un enemigo a partir de su hoja de sprites.
/// Uso: selecciona el PNG de la hoja en el Project y elige el menu Tools > Enemigos.
/// </summary>
public static class EnemyAnimationGenerator
{
    private const int CellSize = 32;

    // Orden de las 4 filas de cada animacion en la hoja (tus hojas: las filas de arriba van derecha, luego izquierda).
    private static readonly string[] DirectionNames = { "DownLeft", "DownRight", "UpRight", "UpLeft" };

    // Posicion de cada direccion en el Blend Tree (MoveX, MoveY). Mismo orden que DirectionNames.
    private static readonly Vector2[] DirectionPositions =
    {
        new Vector2(-1f, -1f), new Vector2(1f, -1f), new Vector2(1f, 1f), new Vector2(-1f, 1f)
    };

    private class ClipSet
    {
        public readonly string name;
        public readonly int firstRow;      // fila de la hoja donde empieza (0 = la de arriba)
        public readonly int frameCount;    // cuantos frames de esa fila se usan
        public readonly float fps;
        public readonly bool loop;
        public AnimationClip[] clips;      // uno por direccion

        public ClipSet(string name, int firstRow, int frameCount, float fps, bool loop)
        {
            this.name = name;
            this.firstRow = firstRow;
            this.frameCount = frameCount;
            this.fps = fps;
            this.loop = loop;
        }
    }

    [MenuItem("Tools/Enemigos/Zancudo - generar clips y controller")]
    private static void GenerateZancudo()
    {
        Generate("Zancudo", new[]
        {
            new ClipSet("Fly",    0, 4, 8f, true),
            new ClipSet("Attack", 4, 5, 8f, false),
            new ClipSet("Death",  8, 4, 8f, false),
        }, idleName: null, moveName: "Fly");
    }

    [MenuItem("Tools/Enemigos/Hipopotamo - generar clips y controller")]
    private static void GenerateHipopotamo()
    {
        Generate("Hipopotamo", new[]
        {
            new ClipSet("Idle",   0, 1, 1f,  true),    // 1 solo frame: el primero de caminar
            new ClipSet("Walk",   0, 8, 12f, true),
            new ClipSet("Attack", 4, 6, 10f, false),
            new ClipSet("Death",  8, 4, 8f,  false),
        }, idleName: "Idle", moveName: "Walk");
    }

    private static void Generate(string enemyName, ClipSet[] sets, string idleName, string moveName)
    {
        Texture2D sheet = GetSelectedSheet();
        if (sheet == null)
        {
            EditorUtility.DisplayDialog("Generar animaciones",
                "Selecciona primero la hoja de sprites (el PNG) en la ventana Project.", "OK");
            return;
        }

        string sheetPath = AssetDatabase.GetAssetPath(sheet);
        string folder = Path.GetDirectoryName(sheetPath).Replace('\\', '/');
        string clipsFolder = folder + "/Clips";
        if (!AssetDatabase.IsValidFolder(clipsFolder))
            AssetDatabase.CreateFolder(folder, "Clips");

        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(sheetPath).OfType<Sprite>().ToArray();
        if (sprites.Length == 0)
        {
            EditorUtility.DisplayDialog("Generar animaciones",
                "La imagen no tiene sprites. Revisa que Sprite Mode sea Multiple y que ya hayas rebanado la hoja.", "OK");
            return;
        }

        // Agrupa los sprites por fila segun su posicion en la hoja (fila 0 = la de arriba)
        // y los ordena de izquierda a derecha.
        int textureHeight = sheet.height;
        Dictionary<int, List<Sprite>> rows = sprites
            .GroupBy(s => Mathf.FloorToInt((textureHeight - s.rect.center.y) / CellSize))
            .ToDictionary(g => g.Key, g => g.OrderBy(s => s.rect.center.x).ToList());

        foreach (ClipSet set in sets)
        {
            set.clips = new AnimationClip[4];

            for (int d = 0; d < 4; d++)
            {
                int row = set.firstRow + d;

                if (!rows.TryGetValue(row, out List<Sprite> frames) || frames.Count < set.frameCount)
                {
                    int found = frames != null ? frames.Count : 0;
                    Debug.LogError($"[EnemyAnimationGenerator] La fila {row + 1} deberia tener al menos {set.frameCount} frames " +
                                   $"para {set.name}, pero tiene {found}. Revisa el rebanado (32x32, Keep Empty Rects apagado).");
                    return;
                }

                string clipPath = $"{clipsFolder}/{set.name}_{DirectionNames[d]}.anim";
                set.clips[d] = BuildClip(clipPath, frames.Take(set.frameCount).ToList(), set.fps, set.loop);
            }
        }

        AssetDatabase.SaveAssets();
        BuildController($"{folder}/{enemyName}.controller", sets, idleName, moveName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[EnemyAnimationGenerator] {enemyName}: clips en '{clipsFolder}'.");
    }

    private static Texture2D GetSelectedSheet()
    {
        UnityEngine.Object selected = Selection.activeObject;
        if (selected is Sprite sprite) return sprite.texture;
        return selected as Texture2D;
    }

    /// <summary>Crea (o actualiza en su sitio) un clip que cambia el sprite frame a frame.</summary>
    private static AnimationClip BuildClip(string path, List<Sprite> frames, float fps, bool loop)
    {
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        bool isNew = clip == null;
        if (isNew) clip = new AnimationClip();

        clip.frameRate = fps;

        // Un keyframe por frame, mas uno extra al final que repite el ultimo sprite.
        // Asi el ultimo frame dura lo mismo que los demas y el clip dura frames/fps.
        var keys = new ObjectReferenceKeyframe[frames.Count + 1];
        for (int i = 0; i < frames.Count; i++)
            keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = frames[i] };
        keys[frames.Count] = new ObjectReferenceKeyframe { time = frames.Count / fps, value = frames[frames.Count - 1] };

        EditorCurveBinding binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        if (isNew) AssetDatabase.CreateAsset(clip, path);
        else EditorUtility.SetDirty(clip);

        return clip;
    }

    private static void BuildController(string path, ClipSet[] sets, string idleName, string moveName)
    {
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null)
        {
            Debug.LogWarning($"[EnemyAnimationGenerator] Los clips se actualizaron, pero '{path}' ya existe y no se toco. " +
                             "Si quieres regenerarlo, borralo y vuelve a ejecutar.");
            return;
        }

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        controller.AddParameter("MoveX", AnimatorControllerParameterType.Float);
        controller.AddParameter("MoveY", AnimatorControllerParameterType.Float);
        if (idleName != null) controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);

        AnimatorStateMachine machine = controller.layers[0].stateMachine;

        // Un estado por animacion; cada uno es un Blend Tree 2D con las 4 diagonales.
        var states = new Dictionary<string, AnimatorState>();
        foreach (ClipSet set in sets)
        {
            AnimatorState state = controller.CreateBlendTreeInController(set.name, out BlendTree tree);
            tree.blendType = BlendTreeType.SimpleDirectional2D;
            tree.blendParameter = "MoveX";
            tree.blendParameterY = "MoveY";

            for (int d = 0; d < 4; d++)
                tree.AddChild(set.clips[d], DirectionPositions[d]);

            states[set.name] = state;
        }

        AnimatorState rest = states[idleName ?? moveName];

        // Hipopotamo: Idle <-> Walk segun el parametro Speed.
        if (idleName != null)
        {
            AnimatorStateTransition toWalk = states[idleName].AddTransition(states[moveName]);
            MakeInstant(toWalk);
            toWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

            AnimatorStateTransition toIdle = states[moveName].AddTransition(states[idleName]);
            MakeInstant(toIdle);
            toIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        }

        // Cualquier estado -> Attack (trigger) y vuelve al terminar el clip.
        AnimatorStateTransition anyToAttack = machine.AddAnyStateTransition(states["Attack"]);
        MakeInstant(anyToAttack);
        anyToAttack.canTransitionToSelf = false;
        anyToAttack.AddCondition(AnimatorConditionMode.If, 0f, "Attack");

        AnimatorStateTransition attackEnd = states["Attack"].AddTransition(rest);
        attackEnd.hasExitTime = true;
        attackEnd.exitTime = 1f;
        attackEnd.hasFixedDuration = true;
        attackEnd.duration = 0f;

        // Cualquier estado -> Death (trigger). De Death no se sale.
        AnimatorStateTransition anyToDeath = machine.AddAnyStateTransition(states["Death"]);
        MakeInstant(anyToDeath);
        anyToDeath.canTransitionToSelf = false;
        anyToDeath.AddCondition(AnimatorConditionMode.If, 0f, "Die");

        machine.defaultState = rest;
        EditorUtility.SetDirty(controller);
    }

    private static void MakeInstant(AnimatorStateTransition transition)
    {
        transition.hasExitTime = false;
        transition.hasFixedDuration = true;
        transition.duration = 0f;
    }
}