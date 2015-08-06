using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nancy;
using Nancy.Responses;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Repository;
using NuFridge.Shared.Logging;

namespace NuFridge.Shared.Database.Services
{
    public class FeedConfigurationService : IFeedConfigurationService
    {
        private readonly IFeedConfigurationRepository _feedConfigurationRepository;

        private readonly ILog _log = LogProvider.For<FeedConfigurationService>();

        public FeedConfigurationService(IFeedConfigurationRepository feedConfigurationRepository)
        {
            _feedConfigurationRepository = feedConfigurationRepository;
        }


        public void Insert(FeedConfiguration feedConfiguration)
        {
           _feedConfigurationRepository.Insert(feedConfiguration);
        }

        public void Delete(FeedConfiguration feedConfiguration)
        {
            _feedConfigurationRepository.Delete(feedConfiguration);
        }

        public FeedConfiguration FindByFeedId(int feedId)
        {
            return _feedConfigurationRepository.FindByFeedId(feedId);
        }

        

        public void Update(FeedConfiguration feedConfig)
        {
            if (string.IsNullOrWhiteSpace(feedConfig.Directory))
            {
                throw new InvalidOperationException("A feed directory must be provided.");
            }

            FeedConfiguration existingFeedConfig = FindByFeedId(feedConfig.FeedId);

            if (existingFeedConfig == null)
            {
                throw new InvalidOperationException("Feed configuration not found.");
            }

            if (existingFeedConfig.Id != feedConfig.Id)
            {
                throw new InvalidOperationException("Feed configuration with the wrong id was found.");
            }

            if (feedConfig.Directory != existingFeedConfig.Directory)
            {
                List<FeedConfiguration> directoryUsedByOtherConfigs =
                    _feedConfigurationRepository.GetAll()
                        .Where(fc => fc.Directory.Equals(feedConfig.Directory, StringComparison.InvariantCultureIgnoreCase))
                        .ToList();

                if (directoryUsedByOtherConfigs.Any(dr => dr.Id != feedConfig.Id))
                {
                    throw new InvalidOperationException("The feed directory is already being used for another feed.");
                }

                _log.Info("Changing the directory from " + existingFeedConfig.Directory + " to " + feedConfig.Directory + " for feed " + feedConfig.FeedId);

                if (Directory.Exists(existingFeedConfig.Directory))
                {
                    if (!Directory.Exists(feedConfig.Directory))
                    {
                        Directory.CreateDirectory(feedConfig.Directory);
                    }

                    try
                    {
                        MoveDirectory(existingFeedConfig.Directory, feedConfig.Directory);
                    }
                    catch (Exception ex)
                    {
                        _log.ErrorException(
                            "There was an error moving the " + existingFeedConfig.Directory + " folder to " +
                            feedConfig.Directory + " for feed id " + feedConfig.FeedId +
                            ". The directory has not been updated for the feed but packages may have been moved.", ex);
                        throw;
                    }
                }
                else
                {
                    _log.Info("No existing feed directory exists for " + feedConfig.FeedId +
                              ", so no packages will be moved.");
                }
            }

            _feedConfigurationRepository.Update(feedConfig);
        }

        public static void MoveDirectory(string source, string target)
        {
            var sourcePath = source.TrimEnd('\\', ' ');
            var targetPath = target.TrimEnd('\\', ' ');
            var files = Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories)
                                 .GroupBy(Path.GetDirectoryName);
            foreach (var folder in files)
            {
                var targetFolder = folder.Key.Replace(sourcePath, targetPath);
                Directory.CreateDirectory(targetFolder);
                foreach (var file in folder)
                {
                    var targetFile = Path.Combine(targetFolder, Path.GetFileName(file));
                    if (File.Exists(targetFile)) File.Delete(targetFile);
                    File.Move(file, targetFile);
                }
            }
            Directory.Delete(source, true);
        }
    }

    public interface IFeedConfigurationService
    {
        void Insert(FeedConfiguration feedConfiguration);
        void Delete(FeedConfiguration feedConfiguration);
        FeedConfiguration FindByFeedId(int feedId);
        void Update(FeedConfiguration feedConfig);
    }
}