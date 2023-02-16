using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RankPanel : MonoSingleton<RankPanel>
{
    //랭크패널의 종속패널
    [SerializeField]
    private RectTransform IndividualRPanel;
    [SerializeField]
    private RectTransform GuildRPanel;

    public void Start()
    {
        DeActiveRankPanel();
    }

    public void ActiveRankPanel()
    {
        this.gameObject.SetActive(true);
        //초기 상태
        IndividualRPanel.gameObject.SetActive(true);
        GuildRPanel.gameObject.SetActive(false);
    }

    public void DeActiveRankPanel()
    {
        this.gameObject.SetActive(false);
    }

    ///////////////////////버튼 클릭 이벤트////////////////////////
    public void OnClickIndividualBtn()
    {
        IndividualRPanel.gameObject.SetActive(true);
        GuildRPanel.gameObject.SetActive(false);
    }
    public void OnClickGuildBtn()
    {
        IndividualRPanel.gameObject.SetActive(false);
        GuildRPanel.gameObject.SetActive(true);
    }

    public void OnClickXbutton()
    {
        DeActiveRankPanel();
    }
}
