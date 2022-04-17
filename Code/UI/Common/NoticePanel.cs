using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoticePanel : MonoSingleton<NoticePanel>
{
    [SerializeField]
    private Text noticeText;

    public void Start()
    {
        this.gameObject.SetActive(false);
    }

    public IEnumerator ActiveNoticePanel(string str, float second)
    {
        this.gameObject.SetActive(true);
        noticeText.text = str;

        yield return new WaitForSeconds(second);

        noticeText.text = null;
        this.gameObject.SetActive(false);
    }
}
