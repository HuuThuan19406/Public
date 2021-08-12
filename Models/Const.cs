using System;

namespace Api.Models
{
    internal static class Const
    {
        internal const string CLOUD_MAIN_FOLDER_URI = "1-EKCB2y_q4hmEcqmRo1fh9E9KADDuFQI";
        internal const string CLOUD_SUPPLIER_FOLDER_URI = "17_J6ekzCGxzr1jU4dxxmTzP-aaPQdYe7";
        internal const string CLOUD_ATTACH_FOLDER_URI = "1NDZ6uQ90n4od_ixg_s3fkMpLaswID9Rm";

        internal static readonly TimeSpan MINIMUM_ORDER_COMPLETION_DURATION = TimeSpan.FromHours(3);
        internal static readonly TimeSpan MAXIMUM_ORDER_COMPLETION_DURATION = TimeSpan.FromDays(7);
        internal static readonly TimeSpan MAXIMUM_CHECK_ORDERDETAIL_DURATION = TimeSpan.FromHours(6);

        internal static decimal COMMISSION_PERCENT_DEFAULT = 5;
    }
}