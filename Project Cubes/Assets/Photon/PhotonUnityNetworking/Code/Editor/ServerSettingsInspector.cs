// ----------------------------------------------------------------------------
// <copyright file="ServerSettingsInspector.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   This is a custom editor for the ServerSettings scriptable object.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------

using UnityEditor;
using UnityEngine;

using Photon.Pun;

using ExitGames.Client.Photon;

[CustomEditor(typeof (ServerSettings))]
public class ServerSettingsInspector : Editor
{
    private string versionPhoton;
    public void Awake()
    {
        this.versionPhoton = System.Reflection.Assembly.GetAssembly(typeof(PhotonPeer)).GetName().Version.ToString();
    }


    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Version");
        EditorGUILayout.LabelField("Pun: " + PhotonNetwork.PunVersion + " Photon lib: " + this.versionPhoton);
        EditorGUILayout.EndHorizontal();

        this.DrawDefaultInspector();

        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Rpc Tools");
        if (GUILayout.Button("Refresh RPCs",EditorStyles.miniButton))
        {
            PhotonEditor.UpdateRpcList();
            Repaint();
        }
        if (GUILayout.Button("Clear RPCs",EditorStyles.miniButton))
        {
            PhotonEditor.ClearRpcList();
        }
        if (GUILayout.Button("Log HashCode",EditorStyles.miniButton))
        {
            Debug.Log("RPC-List HashCode: " + RpcListHashCode() + ". Make sure clients that send each other RPCs have the same RPC-List.");
        }

        GUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Best Region Preference");
        if (GUILayout.Button("Reset", EditorStyles.miniButton))
        {
            ServerSettings.ResetBestRegionCodeInPreferences();
        }
        EditorGUILayout.EndHorizontal();
    }


    private int RpcListHashCode()
    {
        // this is a hashcode generated to (more) easily compare this Editor's RPC List with some other
        int hashCode = PhotonNetwork.PhotonServerSettings.RpcList.Count + 1;
        foreach (string s in PhotonNetwork.PhotonServerSettings.RpcList)
        {
            int h1 = s.GetHashCode();
            hashCode = ((h1 << 5) + h1) ^ hashCode;
        }

        return hashCode;
    }
}