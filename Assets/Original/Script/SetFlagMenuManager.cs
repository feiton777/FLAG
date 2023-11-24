using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;


public class SetFlagMenuManager : MonoBehaviour
{
    [SerializeField]
    RawImage m_LoadImage;

    [SerializeField]
    TMPro.TMP_Dropdown m_Dropdown;

    [SerializeField]
    GameObject m_Text;

    [SerializeField]
    TMPro.TMP_InputField m_InputField;


    // Start is called before the first frame update
    void Start()
    {
        m_Dropdown.onValueChanged.AddListener( delegate {
            ChangeFlag();
        } );
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void OnEnable()
	{
        m_Dropdown.ClearOptions();
        m_Dropdown.value = 0;
        m_Dropdown.options.Add( new TMPro.TMP_Dropdown.OptionData { text = "" } );
        m_Dropdown.options.Add( new TMPro.TMP_Dropdown.OptionData { text = "1C65BF2F977501DF" } );
        foreach( var oneData in Test_AccountManager.Instance.AliceAccountDatum.account.mosaics )
        {
            if( oneData.id.Equals( "72C0212E67A08BCE" ) ) continue;
            m_Dropdown.options.Add( new TMPro.TMP_Dropdown.OptionData { text = oneData.id } );
        }
        m_LoadImage.color = Color.clear;
        m_Text.SetActive( true );
    }

    private void ChangeFlag()
	{
        Test_MosaicManager.LoadMosaicAsync( m_Dropdown.options[ m_Dropdown.value ].text, null, m_LoadImage ).Forget(); ;
        m_Text.SetActive( false );
    }

    public void ClickSetFlagAsync()
    {
        m_LoadImage.color = Color.clear;
        if( LocationManager.LocationList.Count == 0 ) return;
        if( m_Dropdown.options.Count == 0 ) return;
        if( m_Dropdown.value == 0 )
        {
            AnnounceImageManager.Instance.OpenAnnounceImage( $"画面上部のドロップダウンから\nNFTを選択して下さい" );
            return;
        }
        if( Test_AccountManager.Instance.AliceXYM < 1.0f )
        {
            AnnounceImageManager.Instance.OpenAnnounceImage( $"XYMが不足しています\nXYMを補給して下さい" );
            return;
        }
        var setData = LocationManager.GetLocation();
        //var mosaicId = "298182EF0E364B30";
        var mosaicId = m_Dropdown.options[ m_Dropdown.value ].text;
        var setMessage = m_InputField.text + " ";
        var message = $"Test_Set_flag,1,{mosaicId},{setData.x.ToString( "R" )},{setData.y.ToString( "R" )},{setMessage}";
        Test_MosaicManager.SetFlagAsync( message ).Forget();
    }

}
