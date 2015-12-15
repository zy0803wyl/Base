using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fp_Part
{
    interface ICallApi
    {
        string Url { get; set; }
        string PostData { get; set; }
        string Post();
    }
}
