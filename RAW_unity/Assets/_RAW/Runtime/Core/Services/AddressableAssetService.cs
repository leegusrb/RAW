using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableAssetService : MonoBehaviour
{
	public void LoadSprite(string address, Action<Sprite> onSucceeded, Action<string> onFailed = null)
	{
		Addressables.LoadAssetAsync<Sprite>(address).Completed += handle =>
		{
			if (handle.Status == AsyncOperationStatus.Succeeded)
			{
				onSucceeded?.Invoke(handle.Result);
			}
			else
			{
				onFailed?.Invoke(address);
			}
		};
	}
}
