namespace GenMan45
{
    using System;
    using System.Runtime.InteropServices;

    internal class Abbrevs
    {
        private string[] m_aOptions;
        private bool[] m_bRequiresValue;
        private bool[] m_bCanHaveValue;

        public Abbrevs(string[] aOptions)
        {
            this.m_aOptions = new string[aOptions.Length];
            this.m_bRequiresValue = new bool[aOptions.Length];
            this.m_bCanHaveValue = new bool[aOptions.Length];
            for (int i = 0; i < aOptions.Length; i++)
            {
                string str = aOptions[i].ToLower();
                if (str.StartsWith("*"))
                {
                    this.m_bRequiresValue[i] = true;
                    this.m_bCanHaveValue[i] = true;
                    str = str.Substring(1);
                }
                else if (str.StartsWith("+"))
                {
                    this.m_bRequiresValue[i] = false;
                    this.m_bCanHaveValue[i] = true;
                    str = str.Substring(1);
                }
                this.m_aOptions[i] = str;
            }
        }

        public string Lookup(string strOpt, out bool bRequiresValue, out bool bCanHaveValue)
        {
            string str = strOpt.ToLower();
            bool flag = false;
            int index = -1;
            for (int i = 0; i < this.m_aOptions.Length; i++)
            {
                if (str.Equals(this.m_aOptions[i]))
                {
                    bRequiresValue = this.m_bRequiresValue[i];
                    bCanHaveValue = this.m_bCanHaveValue[i];
                    return this.m_aOptions[i];
                }
                if (this.m_aOptions[i].StartsWith(str))
                {
                    if (flag)
                    {
                        throw new ApplicationException(Resource.FormatString("Err_AmbigousOption", strOpt));
                    }
                    flag = true;
                    index = i;
                }
            }
            if (!flag)
            {
                throw new ApplicationException(Resource.FormatString("Err_UnknownOption", strOpt));
            }
            bRequiresValue = this.m_bRequiresValue[index];
            bCanHaveValue = this.m_bCanHaveValue[index];
            return this.m_aOptions[index];
        }
    }
}

