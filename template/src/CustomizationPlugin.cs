using PX.Data;

namespace MyProject
{
    public class CustomizationGraph : PXGraph<CustomizationGraph>
    {
        public void EnsureData(CustomizationPlugin customizationPlugin)
        {
            // Do work
            this.SaveChanges();
        }
    }

    public class CustomizationPlugin : Customization.CustomizationPlugin
    {
        public override void UpdateDatabase()
        {
            PXGraph.CreateInstance<CustomizationGraph>().EnsureData(this);
        }
    }
}
