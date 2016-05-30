using AFT.UGS.Core.BaseModels.Bus;

namespace AFT.RegoV2.Core.Game.Interface.Interfaces
{
    public interface IUgsEventRetranslator
    {
        void Retranslate<TEvent>(TEvent @event) where TEvent: BusEvent;
    }
}
