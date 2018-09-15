// ----------------------------------------------------------------------------
// <copyright file="PhotonEditorUtils.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   Unity Editor Utils
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------

using System;
using System.Linq;

using UnityEditor;
using UnityEngine;

using System.IO;

namespace ExitGames.Client.Photon
{
    [InitializeOnLoad]
    public class PhotonEditorUtils
    {
        /// <summary>True if the ChatClient of the Photon Chat API is available. If so, the editor may (e.g.) show additional options in settings.</summary>
        public static bool HasChat;
        /// <summary>True if the VoiceClient of the Photon Voice API is available. If so, the editor may (e.g.) show additional options in settings.</summary>
        public static bool HasVoice;
        /// <summary>True if the PhotonEditorUtils checked the available products / APIs. If so, the editor may (e.g.) show additional options in settings.</summary>
        public static bool HasCheckedProducts;

        static PhotonEditorUtils()
        {
            HasVoice = Type.GetType("ExitGames.Client.Photon.Voice.VoiceClient, Assembly-CSharp") != null || Type.GetType("ExitGames.Client.Photon.Voice.VoiceClient, Assembly-CSharp-firstpass") != null;
            HasChat = Type.GetType("ExitGames.Client.Photon.Chat.ChatClient, Assembly-CSharp") != null || Type.GetType("ExitGames.Client.Photon.Chat.ChatClient, Assembly-CSharp-firstpass") != null;
            PhotonEditorUtils.HasCheckedProducts = true;


            // MOUNTING SYMBOLS
            #if !PHOTON_UNITY_NETWORKING
                AddScriptingDefineSymbolToAllBuildTargetGroups("PHOTON_UNITY_NETWORKING");
            #endif

            #if !PUN_2_0_OR_NEWER
                AddScriptingDefineSymbolToAllBuildTargetGroups("PUN_2_0_OR_NEWER");
            #endif

            #if !PUN_2_OR_NEWER
                AddScriptingDefineSymbolToAllBuildTargetGroups("PUN_2_OR_NEWER");
            #endif

        }

        /// <summary>
        /// Adds a given scripting define symbol to all build target groups
        /// You can see all scripting define symbols ( not the internal ones, only the one for this project), in the PlayerSettings inspector
        /// </summary>
        /// <param name="defineSymbol">Define symbol.</param>
        public static void AddScriptingDefineSymbolToAllBuildTargetGroups(string defineSymbol)
        {
            foreach (BuildTarget target in Enum.GetValues(typeof(BuildTarget)))
            {
                BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup (target);

                if (group == BuildTargetGroup.Unknown)
                {
                    continue;
                }

                var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').Select(d => d.Trim()).ToList();

                if (!defineSymbols.Contains(defineSymbol))
                {
                    defineSymbols.Add(defineSymbol);

                    try
                    {
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", defineSymbols.ToArray()));
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Could not set Photon "+defineSymbol+" defines for build target: "+ target+ " group: "+ group+" "+e );
                    }
                }
            }
        }


        /// <summary>
        /// Gets the parent directory of a path. Recursive Function, will return null if parentName not found
        /// </summary>
        /// <returns>The parent directory</returns>
        /// <param name="path">Path.</param>
        /// <param name="parentName">Parent name.</param>
        public static string GetParent(string path, string parentName)
        {
            var dir = new DirectoryInfo(path);

            if (dir.Parent == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(parentName))
            {
                return  dir.Parent.FullName;
            }

            if (dir.Parent.Name == parentName)
            {
                return dir.Parent.FullName;
            }

            return GetParent(dir.Parent.FullName, parentName);
        }
    }
}
