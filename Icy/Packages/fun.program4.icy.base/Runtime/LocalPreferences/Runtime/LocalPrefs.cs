// Made by Neonagee https://github.com/Neonagee/LocalPreferences
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Neonagee.LocalPreferences;

public sealed class LocalPrefs : ScriptableObject
{
    public static readonly string defaultFileName = "temp";
    public static readonly string filesExtension = ".xg";
    public string FilesPath => Application.persistentDataPath + "/";
    public static string currentFile;

	public static string KeyPrefix { get; private set; } = null;

    // Rijndael
                                 //01234567890123456789012345678901 - key must be 32 chars
    const string ENCRYPTION_KEY = "zK3rN5m#DhX8[CYE%'Q={?M(#`-eqSSS";
    const string ENCRYPTION_SYMBOL = "_DI";
    public bool enableEncryption = true;
    public bool autoSaveOnQuit = true;
    static readonly Rijndael crypto = new Rijndael();

    // Delegates
    public delegate void OnLoadError(string fileName, string filePath = "");
    public delegate void OnLoadFinish();
    public delegate void OnSaveFinish();
    public static OnLoadError onLoadError;
    public static OnLoadFinish onLoadFinish;
    public static OnSaveFinish onSaveFinish;

    static void SaveToFile(string fileName, bool encrypt)
    {
		if (string.IsNullOrEmpty(fileName))
			fileName = defaultFileName;

		if (fileName.Contains(ENCRYPTION_SYMBOL))
            fileName = fileName.Replace(ENCRYPTION_SYMBOL, "");

        string fullName = fileName + filesExtension;
        string fullCryptoName = fileName + ENCRYPTION_SYMBOL + filesExtension;
        string filePath = Data.FilesPath + fullName;
        string cryptoFilePath = Data.FilesPath + fullCryptoName;

        string json = JsonUtility.ToJson(Data);

        if (encrypt)
        {
            if (File.Exists(filePath)) // Delete old file
            {
                File.Delete(filePath);
                //Debug.Log("delete old " + fullName);
            }
            byte[] encryptedData = crypto.Encrypt(json, ENCRYPTION_KEY);
            File.WriteAllBytes(cryptoFilePath, encryptedData);
            //Debug.Log("save encrypted " + fullCryptoName);
            onSaveFinish?.Invoke();
        }
        else
        {
            if (File.Exists(cryptoFilePath)) // Delete old encrypted file
            {
                File.Delete(cryptoFilePath);
                //Debug.Log("delete crypto " + fullCryptoName);
            }
            File.WriteAllText(filePath, json);
            //Debug.Log("save " + fullName);
            onSaveFinish?.Invoke();
        }
    }

	/// <summary>
	/// 为key设一个前缀；
	/// 这在按账号区分本地记录数据的时候很有用
	/// </summary>
	public static void SetKeyPrefix(string prefix)
	{
		KeyPrefix = prefix;
	}

	/// <summary>
	/// 清除掉key前缀
	/// </summary>
	public static void ClearKeyPrefix()
	{
		KeyPrefix = null;
	}

    public static void Save(string fileName = null)
    {
        SaveToFile(fileName, Data.enableEncryption);
    }

