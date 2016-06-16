﻿using DecisionServicePrivateWeb.Classes;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Research.MultiWorldTesting.Contract;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DecisionServicePrivateWeb.Controllers
{
    [RequireHttps]
    public class AutomationController : Controller
    {
        [HttpGet]
        public async Task UpdateSettings(string trainArguments = null, float? initialExplorationEpsilon = null, bool? isExplorationEnabled = null)
        {
            var token = Request.Headers["Authorization"];
            if (token != ConfigurationManager.AppSettings[ApplicationMetadataStore.AKPassword])
                throw new UnauthorizedAccessException();

            string azureStorageConnectionString = ConfigurationManager.AppSettings[ApplicationMetadataStore.AKConnectionString];
            var storageAccount = CloudStorageAccount.Parse(azureStorageConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var settingsBlobContainer = blobClient.GetContainerReference(ApplicationBlobConstants.SettingsContainerName);

            var blob = settingsBlobContainer.GetBlockBlobReference(ApplicationBlobConstants.LatestClientSettingsBlobName);
            ApplicationClientMetadata clientMeta;
            if (await blob.ExistsAsync())
                clientMeta = ApplicationMetadataUtil.DownloadMetadata<ApplicationClientMetadata>(blob.Uri.ToString());
            else
                clientMeta = new ApplicationClientMetadata();

            if (trainArguments != null)
                clientMeta.TrainArguments = trainArguments;

            if (initialExplorationEpsilon != null)
                clientMeta.InitialExplorationEpsilon = (float)initialExplorationEpsilon;

            if (isExplorationEnabled != null)
                clientMeta.IsExplorationEnabled = (bool)isExplorationEnabled;

            await blob.UploadTextAsync(JsonConvert.SerializeObject(clientMeta));
        }
    }
}