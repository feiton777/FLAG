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

public class Test_DeepLink : MonoBehaviour
{
    //SymbolFacade m_Facade = new SymbolFacade( CatSdk.Symbol.Network.MainNet );

    public string m_ReceivePayload = null;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AppCall()
	{
        Debug.Log( "AppCall" );
        const string callback = "sample://callback/param1=test";
        var url = "alice://sign?&type=request_pubkey&";
        var packageName = "myApplication";

        using( AndroidJavaClass unityPlayer = new AndroidJavaClass( "com.unity3d.player.UnityPlayer" ) )
        using( AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>( "currentActivity" ) )
        using( AndroidJavaClass intentStaticClass = new AndroidJavaClass( "android.content.Intent" ) )
        using( AndroidJavaClass uriClass = new AndroidJavaClass( "android.net.Uri" ) )
        using( AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>( "parse", url ) )
        {
            var actionView = intentStaticClass.GetStatic<string>( "ACTION_VIEW" );

            using( AndroidJavaObject intent = new AndroidJavaObject( "android.content.Intent", actionView, uriObject ) )
            {
                try
                {
                    Debug.Log( "AppCall" );
                    currentActivity.Call( "startActivity", intent );
                }
                catch( System.Exception e )
                {
                    /*
                    // 未インストールならストアを開くなど
                    Application.OpenURL( "https://play.google.com/store/apps/details?id=" + packageName );

                    // ストアアプなら
                    var urlStore = "market://details?id=" + packageName;
                    using( AndroidJavaClass uriClassStore = new AndroidJavaClass( "android.net.Uri" ) )
                    using( AndroidJavaObject uriObjectStore = uriClassStore.CallStatic<AndroidJavaObject>( "parse", urlStore ) )
                    using( AndroidJavaObject intentStore = new AndroidJavaObject( "android.content.Intent", actionView, uriObjectStore ) )
                    {
                        currentActivity.Call( "startActivity", intentStore );
                    }
                    */
                }
            }
        }
    }

