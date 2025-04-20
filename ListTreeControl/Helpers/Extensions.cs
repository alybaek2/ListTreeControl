using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ListTree
{
    public static class Extensions
    {
        public static ReadOnlyObservableCollection<T> ActiveTransform<S, T>(this IEnumerable<S> sourceCollection, Func<S, T> transform)
            where S : class
            where T : class
        {
            ObservableCollection<T> targetCollection = new ObservableCollection<T>();
            ReadOnlyObservableCollection<T> readOnlyCollection = new ReadOnlyObservableCollection<T>(targetCollection);

            TransformedObservableCollection<S, T>.Create(sourceCollection, transform, targetCollection);

            return readOnlyCollection;
        }
    }
}
