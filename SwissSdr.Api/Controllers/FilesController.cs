using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Halcyon.HAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Raven.Client;
using RestApiHelpers.Validation;
using SwissSdr.Api.Configuration;
using SwissSdr.Api.InputModels;
using SwissSdr.Api.Resources;
using SwissSdr.Api.Services;
using SwissSdr.Datamodel;
using SwissSdr.Shared;
using System.Net.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using SwissSdr.Api.Authorization;
using Microsoft.AspNetCore.StaticFiles;
using System.Linq;

namespace SwissSdr.Api.Controllers
{
	[Route("v1/files")]
	public class FilesController : ControllerBase
	{
		private static readonly HttpClient _httpClient;
		private static readonly Dictionary<ImageSize, int[]> _imageSizes = new Dictionary<ImageSize, int[]>()
		{
			{ ImageSize.Thumbnail, new [] {300, 300} },
			{ ImageSize.Large, new [] {1600, 900} }
		};
		private static readonly ILookup<string, string> _mimeMappings;

		private readonly IMapper _mapper;
		private readonly IAsyncDocumentSession _session;
		private readonly ResourceFactory _resourceFactory;
		private readonly CloudBlobClient _blobClient;
		private readonly PermissionService _permissionService;
		private readonly ILogger<FilesController> _logger;
		private readonly AzureFunctionsOptions _azureFunctionsOptions;

		static FilesController()
		{
			_httpClient = new HttpClient()
			{
				Timeout = TimeSpan.FromMinutes(3)
			};

			var mappings = new Dictionary<string, string>(new FileExtensionContentTypeProvider().Mappings);
			mappings.Remove(".jpe");
			mappings.Remove(".jpeg");
			_mimeMappings = mappings.ToLookup(kv => kv.Value, kv => kv.Key);
		}

		public FilesController(IAsyncDocumentSession session,
			IMapper mapper,
			IOptions<AzureFunctionsOptions> azureFunctionsOptionsAccessor,
			PermissionService permissionService,
			ResourceFactory resourceFactory,
			ILoggerFactory loggerFactory,
			CloudBlobClient blobClient)
		{
			_session = session;
			_mapper = mapper;
			_permissionService = permissionService;
			_resourceFactory = resourceFactory;
			_logger = loggerFactory.CreateLogger<FilesController>();
			_azureFunctionsOptions = azureFunctionsOptionsAccessor.Value;
			_blobClient = blobClient;
		}

		[HttpGet]
		[ProducesResponse(typeof(ItemsResource<FileResource>), HttpStatusCode.OK)]
		public async Task<IActionResult> GetFiles(int? skip, int? take)
		{
			RavenQueryStatistics statistics;
			var files = await _session.Query<File>()
						.Statistics(out statistics)
						.Paged(skip, take)
						.ToListAsync();

			var resource = _resourceFactory.CreatePagedCollectionResource<FileResource, File, FilesController>(
				files, skip, take, statistics.TotalResults,
				(s, t) => _ => GetFiles(s, t),
				f => _ => GetFile(_session.GetIdValuePart(f.Id)));
			return Ok(resource);
		}

