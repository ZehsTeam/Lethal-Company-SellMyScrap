using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
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
