using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;
using CatSdk;
using CatSdk.CryptoTypes;
using CatSdk.Facade;
using CatSdk.Utils;
using CatSdk.Symbol;
using CatSdk.Symbol.Factory;
using System.Net;
using System.Net.Http;
using System.Text;
using System;
using System.IO;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using AOT;

using Symnity.Http.Model;
using System.Linq;

using UniRx;
using static Symnity.Http.Model.ApiAccount;

public class Test_AccountManager : MonoBehaviour
{
    public static Test_AccountManager Instance = null;

    public SymbolFacade Facade { get; } = new SymbolFacade( CatSdk.Symbol.Network.TestNet );
    //public const string Node = "https://001-sai-dual.symboltest.net:3001";
    public const string Node = "https://test01.xymnodes.com:3001";
    public const string FileName = "key";

    public PublicKey AlicePublicKey { private set; get; } = null;
    public SymbolAddress AliceAddress { private set; get; } = null;

    public AccountDatum AliceAccountDatum { private set; get; } = null;

    public double AliceXYM = 0;

    private void Awake()
    {
        if( Instance == null )
        {
            Instance = this;
            DontDestroyOnLoad( gameObject );
        }
        else
        {
            Destroy( gameObject );
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadAccount();
        if( AliceAddress == null )
        {
            GenerateAccount();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadAccount()
	{
        var key = LoadPrivateKey();
        if( key == "" )
        {
            Debug.Log( "No Key" );
            return;
        }

        var alicePrivateKey = new PrivateKey( Converter.HexToBytes( key ) );
        var aliceKeyPair = new KeyPair( alicePrivateKey );
        AlicePublicKey = aliceKeyPair.PublicKey;
        AliceAddress = Facade.Network.PublicKeyToAddress( AlicePublicKey );

        UpdateAccountDataAsync().Forget();

        Debug.Log( $"Ok Key : Address = {AliceAddress.ToString()}" );
    }

    public string LoadPrivateKey()
	{
        return LoadText( GetSecureDataPath(), FileName );
	}

    void GenerateAccount()
	{
        if( Facade == null ) return;
        if( AliceAddress != null ) return;

        var aliceKeyPair = KeyPair.GenerateNewKeyPair();
        var alicePrivateKey = aliceKeyPair.PrivateKey;
        AlicePublicKey = aliceKeyPair.PublicKey;
        AliceAddress = Facade.Network.PublicKeyToAddress( AlicePublicKey );

        SaveText(
            GetSecureDataPath(),
            FileName,
            Converter.BytesToHex( alicePrivateKey.bytes )
        );

        Debug.Log( "Generate Key" );
    }

    public async UniTask<int> UpdateAccountDataAsync()
	{
        Debug.Log( "URL : " + Node + $"/accounts/{AliceAddress}" );
        AliceAccountDatum = JsonUtility.FromJson<AccountDatum>( await GetDataFromApi( Node, $"/accounts/{AliceAddress}" ) );
        if( AliceAccountDatum == null ) return 1;
        //if( AliceAccountDatum == null ) throw new NullReferenceException( "account is null" );
        if( AliceAccountDatum.account == null ) return 1;
        var result = Test_AccountManager.Instance.AliceAccountDatum.account.mosaics.Find( n => n.id.Equals( "72C0212E67A08BCE" ) );
        if( result != null )
        {
            AliceXYM = double.Parse( result.amount );
        }
        return 0;
    }

    private string GetSecureDataPath()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        using( var unityPlayer = new AndroidJavaClass( "com.unity3d.player.UnityPlayer" ) )
        using( var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>( "currentActivity" ) )
        using( var getFilesDir = currentActivity.Call<AndroidJavaObject>( "getFilesDir" ) )
        {
            string secureDataPathForAndroid = getFilesDir.Call<string>( "getCanonicalPath" );
            return secureDataPathForAndroid;
        }
#else
        // TODO: 本来は各プラットフォームに対応した処理が必要
        return Application.persistentDataPath;
#endif
    }

    private void SaveText( string filePath, string fileName, string textToSave )
    {
        var combinedPath = Path.Combine( filePath, fileName );
        using( var streamWriter = new StreamWriter( combinedPath ) )
        {
            streamWriter.WriteLine( textToSave );
        }
    }

    public string LoadText( string filePath, string fileName )
    {
        var combinedPath = Path.Combine( filePath, fileName );
        if( !File.Exists( combinedPath ) )
        {
            Debug.Log( $"Not File Exist : {fileName}" );
            return "";
        }

        using( var streamReader = new StreamReader( combinedPath ) )
        {
            return streamReader.ReadLine();
        }
    }

    public static async UniTask<string> GetDataFromApi( string _node, string _param )
    {
        var url = $"{_node}{_param}";
        Debug.Log( "URL : " + url );
        using var client = new HttpClient();
        try
        {
            var response = await client.GetAsync( url );
            if( response.IsSuccessStatusCode )
            {
                return await response.Content.ReadAsStringAsync();
            }
            throw new Exception( $"Error: {response.StatusCode}" );
        }
        catch( Exception ex )
        {
            throw new Exception( ex.Message );
        }
    }

    static async UniTask<string> PostDataFromApi( string _node, string _param, object _obj )
    {
        var url = $"{_node}{_param}";
        using var client = new HttpClient();
        try
        {
            var json = JsonUtility.ToJson( _obj );
            var data = new StringContent( json, Encoding.UTF8, "application/json" );
            var response = await client.PostAsync( url, data );
            if( response.IsSuccessStatusCode )
            {
                return await response.Content.ReadAsStringAsync();
            }
            throw new Exception( $"Error: {response.StatusCode}" );
        }
        catch( Exception ex )
        {
            throw new Exception( ex.Message );
        }
    }
}
