namespace GenMan45
{
    using System;

    internal class CommandLine
    {
        private string[] m_aArgList;
        private Option[] m_aOptList;
        private int m_iArgCursor;
        private int m_iOptCursor;
        private Abbrevs m_sValidOptions;

        public CommandLine(string[] aArgs, string[] aValidOpts)
        {
            this.m_sValidOptions = new Abbrevs(aValidOpts);
            string[] sourceArray = new string[aArgs.Length];
            Option[] optionArray = new Option[aArgs.Length];
            int length = 0;
            int num3 = 0;
            for (int i = 0; i < aArgs.Length; i++)
            {
                if (aArgs[i].StartsWith("/") || aArgs[i].StartsWith("-"))
                {
                    string str;
                    string strValue = null;
                    int num4 = aArgs[i].IndexOfAny(new char[] { ':', '=' });
                    if (num4 == -1)
                    {
                        str = aArgs[i].Substring(1);
                    }
                    else
                    {
                        str = aArgs[i].Substring(1, num4 - 1);
                    }
                    str = this.m_sValidOptions.Lookup(str, out bool flag, out bool flag2);
                    if (!flag2 && (num4 != -1))
                    {
                        throw new ApplicationException(Resource.FormatString("Err_NoValueRequired", str));
                    }
                    if (flag && (num4 == -1))
                    {
                        throw new ApplicationException(Resource.FormatString("Err_ValueRequired", str));
                    }
                    if (flag2 && (num4 != -1))
                    {
                        if (num4 == (aArgs[i].Length - 1))
                        {
                            if ((i + 1) == aArgs.Length)
                            {
                                throw new ApplicationException(Resource.FormatString("Err_ValueRequired", str));
                            }
                            if (aArgs[i + 1].StartsWith("/") || aArgs[i + 1].StartsWith("-"))
                            {
                                throw new ApplicationException(Resource.FormatString("Err_ValueRequired", str));
                            }
                            strValue = aArgs[i + 1];
                            i++;
                        }
                        else
                        {
                            strValue = aArgs[i].Substring(num4 + 1);
                        }
                    }
                    optionArray[num3++] = new Option(str, strValue);
                }
                else
                {
                    sourceArray[length++] = aArgs[i];
                }
            }
            this.m_aArgList = new string[length];
            this.m_aOptList = new Option[num3];
            Array.Copy(sourceArray, this.m_aArgList, length);
            Array.Copy(optionArray, this.m_aOptList, num3);
            this.m_iArgCursor = 0;
            this.m_iOptCursor = 0;
        }

        public string GetNextArg()
        {
            if (this.m_iArgCursor >= this.m_aArgList.Length)
            {
                return null;
            }
            return this.m_aArgList[this.m_iArgCursor++];
        }

        public Option GetNextOption()
        {
            if (this.m_iOptCursor >= this.m_aOptList.Length)
            {
                return null;
            }
            return this.m_aOptList[this.m_iOptCursor++];
        }

        public int NumArgs =>
            this.m_aArgList.Length;

        public int NumOpts =>
            this.m_aOptList.Length;
    }
}

