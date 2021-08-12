using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models
{
    /// <summary>
    /// Thống kê đánh giá.
    /// </summary>
    public class RateStatistics
    {
        /// <summary>
        /// Tỉ lệ đánh giá trung bình.
        /// </summary>
        public double AverageRate { get; set; }

        /// <summary>
        /// Tổng số đánh giá.
        /// </summary>
        public int Count { get; set; }
    }
}
