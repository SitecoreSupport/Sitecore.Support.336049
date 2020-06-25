using System;
using System.IO;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Xdb.Processing.Queue;

namespace Sitecore.Support.PathAnalyzer
{
    public static class ApplicationContainer
    {
        private static Guid _liveProcessingPoolId = Guid.Empty;

        private static Guid _historyProcessingPoolId = Guid.Empty;

        public static IProcessingPool<BinaryKey> GetPathAnalyzerLiveProcessingPool()
        {
            return GetOrCreateProcessingPool(ref _liveProcessingPoolId);
        }

        public static IProcessingPool<BinaryKey> GetPathAnalyzerHistoryProcessingPool()
        {
            return GetOrCreateProcessingPool(ref _historyProcessingPoolId);
        }

        private static IProcessingPool<BinaryKey> GetOrCreateProcessingPool(ref Guid processingPoolId)
        {
            IProcessingPoolFactory processingPoolFactory = CreateObject<IProcessingPoolFactory>("processing/ProcessingPoolFactory");
            if (processingPoolId == Guid.Empty)
            {
                processingPoolId = processingPoolFactory.CreateProcessingPool(new GenericProcessingPoolDefinition(DuplicateKeyStrategy.Overwrite, 2, 10, "description"));
            }
            return processingPoolFactory.GetProcessingPool(processingPoolId);
        }

        private static T CreateObject<T>(string configurationPath) where T : class
        {
            Assert.ArgumentNotNullOrEmpty(configurationPath, "configurationPath");
            if (Factory.GetConfigNode(configurationPath, assert: false) == null)
            {
                Log.Fatal(GetMessage($"Can't find confg element with path '{configurationPath}'. Make sure the referred element is present in configuration", "C:\\BA\\97f3fec426e8d621\\Src\\Sitecore.PathAnalyzer\\ApplicationContainer.cs", "CreateObject"), typeof(ApplicationContainer));
                return null;
            }
            return Factory.CreateObject(configurationPath, assert: false) as T;
        }

        private static string GetMessage(string message, string callerFilePath, string memberName)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(callerFilePath);
            string arg = $"{fileNameWithoutExtension}.{memberName}";
            return string.Format("{0}({1}) {2}", "[Path Analyzer]", arg, message);
        }
    }
}