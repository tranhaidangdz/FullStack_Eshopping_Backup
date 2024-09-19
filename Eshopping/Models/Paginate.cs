namespace Eshopping.Models
{
    public class Paginate
    {
        public int TotalItems { get; set; } // Tổng số item 
        public int PageSize { get; set; } // Tong so item/trang
        public int CurrentPage { get; set; } // Trang hien tai
        public int TotalPages { get; set; }
        public int StartPage { get; set; }
        public int EndPage { get; set; }

        public Paginate() { }

        public Paginate(int totalItems, int page, int pageSize = 10)
        {
            // làm tròn tổng items/10 items tren 1 trang  vd 16 items/10 = làm tròn 2 trang 
            int totalPages = (int)Math.Ceiling((decimal)totalItems / (decimal)pageSize);

            int currentPage = page; // page hien tai = 1

            int startPage = currentPage - 5; // trang bat dau trừ 5 button
            int endPage = currentPage + 4; // trang cuoi cong them 4 button

            if (startPage <= 0)
            {
                // Neu trang bat dau <= 0 thi so trang cuoi se bang
                endPage = endPage - (StartPage - 1); // 6 - (-3 - 1) = 10
                startPage = 1;
            }

            if (endPage > totalPages)
            {
                endPage = totalPages; //  so page cuoi = tong so trang

                if (endPage > 10)
                {
                    startPage = endPage - 9;
                }
            }

            TotalItems = totalItems;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalPages = totalPages;
            StartPage = startPage;
            EndPage = endPage;
        }
    }
}
