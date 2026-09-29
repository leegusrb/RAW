using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterState))]
public class SkillTarget : MonoBehaviour
{
    [SerializeField]
     private Transform hitPosition;

    [SerializeField]
    private CharacterState characterState;

    public Vector3 HitPosition
    {
        get
        {
            return hitPosition.position;
        }
    }

    public void TakeDamage(float damage)
    {
        characterState.TakeDamage(damage);
    }
}
