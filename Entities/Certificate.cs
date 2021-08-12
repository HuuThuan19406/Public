using System;
using System.Collections.Generic;

#nullable disable

namespace Api.Entities
{
    public partial class Certificate
    {
        public int CertificateId { get; set; }
        public string SupplierId { get; set; }
        public string CertificateName { get; set; }
        public string Unit { get; set; }
        public DateTime CertificateDate { get; set; }

        public virtual Supplier Supplier { get; set; }
    }
}
