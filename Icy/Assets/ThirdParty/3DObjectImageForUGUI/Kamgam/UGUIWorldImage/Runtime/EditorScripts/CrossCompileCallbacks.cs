#if UNITY_EDITOR
using System;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Kamgam.UGUIWorldImage
{
    /// <summary>
    /// Allows you to register a callback before compilation
    /// which is then executed automatically after compilation.<br />
    /// <br />
    /// Methods registered are usually executed in FIFO order. Though there
    /// is no guarantee that this will always be the case.
    /// <example>
    /// CrossCompileCallbacks.RegisterCallback(testCallbackA); // It can find the type automatically.
    /// CrossCompileCallbacks.RegisterCallback(typeof(YourClass), "testCallbackA");
    /// </example>
    /// </summary>
    public static class CrossCompileCallbacks
    {
        /// <summary>
        /// If set to true then the callbacks will not be called immediately but
        /// within the next editor update cycle.<br />
        /// Use this to avoid "Calling ... from assembly reloading callbacks are not supported." errors.
        /// </summary>
        public static bool DelayExecutionAfterCompilation
        {
            get => SessionState.GetBool(typeName() + ".DelayExecution", false);
            set => SessionState.SetBool(typeName() + ".DelayExecution", value);
        }

        static string typeName() => typeof(CrossCompileCallbacks).FullName;

        const string _maxIndexKey = ".MaxIndex";
        static string maxIndexKey() => typeName() + _maxIndexKey;

        const string _lastReleasedIndexKey = ".LastReleasedIndex";
        static string lastReleasedIndexKey() => typeName() + _lastReleasedIndexKey;

        const string _indexTypeKey = ".Index[{0}].Type";
        static string indexTypeKey(int index) => string.Format(typeName() + _indexTypeKey, index);

        const string _indexMethodKey = ".Index[{0}].Method";
        static string indexMethodKey(int index) => string.Format(typeName() + _indexMethodKey, index);


        static int getMaxIndex()
        {
            return SessionState.GetInt(maxIndexKey(), -1);
        }

        static int getNextIndex()
        {
            int maxIndex;

            // Try to reuse an old index (update max index if necessary)
            int reusableIndex = SessionState.GetInt(lastReleasedIndexKey(), -1);
            if (reusableIndex >= 0)
            {
                SessionState.SetInt(lastReleasedIndexKey(), -1);

                maxIndex = getMaxIndex();
                if(maxIndex < reusableIndex)
                    SessionState.SetInt(maxIndexKey(), reusableIndex);

                return reusableIndex;
            }

            // New index needed (increase max index).
            maxIndex = SessionState.GetInt(maxIndexKey(), -1);
            maxIndex++;
            SessionState.SetInt(maxIndexKey(), maxIndex);

            return maxIndex;
        }

        public static void ReleaseIndex(int index)
        {
            if (index < 0)
                return;

            SessionState.SetInt(lastReleasedIndexKey(), index);
            SessionState.EraseString(indexTypeKey(index));
            SessionState.EraseString(indexMethodKey(index));

            // Decrease or erase max index if needed.
            int maxIndex = getMaxIndex();
            if(index == maxIndex)
            {
                maxIndex--;
                if(maxIndex < 0)
                    SessionState.EraseInt(maxIndexKey());
                else
                    SessionState.SetInt(maxIndexKey(), maxIndex);
            }
        }

        public static void ReleaseAllOnType(Type type)
        {
            if (type == null)
                return;

            int maxIndex = getMaxIndex();
            for (int i = maxIndex; i >= 0; i--)
            {
                string typeName;
                GetCallbackInfo(i, out typeName, out _);

                if(typeName == type.FullName)
                {
                    ReleaseIndex(i);
                }
            }
        }

        /// <summary>
        /// Registers a callback and returns an index >= 0 on success and -1 on failure.
        /// </summary>
        /// <param name="callback">A static method without any parameters.</param>
        /// <returns></returns>
        public static int RegisterCallback(System.Action callback)
        {
            if (callback == null)
                return -1;

            var methodInfo = callback.GetMethodInfo();
            if (methodInfo == null)
                return -1;

            if (!methodInfo.IsStatic)
            {
                Debug.Log("Method needs to be static.");
                return -1;
            }

            return RegisterCallback(methodInfo.DeclaringType, methodInfo.Name);
        }

        /// <summary>
        /// Registers a callback and returns an index >= 0 on success and -1 on failure.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="staticMethodName">A static method without any parameters.</param>
        /// <returns></returns>
        public static int RegisterCallback(Type type, string staticMethodName)
        {
            if (type == null || string.IsNullOrEmpty(staticMethodName))
            {
                Debug.Assert(type != null);
                Debug.Assert(staticMethodName != null);
                return -1;
            }

            // Check if methods has any parameters (that's not supported)
            try
            {
                var flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
                var methodInfo = type.GetMethod(staticMethodName, flags);
                if (methodInfo == null)
                {
                    Debug.LogError("No static method '" + staticMethodName + "' found in '" + type.FullName + "'.");
                    return -1;
                }
                if (methodInfo.GetParameters().Length > 0)
                {
                    Debug.Assert(methodInfo.GetParameters().Length == 0);
                    return -1;
                } 
            }
            catch (System.Exception e)
            {
                Debug.LogError($"CrossCompileCallbacks: Error while checking '{staticMethodName}' method parameters. Error:\n" + e.Message);
            }

            int index = getNextIndex();
            SessionState.SetString(indexTypeKey(index), type.FullName);
            SessionState.SetString(indexMethodKey(index), staticMethodName);

            return index;
        }

        public static void GetCallbackInfo(int index, out string typeName, out string methodName)
        {
            typeName = SessionState.GetString(indexTypeKey(index), null);
            methodName = SessionState.GetString(indexMethodKey(index), null);
        }

        [DidReloadScripts(-1)]
        static void onAfterCompilation()
        {
            if (DelayExecutionAfterCompilation)
            {
                EditorApplication.delayCall -= delayedExecuteRegisteredCallbacks;
                EditorApplication.delayCall += delayedExecuteRegisteredCallbacks;
            }
            else
            {
                executeRegisteredCallbacks();
            }
        }

        static void delayedExecuteRegisteredCallbacks()
        {
            EditorApplication.delayCall -= delayedExecuteRegisteredCallbacks;
            executeRegisteredCallbacks();
        }

        static void executeRegisteredCallbacks()
        {
            int maxIndex = getMaxIndex();
            for (int i = maxIndex; i >= 0; i--)
            {
                string typeName;
                string methodName;
                GetCallbackInfo(i, out typeName, out methodName);

                try 
                {
                    ReleaseIndex(i);

                    if (string.IsNullOrEmpty(typeName) || string.IsNullOrEmpty(methodName))
                        continue;

                    var methodInfo = findStaticMethod(typeName, methodName);
                    methodInfo.Invoke(null, null);
                }
                catch (System.Exception e)
                {
                    string errorMsg = e.Message;
                    if(errorMsg.Contains("invocation") && e.InnerException != null)
                    {
                        errorMsg += "\n" + e.InnerException.Message;
                    }
                    Debug.LogError($"CrossCompileCallbacks: Calling '{typeName}.{methodName}' failed. Error:\n" + errorMsg);
                }
            }
        }

        static MethodInfo findStaticMethod(string fullTypeName, string methodName)
        {
            var type = findType(fullTypeName);
            if (type == null)
                return null;

            var flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
            var methodInfo = type.GetMethod(methodName, flags);

            return methodInfo;
        }

        static Type findType(string fullTypeName)
        {
            Debug.Assert(fullTypeName != null);

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                Type t = assembly.GetType(fullTypeName, throwOnError: false);
                if (t != null)
                    return t;
            }

            throw new ArgumentException("Type " + fullTypeName + " doesn't exist in the current app domain.");
        }

        /// <summary>
        /// Utility method to store a static parameterless Action
        /// in the SessionState for retrieval at a later time.
        /// </summary>
        /// <param name="sessionStorageKey"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static bool StoreAction(string sessionStorageKey, System.Action action)
        {
            if (action == null)
                return false;

            var methodInfo = action.GetMethodInfo();
            if (methodInfo == null)
                return false;

            if (!methodInfo.IsStatic)
            {
                Debug.Log("Method '"+ methodInfo.Name + "'needs to be static.");
                return false;
            }

            SessionState.SetString(sessionStorageKey + ".Type", methodInfo.DeclaringType.FullName);
            SessionState.SetString(sessionStorageKey + ".Method", methodInfo.Name);

            return true;
        }

        /// <summary>
        /// Retrieves the Action from the SessionState.
        /// </summary>
        /// <param name="sessionStorageKey"></param>
        /// <returns></returns>
        public static System.Action GetStoredAction(string sessionStorageKey)
        {
            var typeName = SessionState.GetString(sessionStorageKey + ".Type", null);
            var methodName = SessionState.GetString(sessionStorageKey + ".Method", null);

            if (string.IsNullOrEmpty(typeName) || string.IsNullOrEmpty(methodName))
            {
                return null;
            }

            var type = findType(typeName);
            if (type == null)
                return null;

            var methodInfo = findStaticMethod(typeName, methodName);
            if (methodInfo == null)
                return null;

            return (Action) Delegate.CreateDelegate(typeof(Action), methodInfo);
        }

        public static void ClearStoredAction(string sessionStorageKey)
        {
            SessionState.EraseString(sessionStorageKey + ".Type");
            SessionState.EraseString(sessionStorageKey + ".Method");
        }

        // Testing
        /*
        [DidReloadScripts]
        static void StartTest()
        {
            Debug.Log("CrossCompileCallbacks: Starting test.");
            RegisterCallback(testCallbackA);
            RegisterCallback(typeof(CrossCompileCallbacks), "testCallbackB");

            var action = GetStoredAction("storedActionA");
            ClearStoredAction("storedActionA");
            if (action != null)
                action.Invoke();
            StoreAction("storedActionA", storedActionA);            
        }

        static void testCallbackA()
        {
            Debug.Log("Test callback A executed."); 
        }

        static void testCallbackB()
        {
            Debug.Log("Test callback B executed."); 
        }

        static void storedActionA()
        {
            Debug.Log("Stored action A executed.");
        }
        //*/
    }
}
#endif