    public static void Load(string fileName, bool encrypt = false)
    {
        if (fileName.Contains(ENCRYPTION_SYMBOL))
        {
            fileName = fileName.Replace(ENCRYPTION_SYMBOL, "");
        }
        string fullName = fileName + filesExtension;
        string fullCryptoName = fileName + ENCRYPTION_SYMBOL + filesExtension;
        string filePath = Data.FilesPath + fullName;
        string cryptoFilePath = Data.FilesPath + fullCryptoName;

        string json;
        bool normalFileExists = File.Exists(filePath);
        bool cryptoFileExists = File.Exists(cryptoFilePath);

        if (normalFileExists) // Not encrypted file exists
        {
            Data.enableEncryption = false;
            try
            {
                json = File.ReadAllText(filePath);
                JsonUtility.FromJsonOverwrite(json, Data);
                currentFile = fileName;

                if (encrypt) // Save loaded file as encrypted
                {
                    Data.enableEncryption = true;
                    Save(fileName);
                    currentFile = fileName;
                }
            }
            catch // The file is damaged or encrypted
            {
                json = Decrypt(filePath); // Try to decrypt
                if (json != "")
                {
                    JsonUtility.FromJsonOverwrite(json, Data);
                    currentFile = fileName;
                    onLoadFinish?.Invoke();
                }
                else // File is damaged
                    onLoadError?.Invoke(fullCryptoName, filePath);
            }
            
        }
        if (cryptoFileExists) // File is encrypted
        {
            if (normalFileExists && !encrypt) // We don't need encrypted file - delete it
            {
                File.Delete(cryptoFilePath);
            }
            else
            {
                Data.enableEncryption = true;
                json = Decrypt(cryptoFilePath);
                if (json != "")
                {
                    JsonUtility.FromJsonOverwrite(json, Data);
                    currentFile = fileName;
                }
                else // File is marked as encrypted, but decryption has been failed
                {
                    try
                    {
                        json = File.ReadAllText(cryptoFilePath); // Try to read it straightforward
                        JsonUtility.FromJsonOverwrite(json, Data);
                        currentFile = fileName;
                    }
                    catch // File is damaged?
                    {
                        onLoadError?.Invoke(fullCryptoName, filePath);
                    }
                }
                currentFile = fileName;
            }
        }
        if(!normalFileExists && !cryptoFileExists) // No file found, create new empty one
        {
            ClearAll();
            Save(fileName);
            currentFile = fileName;
        }
    }

    static string Decrypt(string filePath)
    {
        byte[] encryptedData = File.ReadAllBytes(filePath);
        return crypto.Decrypt(encryptedData, ENCRYPTION_KEY);
    }
    public static void DeleteFile(string fileName)
    {
        string filePath = Data.FilesPath + fileName + filesExtension;
        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    static LocalPrefs m_Data;
    public static LocalPrefs Data
    {
        get
        {
            if (m_Data != null)
                return m_Data;

            m_Data = CreateInstance<LocalPrefs>();

            Load(defaultFileName);
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(m_Data);
                if(m_Data.autoSaveOnQuit)
                    Application.wantsToQuit += SaveOnQuit;
            }
            m_Data.Initialize();

            return m_Data;
        }
    }
    static bool SaveOnQuit()
    {
        Save(currentFile);
        return true;
    }

    public Dictionary<Type, IPrefs> prefs = new Dictionary<Type, IPrefs>();
    public void Initialize()
    {
        AddPref(bools);
        AddPref(ints);
        AddPref(longs);
		AddPref(floats);
        AddPref(vector2);
        AddPref(vector3);
        AddPref(vector4);
        AddPref(strings);
    }
    void AddPref<T>(T pref) where T : IPrefs
    {
        prefs.Add(pref.type, pref);
    }
    public PrefsBool bools = new PrefsBool();
    public PrefsInt ints = new PrefsInt();
    public PrefsLong longs = new PrefsLong();
	public PrefsFloat floats = new PrefsFloat();
    public PrefsVector2 vector2 = new PrefsVector2();
    public PrefsVector3 vector3 = new PrefsVector3();
    public PrefsVector4 vector4 = new PrefsVector4();
    public PrefsString strings = new PrefsString();

    // Bool
    public static bool GetBool(string key, bool defaultValue = default)
    {
        return Data.bools.GetPref(key, defaultValue);
    }
    public static bool SetBool(string key, bool value)
    {
        return Data.bools.SetPref(key, value);
    }

    // Integer
    public static int GetInt(string key, int defaultValue = default)
    {
        return Data.ints.GetPref(key, defaultValue);
    }
    public static int SetInt(string key, int value)
    {
        return Data.ints.SetPref(key, value);
    }

	// Long
	public static long GetLong(string key, long defaultValue = default)
	{
		return Data.longs.GetPref(key, defaultValue);
	}
	public static long SetLong(string key, long value)
	{
		return Data.longs.SetPref(key, value);
	}

