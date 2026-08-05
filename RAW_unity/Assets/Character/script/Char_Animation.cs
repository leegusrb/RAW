using UnityEngine;

public class Char_Animation : MonoBehaviour
{

    [SerializeField] private Animator animator;
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
        overrideController["skill_Q"] = DataBase.Instance.mySkillKeyMap["q"].animationClip;
        overrideController["skill_W"] = DataBase.Instance.mySkillKeyMap["w"].animationClip;
        overrideController["skill_E"] = DataBase.Instance.mySkillKeyMap["e"].animationClip;
        overrideController["skill_R"] = DataBase.Instance.mySkillKeyMap["r"].animationClip;
    }
    public void SetAnim(string anim)
    {
        //animator.SetBool(anim);
    }
}
