using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

/// <summary>
/// 스킬 해금 상태를 런타임에 관리하는 싱글톤 매니저.
/// 씬에 별도 GameObject로 배치한다.
/// </summary>
public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    [SerializeField] private SkillDatabase database;

    /// <summary>
    /// 게임 시작 시 기본으로 해금할 스킬 목록.
    /// 개발 중에는 모든 스킬을 넣어두고, 스킬트리 UI 완성 후 비운다.
    /// </summary>
    [SerializeField] private SkillId[] initiallyUnlockedSkills;

    private readonly HashSet<SkillId> unlockedSkills = new();

    /// <summary>스킬이 해금될 때 발행된다. 스킬트리 UI 등에서 구독한다.</summary>
    public event Action<SkillId> OnSkillUnlocked;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        foreach (var id in initiallyUnlockedSkills)
        {
            if (id != SkillId.None)
                unlockedSkills.Add(id);
        }
    }

    /// <summary>
    /// 해당 스킬이 해금되어 있는지 확인한다.
    /// </summary>
    public bool IsUnlocked(SkillId id) => unlockedSkills.Contains(id);

    /// <summary>
    /// 스킬 해금을 시도한다. 선행 조건이 모두 충족되면 해금하고 true를 반환한다.
    /// 이미 해금됐거나 선행 조건 미충족 시 false를 반환한다.
    /// </summary>
    public bool TryUnlock(SkillId id)
    {
        if (id == SkillId.None) return false;
        if (unlockedSkills.Contains(id)) return false;

        if (database != null)
        {
            SkillData data = database.GetSkill(id);
            if (data != null)
            {
                foreach (var prerequisite in data.Prerequisites)
                {
                    if (!unlockedSkills.Contains(prerequisite.SkillId))
                        return false;
                }
            }
        }

        unlockedSkills.Add(id);
        OnSkillUnlocked?.Invoke(id);
        return true;
    }

    /// <summary>현재 해금 상태를 세이브 데이터로 변환한다.</summary>
    public SkillSaveData ToSaveData() => new() { unlockedIds = unlockedSkills.ToArray() };

    /// <summary>세이브 데이터를 불러와 해금 상태를 복원한다.</summary>
    public void LoadSaveData(SkillSaveData data)
    {
        unlockedSkills.Clear();
        if (data.unlockedIds == null) return;
        foreach (var id in data.unlockedIds)
        {
            if (id != SkillId.None)
                unlockedSkills.Add(id);
        }
    }
}

/// <summary>스킬 해금 상태의 세이브/로드용 직렬화 구조체.</summary>
[Serializable]
public struct SkillSaveData
{
    public SkillId[] unlockedIds;
}
