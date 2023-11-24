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
using CatSdk.Crypto;
using UnityEngine.UI;

public class Test_MosaicManager : MonoBehaviour
{
    public static Test_MosaicManager Instance = null;

    [SerializeField]
    public Image m_Image;

    [SerializeField]
    public GameObject m_CreateFlagMenu;

    [SerializeField]
    public GameObject m_ExplanationText;

    [SerializeField]
    public GameObject m_LoadImageButton;

    [SerializeField]
    public GameObject m_CreateFlagButton;

    [SerializeField]
    public GameObject m_CreateWaitImage;

    static Dictionary<string, Texture2D> m_DataStock = new Dictionary<string, Texture2D>();

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
        
    }

    public void ClickCreateMosaic()
	{
        CreateMosaicAsync().Forget();
    }

    async UniTask<int> CreateMosaicAsync()
	{
        m_CreateWaitImage.SetActive( true );
        m_CreateFlagButton.SetActive( false );

        var key = Test_AccountManager.Instance.LoadPrivateKey();
        if( key == "" )
        {
            Debug.Log( "No Key" );
            m_CreateWaitImage.SetActive( false );
            AnnounceImageManager.Instance.OpenAnnounceImage("秘密鍵の確認ができませんでした");
            return 1;
        }
        var alicePrivateKey = new PrivateKey( Converter.HexToBytes( key ) );
        var aliceKeyPair = new KeyPair( alicePrivateKey );

        if( Test_AccountManager.Instance.AliceXYM < 1.0f )
        {
            m_CreateWaitImage.SetActive( false );
            AnnounceImageManager.Instance.OpenAnnounceImage( $"XYMが不足しています\nXYMを補給して下さい" );
            return 1;
        }

        //モザイク定義
        var nonce = BitConverter.ToUInt32( Crypto.RandomBytes( 8 ), 0 );
        var mosaicId = IdGenerator.GenerateMosaicId( Test_AccountManager.Instance.AliceAddress, nonce );
        var newMosaicId = new UnresolvedMosaicId( mosaicId );

        {
            var supplyMutable = false; //供給量変更の可否
            var transferable = true;   //第三者への譲渡可否
            var restrictable = false;  //制限設定の可否
            var revokable = true;      //発行者からの還収可否

            //モザイク定義
            //var nonce = BitConverter.ToUInt32( Crypto.RandomBytes( 8 ), 0 );
            //var mosaicId = IdGenerator.GenerateMosaicId( Test_AccountManager.Instance.AliceAddress, nonce );
            var mosaicDefTx = new EmbeddedMosaicDefinitionTransactionV1()
            {
                Network = NetworkType.TESTNET,
                Nonce = new MosaicNonce( nonce ),
                SignerPublicKey = Test_AccountManager.Instance.AlicePublicKey,
                Id = new MosaicId( mosaicId ),
                Duration = new BlockDuration( 0 ),
                Divisibility = 0,
                Flags = new MosaicFlags( Converter.CreateMosaicFlags( supplyMutable, transferable, restrictable, revokable ) ),
            };

            //モザイク変更
            var mosaicChangeTx = new EmbeddedMosaicSupplyChangeTransactionV1()
            {
                Network = NetworkType.TESTNET,
                SignerPublicKey = Test_AccountManager.Instance.AlicePublicKey,
                MosaicId = new UnresolvedMosaicId( mosaicId ),
                Action = MosaicSupplyChangeAction.INCREASE,
                Delta = new Amount( 1000000 ),
            };

            var innerTransactions = new IBaseTransaction[] { mosaicDefTx, mosaicChangeTx };
            var merkleHash = SymbolFacade.HashEmbeddedTransactions( innerTransactions );
            var aggregateTx = new AggregateCompleteTransactionV2()
            {
                Network = NetworkType.TESTNET,
                SignerPublicKey = Test_AccountManager.Instance.AlicePublicKey,
                Deadline = new Timestamp( Test_AccountManager.Instance.Facade.Network.FromDatetime<CatSdk.NetworkTimestamp>( DateTime.UtcNow ).AddHours( 2 ).Timestamp ),
                Transactions = innerTransactions,
                TransactionsHash = merkleHash,
            };
            TransactionHelper.SetMaxFee( aggregateTx, 100 );

            var signature = Test_AccountManager.Instance.Facade.SignTransaction( aliceKeyPair, aggregateTx );
            var payload = TransactionsFactory.AttachSignature( aggregateTx, signature );
            var hash = Test_AccountManager.Instance.Facade.HashTransaction( aggregateTx, signature );
            var result = await Announce( payload );
            //Console.WriteLine( result );

            if( result.Contains( "Uncaught Error" ) )
            {
                Debug.Log( $"Announce Error ; {result}" );
                m_CreateWaitImage.SetActive( false );
                AnnounceImageManager.Instance.OpenAnnounceImage( "NFTの作成に失敗しました" );
                Test_AccountManager.Instance.UpdateAccountDataAsync().Forget();
                return 1;
            }
        }

        // 
        while( true )
        {
            Debug.Log( $"newMosaicId : {newMosaicId.ToString()}" );

            await UniTask.Delay( 5 * 1000 );
            await Test_AccountManager.Instance.UpdateAccountDataAsync();

            if( Test_AccountManager.Instance.AliceAccountDatum.account.mosaics.Find( n => newMosaicId.ToString().Contains( n.id ) ) != null )
            {
                break;
            }
            //if( Test_AccountManager.Instance.AliceAccountDatum.account.mosaics.Find( n => n.id.Contains( newMosaicId.ToString() ) ) != null )
            //{
            //    break;
            //}
            await UniTask.Delay( 5 * 1000 );
        }
        //await UniTask.Delay(10 * 1000);


        //var mosaicIdString = "479DFBC9EEEDE40F";
        //ulong mosaicId = Convert.ToUInt64( mosaicIdString, 16 );
        {
            // メタデータ登録
            var dataSize = 0;
            var originalValueBytes = m_Image.sprite.texture.EncodeToJPG( 100 );
            //var originalValue = "test";
            //var originalValueBytes = Converter.Utf8ToBytes( originalValue );
            var index = 1;

            while( dataSize < originalValueBytes.Length )
			{
                var innerTransactionList = new List<IBaseTransaction>();

                while( dataSize < originalValueBytes.Length && innerTransactionList.Count < 100 )
                {
                    var setDataSize = Math.Min( originalValueBytes.Length - dataSize, 1023 );
                    var setDataBytes = new byte[ setDataSize ];
                    Array.Copy( originalValueBytes, dataSize, setDataBytes, 0, setDataSize );

                    var mosaicKey = IdGenerator.GenerateUlongKey( $"key_mosaic_{index}" );
                    var tx = new EmbeddedMosaicMetadataTransactionV1()
                    {
                        Network = NetworkType.TESTNET,
                        SignerPublicKey = Test_AccountManager.Instance.AlicePublicKey,
                        TargetAddress = new UnresolvedAddress( Test_AccountManager.Instance.AliceAddress.bytes ), //モザイク作成者アドレス
                        TargetMosaicId = new UnresolvedMosaicId( mosaicId ), // mosaic id
                        ScopedMetadataKey = mosaicKey, // Key
                        Value = setDataBytes, // Value
                        ValueSizeDelta = (ushort)setDataSize
                    };
                    innerTransactionList.Add( tx );
                    dataSize += setDataSize;
                    index++;
                }

                //var tx = new EmbeddedMosaicMetadataTransactionV1()
                //{
                //    Network = NetworkType.TESTNET,
                //    SignerPublicKey = Test_AccountManager.Instance.AlicePublicKey,
                //    TargetAddress = new UnresolvedAddress( Test_AccountManager.Instance.AliceAddress.bytes ), //モザイク作成者アドレス
                //    TargetMosaicId = new UnresolvedMosaicId( mosaicId ), // mosaic id
                //    ScopedMetadataKey = mosaicKey, // Key
                //    Value = valueBytes, // Value
                //    ValueSizeDelta = (byte)valueBytes.Length
                //};
                //var innerTransactions = new IBaseTransaction[] { tx };

                var innerTransactions = innerTransactionList.ToArray();
                var merkleHash = SymbolFacade.HashEmbeddedTransactions( innerTransactions );
                var aggregateTx = new AggregateCompleteTransactionV2()
                {
                    Network = NetworkType.TESTNET,
                    SignerPublicKey = Test_AccountManager.Instance.AlicePublicKey,
                    Transactions = innerTransactions,
                    TransactionsHash = merkleHash,
                    Deadline = new Timestamp( Test_AccountManager.Instance.Facade.Network.FromDatetime<CatSdk.NetworkTimestamp>( DateTime.UtcNow ).AddHours( 2 ).Timestamp )
                };
                TransactionHelper.SetMaxFee( aggregateTx, 100 );
                var signature = Test_AccountManager.Instance.Facade.SignTransaction( aliceKeyPair, aggregateTx );
                var payload = TransactionsFactory.AttachSignature( aggregateTx, signature );
                var result = await Announce( payload );

                Debug.Log($"Create Mosaic : {result}");

                await UniTask.Delay( 100 );

                //Console.WriteLine( result );
            }
        }


        string filePath = Application.persistentDataPath + "/" + mosaicId.ToString( "X16" );
        {
            Debug.Log( "Data Write : " + filePath );
            using( var fs = new System.IO.FileStream( filePath, System.IO.FileMode.Create, System.IO.FileAccess.Write ) )
            {
                var writeData = AsyncReader.ImageConverter.Encode( m_Image.sprite.texture.GetRawTextureData(), m_Image.sprite.texture.width, m_Image.sprite.texture.height, m_Image.sprite.texture.format );
                fs.Write( writeData, 0, writeData.Length );
                //fs.Write( bytes, 0, bytes.Length );
            }
        }
        m_DataStock.Add( mosaicId.ToString( "X16" ), m_Image.sprite.texture );

        m_CreateWaitImage.SetActive( false );
        Debug.Log( "Mosaic Create End" );
        AnnounceImageManager.Instance.OpenAnnounceImage( $"NFTの作成に成功しました\nIDは以下になります\n{mosaicId.ToString( "X16" )}" );
        Test_AccountManager.Instance.UpdateAccountDataAsync().Forget();
        return 0;
    }

    public static async UniTask<int> LoadMosaicAsync(string mosaicId, MeshRenderer meshRenderer, RawImage rawImage = null)
	{
        //var mosaicId = "298182EF0E364B30";

        if( m_DataStock.ContainsKey( mosaicId ) )
        {
            if( rawImage != null )
            {
                //var setTexture = new Texture2D( 1, 1 );
                //setTexture.LoadImage( m_DataStock[ mosaicId ] );
                //setTexture.LoadRawTextureData( m_DataStock[ mosaicId ] );
                //setTexture.Apply();
                rawImage.GetComponent<RectTransform>().sizeDelta = new Vector2( m_DataStock[ mosaicId ].width, m_DataStock[ mosaicId ].height );
                rawImage.texture = m_DataStock[ mosaicId ];
                rawImage.color = Color.white;
                var wRate = 1080.0f / m_DataStock[ mosaicId ].width;
                var hRate = 1980.0f / m_DataStock[ mosaicId ].height;
                var minRate = Mathf.Min( wRate, hRate );
                rawImage.transform.localScale = new Vector3( minRate, minRate, minRate );
            }
            else
			{
                Destroy( meshRenderer.material.mainTexture );
                meshRenderer.material.SetTexture( "_MainTex", m_DataStock[ mosaicId ] );
            }

            return 0;
        }

        string filePath = Application.persistentDataPath + "/" + mosaicId;
        Debug.Log( "persistentDataPath : " + filePath );
        if( File.Exists( filePath ) )
        {
            Debug.Log( "FileExists : " + filePath );
            //using( var fs = new System.IO.FileStream( filePath, System.IO.FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true ) 
            //using( var fs = new System.IO.FileStream( filePath, System.IO.FileMode.Open ) )
            //{
            //    var bs = new byte[ fs.Length ];
            //    await fs.ReadAsync( bs, 0, bs.Length );
            //    var setTexture = new Texture2D( 1, 1 );
            //    setTexture.LoadImage( bs );
            //    meshRenderer.material.SetTexture( "_MainTex", setTexture );
            //    m_DataStock.Add( mosaicId, bs );
            //    return 0;
            //}

            using( AsyncReader.AsyncFileReader reader = new AsyncReader.AsyncFileReader() )
            {
                (IntPtr ptr, long size) = await reader.LoadAsync( filePath );
                AsyncReader.ImageInfo info = AsyncReader.ImageConverter.Decode( ptr, (int)size );
                Texture2D setTexture = new Texture2D( info.header.width, info.header.height, info.header.Format, false );
                setTexture.LoadRawTextureData( info.buffer, info.fileSize );
                setTexture.Apply();

                if( rawImage != null )
                {
                    rawImage.GetComponent<RectTransform>().sizeDelta = new Vector2( setTexture.width, setTexture.height );
                    rawImage.texture = setTexture;
                    rawImage.color = Color.white;
                    var wRate = 1080.0f / setTexture.width;
                    var hRate = 1980.0f / setTexture.height;
                    var minRate = Mathf.Min( wRate, hRate );
                    rawImage.transform.localScale = new Vector3( minRate, minRate, minRate );
                }
                else
                {
                    Destroy( meshRenderer.material.mainTexture );
                    meshRenderer.material.SetTexture( "_MainTex", setTexture );
                }

                m_DataStock.Add( mosaicId, setTexture );

                return 0;
            }
        }
        Debug.Log( "Not FileExists : " + filePath );

        var data = "";

        var metaDatasList = new List<ApiMetadata.MetadataDatum>();
        int pageIndex = 1;
        bool endFlag = false;
        while( true )
        {
            async UniTask<int> LoadDataAsync(int loadIndex)
            {
                var metadatas = JsonUtility.FromJson<ApiMetadata.MetadataRoot>( await Test_AccountManager.GetDataFromApi( Test_AccountManager.Node, $"/metadata?targetId={mosaicId}&metadataType=1&pageSize=100&pageNumber={loadIndex}" ) );
                if( metadatas.data.Count() <= 0 )
                {
                    endFlag = true;
                    return 0;
                }
                metaDatasList.AddRange( metadatas.data );
                return 0;
            }

            var loadAsyncList = new List<UniTask<int>>();
            for( int i = 0; i < 10; i++ )
            {
                loadAsyncList.Add( LoadDataAsync( pageIndex ) );
                pageIndex++;
            }
            await UniTask.WhenAll( loadAsyncList );

            if( endFlag ) break;
        }

        /*
        var dataDictionary = new Dictionary<string, string>();
        for( int i = 0; i < metaDatasList.Count; i++ )
        {
            var mosaicKey = IdGenerator.GenerateUlongKey( $"key_mosaic_{i + 1}" ).ToString( "X" );
            dataDictionary.Add( mosaicKey, null );
        }

        for( int i = 0; i < metaDatasList.Count; i++ )
        {
            dataDictionary[ metaDatasList[ i ].metadataEntry.scopedMetadataKey ] = metaDatasList[ i ].metadataEntry.value;
        }

        foreach( var oneData in dataDictionary )
        {
            if( oneData.Value == null ) continue;
            data += oneData.Value;
        }
        */


        int dataNum = metaDatasList.Count;
        for( int i = 0; i < dataNum; i++ )
        {
            var mosaicKey = IdGenerator.GenerateUlongKey( $"key_mosaic_{i + 1}" ).ToString( "X" );
            var result = metaDatasList.Find( m => m.metadataEntry.scopedMetadataKey.Equals( mosaicKey ) );
            if( result == null ) continue;
            data += result.metadataEntry.value;
            metaDatasList.Remove( result );
            if( i % 10 == 0 )
            {
                await UniTask.Yield( PlayerLoopTiming.Update );
            }
        }

        if( data == "" )
        {
            if( rawImage != null )
            {
                rawImage.color = Color.clear;
            }
            return 1;
        }

        byte[] bytes = Converter.HexToBytes( data );

		var texture = new Texture2D( 1, 1 );
		texture.LoadImage( bytes );
        //m_Image.sprite = Sprite.Create( texture, new Rect( 0, 0, texture.width, texture.height ), Vector2.zero );
        //RectTransform rectTransform = m_Image.gameObject.GetComponent<RectTransform>();
        //rectTransform.sizeDelta = new Vector2( texture.width, texture.height );
        //m_Image.color = Color.white;

        if( rawImage != null )
        {
            rawImage.GetComponent<RectTransform>().sizeDelta = new Vector2( texture.width, texture.height );
            rawImage.texture = texture;
            rawImage.color = Color.white;
            var wRate = 1080.0f / texture.width;
            var hRate = 1980.0f / texture.height;
            var minRate = Mathf.Min( wRate, hRate );
            rawImage.transform.localScale = new Vector3( minRate, minRate, minRate );
        }
        else
        {
            Destroy( meshRenderer.material.mainTexture );
            meshRenderer.material.SetTexture( "_MainTex", texture );
        }

        {
            Debug.Log( "Data Write : " + filePath );
            using( var fs = new System.IO.FileStream( filePath, System.IO.FileMode.Create, System.IO.FileAccess.Write ) )
            {
                var writeData = AsyncReader.ImageConverter.Encode( texture.GetRawTextureData(), texture.width, texture.height, texture.format );
                fs.Write( writeData, 0, writeData.Length );
                //fs.Write( bytes, 0, bytes.Length );
            }
        }

        m_DataStock.Add( mosaicId, texture );

        return 0;
	}


    //public static void ClickSetFlagAsync( string mosaicId )
    //{
    //    if( LocationManager.LocationList.Count == 0 ) return;
    //    var setData = LocationManager.GetLocation();
    //    //var mosaicId = "298182EF0E364B30";
    //    var message = $"Test_Set_flag,1,{mosaicId},{setData.x.ToString( "R" )},{setData.y.ToString( "R" )},あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわをん";
    //    SetFlagAsync( message ).Forget();
    //}

    public static async UniTask<int> SetFlagAsync( string message )
    {
        var key = Test_AccountManager.Instance.LoadPrivateKey();
        if( key == "" )
        {
            Debug.Log( "No Key" );
            return 1;
        }
        var alicePrivateKey = new PrivateKey( Converter.HexToBytes( key ) );
        var aliceKeyPair = new KeyPair( alicePrivateKey );

        var bobAddress = new SymbolAddress( "TAWGU4RS2XAD52VSAPMU3TKDCBJNHSAITF7YELQ" ); //TANYEISCFJ7ZYLPIRXNQZCZUOM3UIDN4QOI6N5Q

        var tx = new TransferTransactionV1
        {
            Network = NetworkType.TESTNET, //テストネット・メインネット区分
            RecipientAddress = new UnresolvedAddress( bobAddress.bytes ),
            SignerPublicKey = Test_AccountManager.Instance.AlicePublicKey,
            Mosaics = new UnresolvedMosaic[]
            {
                new ()
                {
                    MosaicId = new UnresolvedMosaicId(0x72C0212E67A08BCE), //テスネットXYM
                    Amount = new Amount(1000000) //1XYM(divisibility:6)
                }
            },
            Message = Converter.Utf8ToPlainMessage( message ), //メッセージ
            Deadline = new Timestamp( Test_AccountManager.Instance.Facade.Network.FromDatetime<CatSdk.NetworkTimestamp>( DateTime.UtcNow ).AddHours( 2 ).Timestamp ) //Deadline:有効期限
        };
        tx.Sort();
        TransactionHelper.SetMaxFee( tx, 100 ); //手数料

        var signature = Test_AccountManager.Instance.Facade.SignTransaction( aliceKeyPair, tx );
        var payload = TransactionsFactory.AttachSignature( tx, signature );
        var hash = Test_AccountManager.Instance.Facade.HashTransaction( tx, signature );
        //Console.WriteLine( hash );
        var result = await Announce( payload );
        //Console.WriteLine( result );

        // hash,TANYEISCFJ7ZYLPIRXNQZCZUOM3UIDN4QOI6N5Q,32543886000,Test_Set_flag,1,298182EF0E364B30,-90.000000,-180.000000,あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわをん

        FlagObjectsManager.Instance.SetFlagObject( $"{UnityEngine.Random.Range( 1, 1000000 )},{Test_AccountManager.Instance.AliceAddress},32543886000," + message ).Forget();

        Test_AccountManager.Instance.UpdateAccountDataAsync().Forget();
        return 0;
	}

    static async UniTask<string> Announce( string payload )
    {
        using var client = new HttpClient();
        var content = new StringContent( payload, Encoding.UTF8, "application/json");
        var response = client.PutAsync( Test_AccountManager.Node + "/transactions", content ).Result;
        return await response.Content.ReadAsStringAsync();
    }
}
