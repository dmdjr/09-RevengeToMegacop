using System.IO;

using UnityEditor;

using UnityEngine;

/// <summary>
/// 스킬 세이브 파일 관리 에디터 유틸리티.
/// </summary>
public static class SkillSaveUtility
{
    private static string SaveFilePath =>
        Path.Combine(Application.persistentDataPath, "skills.json");

    [MenuItem("Tools/Skill/Clear Skill Save")]
    private static void ClearSkillSave()
    {
        if (!File.Exists(SaveFilePath))
        {
            Debug.Log("SkillSaveUtility: skills.json 파일이 없습니다.");
            return;
        }

        File.Delete(SaveFilePath);
        Debug.Log($"SkillSaveUtility: skills.json 삭제 완료. ({SaveFilePath})");
    }
}
