using AuthJWT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace AuthJWT.Controllers
{
    [Authorize]
    [Route("api/hostname")]
    [ApiController]
    public class HostNameController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public HostNameController()
        {
            _httpClient = new HttpClient();
        }
        object dataReturn;
        private string RemoveHtmlTags(string input)
        {
            return Regex.Replace(input, "<.*?>", string.Empty);
        }
        [HttpGet("{tenmien}")]
        public async Task<IActionResult> search(string tenmien)
        {

           // string tenmien = "voz.vn";
            string key = "JpbmNlcyBTdHJl311dsjdsj1144RnRyYWwsIEF1Y2tsYW";
            string apiUrl = $"http://49.156.54.103:8088/check?tenmien={tenmien}&key={key}";
            try
            {
            /*    string apiUrl = "http://49.156.54.103:8088/check";
                string queryString = $"?tenmien={tenmien}&key={key}";*/
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                if (response != null)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // Define the start and end markers as regular expressions
                    string startMarkerPattern = "<!--\\s*Main content\\s*-->";
                    string endMarkerPattern = "<!--\\.\\/Main content-->";
                    // Find the content between the markers using regular expressions
                    Regex startRegex = new Regex(startMarkerPattern);
                    Regex endRegex = new Regex(endMarkerPattern);
                    Match startMatch = startRegex.Match(responseBody);
                    Match endMatch = endRegex.Match(responseBody);

                    if (startMatch.Success && endMatch.Success && endMatch.Index > startMatch.Index)
                    {
                        // Extract the content between the markers
                        int startIndex = startMatch.Index + startMatch.Length;
                        int endIndex = endMatch.Index;
                        string extractedContent = responseBody.Substring(startIndex, endIndex - startIndex);

                        string cleanedContent = RemoveHtmlTags(extractedContent);
                        string data = cleanedContent.Replace("\n", "").Replace("\t", "");


                        int domainNameIndex = data.IndexOf("THÔNG TIN TRA CỨU") + "THÔNG TIN TRA CỨU".Length;
                        int domainNameEnd = data.IndexOf("Loại tên miền:");
                        string SearchInformation = data.Substring(domainNameIndex, domainNameEnd - domainNameIndex).Trim();
                        string hosttype;
                    

                        domainNameIndex = data.IndexOf("Loại tên miền:") + "Loại tên miền:".Length;
                        domainNameEnd = data.IndexOf("Tên chủ thể");
                        int domainNameEnd2 = data.IndexOf("Trạng thái:");
                        try
                        {
                            hosttype = data.Substring(domainNameIndex, domainNameEnd - domainNameIndex).Trim();
                        }
                        catch {  
                            hosttype = data.Substring(domainNameIndex, domainNameEnd2 - domainNameIndex).Trim();

                        }

                        if (hosttype == "Tên miền quốc gia .VN")
                        {
                            domainNameIndex = data.IndexOf("Tên chủ thể đăng ký sử dụng:") + "Tên chủ thể đăng ký sử dụng:".Length;
                            domainNameEnd = data.IndexOf("Nhà đăng ký quản lý:");
                         string owner = data.Substring(domainNameIndex, domainNameEnd - domainNameIndex).Trim();

                            domainNameIndex = data.IndexOf("Nhà đăng ký quản lý:") + "Nhà đăng ký quản lý:".Length;
                            domainNameEnd = data.IndexOf("Ngày đăng ký:");
                           string registrarmanagement = data.Substring(domainNameIndex, domainNameEnd - domainNameIndex).Trim();

                            domainNameIndex = data.IndexOf("Ngày đăng ký:") + "Ngày đăng ký:".Length;
                            domainNameEnd = data.IndexOf("Ngày hết hạn:");
                           string registrationdate = data.Substring(domainNameIndex, domainNameEnd - domainNameIndex).Trim();

                             domainNameIndex = Math.Max(0, data.Length - 10);
                            string expirationdate = data.Substring(domainNameIndex);
                            //  string expirationdate = data.Substring(domainNameEnd, -10).Trim();

                            HostName hostname = new()
                            {
                                SearchInformation = SearchInformation,
                                hosttype= hosttype,
                                owner = owner,
                                registrarmanagement = registrarmanagement,
                                registrationdate = registrationdate,
                                expirationdate = expirationdate,
                            };
                            dataReturn = hostname;

                            // domainNameIndex = data.LastIndexOf("Ngày hết hạn: ") + "Ngày hết hạn: ".Length;
                            //  domainName = data.Substring(domainNameIndex, 10).Trim();
                        }
                        else
                                if (hosttype == "Tên miền quốc tế")
                        {
                            /* domainNameIndex = data.IndexOf("Loại tên miền:") + "Loại tên miền:".Length;
                             domainNameEnd = data.IndexOf("Trạng thái:");
                            //  domainName = data.Substring(domainNameIndex, domainNameEnd - domainNameIndex).Trim();

                             domainNameIndex = data.IndexOf("Trạng thái:") + "Trạng thái:".Length;
                             domainNameEnd = data.IndexOf("Thông tin chi tiết:");
                             //   domainName = data.Substring(domainNameIndex, domainNameEnd - domainNameIndex).Trim();*/

                            domainNameIndex = data.IndexOf("Trạng thái:") + "Trạng thái:".Length;
                            domainNameEnd = data.IndexOf("Thông tin chi tiết:");
                            string status= data.Substring(domainNameIndex, domainNameEnd - domainNameIndex).Trim();
                            InternationalDomainName hostname = new()
                            {
                                SearchInformation = SearchInformation,
                                hosttype = "Tên miền quốc tế",
                                status = status,
                            };
                            dataReturn = hostname;


                        }

                    }


                        return Ok(dataReturn);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, "Failed to call external API.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
