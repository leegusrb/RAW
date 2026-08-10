using System;
using UnityEngine;

namespace RAW.Contracts.Skills
{
	[Serializable]
	public struct SkillUseRequest
	{
		public ulong RequestId;
		public KeyMapping Slot;

		public ulong TargetEntityId;
		public Vector2 TargetPosition;
		public Vector2 AimDirection;
	}
}