using System.Collections.Generic;
using System.Linq;

namespace SIMPLSharpLightingManager.UserInterfaces
{
    public class PagedList<T> : List<T>, IPagedList<T>
    {

        public static int DefaultPageSize { get { return 10; } }

        private IEnumerable<T> _sourceList = null;
        private int _pageSize = DefaultPageSize;
        private int _pageIndex = 0;


        //public PagedList() : base() { }

        public PagedList(IEnumerable<T> sourceList)
            : base()
        {
            _sourceList = sourceList;
            RefreshDisplayedRows();
        }

        public IEnumerable<T> SourceList
        {
            get { return _sourceList; }
            set
            {
                _sourceList = value;
                RefreshDisplayedRows();
            }
        }

        public int PageSize
        {
            get { return _pageSize; }
            set
            {
                _pageSize = (value < 0 ? 0 : value);
                RefreshDisplayedRows();
            }
        }

        public int TotalCount { get { return _sourceList == null ? 0 : _sourceList.Count(); } }
        public int PageCount { get { return _pageSize == 0 ? 1 : ((int)(TotalCount / _pageSize) + (TotalCount % _pageSize == 0 ? 0 : 1)); } }
        public int PageIndex { get { return _pageIndex; } set { SetPage(value); } }

        private void SetPage(int pageIndex)
        {
            if (TotalCount == 0 || pageIndex < 0) _pageIndex = 0;
            else if (pageIndex > (PageCount - 1)) _pageIndex = (PageCount - 1);
            else _pageIndex = pageIndex;
            RefreshDisplayedRows();
        }

        private void RefreshDisplayedRows()
        {
            Clear();
            if (_sourceList != null) AddRange(_sourceList.Skip(_pageIndex * _pageSize).Take(_pageSize).ToList());
        }

        public bool HasPreviousPage { get { return PageIndex > 0; } }
        public bool HasNextPage { get { return PageIndex < (PageCount - 1); } }

        public void GetFirstPage() { if (HasPreviousPage) SetPage(0); }
        public void GetPreviousPage() { if (HasPreviousPage) SetPage(PageIndex - 1); }
        public void GetNextPage() { if (HasNextPage) SetPage(PageIndex + 1); }
        public void GetLastPage() { if (HasNextPage) SetPage(PageCount - 1); }

        public int PageIndexOf(T item)
        {
            var index = _sourceList.ToList().IndexOf(item);
            if (index < 0) return -1;
            return ((int)((index + 1) / _pageSize) + ((index + 1) % _pageSize == 0 ? 0 : 1)) - 1;
        }

        //public int LastPageOf(T item) {
        //    var index = _sourceList.ToList().LastIndexOf(item);
        //    if (index < 0) return -1;
        //    return (int)((index + 1) / _pageSize) + ((index + 1) % _pageSize == 0 ? 0 : 1);
        //}

        public void ShowPageOf(T item)
        {
            var pageIndex = PageIndexOf(item);
            if (pageIndex < 0) return;
            SetPage(pageIndex);
        }

        //public void ShowLastPageOf(T item) {
        //    var pageIndex = LastPageOf(item);
        //    if (pageIndex < 0) return;
        //    SetPage(pageIndex);
        //}

        public bool IsVisible(T item)
        {
            return _pageIndex == PageIndexOf(item);// || PageIndex == LastPageOf(item));
        }
    }
}

namespace System.Collections.Generic
{
    public interface IPagedList<T> : IList<T>
    {
        int PageSize { get; set; }

        int TotalCount { get; }
        int PageCount { get; }
        int PageIndex { get; set; }

        //void SetPage(int pageIndex);

        bool HasPreviousPage { get; }
        bool HasNextPage { get; }

        void GetFirstPage();
        void GetPreviousPage();
        void GetNextPage();
        void GetLastPage();

        int PageIndexOf(T item);
        void ShowPageOf(T item);

        //int LastPageOf(T item);
        //void ShowLastPageOf(T item);

        bool IsVisible(T item);
    }
}
