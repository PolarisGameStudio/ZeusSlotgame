namespace Libs
{
    public class CashAwardItem:BaseAwardItem
    {
        public override string GetAwardCountDesc()
        {
            //配置金钱时使用的时原值，需要放大到网赚模块的倍数
            return OnLineEarningMgr.Instance.GetCashStr(count, 0, false, true);
        }
    }
}