		[HttpGet("{id:int}")]
		[ProducesResponse(typeof(FileResource), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetFile(int id)
		{
			var file = await _session.LoadAsyncAndThrowIfNull<File>(id);

			var resource = CreateFileRepresentation(file);
			return Ok(resource);
		}

		[HttpGet("{id:int}/{imageSize}/{filename}")]
		public async Task<IActionResult> GetFileData(int id, ImageSize imageSize)
		{
			var file = await _session.LoadAsyncAndThrowIfNull<File>(id);
			var container = _blobClient.GetContainerReference(SwissSdrConstants.Storage.FileBlobContainerName);
			var blob = container.GetBlobReference(file.GetBlobName());
			if (!await blob.ExistsAsync())
			{
				_logger.LogError("Could not find blob named '{0}' for file '{1}'.", file.GetBlobName(), file.Id);
				return StatusCode(500, "No blob for this file");
			}

			var resizedBlob = container.GetBlockBlobReference($"{file.Id.Replace("/", "-")}__{imageSize.ToString().ToLowerInvariant()}.jpg");
			if (await resizedBlob.ExistsAsync())
			{
				var stream = await resizedBlob.OpenReadAsync();
				return File(stream, "image/jpeg");
			}
			else
			{
				var resizeResponse = await _httpClient.GetAsync(GetResizeUrl(file.Url, _imageSizes[imageSize]));
				if (!resizeResponse.IsSuccessStatusCode)
				{
					_logger.LogError("Could not resize file '{0}'. Received Statuscode {1} from resize function.", file.Id, resizeResponse.StatusCode);
					return StatusCode(500, "Could not resize image");
				}
				var data = await resizeResponse.Content.ReadAsByteArrayAsync();

				await resizedBlob.UploadFromByteArrayAsync(data, 0, data.Length);
				resizedBlob.Properties.ContentType = "image/jpeg";
				await resizedBlob.SetPropertiesAsync();

				return File(data, "image/jpeg");
			}
		}
		private string GetResizeUrl(string url, int[] size) => $"{_azureFunctionsOptions.Endpoint}/api/resizeimage?url={Uri.EscapeDataString(url)}&width={size[0]}&height={size[1]}&code={Uri.EscapeDataString(_azureFunctionsOptions.ApiKey)}";

		/*
		 * Upload sequence
		 * 
		 * POST /files/uploads
		 * 201 Created at /files/uploads/1, { uploadUrl, SasToken }
		 * PUT /files/uploads/1, { file metadata }
		 * OK
		 * PUT /files/uploads/1, { changed file metadata }
		 * OK
		 * POST /files/uploads/1, { changed file metadata }
		 * 201 Created at /files/1234, { final file resource }
		 * 
		 */

		[HttpPost("uploads")]
		[Authorize(Policies.Authenticated)]
		[ProducesResponse(typeof(FileUploadResource), HttpStatusCode.Created)]
		public async Task<IActionResult> CreateUpload()
		{
			var uploadBlobId = Guid.NewGuid();

			var container = _blobClient.GetContainerReference(SwissSdrConstants.Storage.UploadBlobContainerName);
			var blob = container.GetBlobReference(uploadBlobId.ToString());

			var policy = new SharedAccessBlobPolicy()
			{
				SharedAccessExpiryTime = DateTimeOffset.Now.AddMinutes(30),
				Permissions = SharedAccessBlobPermissions.Create | SharedAccessBlobPermissions.Write
			};
			var upload = new FileUpload();
			_permissionService.AddCreatorPermissions(User, upload);

			upload.UploadBlobId = uploadBlobId;
			upload.UploadUrl = blob.Uri.ToString();
			upload.SasToken = blob.GetSharedAccessSignature(policy);

			await _session.StoreAsync(upload);
			await _session.SaveChangesAsync();

			var representation = CreateUploadRepresentation(upload);
			return this.CreatedAtAction(_ => GetUpload(_session.GetIdValuePart(upload.Id)), representation);
		}

		[HttpGet("uploads/{id:int}")]
		[Authorize(Policies.Authenticated)]
		[ProducesResponse(typeof(FileUploadResource), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetUpload(int id)
		{
			var upload = await _session.LoadAsyncAndThrowIfNull<FileUpload>(id);

			var representation = CreateUploadRepresentation(upload);
			return Ok(representation);
		}

		[HttpPut("uploads/{id:int}")]
		[Authorize(Policies.Authenticated)]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[ProducesResponse(typeof(FileUploadResource), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.BadRequest)]
		public async Task<IActionResult> ChangeUpload(int id, [FromBody]FileUploadUpdateInputModel updateModel)
		{
			var upload = await _session.LoadAsyncAndThrowIfNull<FileUpload>(id);

			_mapper.Map(updateModel, upload);
			await _session.SaveChangesAsync();

			return await GetUpload(id);
		}

		[HttpPost("uploads/{id:int}")]
		[Authorize(Policies.Authenticated)]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[ProducesResponse(typeof(FileResource), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.BadRequest)]
		public async Task<IActionResult> CreateFile(int id, [FromBody]FileUploadUpdateInputModel updateModel)
		{
			var upload = await _session.LoadAsyncAndThrowIfNull<FileUpload>(id);
			var file = new File();
			await _session.StoreAsync(file);

			_mapper.Map(updateModel, file);

			try
			{
				file = await ProcessUploadedFile(upload.UploadBlobId, file);
				_session.Delete(upload);
			}
			catch (ApiException ex)
			{
				return BadRequest(ex);
			}

			await _session.SaveChangesAsync();

			var resource = CreateFileRepresentation(file);
			return this.CreatedAtAction(c => c.GetFile(_session.GetIdValuePart(file.Id)), resource);
		}

		[HttpPut("{id:int}")]
		[Authorize(Policies.Authenticated)]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[ProducesResponse(typeof(FileResource), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.BadRequest)]
		public async Task<IActionResult> UpdateFile(int id, [FromBody]FileUpdateInputModel updateModel)
		{
			var file = await _session.LoadAsyncAndThrowIfNull<File>(id);

			_mapper.Map(updateModel, file);
			await UpdateBlobContentDisposition(file);
			await _session.SaveChangesAsync();

			var resource = CreateFileRepresentation(file);
			return Ok(resource);
		}

		[HttpDelete("{id:int}")]
		[Authorize(Policies.Authenticated)]
		[ProducesResponse(HttpStatusCode.NoContent)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		public async Task<IActionResult> DeleteFile(int id)
		{
			var file = await _session.LoadAsyncAndThrowIfNull<File>(id);

			var blob = await _blobClient.GetBlobReferenceFromServerAsync(new Uri(file.Url));
			await blob.DeleteIfExistsAsync();
			_session.Delete(file);

			await _session.SaveChangesAsync();

			return NoContent();
		}

		protected async Task UpdateBlobContentDisposition(File file)
		{
			var container = _blobClient.GetContainerReference(SwissSdrConstants.Storage.FileBlobContainerName);
			var blob = container.GetBlobReference(file.GetBlobName());

			if (await blob.ExistsAsync())
			{
				blob.Properties.ContentDisposition = $"{(!blob.IsMediaContentType() ? "attachment; " : "")}filename=\"{file.GetSafeFilename()}\"";
			}

			await blob.SetPropertiesAsync();
			_logger.LogDebug("Changed '{0}' Content-Disposition to '{1}'", blob.Name, blob.Properties.ContentDisposition);
		}

		protected async Task<File> ProcessUploadedFile(Guid blobId, File file)
		{
			var tempContainer = _blobClient.GetContainerReference(SwissSdrConstants.Storage.UploadBlobContainerName);
			var tempBlob = tempContainer.GetBlobReference(blobId.ToString());

			if (!await tempBlob.ExistsAsync())
			{
				throw new ApiException($"No upload with UploadBlobId '{blobId}' exist");
			}

			file.MimeType = tempBlob.Properties.ContentType;
			file.Size = tempBlob.Properties.Length;

			file.Extension = _mimeMappings[file.MimeType].FirstOrDefault() ?? ".data";

			var container = _blobClient.GetContainerReference(SwissSdrConstants.Storage.FileBlobContainerName);
			var blob = container.GetBlobReference(file.GetBlobName());
			_logger.LogDebug("Creating blob '{0}', File: {1}", file.GetBlobName(), file);

			await blob.StartCopyAsync(tempBlob.Uri);
			await tempBlob.DeleteAsync();
			await UpdateBlobContentDisposition(file);

			file.Url = blob.Uri.ToString();

			return file;
		}

		private HALResponse CreateFileRepresentation(File file)
		{
			var resource = _mapper.Map<FileResource>(file);
			return new HALResponse(resource)
				.AddLinks(this.CreateSelfLink(_ => GetFile(_session.GetIdValuePart(file.Id))));
		}

		private HALResponse CreateUploadRepresentation(FileUpload upload)
		{
			var resource = _mapper.Map<FileUploadResource>(upload);
			return new HALResponse(resource)
				.AddLinks(this.CreateSelfLink(_ => GetUpload(_session.GetIdValuePart(upload.Id))));
		}
	}
}
