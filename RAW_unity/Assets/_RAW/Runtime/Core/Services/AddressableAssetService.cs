using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[DisallowMultipleComponent]
public class AddressableAssetService : MonoBehaviour
{
	public static AddressableAssetService Instance {get; private set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;

		if (transform.parent != null)
		{
			Debug.LogError(
				"AddressableAssetService must be placed on a root GameObject. " +
				"Move this GameObject to the top level of the Hierarchy."
			);
			return;
		}
		
		DontDestroyOnLoad(gameObject);
	}

	public AsyncOperationHandle<Sprite> LoadSprite(string address)
	{
		return Addressables.LoadAssetAsync<Sprite>(address);
	}

	public void ReleaseSprite(AsyncOperationHandle<Sprite> handle)
	{
		if (handle.IsValid())
		{
			Addressables.Release(handle);
		}
	}
}
