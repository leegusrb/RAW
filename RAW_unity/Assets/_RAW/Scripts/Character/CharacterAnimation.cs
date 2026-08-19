using UnityEngine;

[DisallowMultipleComponent]
public class CharacterAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;

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

    public bool PlaySkill(KeyMapping slot, SkillSpec skill)
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

        if (!TryGetSlotAnimationNames(slot, out string clipName, out string triggerName))
        {
            Debug.LogWarning($"{slot} 슬롯의 Animator 바인딩이 없습니다.", this);
            return false;
        }

        overrideController[clipName] = skill.animationClip;
        animator.SetTrigger(triggerName);
        return true;
    }

    private static bool TryGetSlotAnimationNames(
        KeyMapping slot,
        out string clipName,
        out string triggerName
    )
    {
        switch (slot)
        {
            case KeyMapping.Q:
                clipName = "skill_Q";
                triggerName = "q";
                return true;

            case KeyMapping.W:
                clipName = "skill_W";
                triggerName = "w";
                return true;

            case KeyMapping.E:
                clipName = "skill_E";
                triggerName = "e";
                return true;

            case KeyMapping.R:
                clipName = "skill_R";
                triggerName = "r";
                return true;

            default:
                clipName = null;
                triggerName = null;
                return false;
        }
    }
}
