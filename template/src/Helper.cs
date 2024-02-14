using PX.Data;

namespace MyProject
{
    internal static class Helper
    {
        public static void SaveChanges(this PXGraph graph)
        {
            graph.Actions.PressSave();
        }
    }
}
