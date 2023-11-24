#if UNITY_EDITOR
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using System.IO;


internal class PickerEditor : IPicker
{
    public void Show( string title, string outputFileName )
    {
        var path = EditorUtility.OpenFilePanel( title, "", "png" );
        if( path.Length != 0 )
        {
            string destination = Application.persistentDataPath + "/" + outputFileName;
            if( File.Exists( destination ) )
                File.Delete( destination );
            File.Copy( path, destination );
            Debug.Log( "PickerEditor:" + destination );
            var receiver = GameObject.Find( "ImgPicker" ); //"Unimgpicker"
            if( receiver != null )
            {
                receiver.SendMessage( "OnComplete", Application.persistentDataPath + "/" + outputFileName );
            }
        }
    }
}
#endif