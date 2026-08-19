using UnityEngine;

[DisallowMultipleComponent]
public class CharacterAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private const string SkillPlaceholderName = "skill_cast";
    private static readonly int CastSkillHash = Animator.StringToHash("castSkill");

    private AnimatorOverrideController overrideController;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("Animator 컴포넌트를 찾을 수 없습니다.", this);
            enabled = false;
            return;
        }

        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError("Runtime Animator Controller가 연결되지 않았습니다.", this);
            enabled = false;
            return;
        }

        overrideController = new AnimatorOverrideController(
            animator.runtimeAnimatorController
        );

        animator.runtimeAnimatorController = overrideController;
    }

    public bool PlaySkill(SkillSpec skill)
    {
        if (skill == null)
            return false;

        if (overrideController == null || animator == null)
        {
            Debug.LogError("스킬 애니메이션을 실행할 Animator가 준비되지 않았습니다.", this);
            return false;
        }

        if (skill.animationClip == null)
        {
            Debug.LogWarning($"{skill.name} 스킬에 AnimationClip이 연결되지 않았습니다.", skill);
            return false;
        }

        overrideController[SkillPlaceholderName] = skill.animationClip;
        animator.SetTrigger(CastSkillHash);
        return true;
    }
}
