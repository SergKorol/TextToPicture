using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp.Formats.Png;
using TextToPicture.Models;



namespace TextToPicture.Controllers;

public class HomeController : Controller
{

    private readonly HttpClient _httpClient;
    private readonly IWebHostEnvironment _hostingEnvironment;
    
    public HomeController(IWebHostEnvironment hostingEnvironment)
    {
        _hostingEnvironment = hostingEnvironment;
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("https://api.openai.com/");
    }
    
    [HttpPost]
    public async Task<ActionResult> GenerateImage(GenerateInput generateInput)
    {
        try
        {
            //Request creating
            var request = CreateBaseRequest(HttpMethod.Post, "v1/images/generations");
            var jsonRequest = JsonSerializer.Serialize(generateInput);
            request.Content = new StringContent(jsonRequest);
            request.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            
            //Result 
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            // Pass the image URL to the view
            var model = new Dalle { Links = await GetUrls(response)};
            return RedirectToAction("Index", model);
        }
        catch (Exception ex)
        {
            // Handle any error that occurred during the API request
            ViewBag.Error = ex.Message;
            return View("Index");
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> EditImage(EditInput editInput)
    {
        try
        {
            // Add the form data
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(editInput.Prompt), "prompt");
            formData.Add(new StringContent(editInput.N.ToString()), "n");
            formData.Add(new StringContent(editInput.Size), "size");
    
            // Add the image file
            await AddFormDataFile(formData, editInput.Image, "image");
    
            //Add the mask file
            await AddFormDataFile(formData, editInput.Mask, "mask");
    
            // Prepare the form data
            var request = CreateBaseRequest(HttpMethod.Post, "v1/images/edits");
            request.Content = formData;
    
            // Make the API request
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
    
            // Pass the image URL to the view
            var model = new Dalle { Links = await GetUrls(response)};
            return RedirectToAction("Index", model);
        }
        catch (Exception ex)
        {
            // Handle any error that occurred during the API request
            ViewBag.Error = ex.Message;
            return View("Index");
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> VariationImage(VariationInput variationInput)
    {
        try
        {
            // Add the form data
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(variationInput.N.ToString()), "n");
            formData.Add(new StringContent(variationInput.Size), "size");
    
            // Add the image file
            await AddFormDataFile(formData, variationInput.Image, "image");
    
    
            // Prepare the form data
            var request = CreateBaseRequest(HttpMethod.Post, "v1/images/variations");
            request.Content = formData;
    
            // Make the API request
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
    
            // Pass the image URL to the view
            var model = new Dalle { Links = await GetUrls(response)};
            return RedirectToAction("Index", model);
        }
        catch (Exception ex)
        {
            // Handle any error that occurred during the API request
            ViewBag.Error = ex.Message;
            return View("Index");
        }
    }

    public IActionResult Index(Dalle model)
    {
        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }
    
    public async Task<byte[]> DownloadImage(string url)
    {
        return await _httpClient.GetByteArrayAsync(url);
    }

    private async Task AddFormDataFile(MultipartFormDataContent formData, IFormFile file, string name)
    {
        using var memoryStream = new MemoryStream();
        await using (var fileStream = file.OpenReadStream())
        {
            await fileStream.CopyToAsync(memoryStream);
        }

        var imageData = ConvertRgb24ToRgba32(memoryStream.ToArray());
        var imageContent = new ByteArrayContent(imageData);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        formData.Add(imageContent, name, file.FileName);
    }

    private async Task<List<string>> GetUrls(HttpResponseMessage response)
    {
        // Deserialize the JSON response
        var responseObject = await response.Content.ReadFromJsonAsync<ResponseModel>();

        var links = responseObject?.Data!.Select(x => x.Url).ToList();
        foreach (var link in links!.Where(link => !string.IsNullOrEmpty(link)))
        {
            if (link != null) await SaveFileByLink(link);
        }

        return links;
    }

    public async Task SaveFileByLink(string link)
    {
        var date = DateTime.Now.Ticks.ToString();
        var fileName = $"file" + date + ".png";
        var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads");
        var filePath = Path.Combine(uploadsFolder, fileName);
        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await fileStream.WriteAsync(await DownloadImage(link));
    }

    private HttpRequestMessage CreateBaseRequest(HttpMethod method, string uri)
    {
        var httpRequestMessage = new HttpRequestMessage(method, uri);
        var apiKey = "your_secret_key";
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        return httpRequestMessage;
    }

    public byte[] ConvertRgb24ToRgba32(byte[] inputImage)
    {
        using var inputStream = new MemoryStream(inputImage);
        using var outputStream = new MemoryStream();

        // Load the input image using ImageSharp
        using var image = Image.Load<Rgb24>(inputStream);

        // Create a new image with RGBA32 pixel format
        using var convertedImage = new Image<Rgba32>(image.Width, image.Height);

        // Convert RGB to RGBA
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                Rgb24 inputPixel = image[x, y];
                Rgba32 outputPixel = new Rgba32(inputPixel.R, inputPixel.G, inputPixel.B, byte.MaxValue);
                convertedImage[x, y] = outputPixel;
            }
        }

        // Save the converted image to the output stream
        convertedImage.Save(outputStream, new PngEncoder());

        // Return the converted image as a byte array
        return outputStream.ToArray();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}