using UnityEditor.Animations;
using UnityEngine;

public class Char_Animation : MonoBehaviour
{

    public Animator animator;
    public Char_State state;

    [SerializeField] private AnimationClip skillQClip;
    [SerializeField] private AnimationClip skillWClip;
    [SerializeField] private AnimationClip skillEClip;
    [SerializeField] private AnimationClip skillRClip;
    private AnimatorOverrideController overrideController;
    void Awake()
    {
        overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = overrideController;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetSkillAnimationClip();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    void SetSkillAnimationClip()
    {
        overrideController[skillQClip] = DataBase.Instance.mySkillKeyMap["q"].animationClip;
        overrideController[skillWClip] = DataBase.Instance.mySkillKeyMap["w"].animationClip;
        overrideController[skillEClip] = DataBase.Instance.mySkillKeyMap["e"].animationClip;
        overrideController[skillRClip] = DataBase.Instance.mySkillKeyMap["r"].animationClip;
    }
    public void SetAnim(string anim)
    {
        //animator.SetBool(anim);
    }
}
