using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

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
using CatSdk.Crypto;
using UnityEngine.UI;

public class FlagObjectsManager : MonoBehaviour
{
    public static FlagObjectsManager Instance = null;

    [SerializeField]
    GameObject m_FlagObjectPrefab;

    //public static string[] FlagDataList { private set; get; } = null;
    public static List<string> FlagDataList { private set; get; } = new List<string>();


    private void Awake()
    {
        if( Instance == null )
        {
            Instance = this;
            //DontDestroyOnLoad( gameObject );
        }
        else
        {
            Destroy( gameObject );
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadFlagObject().Forget();
    }

    /*
    IEnumerator GetFlagData()
    {
        UnityWebRequest www = UnityWebRequest.Get( "http://feiton.xsrv.jp/FLAG_DATA/FLAG_DATA.dat" );
        yield return www.SendWebRequest();

        if( www.isNetworkError || www.isHttpError )
        {
            Debug.Log( www.error );
        }
        else
        {
            // 結果をテキストとして表示します
            Debug.Log( www.downloadHandler.text );

            //  または、結果をバイナリデータとして取得します
            //byte[] results = www.downloadHandler.data;

            //m_Text.text = www.downloadHandler.text;
        }
    }
    */

    public async UniTask<string> AsyncHttpGet( string url )
    {
        // URL文字列からURIが作成可能か
        Uri uriResult;
        if( !Uri.TryCreate( url, UriKind.Absolute, out uriResult ) )
        {
            //// URIが作成できなければ失敗
            //GetFailed?.Invoke();
            return "";
        }

        // URL文字列からファイル名を取得する
        string fileName = Path.GetFileName( url );

        // HTTP GET のリクエストメッセージを作成する
        HttpClient httpClient = new HttpClient();
        HttpRequestMessage request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = uriResult
        };

        // リクエストの実行
        HttpResponseMessage response = await httpClient.SendAsync( request );

        if( response.StatusCode == HttpStatusCode.OK )
        {
            string ret = await response.Content.ReadAsStringAsync();
            return ret;

            //// レスポンスコードが成功(200)の場合
            //// バイト配列をファイルとして書き出す
            //byte[] contentByteData = await response.Content.ReadAsByteArrayAsync();
            //string saveFilePath = GetSaveFilePath( fileName );
            //File.WriteAllBytes( saveFilePath, contentByteData );

            //// ダウンロード完了イベントを実行
            //GetComplete?.Invoke( saveFilePath );
        }
        else
        {
            // レスポンスコードが成功以外の場合
            Debug.LogError( "HttpGet Error : " + response.StatusCode );
            //// ダウンロード失敗イベントを実行
            //GetFailed?.Invoke();
        }
        return "";
    }


    async UniTask<int> LoadFlagObject()
	{
        // ロケーション情報がある程度取れるまでウェイト
		while( LocationManager.Instance == null || LocationManager.LocationList.Count <= 30 )
		{
			await UniTask.Yield( PlayerLoopTiming.Update );
		}

        // リスト取得
		string dataListString = await AsyncHttpGet( "http://feiton.xsrv.jp/FLAG_DATA/FLAG_DATA.dat" );
        string[] del = { "\n" };
        FlagDataList = dataListString.Split( del, StringSplitOptions.None ).ToList();

        if( FlagDataList == null ) return 1;

        for( int i = 0; i < FlagDataList.Count; i++ )
        {
            string[] delComma = { "," };
            string[] splitData = FlagDataList[ i ].Split( delComma, StringSplitOptions.None );

            // hash,TANYEISCFJ7ZYLPIRXNQZCZUOM3UIDN4QOI6N5Q,32543886000,Test_Set_flag,1,298182EF0E364B30,-90.000000,-180.000000,あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわをん
            if( splitData.Length < 9 ) continue;

            // 一定範囲外は生成しない
            if( !LocationManager.CheckDistance( float.Parse( splitData[ 6 ] ), float.Parse( splitData[ 7 ] ) ) ) continue;

            var flagObject = Instantiate( m_FlagObjectPrefab );
            var flagObjectData = flagObject.GetComponent<FlagObject>();
            await flagObjectData.SetData( splitData[ 5 ], splitData[ 8 ] ); //"298182EF0E364B30"
            flagObject.transform.SetParent( gameObject.transform );

            while( !LocationManager.UpdateObjectPos( Camera.main.transform, flagObject.transform, float.Parse( splitData[ 6 ] ), float.Parse( splitData[ 7 ] ), 1.0f ) )
            {
                await UniTask.Yield( PlayerLoopTiming.Update );
            }
            await UniTask.Delay( 100 );
        }

        return 0;
	}
    public async UniTask<int> SetFlagObject(string setData)
    {
        FlagDataList.Add( setData );

        string[] delComma = { "," };
        string[] splitData = setData.Split( delComma, StringSplitOptions.None );

        var flagObject = Instantiate( m_FlagObjectPrefab );
        var flagObjectData = flagObject.GetComponent<FlagObject>();
        await flagObjectData.SetData( splitData[ 5 ], splitData[ 8 ] ); //"298182EF0E364B30"
        flagObject.transform.SetParent( gameObject.transform );

        flagObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward;

        //while( !LocationManager.UpdateObjectPos( Camera.main.transform, flagObject.transform, float.Parse( splitData[ 6 ] ), float.Parse( splitData[ 7 ] ), 1.0f ) )
        //{
        //    await UniTask.Yield( PlayerLoopTiming.Update );
        //}

        return 0;
    }

}
