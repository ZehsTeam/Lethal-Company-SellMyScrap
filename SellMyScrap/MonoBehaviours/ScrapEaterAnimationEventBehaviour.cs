using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

public class ScrapEaterAnimationEventBehaviour : MonoBehaviour
{
    public ScrapEaterBehaviour scrapEaterBehaviour;

    public void ShowModel()
    {
        scrapEaterBehaviour.modelObject.SetActive(true);
    }

    public void HideMode()
    {
        scrapEaterBehaviour.modelObject.SetActive(false);
    }
}
