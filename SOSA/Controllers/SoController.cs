using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace SOSA.Controllers
{
    public class SoController : Controller
    {
        // GET: So
        public ActionResult Index()
        {
            return View();
        }

        // GET: So/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: So/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: So/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: So/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: So/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: So/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: So/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public async Task<ActionResult> SentiAnalays()
        {
            string phraseToCheck = HttpContext.Request.Form["Description"];
            var sentimentResult = await InvokeRequestResponseService("createtable", "");
            //var sentimentResult = await InvokeRequestResponseService("is_training##false", phraseToCheck.Trim());//=>Get the sentiment 
            //HttpContext.Session.SetString("SentimentResult", sentimentResult);
            ViewBag.SentimentResult = sentimentResult;
            return View("Index");
        }

        public async Task<ActionResult> CreateModel()
        {
            string phraseToCheck = "";
            var sentimentResult = await InvokeRequestResponseService("is_training##true", phraseToCheck);//=>Train the Model
            ViewBag.SentimentResult = sentimentResult;
            return View("Index");
        }
        public async Task<ActionResult> GetLog()
        {
            string phraseToCheck = "";
            var sentimentResult = await InvokeRequestResponseService("getsentimentlog", phraseToCheck);//=>Train the Model
            ViewBag.SentimentResult = sentimentResult;
            return View("Index");
        }
        static async Task<string> InvokeRequestResponseService(string isTraining, string phraseToCheck)
        {
            using (var client = new HttpClient())
            {
                var scoreRequest = new
                {

                    Inputs = new Dictionary<string, StringTable>() {
                        {
                            "input1",
                            new StringTable()
                            {
                                ColumnNames = new string[] {"Col1", "Col2"},
                                Values = new string[,] {  { isTraining, phraseToCheck }  }
                            }
                        },
                    },
                    GlobalParameters = new Dictionary<string, string>()
                    {
                    }
                };
                const string apiKey = "y8+lqopLd5zTRTU9leW3KpNAxn09L6ot+vycFT8Dv0mOhgUKOManXv4Ab5+8UdZRVl7pRNKPp6wYarQeRnVBag=="; // Replace this with the API key for the web service
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                client.BaseAddress = new Uri("https://ussouthcentral.services.azureml.net/workspaces/460c1912b3534ac182923745d4d2f34f/services/5ff4a9daf7554932bcbe8bdcb6c579c3/execute?api-version=2.0&details=true");

                // WARNING: The 'await' statement below can result in a deadlock if you are calling this code from the UI thread of an ASP.Net application.
                // One way to address this would be to call ConfigureAwait(false) so that the execution does not attempt to resume on the original context.
                // For instance, replace code such as:
                //      result = await DoSomeTask()
                // with the following:
                //      result = await DoSomeTask().ConfigureAwait(false)


                //var myContent = JsonConvert.SerializeObject(scoreRequest);
                //var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
                //var byteContent = new ByteArrayContent(buffer);
                HttpResponseMessage response = await client.PostAsJsonAsync("", scoreRequest);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    var deserializedResult = JsonConvert.DeserializeObject<APIResultRootObj>(result);
                    var actualResult = deserializedResult.Results.output1.value.Values[0][0].ToString();
                    //Console.WriteLine("Result: {0}", result);
                    // Console.WriteLine("Result: {0}", actualResult);
                    return actualResult;
                }
                else
                {
                    //Console.WriteLine(string.Format("The request failed with status code: {0}", response.StatusCode));

                    // Print the headers - they include the requert ID and the timestamp, which are useful for debugging the failure
                    //Console.WriteLine(response.Headers.ToString());

                    string responseContent = await response.Content.ReadAsStringAsync();
                    //Console.WriteLine(responseContent);
                    return responseContent;
                }
            }
        }

        public class StringTable
        {
            public string[] ColumnNames { get; set; }
            public string[,] Values { get; set; }
        }

        #region "JSON Deserialization classes"

        public class Value
        {
            public List<string> ColumnNames { get; set; }
            public List<string> ColumnTypes { get; set; }
            public List<List<string>> Values { get; set; }
        }

        public class Output1
        {
            public string type { get; set; }
            public Value value { get; set; }
        }

        public class Results
        {
            public Output1 output1 { get; set; }
        }

        public class APIResultRootObj
        {
            public Results Results { get; set; }
        }

        #endregion
    }
}