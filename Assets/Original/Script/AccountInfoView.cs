using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class AccountInfoView : MonoBehaviour
{
    [SerializeField]
    TMPro.TMP_Text m_Address;
    [SerializeField]
    TMPro.TMP_Text m_PublicKey;
    [SerializeField]
    TMPro.TMP_Text m_PrivateKey;
    [SerializeField]
    TMPro.TMP_Text m_XYM;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void ClickFaucet()
	{
        Application.OpenURL( "https://testnet.symbol.tools/" );
    }

	private void OnEnable()
	{
        SetAccountDataAsync().Forget();
    }

    async UniTask<int> SetAccountDataAsync()
	{
        m_Address.text = Test_AccountManager.Instance.AliceAddress.ToString();
        m_PublicKey.text = CatSdk.Utils.Converter.BytesToHex( Test_AccountManager.Instance.AlicePublicKey.bytes );
        m_PrivateKey.text = Test_AccountManager.Instance.LoadPrivateKey();

        await Test_AccountManager.Instance.UpdateAccountDataAsync();

        if( Test_AccountManager.Instance.AliceAddress == null ) return 1;
        m_Address.text = Test_AccountManager.Instance.AliceAddress.ToString();
        //m_Address.text = CatSdk.Utils.Converter.BytesToHex( Test_AccountManager.Instance.AliceAddress.bytes );
        m_PublicKey.text = CatSdk.Utils.Converter.BytesToHex( Test_AccountManager.Instance.AlicePublicKey.bytes );
        m_PrivateKey.text = Test_AccountManager.Instance.LoadPrivateKey();

        if( Test_AccountManager.Instance.AliceAccountDatum != null && Test_AccountManager.Instance.AliceAccountDatum.account != null && Test_AccountManager.Instance.AliceAccountDatum.account.mosaics != null )
        {
            var result = Test_AccountManager.Instance.AliceAccountDatum.account.mosaics.Find( n => n.id.Equals( "72C0212E67A08BCE" ) );
            if( result != null )
            {
                m_XYM.text = ( double.Parse( result.amount ) / 1000000.0f ).ToString();
            }
        }

        return 0;
	}
}
