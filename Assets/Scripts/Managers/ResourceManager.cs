using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Hidenet.Managers
{
    public class ResourceManager
    {
        private static ResourceManager _instance;
        public static ResourceManager Instance => _instance ??= new ResourceManager();

        private readonly Dictionary<string, UnityEngine.Object> _cache = new();

        public void LoadAsync<T>(string key, Action<T> callback = null) where T : UnityEngine.Object
        {
            if (_cache.TryGetValue(key, out var cached))
            {
                callback?.Invoke(cached as T);
                return;
            }

            Addressables.LoadAssetAsync<T>(key).Completed += (op) =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    _cache[key] = op.Result;
                    callback?.Invoke(op.Result);
                }
                else
                {
                    Debug.LogWarning($"[ResourceManager] Failed to load: {key}");
                }
            };
        }

        public void LoadAllAsync<T>(string label, Action<string, int, int> callback) where T : UnityEngine.Object
        {
            Addressables.LoadResourceLocationsAsync(label, typeof(T)).Completed += (op) =>
            {
                if (op.Status != AsyncOperationStatus.Succeeded) return;

                int loadCount = 0;
                int totalCount = op.Result.Count;

                foreach (var result in op.Result)
                {
                    LoadAsync<T>(result.PrimaryKey, (obj) =>
                    {
                        loadCount++;
                        callback?.Invoke(result.PrimaryKey, loadCount, totalCount);
                    });
                }
            };
        }

        public void Release(string key)
        {
            if (!_cache.TryGetValue(key, out var resource)) return;
            Addressables.Release(resource);
            _cache.Remove(key);
        }

        public void ReleaseAll()
        {
            foreach (var resource in _cache.Values)
                Addressables.Release(resource);
            _cache.Clear();
        }
    }
}
