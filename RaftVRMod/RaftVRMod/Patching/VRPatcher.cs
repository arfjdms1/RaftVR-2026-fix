using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.XR;

namespace RaftVR.Patching
{
    public static class VRPatcher
    {
        // The CLDB bundled in Resources/cldb was generated for this Unity version family.
        // If Raft upgrades its Unity engine, the CLDB will not match and patching
        // must be skipped to avoid corrupting globalgamemanagers.
        private const string EXPECTED_UNITY_VERSION_PREFIX = "2021.";

        private static string DataPath => Application.dataPath;
        private static string PluginsPath => Path.Combine(DataPath, "Plugins");
        private static string SteamVRPath => Path.Combine(DataPath, "StreamingAssets", "SteamVR");

        public static PatchErrorCode PatchVR()
        {
            PatchErrorCode patchResult = PatchGGM(Path.Combine(DataPath, "globalgamemanagers"));

            // If the Unity version is incompatible, stop immediately — don't
            // copy VR plugins into a game install we can't safely patch.
            if (patchResult == PatchErrorCode.IncompatibleVersion)
                return PatchErrorCode.IncompatibleVersion;

            if (patchResult != PatchErrorCode.Failed)
            {
                PatchErrorCode copyResult = CopyPlugins();
                if (patchResult == PatchErrorCode.Success) return PatchErrorCode.Success;
                return copyResult;
            }

            return PatchErrorCode.Failed;
        }

        private static PatchErrorCode CopyPlugins()
        {
            Debug.Log("[RaftVR] Checking for VR plugins...");

            PatchErrorCode result = PatchErrorCode.Failed;

            Dictionary<string, byte[]> plugins = new Dictionary<string, byte[]>()
            {
                { "AudioPluginOculusSpatializer.dll", Properties.Resources.AudioPluginOculusSpatializer },
                { "openvr_api.dll", Properties.Resources.openvr_api },
                { "OVRPlugin.dll", Properties.Resources.OVRPlugin }
            };

            try
            {
                if (CopyFiles(PluginsPath, plugins))
                {
                    Debug.Log("[RaftVR] Successfully copied VR plugins!");
                    result = PatchErrorCode.Success;
                }
                else
                {
                    Debug.Log("[RaftVR] VR plugins already present");
                    result = PatchErrorCode.AlreadyPatched;
                }
            }
            catch(Exception e)
            {
                Debug.LogError("[RaftVR] Error while copying VR plugins");
                Debug.LogException(e);

                return PatchErrorCode.Failed;
            }
            
            Debug.Log("[RaftVR] Checking for binding files...");


            if (!Directory.Exists(SteamVRPath))
            {
                try
                {
                    Directory.CreateDirectory(SteamVRPath);
                }
                catch (Exception e)
                {
                    Debug.LogError("[RaftVR] Could not create SteamVR folder");
                    Debug.LogException(e);
                    return PatchErrorCode.Failed;
                }
            }

            Dictionary<string, byte[]> bindingFiles = new Dictionary<string, byte[]>()
            {
                { "actions.json", Properties.Resources.actions },
                { "binding_holographic_hmd.json", Properties.Resources.binding_holographic_hmd },
                { "binding_index_hmd.json", Properties.Resources.binding_index_hmd },
                { "binding_rift.json", Properties.Resources.binding_rift },
                { "binding_vive.json", Properties.Resources.binding_vive },
                { "binding_vive_cosmos.json", Properties.Resources.binding_vive_cosmos },
                { "binding_vive_pro.json", Properties.Resources.binding_vive_pro },
                { "binding_vive_tracker_camera.json", Properties.Resources.binding_vive_tracker_camera },
                { "bindings_holographic_controller.json", Properties.Resources.bindings_holographic_controller },
                { "bindings_knuckles.json", Properties.Resources.bindings_knuckles },
                { "bindings_logitech_stylus.json", Properties.Resources.bindings_logitech_stylus },
                { "bindings_oculus_touch.json", Properties.Resources.bindings_oculus_touch },
                { "bindings_vive_controller.json", Properties.Resources.bindings_vive_controller },
                { "bindings_vive_cosmos_controller.json", Properties.Resources.bindings_vive_cosmos_controller }
            };

            try
            {
                bool flag = CopyFiles(SteamVRPath, bindingFiles, true);

                if (flag)
                {
                    Debug.Log("[RaftVR] Successfully copied binding files!");
                    result = PatchErrorCode.Success;
                }
                else
                    Debug.Log("[RaftVR] Binding files already present");
            }
            catch (Exception e)
            {
                Debug.LogError("[RaftVR] Error while copying binding files");
                Debug.LogException(e);

                return PatchErrorCode.Failed;
            }
            
            return result;
        }

