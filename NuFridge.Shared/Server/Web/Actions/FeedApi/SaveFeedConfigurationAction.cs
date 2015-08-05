using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.FeedApi
{
    public class SaveFeedConfigurationAction : IAction
    {
        private readonly IStore _store;
        private readonly ILog _log = LogProvider.For<SaveFeedConfigurationAction>();

        public SaveFeedConfigurationAction(IStore store)
        {
            _store = store;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            FeedConfiguration feedConfig;

            try
            {
                int feedId = int.Parse(parameters.id);

                feedConfig = module.Bind<FeedConfiguration>();

                if (feedId != feedConfig.FeedId)
                {
                    return HttpStatusCode.BadRequest;
                }

                if (string.IsNullOrWhiteSpace(feedConfig.Directory))
                {
                    return new TextResponse(HttpStatusCode.BadRequest, "A feed directory must be provided.");
                }

                FeedConfiguration existingFeedConfig;

                using (var dbContext = new DatabaseContext())
                {
                    existingFeedConfig = dbContext.FeedConfigurations.AsNoTracking().FirstOrDefault(f => f.FeedId == feedId);
                }

                if (existingFeedConfig == null)
                {
                    return HttpStatusCode.NotFound;
                }

                if (feedConfig.Directory != existingFeedConfig.Directory)
                {
                    IQueryable<FeedConfiguration> directoryUsedByOtherConfigs;

                    using (var dbContext = new DatabaseContext())
                    {
                        directoryUsedByOtherConfigs =
                              dbContext.FeedConfigurations.AsNoTracking()
                                  .Where(
                                      fc =>
                                          fc.Directory.Equals(feedConfig.Directory,
                                              StringComparison.InvariantCultureIgnoreCase));
                    }

                    if (directoryUsedByOtherConfigs.Any(dr => dr.Id != feedConfig.Id))
                    {
                        return new TextResponse(HttpStatusCode.BadRequest,
                            "The feed directory is already being used for another feed.");
                    }

                    _log.Info("Changing the directory from " + existingFeedConfig.Directory + " to " +
                              feedConfig.Directory + " for feed " + feedId);

                    if (Directory.Exists(existingFeedConfig.Directory))
                    {
                        if (!Directory.Exists(feedConfig.Directory))
                        {
                            _log.Debug("Creating the " + feedConfig.Directory + " folder for feed id " + feedId);
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
                                feedConfig.Directory + " for feed id " + feedId +
                                ". The directory has not been updated for the feed but packages may have been moved. Error: " +
                                ex.Message, ex);
                            return
                                new TextResponse("There was an error moving the " + existingFeedConfig.Directory +
                                                 " folder to " + feedConfig.Directory + " for feed id " + feedId +
                                                 ". The directory has not been updated for the feed but packages may have been moved. Error: " +
                                                 ex.Message);
                        }
                    }
                    else
                    {
                        _log.Info("No existing feed directory exists for " + feedId +
                                  ", so no packages will be moved.");
                    }
                }

                using (var dbContext = new DatabaseContext())
                {
                    dbContext.FeedConfigurations.Attach(feedConfig);
                    dbContext.Entry(feedConfig).State = EntityState.Modified;
                    dbContext.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                _log.ErrorException(ex.Message, ex);

                return HttpStatusCode.InternalServerError;
            }


            return feedConfig;
        }

        public static void MoveDirectory(string source, string target)
        {
            var sourcePath = source.TrimEnd('\\', ' ');
            var targetPath = target.TrimEnd('\\', ' ');
            var files = Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories)
                                 .GroupBy(s => Path.GetDirectoryName(s));
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
}