	// Float
	public static float GetFloat(string key, float defaultValue = default)
    {
        return Data.floats.GetPref(key, defaultValue);
    }
    public static float SetFloat(string key, float value)
    {
        return Data.floats.SetPref(key, value);
    }

    // Vector2
    public static Vector2 GetVector2(string key, Vector2 defaultValue = default)
    {
        return Data.vector2.GetPref(key, defaultValue);
    }
    public static Vector2 SetVector2(string key, Vector2 value)
    {
        return Data.vector2.SetPref(key, value);
    }

    // Vector3
    public static Vector3 GetVector3(string key, Vector3 defaultValue = default)
    {
        return Data.vector3.GetPref(key, defaultValue);
    }
    public static Vector3 SetVector3(string key, Vector3 value)
    {
        return Data.vector3.SetPref(key, value);
    }

    // Vector4
    public static Vector4 GetVector4(string key, Vector4 defaultValue = default)
    {
        return Data.vector4.GetPref(key, defaultValue);
    }
    public static Vector4 SetVector4(string key, Vector4 value)
    {
        return Data.vector4.SetPref(key, value);
    }

    // String
    public static string GetString(string key, string defaultValue = default)
    {
        return Data.strings.GetPref(key, defaultValue);
    }
    public static string SetString(string key, string value = default)
    {
        return Data.strings.SetPref(key, value);
    }

    /// <summary>Returns true if key exist in preferences.</summary>
    public static bool HasKey(string key)
    {
        foreach (var pref in Data.prefs.Values)
            if (pref.ContainsKey(key))
                return true;
        return false;
    }
    /// <summary>Clear all keys and values from all preferences. Use with caution.</summary>
    public static void ClearAll()
    {
        foreach (var pref in Data.prefs.Values)
            pref.ClearAll();
    }
    /// <summary>Remove key and it's value from preferences.</summary>
    public static bool RemoveKey(string key)
    {
        bool removed = false;
        foreach (var pref in Data.prefs.Values)
            if (pref.RemoveKey(key))
                removed = true;
        return removed;
    }