        private static bool CopyFiles(string destinationPath, Dictionary<string, byte[]> filesToCopy, bool replaceIfDifferent = false)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(destinationPath);
            FileInfo[] files = directoryInfo.GetFiles();
            bool flag = false;
            foreach (var file in filesToCopy)
            {
                string fileName = file.Key;
                if (!Array.Exists(files, (FileInfo fileInfo) => fileName == fileInfo.Name))
                {
                    flag = true;
                    using (MemoryStream manifestResourceStream = new MemoryStream(file.Value))
                    {
                        using (FileStream fileStream = new FileStream(Path.Combine(directoryInfo.FullName, fileName), FileMode.Create, FileAccess.ReadWrite, FileShare.Delete))
                        {
                            Debug.Log("[RaftVR] Copying " + fileName);
                            manifestResourceStream.CopyTo(fileStream);
                        }
                    }
                }
                else if (replaceIfDifferent)
                {
                    string resourceFileContent;
                    using (MemoryStream manifestResourceStream = new MemoryStream(file.Value))
                    {
                        using (StreamReader reader = new StreamReader(manifestResourceStream))
                        {
                            resourceFileContent = reader.ReadToEnd();
                        }
                    }

                    FileInfo installedFile = files.First(fileInfo => fileInfo.Name == fileName);
                    string installedFileContent = File.ReadAllText(@installedFile.FullName);

                    if (resourceFileContent != installedFileContent)
                    {
                        flag = true;
                        Debug.Log("Overwriting " + fileName);
                        File.WriteAllText(installedFile.FullName, resourceFileContent);
                    }
                }
            }
            return flag;
        }

        /// <summary>
        /// Checks whether the Unity version string from the GGM file matches
        /// the version family that our bundled CLDB was built for.
        /// </summary>
        private static bool IsCompatibleUnityVersion(string unityVersion)
        {
            if (string.IsNullOrEmpty(unityVersion))
                return false;

            return unityVersion.StartsWith(EXPECTED_UNITY_VERSION_PREFIX, StringComparison.Ordinal);
        }

