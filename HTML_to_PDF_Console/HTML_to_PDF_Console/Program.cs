using RestSharp;
using System.Text.Json.Nodes;

var client = new RestClient("https://localhost:7045/pdf");

string path = Path.GetFullPath("../../../Templates/");

string currentDirectory = Directory.GetCurrentDirectory();

var request = new RestRequest().AddFile("index.html", path + "index.html")
    .AddFile("style.css", path + "style.css").AddFile("data.json", path + "data.json").AddFile("logo.png", path + "logo.png").AddFile("barcode.png", path + "barcode.png").AddFile("Inter-Regular.ttf", path + "Inter-Regular.ttf");

request.Method = Method.Post;

var json = new JsonObject
{
    ["index"] = "index.html",
    ["data"] = "data.json",
    ["width"] = 0,
    ["height"] = 0,
    ["margin"] = 0,
    ["assets"] = new JsonArray
              {
                "style.css",
                "logo.png",
                "barcode.png",
                "Inter-Regular.ttf"
              }
};

request.AddParameter("application/json", json.ToString(), ParameterType.RequestBody);

var response = await client.ExecuteAsync(request);

if (response.StatusCode == System.Net.HttpStatusCode.OK)
{
    System.IO.File.WriteAllBytes("result.pdf", response.RawBytes);

}