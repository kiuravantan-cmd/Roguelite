using System.Collections.Generic;

namespace Core.MasterData
{
    /// <summary>
    /// ScriptableObjectが必ずレコードのリストを持つことを保証する
    /// </summary>
    public interface IMasterDataContainer<T> where T : IMasterData
    {
        List<T> Records { get; }
    }
}
