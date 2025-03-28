namespace Automation
{
    public class EnvironmentHandler
    {
        internal readonly bool IsMultiuser;
        internal readonly bool IsSingleUser;

        public EnvironmentHandler(string environmentType)
        {
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
