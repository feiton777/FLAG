using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CopyText : MonoBehaviour
{
    [SerializeField]
    private TMPro.TMP_Text m_TargetText;

    public void ClickCopyText()
	{
        GUIUtility.systemCopyBuffer = m_TargetText.text;
    }
}
