using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Logging;
using EPiServer.PlugIn;
using EPiServer.Scheduler;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Epicweb.Optimizely.ContentDelivery.Sync;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;


namespace Epicweb.Optimizely.Web
{
    /// <summary>
    /// This file should be in main projects, where scheduled jobs are
    /// </summary>
    [ScheduledPlugIn(DisplayName = "Site Sync/import job", SortIndex = 10000, GUID = "c03c32df-0326-4e97-8b48-40ecc6d839ce")]
    public class ImportAndSyncJob : ScheduledJobBase
    {

        private readonly ILogger _log = EPiServer.Logging.LogManager.GetLogger();
        private static bool _stop;
        private string returnMessage = string.Empty;
        private int countUpdate = 0;
        private int countNew = 0;
        private int countErrors = 0;
        private int totals = 0;
        private int count = 0;

        private static readonly IdentityMappingService _identityMappingService = ServiceLocator.Current.GetInstance<IdentityMappingService>();

        private readonly IContentRepository _contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
        private readonly IContentTypeRepository _contentTypeRepository = ServiceLocator.Current.GetInstance<IContentTypeRepository>();
        private readonly ISerializeService _serializeService = ServiceLocator.Current.GetInstance<ISerializeService>();
        private readonly ISiteSettings _siteSettings = ServiceLocator.Current.GetInstance<ISiteSettings>();
        private readonly ContentDeliveryMapper _contentDeliveryMapper = ServiceLocator.Current.GetInstance<ContentDeliveryMapper>();
        private readonly IUrlSegmentGenerator _segment = ServiceLocator.Current.GetInstance<IUrlSegmentGenerator>();

        private static bool IsStopped { get; set; }
        public override void Stop() => IsStopped = IsStoppable;


        public ImportAndSyncJob()
            : base()
        {
            // Make the job interruptable
            this.IsStoppable = true;
        }

        public object SerializeCustomPropertyLists(string propertyDataType, System.Text.Json.JsonElement jsonElement)
        {
            switch (propertyDataType)
            {

                case "OpenTimeListProperty": //our list property, needs custom Serializer
                    return jsonElement.ValueKind != JsonValueKind.Null ? jsonElement.Deserialize<IList<OpenTime>>(new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }): null;
                default:
                    break;
            }
            return null;
        }


        public override string Execute()
        {
            _log.Information("Import Job started at: " + DateTime.Now);
            _stop = IsStopped = false;

            if (_siteSettings.PrisonParentPage != null)
            {

                base.OnStatusChanged($"Gets data from Content Delivery API");
                var pages = _serializeService.GetPageModels(
                    new string[] { "StandardPage", "ProductPage" },// names of pagetypes you want to import
                    propsToSync: null); // new string[] { "fax", "longitude", "telephone", "visitReservationXHtml", "VideoConferencing", "extraAddress1City" }); //list of props you want to import, if null, import all, this will not create new props, they need to be in code, only if exists then sync

                try
                {

                    totals = pages.Count();
                    foreach (var pageModel in pages.ToList())
                    {

                        if (IsStopped)
                        {
                            return $"Aborted by user, created { countNew} / updated: { countUpdate } / errors: { countErrors}";
                        }

                        try
                        {
                            count++;
                            PageData page; //generic pagetype
                            Uri externalId = MappedIdentity.ConstructExternalIdentifier("syncjob", pageModel.contentLink.id.ToString());
                            var existingMapping = _identityMappingService.Get(externalId);

                            if (existingMapping != null)
                            {
                                page = _contentRepository.Get<PageData>(existingMapping.ContentLink).CreateWritableClone();
                                
                                if (page.IsDeleted)
                                {
                                    _identityMappingService.Delete(externalId);
                                    _log.Debug("Found one, but removed, create new page, title='" + pageModel.name + "'");
                                    var pagetypeNameToCreate = pageModel.contentType[pageModel.contentType.Length - 1];
                                    page = _contentRepository.GetDefault<PageData>(_siteSettings.ParentPage.ToPageReference(), _contentTypeRepository.Load(pagetypeNameToCreate).ID);
                                }
                            }
                            else
                            {
                                _log.Debug("Found none, create new page, title='" + pageModel.name + "'");
                                var pagetypeNameToCreate = pageModel.contentType[pageModel.contentType.Length - 1];
                                page = _contentRepository.GetDefault<PageData>(_siteSettings.ParentPage.ToPageReference(), _contentTypeRepository.Load(pagetypeNameToCreate).ID);
                            }

                            if (page.VisibleInMenu)
                                page.VisibleInMenu = false;//change depending how you want

                            _contentDeliveryMapper.Map(page, pageModel, SerializeCustomPropertyLists);
                            try
                            {
                                if (page.IsModified)
                                {
                                    ContentReference savedpage = _contentRepository.Save(page, EPiServer.DataAccess.SaveAction.Publish, AccessLevel.NoAccess);

                                    if (savedpage != null && existingMapping == null)
                                    {
                                        _identityMappingService.MapContent(externalId, page);
                                        countNew++;
                                    }
                                    else
                                    {
                                        countUpdate++;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                //check if same url, handle
                                if (ex.Message.Contains("Name in URL"))
                                {
                                    page.URLSegment = _segment.Create(page.Name);
                                    ContentReference savedpage = _contentRepository.Save(page, EPiServer.DataAccess.SaveAction.Publish, AccessLevel.NoAccess);

                                    if (savedpage != null && existingMapping == null)
                                    {
                                        _identityMappingService.MapContent(externalId, page);
                                        countNew++;
                                    }
                                    else
                                    {
                                        countUpdate++;
                                    }
                                }
                            }
                        }
                        catch (Exception ex2)
                        {
                            countErrors++;
                            _log.Debug("Error import job " + pageModel.name + " was not found", ex2);
                        }

                        base.OnStatusChanged($"{count} of {totals}, created { countNew} / updated: { countUpdate } - errors: { countErrors }");
                    }
                }
                catch (Exception ex)
                {
                    _log.Debug("Error in Import job", ex);
                    throw new Exception(returnMessage);
                }
            }
            else
            {
                returnMessage = " - Could not found where to save, no parent root was configured";
            }

            _log.Information("Import Job ended at: " + DateTime.Now);

            // Return message indicating finished status  
            return string.Format($"Job completed. {count} of {totals}, created { countNew} / updated: { countUpdate } - errors: { countErrors } <br/>" + returnMessage).Replace("<br/>", " | ");
        }
    }
}
