using System.Collections.Generic;
using System.Linq;

namespace com.github.zehsteam.SellMyScrap;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class ScrapToSell
{
    public List<GrabbableObject> Scrap;
    public int Amount => Scrap.Count;
    public int Value = 0;
    public int RealValue => ScrapHelper.GetRealValue(Value);

    public ScrapToSell(List<GrabbableObject> scrap)
    {
        Scrap = scrap;
        Value = scrap.Sum(item => item.scrapValue);
    }
}
