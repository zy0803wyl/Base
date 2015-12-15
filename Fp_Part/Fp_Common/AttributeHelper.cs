using System;

namespace Fp_Part.Fp_Common
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    public class HelpAttribute : Attribute
    {
        public HelpAttribute(String Description_in)
        {
            this.description = Description_in;
        }

        protected String description;

        public String Description
        {
            get
            {
                return this.description;
            }
        }
    }
}