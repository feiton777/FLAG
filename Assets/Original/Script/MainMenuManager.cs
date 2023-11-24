using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    GameObject m_ExplanationImage;

    RaycastHit m_Hit;

    // Start is called before the first frame update
    void Start()
    {
        if( PlayerPrefs.HasKey( "FLAG01" ) )
        {
            m_ExplanationImage.SetActive( false );
        }
    }

    // Update is called once per frame
    void Update()
    {

        bool isMouseClick = Input.GetMouseButtonDown( 0 );

        if( isMouseClick )
        {
            Vector3 mousePosition = Input.mousePosition;

            Debug.Log( mousePosition );

            Ray ray = Camera.main.ScreenPointToRay( mousePosition );

            bool isHit = Physics.Raycast( ray, out RaycastHit hitInfo );

            if( isHit )
            {
                Debug.Log( hitInfo.collider.name );
                var data = hitInfo.collider.transform.parent.gameObject.GetComponent<FlagObject>();
                //var data = hitInfo.collider.gameObject.GetComponent<FlagObject>();
                if( data != null )
                {
                    // 選択されたオブジェクトへの処理
                    AnnounceImageManager.Instance.OpenAnnounceImage( "message\n" + data.MESSAGE );
                }
            }
        }

        /*
        if( Input.touchCount > 0 )
        {
            Touch touch = Input.touches[ 0 ];
            if( touch.phase == TouchPhase.Began )
            {
                var _ray = Camera.main.ScreenPointToRay( touch.position );
                if( Physics.Raycast( _ray, out m_Hit ) )
                {
                    var data = m_Hit.collider.gameObject.GetComponent<FlagObject>();
                    // 選択されたオブジェクトへの処理
                    AnnounceImageManager.Instance.OpenAnnounceImage( "message\n" + data.MESSAGE );
                }
            }
        }
        */
    }

    public void CloseExplanationImage()
	{
        m_ExplanationImage.SetActive( false );
        AnnounceImageManager.Instance.OpenAnnounceImage("ブロックチェーンを利用するには\n暗号資産であるXYMが必要になります\nAccount画面からFausetページを開き\nXYMをゲットしましょう");
        PlayerPrefs.SetInt( "FLAG01", 1 );
        PlayerPrefs.Save();
    }
}
