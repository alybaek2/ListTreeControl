using System.Collections.Generic;
using System.Collections.Specialized;

namespace ListTree
{
    public interface IReadOnlyObservableList<out T> : INotifyCollectionChanged, IReadOnlyList<T>
    {
    }
}
