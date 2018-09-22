namespace GenMan45
{
    using System;

    internal class Option
    {
        private string m_strName;
        private string m_strValue;

        public Option(string strName, string strValue)
        {
            this.m_strName = strName;
            this.m_strValue = strValue;
        }

        public string Name =>
            this.m_strName;

        public string Value =>
            this.m_strValue;
    }
}