        /// <summary>
        /// Reads just the Unity version string from the GGM header without loading
        /// the full asset database. This is used for the pre-flight compatibility
        /// check so we never attempt to deserialize with a mismatched CLDB.
        /// </summary>
        private static string ReadGGMUnityVersion(string path)
        {
            try
            {
                // AssetsTools.NET v2: load the file just to read the header version.
                using (FileStream fs = File.OpenRead(path))
                {
                    AssetsFile tempFile = new AssetsFile(new AssetsFileReader(fs));
                    return tempFile.typeTree?.unityVersion;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[RaftVR] Could not read Unity version from GGM header: " + e.Message);
                return null;
            }
        }

        private static PatchErrorCode PatchGGM(string path)
        {
            if (XRSettings.supportedDevices.Length == 3)
            {
                Debug.Log("[RaftVR] GGM patch not necessary. Supported devices count is 3 as it should be.");
                return PatchErrorCode.AlreadyPatched;
            }

            // ── Pre-flight: version compatibility check ────────────────────
            string detectedUnityVersion = ReadGGMUnityVersion(path);
            Debug.Log("[RaftVR] Detected Unity version in GGM: " + (detectedUnityVersion ?? "<unknown>"));

            if (!IsCompatibleUnityVersion(detectedUnityVersion))
            {
                Debug.LogError(
                    "[RaftVR] INCOMPATIBLE UNITY VERSION DETECTED!\n" +
                    "[RaftVR] Expected: " + EXPECTED_UNITY_VERSION_PREFIX + "x  |  Found: " + (detectedUnityVersion ?? "<unknown>") + "\n" +
                    "[RaftVR] The bundled class database (CLDB) was built for Unity " + EXPECTED_UNITY_VERSION_PREFIX + "x.\n" +
                    "[RaftVR] Patching with a mismatched CLDB would CORRUPT the globalgamemanagers file and\n" +
                    "[RaftVR] prevent the game from launching. The GGM patch has been SKIPPED to protect your install.\n" +
                    "[RaftVR] Please check for an updated version of RaftVR that supports Unity " + (detectedUnityVersion ?? "<unknown>") + ".");
                return PatchErrorCode.IncompatibleVersion;
            }

            Debug.Log("[RaftVR] Patching GGM...");

            string backupPath = path + ".bak";
            string tempPath = path + ".tmp";
            AssetsManager assetsManager = new AssetsManager();

            Debug.Log("[RaftVR] Loading GGM from path " + path);
            AssetsFileInstance assetsFileInstance = assetsManager.LoadAssetsFile(path, false);

            using (MemoryStream cldbStream = new MemoryStream(Properties.Resources.cldb))
            {
                assetsManager.LoadClassPackage(cldbStream);
            }

            Debug.Log("[RaftVR] Starting patch...");

            int num = 0;
            while ((long)num < (long)((ulong)assetsFileInstance.table.assetFileInfoCount))
            {
                bool foundArray = false;
                try
                {
                    AssetFileInfoEx assetInfo = assetsFileInstance.table.GetAssetInfo((long)num);
                    AssetTypeInstance ati = assetsManager.GetATI(assetsFileInstance.file, assetInfo, false);
                    AssetTypeValueField globalField = (ati != null) ? ati.GetBaseField(0) : null;
                    AssetTypeValueField vrDevicesField = (globalField != null) ? globalField.Get("enabledVRDevices") : null;
                    if (vrDevicesField != null && vrDevicesField.childrenCount != -1)
                    {
                        Debug.Log("[RaftVR] Found VR devices field! Attempting patch...");

                        AssetTypeValueField devicesArray = vrDevicesField.Get("Array");

                        if (devicesArray != null)
                        {
                            foundArray = true;

                            bool wasPatched = devicesArray.GetChildrenCount() == 3;

                            if (wasPatched)
                            {
                                Debug.Log("[RaftVR] GGM already patched.");

                                return PatchErrorCode.AlreadyPatched;
                            }
                            else
                            {
                                AssetTypeValueField noneValueField = ValueBuilder.DefaultValueFieldFromArrayTemplate(devicesArray);
                                noneValueField.GetValue().Set("None");
                                AssetTypeValueField openVRValuefield = ValueBuilder.DefaultValueFieldFromArrayTemplate(devicesArray);
                                openVRValuefield.GetValue().Set("OpenVR");
                                AssetTypeValueField oculusValueField = ValueBuilder.DefaultValueFieldFromArrayTemplate(devicesArray);
                                oculusValueField.GetValue().Set("Oculus");
                                devicesArray.SetChildrenList(new AssetTypeValueField[]
                                {
                                noneValueField,
                                openVRValuefield,
                                oculusValueField
                                });
                                byte[] array;
                                using (MemoryStream memoryStream = new MemoryStream())
                                {
                                    using (AssetsFileWriter assetsFileWriter = new AssetsFileWriter(memoryStream))
                                    {
                                        assetsFileWriter.bigEndian = false;
                                        AssetWriters.Write(globalField, assetsFileWriter, 0);
                                        array = memoryStream.ToArray();
                                    }
                                }
                                List<AssetsReplacer> list = new List<AssetsReplacer>
                                {
                                    new AssetsReplacerFromMemory(0, (long)num, (int)assetInfo.curFileType, ushort.MaxValue, array)
                                };

                                // ── Atomic write: backup → temp → rename ──────────
                                try
                                {
                                    // Create backup of the original GGM before writing
                                    File.Copy(path, backupPath, true);
                                    Debug.Log("[RaftVR] Created backup at " + backupPath);

                                    // Write to a temp file first, then rename
                                    using (MemoryStream memoryStream2 = new MemoryStream())
                                    {
                                        using (AssetsFileWriter assetsFileWriter2 = new AssetsFileWriter(memoryStream2))
                                        {
                                            assetsFileInstance.file.Write(assetsFileWriter2, 0UL, list, 0U, null);
                                            assetsFileInstance.stream.Close();

                                            byte[] patchedBytes = memoryStream2.ToArray();

                                            // Sanity check: patched file should be roughly the same size
                                            FileInfo originalInfo = new FileInfo(backupPath);
                                            long sizeDiff = Math.Abs(patchedBytes.Length - originalInfo.Length);
                                            if (sizeDiff > originalInfo.Length / 2)
                                            {
                                                Debug.LogError("[RaftVR] Patched GGM size differs drastically from original (" +
                                                    patchedBytes.Length + " vs " + originalInfo.Length + " bytes). " +
                                                    "Aborting write to prevent corruption.");
                                                return PatchErrorCode.Failed;
                                            }

                                            File.WriteAllBytes(tempPath, patchedBytes);
                                        }
                                    }

                                    // Atomic-ish replace: delete original, rename temp
                                    File.Delete(path);
                                    File.Move(tempPath, path);

                                    Debug.Log("[RaftVR] Successfully patched GGM!");
                                    return PatchErrorCode.Success;
                                }
                                catch (Exception writeEx)
                                {
                                    Debug.LogError("[RaftVR] CRITICAL: Error writing patched GGM. Attempting to restore backup...");
                                    Debug.LogException(writeEx);

                                    // Clean up temp file if it exists
                                    try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch { }

                                    // Restore backup if the original was deleted/corrupted
                                    try
                                    {
                                        if (File.Exists(backupPath))
                                        {
                                            if (!File.Exists(path))
                                            {
                                                File.Copy(backupPath, path);
                                                Debug.Log("[RaftVR] Successfully restored GGM from backup.");
                                            }
                                            else
                                            {
                                                Debug.Log("[RaftVR] Original GGM still exists; no restore needed.");
                                            }
                                        }
                                        else
                                        {
                                            Debug.LogError("[RaftVR] No backup file found! You may need to verify game files through Steam.");
                                        }
                                    }
                                    catch (Exception restoreEx)
                                    {
                                        Debug.LogError("[RaftVR] CRITICAL: Failed to restore backup! Verify game files through Steam.");
                                        Debug.LogException(restoreEx);
                                    }

                                    return PatchErrorCode.Failed;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (foundArray)
                    {
                        Debug.LogException(e);
                        foundArray = false;
                    }
                }
                num++;
            }
            Debug.LogError("[RaftVR] VR devices field could not be found! The GGM patch has failed. Contact DrBibop#7000 in the RaftModding or Flat2VR Discord server.");

            return PatchErrorCode.Failed;
        }

        public enum PatchErrorCode
        {
            Success,
            AlreadyPatched,
            Failed,
            /// <summary>
            /// The game's Unity version does not match the bundled class database.
            /// Patching was skipped to prevent data corruption.
            /// </summary>
            IncompatibleVersion
        }
    }
}