    // Generic functions
    /// <summary>Returns true if key exist in given preference.</summary>
    public static bool HasKey<T>(string key)
    {
        Type t = typeof(T);
        bool isSupported = false;
        if (Data.prefs.TryGetValue(t, out IPrefs pref))
        {
            isSupported = true;
            if (pref.ContainsKey(key))
                return true;
        }
        if (!isSupported)
            Debug.LogError(TypeIsNotSupported("LocalPrefs HasKey<T>", t));
        return false;
    }
    /// <summary>Remove key and it's value from given preference.</summary>
    public static bool RemoveKey<T>(string key)
    {
        Type t = typeof(T);
        if (Data.prefs.TryGetValue(t, out IPrefs pref))
        {
            if (pref.RemoveKey(key))
            {
                pref.RemoveKey(key);
                return true;
            }
        }
        Debug.LogError(TypeIsNotSupported("LocalPrefs RemoveKey<T>", t));
        return false;
    }
    /// <summary>Clear all keys and values from given preference. Use with caution.</summary>
    public static void ClearAll<T>()
    {
        Type t = typeof(T);
        if (Data.prefs.TryGetValue(t, out IPrefs pref))
        {
            pref.ClearAll();
            return;
        }
        Debug.LogError(TypeIsNotSupported("LocalPrefs ClearAll", t));
    }
    /// <summary>Find key in all preferences and change it.</summary>
    public static string ChangeKey(string oldKey, string newKey)
    {
        foreach (var pref in Data.prefs.Values)
            pref.ChangeKey(oldKey, newKey);
        return newKey;
    }
    /// <summary>Find key in given preference and change it.</summary>
    public static string ChangeKey<T>(string oldKey, string newKey)
    {
        Type t = typeof(T);
        if (Data.prefs.TryGetValue(t, out IPrefs pref))
        {
            return pref.ChangeKey(oldKey, newKey);
        }
        Debug.LogError(TypeIsNotSupported("LocalPrefs ChangeKey", t));
        return oldKey;
    }
    /// <summary>Find first key with given value and change it to new one.
    /// <para>This operation is much slower that changing by previous key. Use only if performance is not a consideration.</para></summary>
    public static string ChangeKey<T>(T value, string newKey)
    {
        Type t = typeof(T);
        if (Data.prefs.TryGetValue(t, out IPrefs pref))
        {
            string key = pref.KeyByValue(value);
            return pref.ChangeKey(key, newKey);
        }
        Debug.LogError(TypeIsNotSupported("LocalPrefs ChangeKey<T>", t));
        return default;
    }
    /// <summary>Look up for one key with given value and removes it.<para />
    /// Returns true if key is found.<para />
    /// This operation is slow, don't use it constantly.</summary>
    public static bool RemoveKeyByValue<T>(T value)
    {
        Type t = typeof(T);
        if (Data.prefs.TryGetValue(t, out IPrefs pref))
        {
            string key = pref.KeyByValue(value);
            if (key != default)
                pref.RemoveKey(key);
            return key != default;
        }
        Debug.LogError(TypeIsNotSupported("LocalPrefs RemoveKeyByValue", t));
        return false;
    }
    /// <summary>Look up for keys with given value and remove them.<para /> 
    /// Returns true if at least one key is found.
    /// <para>This operation is slow, don't use it constantly.</para></summary>
    public static bool RemoveKeysByValue<T>(T value)
    {
        Type t = typeof(T);
        if (Data.prefs.TryGetValue(t, out IPrefs pref))
        {
            List<string> keys = pref.KeysByValue(value);
            pref.RemoveKeys(keys);
            return keys.Count > 0;
        }
        Debug.LogError(TypeIsNotSupported("LocalPrefs RemoveKeysByValue", t));
        return false;
    }
    /// <summary>Find value in this preference by key and set new value to it. 
    public static T Set<T>(string key, T value)
    {
        Type t = typeof(T);
        if (Data.prefs.TryGetValue(t, out IPrefs pref))
        {
            pref.SetPref(key, value);
            return value;
        }
        Debug.LogError("LocalPrefs: Trying to Set not supported type (" + t.Name + ")");
        return default;
    }
    /// <summary>Return value of key in this preference.
    public static T Get<T>(string key, T defaultValue = default)
    {
        Type t = typeof(T);
        if (Data.prefs.TryGetValue(t, out IPrefs pref))
        {
            return (T)pref.GetPref(key, defaultValue);
        }
        Debug.LogError("LocalPrefs: Trying to Get not supported type (" + t.Name + ")");
        return default;
    }
    /// <summary>Returns the count of keys presented in this preference.</summary>
    public static int KeysCount<T>()
    {
        Type t = typeof(T);
        if (Data.prefs.TryGetValue(t, out IPrefs pref))
        {
            return pref.Count;
        }
        Debug.LogError(TypeIsNotSupported("LocalPrefs KeysCount", t));
        return 0;
    }
    /// <summary>Returns every existing key in this preference. Consider to use it only once.</summary>
    public static string[] AllKeys<T>()
    {
        Type t = typeof(T);
        if (Data.prefs.TryGetValue(t, out IPrefs pref))
        {
            return pref.AllKeys(t);
        }
        Debug.LogError(TypeIsNotSupported("LocalPrefs GetAllKeys", t));
        return null;
    }
    /// <summary>Returns every existing value of this type. It's a very slow operation, consider to use it only once.</summary>
    public static List<T> AllValues<T>()
    {
        Type t = typeof(T);
        if (Data.prefs.TryGetValue(t, out IPrefs pref))
        {
            List<object> tempValues = pref.AllValues(t);
            List<T> allValues = new List<T>(tempValues.Count);
            for (int v = 0; v < tempValues.Count; v++)
                allValues.Add((T)tempValues[v]);
            return allValues;
        }
        Debug.LogError(TypeIsNotSupported("LocalPrefs GetAllKeys", t));
        return null;
    }
    static string TypeIsNotSupported(string methodName, Type t)
    {
        return methodName + ": Type \"" + t.Name + "\" is not supported.";
    }
}
