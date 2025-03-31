using System;

namespace Automation
{
    public class EnvironmentHandler
    {
        internal readonly bool IsMultiuser;
        internal readonly bool IsSingleUser;
        internal readonly string ProfileName;

        public EnvironmentHandler(string environmentType)
        {

            ProfileName = Environment.UserName;

            if (environmentType.Trim() == "MULTI")
            {
                IsMultiuser = true;
                IsSingleUser = false;
            }
            else
            {
                IsMultiuser = false;
                IsSingleUser = true;
            }
        }
    }
}
