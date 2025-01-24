using com.github.zehsteam.SellMyScrap.Helpers.ScrapMatchAlgorithms;
using com.github.zehsteam.SellMyScrap.Patches;
using System.Collections.Generic;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Data
{
    /// <summary>
    /// Scrap sell request with settings from config and flags to choose correct 
    /// </summary>
    /// <param name="value">Scrap value the player needs to get from current selling</param>
    public class SellCommandRequest(int value)
    {
        /// <summary>Scrap value the player needs to get from current selling</summary>
        public int Value => value;
        /// <summary>
        /// Whether should sell algorithms include shotguns, ammo, pickles, gift boxes, etc. to <see cref="BaseScrapMatchAlgorithm.items"/><br></br>
        /// Default: <see langword="true"/>
        /// </summary>
        public bool OnlyAllowedScrap { get; set; } = true;
        /// <summary>
        /// Whether should <see cref="Value"/> be normalized using overtime bonus<br></br>
        /// Default: <see langword="false"/>
        /// </summary>
        public bool WithOvertimeBonus { get; set; } = false;
        /// <summary>
        /// Whether should sell algorithms use only ship inventory items as <see cref="BaseScrapMatchAlgorithm.items"/><br></br>
        /// Default: <see langword="false"/>
        /// </summary>
        public bool OnlyUseShipInventory { get; set; } = false;
        /// <summary>
        /// Algorithm to find correct scrap to fulfill the value<br></br>
        /// Default: <see cref="DefaultScrapMatchAlgorithm"/>
        /// </summary>
        public BaseScrapMatchAlgorithm ScrapMatchAlgorithm { get; set; } = BaseScrapMatchAlgorithm.Default;

        /// <summary>
        /// Scrap value the player needs to get from current selling accounting <see cref="WithOvertimeBonus"/>
        /// </summary>
        public int TargetValue => WithOvertimeBonus ? GetSellValueWithOvertime() : GetSellValue(Value);

        /// <summary>
        /// Retrieves a match for <paramref name="items"/> to sell to fulfill <see cref="TargetValue"/>
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public ScrapToSell GetScrapToSell(List<ItemData> items)
        {
            if (Value == int.MaxValue)
            {
                return new ScrapToSell(items);
            }

            return new ScrapToSell(ScrapMatchAlgorithm.FindMatch(items, TargetValue, Plugin.ConfigManager.PrioritySellListArray));
        }

        private int GetSellValue(int value)
        {
            if (value == int.MaxValue) return value;
            return Mathf.CeilToInt(Value / StartOfRound.Instance.companyBuyingRate);
        }

        private int GetSellValueWithOvertime()
        {
            int profitQuota = TimeOfDay.Instance.profitQuota;
            int quotaFulfilled = TimeOfDay.Instance.quotaFulfilled;
            int valueOver = quotaFulfilled + Value - profitQuota;
            if (valueOver <= 0) return GetSellValue(Value);

            int v = Value;
            int profitQuotaLeft = Mathf.Max(profitQuota - quotaFulfilled, 0);
            v -= (TimeOfDayPatch.GetDaysUntilDeadline() + 1) * 15;
            int newValue = Mathf.CeilToInt((5 * v + profitQuotaLeft + 75) / 6f);

            return GetSellValue(newValue);
        }
    }
}
