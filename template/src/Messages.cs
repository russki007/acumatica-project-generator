using PX.Common;

namespace MyProject
{
	[PXLocalizable(Messages.Prefix)]
	public static class Messages
	{
		public const string Prefix = "MA Error";
        internal static readonly string MememberIsNotFound = "Specified memember '{0}' has not been found '{1}'. Check it has been depricated or changed.";
    }
}
