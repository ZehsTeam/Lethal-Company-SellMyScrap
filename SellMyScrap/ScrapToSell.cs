﻿using System.Collections.Generic;
using System.Linq;

namespace com.github.zehsteam.SellMyScrap;

public class ScrapToSell
{
    public List<GrabbableObject> scrap;
    public int amount => scrap.Count;
    public int value = 0;
    public int realValue => ScrapHelper.GetRealValue(value);

    public ScrapToSell(List<GrabbableObject> scrap)
    {
        this.scrap = scrap;
        value = scrap.Sum(item => item.scrapValue);
    }
}
