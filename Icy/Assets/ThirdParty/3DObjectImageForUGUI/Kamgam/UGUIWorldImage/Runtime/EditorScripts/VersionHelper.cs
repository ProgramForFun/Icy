#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;

namespace Kamgam.UGUIWorldImage
{
    public static class VersionHelper
    {
        public static string VersionFileName = "." + typeof(VersionHelper).FullName + ".txt";
        public static Version DefaultVersion = new Version(0, 0, 0, 1);

        public delegate bool UpgradeVersionDelegate(Version oldVersion, Version newVersion);

        public static Version Parse(string version)
        {
            if (string.IsNullOrEmpty(version))
                return DefaultVersion;

            if (Version.TryParse(version, out var versionObj))
                return versionObj;
            else
                return VersionHelper.DefaultVersion;
        }

        /// <summary>
        /// Returns false if upgrading was not necessary, True otherwise.
        /// </summary>
        /// <param name="getVersionFunc">Change this to return the version of the software.<br />
        /// This is a separate method because your version may be stored in another class or a file.<br />
        /// The return value if this is compared against the install version marker.</param>
        /// <param name="upgradeVersionFunc">Use this to execute some custom code before upgrading
        /// the installed version info. If this returns false then the installed version will NOT be changed.</param>
        /// <returns></returns>
        public static bool UpgradeVersion(Func<Version> getVersionFunc, UpgradeVersionDelegate upgradeVersionFunc = null)
        {
            return UpgradeVersion(getVersionFunc, out _, out _, upgradeVersionFunc);
        }

        /// <summary>
        /// Returns false if upgrading was not necessary, True otherwise.
        /// </summary>
        /// <param name="getVersionFunc">Change this to return the version of the software.<br />
        /// This is a separate method because your version may be stored in another class or a file.<br />
        /// The return value if this is compared against the install version marker.</param>
        /// <param name="oldVersion"></param>
        /// <param name="newVersion"></param>
        /// <param name="upgradeVersionFunc">Use this to execute some custom code before upgrading
        /// the installed version info. If this returns false then the installed version will NOT be changed.</param>
        /// <returns></returns>
        public static bool UpgradeVersion(Func<Version> getVersionFunc, out Version oldVersion, out Version newVersion, UpgradeVersionDelegate upgradeVersionFunc = null)
        {
            oldVersion = GetInstalledVersion();
            newVersion = getVersionFunc();

            // Notice: this also cover downgrades.
            if (oldVersion != newVersion)
            {
                if (upgradeVersionFunc != null)
                {
                    bool upgradeSucceeded = upgradeVersionFunc(oldVersion, newVersion);
                    if (upgradeSucceeded)
                        SetInstalledVersion(newVersion);
                    return upgradeSucceeded;
                }
                else
                {
                    SetInstalledVersion(newVersion);
                }
                return true;
            }

            return false;
        }

        public static void SetInstalledVersion(Version version)
        {
            if (version == null)
                return;

            string versionString = version.ToString();

            string filePath = getVersionFilePath();
            string tmpPath = filePath + ".tmp";

            if (File.Exists(tmpPath))
            {
                File.Delete(tmpPath);
            }

            File.WriteAllText(tmpPath, versionString);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            File.Move(tmpPath, filePath);
        }

        public static Version GetInstalledVersion()
        {
            string filePath = getVersionFilePath();

            if (!File.Exists(filePath))
            {
                return DefaultVersion;
            }

            string version = File.ReadAllText(filePath);
            return Parse(version);
        }

        static string getVersionFilePath()
        {
            string dir = getVersionFileDir();

            // fix empty dir path
            if (string.IsNullOrEmpty(dir))
            {
                dir = "Assets/";
            }

            // Fix missing ending slash
            if (!dir.EndsWith("/") && !dir.EndsWith("\\"))
            {
                dir = dir + "/";
            }

            return getBasePath() + dir + VersionFileName;
        }

        static string getVersionFileDir()
        {
            return Installer.AssetRootPath.Trim();
        }

        static string getBasePath()
        {
            // Unity Editor: <path to project folder>/Assets
            // See: https://docs.unity3d.com/ScriptReference/Application-dataPath.html
            string basePath = Application.dataPath.Replace("/Assets", "/");
            basePath = basePath.Replace("\\Assets", "\\");
            return basePath;
        }
    }
}
#endif