namespace ListTree
{
    public interface IListTree
    {
        /// <summary>
        /// Returns the root node of the ListTree.
        /// </summary>
        IListTreeNode Root { get; }

        /// <summary>
        /// Contains all nodes in the ListTree, sorted in ascending order of VerticalIndex
        /// </summary>
        IReadOnlyObservableList<IListTreeNode> NodesByVerticalIndex { get; }
    }
}
