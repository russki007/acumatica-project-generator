using PX.Data;
using PX.Objects.CS;

namespace MyProject
{
    /// <summary>
    ///  Custom Features - for more info refer to.
    /// https://help-2023r1.acumatica.com/Help?ScreenId=ShowWiki&pageid=8285172e-d3b1-48d9-bcc1-5d20e39cc3f0
    /// </summary>
    class Features
    {
        private const string FeatureRequiredError = "Feature '{0}' should be enabled";

        public static bool MyFeature => CheckFeature<FeaturesSetExt.usrMyFeature>("My Feature");

        private static bool CheckFeature<TFeature>(string featureName, bool throwIfRequired = false)
            where TFeature : IBqlField
        {
            if (!PXAccess.FeatureInstalled<TFeature>() && throwIfRequired)
                throw new PXException(string.Format(FeatureRequiredError, featureName));

            return true;
        }
    }

    public class FeaturesSetExt : PXCacheExtension<FeaturesSet>
    {
        #region UsrMyFeature
        public abstract class usrMyFeature : PX.Data.BQL.BqlBool.Field<usrMyFeature> { }
        [Feature(false, typeof(FeaturesSetExt.usrMyFeature), DisplayName = "My Feature")]

        public virtual bool? UsrMyFeature
        {
            get; set;
        }
        #endregion
    }
}
