using System;
using UnityEngine;

namespace RAW.Network
{
	[Serializable]
	public class DefaultSkillLoadoutEntry
	{
		[SerializeField] private KeyMapping slot;
		[SerializeField] private SkillSpec skill;

		public KeyMapping Slot => slot;
		public SkillSpec Skill => skill;
	}
}
