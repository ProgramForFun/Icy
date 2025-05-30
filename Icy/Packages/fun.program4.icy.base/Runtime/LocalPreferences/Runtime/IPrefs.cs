using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Neonagee.LocalPreferences
{
    public struct Pref<T>
    {
        public string name;
        public T value;
        public Pref(string name, T value)
        {
            this.name = name;
            this.value = value;
        }
    }
    public interface IPrefs
    {
        Type type { get; }
        int Length { get; }
        int Count { get; }
        object SetPref(string key, object value);
        object GetPref(string key, object defaultValue);
        bool ContainsKey(string key);
        bool ContainsValue(object value);
        string ChangeKey(string oldKey, string newKey);
        string KeyByValue(object value);
        List<string> KeysByValue(object value);
        bool RemoveKey(string key);
        void RemoveKeys(List<string> keys);
        string[] AllKeys(Type type);
        List<object> AllValues(Type type);
        void ClearAll();
    }
    [Serializable]
    public class Prefs<T> : ISerializationCallbackReceiver, IPrefs
    {
        public string[] keys = new string[0];
        public T[] values = new T[0];
        public Dictionary<string, T> dictionary = new Dictionary<string, T>();
        public Pref<T> this[int index] { get { return new Pref<T>(keys[index], values[index]); } }
        public T this[string name] { get { return dictionary[name]; } set { dictionary[name] = value; } }
        public int Length { get { return keys.Length; } }
        public int Count { get { return dictionary.Count; } }
        public Type type { get; } = typeof(T);

		private string GetKey(string key)
		{
			if (key != null && !string.IsNullOrEmpty(LocalPrefs.KeyPrefix))
				key = LocalPrefs.KeyPrefix + key;
			return key;
		}

        public T GetPref(string key, T defaultValue)
        {
			key = GetKey(key);
            if (dictionary.TryGetValue(key, out T value))
                return value;
            else
                return SetPref(key, defaultValue);
        }
        public object GetPref(string key, object defaultValue)
        {
			key = GetKey(key);
            if (dictionary.TryGetValue(key, out T value))
				return value;
            else
                return SetPref(key, (T)defaultValue);
        }
        public T SetPref(string key, T newValue)
        {
			key = GetKey(key);
            if (dictionary.ContainsKey(key))
				dictionary[key] = newValue;
            else
                dictionary.Add(key, newValue);
            return newValue;
        }
        public object SetPref(string key, object newValue)
        {
			key = GetKey(key);
            if (dictionary.ContainsKey(key))
				dictionary[key] = (T)newValue;
            else
                dictionary.Add(key, (T)newValue);
            return newValue;
        }
        public string[] AllKeys(Type type)
        {
            if (this.type == type)
                return dictionary.Keys.ToArray();
            return null;
        }
        public List<object> AllValues(Type type)
        {
            if (type == this.type)
            {
                List<object> allValues = new List<object>(dictionary.Count);
                foreach (var val in dictionary.Values)
                    allValues.Add(val);
                return allValues;
            }
            return null;
        }
        public string ChangeKey(string oldKey, string newKey)
        {
			oldKey = GetKey(oldKey);
			newKey = GetKey(newKey);
			if (dictionary.TryGetValue(oldKey, out T value))
			{
                dictionary.Remove(oldKey);
                dictionary.Add(newKey, value);
                return newKey;
            }
            return oldKey;
        }
        public string KeyByValue(object value)
        {
            string key = default;
            T Value = (T)value;
            foreach (var pair in dictionary)
                if (EqualityComparer<T>.Default.Equals(pair.Value, Value))
                {
                    key = pair.Key;
                    break;
                }
            return key;
        }
        public List<string> KeysByValue(object value)
        {
            List<string> keys = new List<string>();
            T Value = (T)value;
            foreach (var pair in dictionary)
                if (EqualityComparer<T>.Default.Equals(pair.Value, Value))
                {
                    keys.Add(pair.Key);
                }
            return keys;
        }
        public bool RemoveKey(string key)
        {
			key = GetKey(key);
            if (dictionary.ContainsKey(key))
			{
                dictionary.Remove(key);
                return true;
            }
            return false;
        }
        public void RemoveKeys(List<string> keys)
        {
			for (int k = 0; k < keys.Count; k++)
			{
				if (keys[k] != default)
				{
					string key = GetKey(keys[k]);
					dictionary.Remove(key);
				}
			}
        }
        public void Add(string key, T value)
        {
			key = GetKey(key);
            dictionary.Add(key, value);
        }
        public void Remove(string key)
        {
			key = GetKey(key);
            dictionary.Remove(key);
		}
        public bool TryGetValue(string key, out T value)
        {
			key = GetKey(key);
            return dictionary.TryGetValue(key, out value);
		}
        public bool ContainsKey(string key)
        {
			key = GetKey(key);
            return dictionary.ContainsKey(key);
		}
        public bool ContainsValue(T value)
        {
            return dictionary.ContainsValue(value);
        }
        public bool ContainsValue(object value)
        {
            if (value.GetType() == type)
            {
                return dictionary.ContainsValue((T)value);
            }
            return false;
        }
        public void ClearAll()
        {
            dictionary.Clear();
            keys = new string[0];
            values = new T[0];
        }
        // Unload from dictionary to array
        public void OnBeforeSerialize()
        {
            if (dictionary.Count == 0)
                return;
            keys = new string[dictionary.Count];
            values = new T[dictionary.Count];
            int i = 0;
            foreach (var kvp in dictionary)
            {
                keys[i] = kvp.Key;
                values[i] = kvp.Value;
                i++;
            }
        }
        // Load items to dictionary
        public void OnAfterDeserialize()
        {
            dictionary = new Dictionary<string, T>();
            for (int i = 0; i < keys.Length; i++)
                dictionary.Add(keys[i], values[i]);
        }
    }
}
