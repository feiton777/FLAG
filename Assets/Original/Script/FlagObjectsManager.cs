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
            // ���ʂ��e�L�X�g�Ƃ��ĕ\�����܂�
            Debug.Log( www.downloadHandler.text );

            //  �܂��́A���ʂ��o�C�i���f�[�^�Ƃ��Ď擾���܂�
            //byte[] results = www.downloadHandler.data;

            //m_Text.text = www.downloadHandler.text;
        }
    }
    */

    public async UniTask<string> AsyncHttpGet( string url )
    {
        // URL�����񂩂�URI���쐬�\��
        Uri uriResult;
        if( !Uri.TryCreate( url, UriKind.Absolute, out uriResult ) )
        {
            //// URI���쐬�ł��Ȃ���Ύ��s
            //GetFailed?.Invoke();
            return "";
        }

        // URL�����񂩂�t�@�C�������擾����
        string fileName = Path.GetFileName( url );

        // HTTP GET �̃��N�G�X�g���b�Z�[�W���쐬����
        HttpClient httpClient = new HttpClient();
        HttpRequestMessage request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = uriResult
        };

        // ���N�G�X�g�̎��s
        HttpResponseMessage response = await httpClient.SendAsync( request );

        if( response.StatusCode == HttpStatusCode.OK )
        {
            string ret = await response.Content.ReadAsStringAsync();
            return ret;

            //// ���X�|���X�R�[�h������(200)�̏ꍇ
            //// �o�C�g�z����t�@�C���Ƃ��ď����o��
            //byte[] contentByteData = await response.Content.ReadAsByteArrayAsync();
            //string saveFilePath = GetSaveFilePath( fileName );
            //File.WriteAllBytes( saveFilePath, contentByteData );

            //// �_�E�����[�h�����C�x���g�����s
            //GetComplete?.Invoke( saveFilePath );
        }
        else
        {
            // ���X�|���X�R�[�h�������ȊO�̏ꍇ
            Debug.LogError( "HttpGet Error : " + response.StatusCode );
            //// �_�E�����[�h���s�C�x���g�����s
            //GetFailed?.Invoke();
        }
        return "";
    }


    async UniTask<int> LoadFlagObject()
	{
        // ���P�[�V������񂪂�����x����܂ŃE�F�C�g
		while( LocationManager.Instance == null || LocationManager.LocationList.Count <= 30 )
		{
			await UniTask.Yield( PlayerLoopTiming.Update );
		}

        // ���X�g�擾
		string dataListString = await AsyncHttpGet( "http://feiton.xsrv.jp/FLAG_DATA/FLAG_DATA.dat" );
        string[] del = { "\n" };
        FlagDataList = dataListString.Split( del, StringSplitOptions.None ).ToList();

        if( FlagDataList == null ) return 1;

        for( int i = 0; i < FlagDataList.Count; i++ )
        {
            string[] delComma = { "," };
            string[] splitData = FlagDataList[ i ].Split( delComma, StringSplitOptions.None );

            // hash,TANYEISCFJ7ZYLPIRXNQZCZUOM3UIDN4QOI6N5Q,32543886000,Test_Set_flag,1,298182EF0E364B30,-90.000000,-180.000000,�����������������������������������ĂƂȂɂʂ˂̂͂Ђӂւق܂݂ނ߂��������������
            if( splitData.Length < 9 ) continue;

            // ���͈͊O�͐������Ȃ�
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