    public async void AppCall_SendMosaic()
    {
        Debug.Log( "Call : AppCall_SendMosaic" );

        CatSdk.Symbol.MosaicFlags testMosaicFlag;

        //var assm = System.Reflection.Assembly.GetExecutingAssembly();
        //var original_types = assm.GetTypes();
        //var types = assm.GetTypes()
        //    .Where( p => p.Namespace == "CatSdk.Symbol" )
        //    .OrderBy( o => o.Name )
        //    .Where( s => !s.Name.Contains( "<>" ) )
        //    .ToList();

        var m_Facade = new SymbolFacade( CatSdk.Symbol.Network.MainNet );

        if( m_Facade == null )
        {
            Debug.Log( "Facade : null" );
            return;
        }

        Debug.Log( "Facade : Get" );

        // 送信元アドレス、キー取得
        var alicePublicKey = new PublicKey( Converter.HexToBytes( "69FF0FE81BBDC33B8C8EFF355B5513ACB877879FEAEFBF6F3EC4E7A36205C783" ) ); //COMSA
        var aliceAddress = m_Facade.Network.PublicKeyToAddress( alicePublicKey.ToString() );

        // 送信先アドレス、キー取得
        var bobPublicKey = new PublicKey( Converter.HexToBytes( "787B1FD2AB7A286C7A3503FB29E7E53D3C1ABF5123DB6C0E2D26F673F9679B6F" ) );
        //var bobPublicKey = new PublicKey( Converter.HexToBytes( "079B79DAD71DD22250F810C48814DABA008D3E4B1F0F0241C6D8A7C315367497" ) ); //NFTDrive
        var bobAddress = m_Facade.Network.PublicKeyToAddress( bobPublicKey.ToString() );

        List<UnresolvedMosaic> sendMosaicList = new List<UnresolvedMosaic>();

        // 送信モザイク(1XYM)
        UnresolvedMosaic item1 = new UnresolvedMosaic();
        item1.MosaicId = new UnresolvedMosaicId( 0x6BED913FA20223F8 );
        item1.Amount = new Amount( 1 * 1000000 );
        sendMosaicList.Add( item1 );

        ulong timeStamp = m_Facade.Network.FromDatetime<CatSdk.NetworkTimestamp>( DateTime.UtcNow ).AddHours( 2 ).Timestamp;

        // トランザクションデータ用意
        var tx = new TransferTransactionV1
        {
            Network = NetworkType.MAINNET,
            RecipientAddress = new UnresolvedAddress( bobAddress.bytes ),
            Mosaics = sendMosaicList.ToArray(),
            SignerPublicKey = alicePublicKey,
            Message = Converter.Utf8ToPlainMessage( "test" ),
            Fee = new Amount( 100000 ),
            Deadline = new Timestamp( timeStamp )
            //Deadline = new Timestamp( m_Facade.Network.FromDatetime<NetworkTimestamp>( DateTime.UtcNow ).AddHours( 2 ).Timestamp )
        };
        tx.Sort();

        Debug.Log( "Deadline : " + tx.Deadline );

        // SSS署名
        m_ReceivePayload = null; // 受け取り用
		//signTransactionByPayload( Converter.BytesToHex( tx.Serialize() ) );

		{
            Debug.Log( "AppCall_SendMosaic" );
            //const string callback = "symbol_flag://mylink/param1=test";
            const string callback = "symbolflag://mylink";
            //var url = "alice://sign?&type=request_sign_transaction&data=" + Converter.BytesToHex( tx.Serialize() ) + "&callback=" + callback;

            string hexCallback = "";
			{
                //char[] values = callback.ToCharArray();
                //foreach( char letter in values )
                //{
                //    int value = Convert.ToInt32( letter );
                //    hexCallback += value.ToString();
                //}

                byte[] data = System.Text.Encoding.UTF8.GetBytes( callback );
                //byte[] data = System.Text.Encoding.Unicode.GetBytes( callback );
                hexCallback = BitConverter.ToString( data ).Replace( "-", "" );
            }

            var test = "687474703A2F2F73796D766F6C7574696F6E2E636F6D2F616C6963652F696E646578322E68746D6C";



            var url = "alice://sign?&type=request_pubkey" + "&callback=" + hexCallback;
            //var url = "alice://sign?&type=request_pubkey" + "&callback=" + Converter.Utf8ToHex( callback );
            //var url = "alice://sign";
            var packageName = "myApplication";

            // sample
            //var url = "alice://sign?type=request_pubkey&callback=687474703A2F2F73796D766F6C7574696F6E2E636F6D2F616C6963652F696E646578322E68746D6C";

            using( AndroidJavaClass unityPlayer = new AndroidJavaClass( "com.unity3d.player.UnityPlayer" ) )
            using( AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>( "currentActivity" ) )
            using( AndroidJavaClass intentStaticClass = new AndroidJavaClass( "android.content.Intent" ) )
            using( AndroidJavaClass uriClass = new AndroidJavaClass( "android.net.Uri" ) )
            using( AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>( "parse", url ) )
            {
                var actionView = intentStaticClass.GetStatic<string>( "ACTION_VIEW" );

                using( AndroidJavaObject intent = new AndroidJavaObject( "android.content.Intent", actionView, uriObject ) )
                {
                    try
                    {
                        Debug.Log( "AppCall_SendMosaic" );
                        currentActivity.Call( "startActivity", intent );
                    }
                    catch( System.Exception e )
                    {
                        //
                    }
                }
            }
        }

        // 入力待ち
        while( m_ReceivePayload == null )
        {
            await UniTask.Yield( PlayerLoopTiming.FixedUpdate );
        }

        //m_Text.text = m_ReceivePayload;

        string signedPayLoad = m_ReceivePayload;
        m_ReceivePayload = null;

        // サイン済ペイロード
        var signedPayLoadMem = new MemoryStream( Converter.HexToBytes( signedPayLoad ) );
        var signedPayLoadBin = new BinaryReader( signedPayLoadMem );
        TransferTransactionV1 signedTx = TransferTransactionV1.Deserialize( signedPayLoadBin );

        // ペイロード作成
        var sendPayload = TransactionsFactory.CreatePayload( signedTx );

        // 送信
        string m_Node = "http://wolf.importance.jp:3000";
        byte[] sendData = Encoding.UTF8.GetBytes( sendPayload );
        UnityWebRequest www = UnityWebRequest.Put( m_Node + "/transactions", sendData );
        www.SetRequestHeader( "Content-Type", "application/json" );
        await www.SendWebRequest();
        var response = www.downloadHandler.text;
        Debug.Log( "response : " + response );
    }
}
