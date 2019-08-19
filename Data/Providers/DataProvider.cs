using System.Collections.Generic;

namespace TFT_Overlay.Data.Providers
{
    public interface IDataProvider
    {
        Dictionary<string, TftItem> Items { get; }
        Dictionary<string, TftChampion> Champions { get; }
        Dictionary<string, TftClass> Classes { get; }
        Dictionary<string, TftOrigin> Origins { get; }
    }
}