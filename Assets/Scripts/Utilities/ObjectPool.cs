using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic object pool for reusing GameObjects to reduce instantiation overhead
/// </summary>
/// <typeparam name="T">The component type to pool (must be MonoBehaviour)</typeparam>
public class ObjectPool<T> where T : MonoBehaviour
{
    private readonly Queue<T> pool = new Queue<T>();
    private readonly GameObject prefab;
    private readonly Transform parent;
    private readonly int maxPoolSize;
    
    /// <summary>
    /// Creates a new object pool
    /// </summary>
    /// <param name="prefab">The prefab to instantiate</param>
    /// <param name="initialSize">Initial number of objects to create</param>
    /// <param name="maxSize">Maximum pool size (0 = unlimited)</param>
    /// <param name="parent">Parent transform for pooled objects</param>
    public ObjectPool(GameObject prefab, int initialSize = 5, int maxSize = 20, Transform parent = null)
    {
        this.prefab = prefab;
        this.maxPoolSize = maxSize;
        this.parent = parent;
        
        // Pre-populate the pool
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewObject();
        }
    }
    
    /// <summary>
    /// Gets an object from the pool or creates a new one if pool is empty
    /// </summary>
    /// <returns>A pooled object of type T</returns>
    public T Get()
    {
        T obj;
        
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            obj = CreateNewObject();
        }
        
        // Activate the object
        if (obj != null)
        {
            obj.gameObject.SetActive(true);
        }
        
        return obj;
    }
    
    /// <summary>
    /// Returns an object to the pool for reuse
    /// </summary>
    /// <param name="obj">The object to return to the pool</param>
    public void Return(T obj)
    {
        if (obj == null) return;
        
        // Deactivate the object
        obj.gameObject.SetActive(false);
        
        // Reset position and rotation
        obj.transform.position = Vector3.zero;
        obj.transform.rotation = Quaternion.identity;
        
        // Add back to pool if under max size
        if (maxPoolSize == 0 || pool.Count < maxPoolSize)
        {
            pool.Enqueue(obj);
        }
        else
        {
            // Pool is full, destroy the object
            Object.Destroy(obj.gameObject);
        }
    }
    
    /// <summary>
    /// Creates a new object instance
    /// </summary>
    /// <returns>New object of type T</returns>
    private T CreateNewObject()
    {
        GameObject instance = Object.Instantiate(prefab, parent);
        instance.SetActive(false); // Start inactive
        
        T component = instance.GetComponent<T>();
        if (component == null)
        {
            Debug.LogError($"ObjectPool: Prefab {prefab.name} does not have component of type {typeof(T).Name}");
            Object.Destroy(instance);
            return null;
        }
        
        return component;
    }
    
    /// <summary>
    /// Gets the current number of objects in the pool
    /// </summary>
    public int PoolCount => pool.Count;
    
    /// <summary>
    /// Clears the pool and destroys all pooled objects
    /// </summary>
    public void Clear()
    {
        while (pool.Count > 0)
        {
            T obj = pool.Dequeue();
            if (obj != null)
            {
                Object.Destroy(obj.gameObject);
            }
        }
    }
}