using UnityEngine;

/// <summary>
/// Prepara el campo al empezar la pelea del jefe: apaga el spawner y el director de dificultad
/// y elimina a los enemigos normales, para que la primera etapa sea realmente el jefe solo.
/// Los refuerzos de cada etapa los pone BossSummons.
/// </summary>
public static class BossEncounter
{
    public static void Begin(GameObject boss)
    {
        foreach (var spawner in Object.FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None))
            spawner.enabled = false;

        foreach (var director in Object.FindObjectsByType<DifficultyDirector>(FindObjectsSortMode.None))
            director.enabled = false;   // al apagarse deja DifficultyScale en x1

        int cleared = 0;
        foreach (var chase in Object.FindObjectsByType<EnemyChase>(FindObjectsSortMode.None))
        {
            if (chase.gameObject == boss) continue;
            Object.Destroy(chase.gameObject);   // sin loot ni EnemyKilled: no cuenta como kill ni da XP
            cleared++;
        }

        Debug.Log($"[Jefe] Campo limpiado: {cleared} enemigos eliminados; spawner y director apagados.");
    }
}
