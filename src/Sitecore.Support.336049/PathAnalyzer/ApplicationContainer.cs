using System;
using System.IO;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Xdb.Processing.Queue;

namespace Sitecore.Support.PathAnalyzer
{
    public static class ApplicationContainer
    {
        public static IProcessingPool<BinaryKey> GetPathAnalyzerLiveProcessingPool()
        {
            return GetOrCreateProcessingPool(Guid.Parse(Settings.GetSetting("PathAnalyzer.Processing.LiveProcessingPoolId", "{384c8344-cb6d-48e6-a70c-e5a3f56a1960}")), "pathAnalyzerLiveProcessingPool");
        }

        public static IProcessingPool<BinaryKey> GetPathAnalyzerHistoryProcessingPool()
        {
            return GetOrCreateProcessingPool(Guid.Parse(Settings.GetSetting("PathAnalyzer.Processing.HistoryProcessingPoolId", "{8a532051-697b-45b0-8240-ab4108ab6e12}")), "pathAnalyzerHistoryProcessingPool");
        }

        private static IProcessingPool<BinaryKey> GetOrCreateProcessingPool(Guid processingPoolId, string description)
        {
            IProcessingPool<BinaryKey> processingPool = null;
            IProcessingPoolFactory processingPoolFactory = CreateObject<IProcessingPoolFactory>("processing/ProcessingPoolFactory");
            try
            {
                processingPool = processingPoolFactory.GetProcessingPool(processingPoolId);
            }
            catch (ProcessingPoolDoesNotExistException)
            {
                processingPoolFactory.CreateProcessingPool(
                    new GenericProcessingPoolDefinition(DuplicateKeyStrategy.Overwrite, 2, 10, description),
                    processingPoolId);
                processingPool = processingPoolFactory.GetProcessingPool(processingPoolId);
            }
            Assert.IsNotNull(processingPool, $"Can't get Processing Pool with an Id {processingPoolId}");
            return processingPool;
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