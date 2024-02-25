using System.Collections.Generic;

namespace com.github.zehsteam.SellMyScrap;

public class ScrapToSell
{
    public List<GrabbableObject> scrap;
    public int amount => scrap.Count;
    public int value;
    public int realValue => ScrapHelper.GetRealValue(value);

    public ScrapToSell(List<GrabbableObject> scrap)
    {
        this.scrap = scrap;

        value = 0;
        scrap.ForEach(item => value += item.scrapValue);
    }
}
