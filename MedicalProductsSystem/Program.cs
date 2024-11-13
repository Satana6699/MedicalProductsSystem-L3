using MedicalProductsSystem.Services.CachedServices;
using MedicalProductsSystem.Services.ICachedServices;
using MedicinalProductsSystem.Infrastructure;
using MedicinalProductsSystem.Middleware;
using MedicinalProductsSystem.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace MedicinalProductsSystem
{
    public class Program
    {
        private static object cachedMedicinalProductsService;

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var services = builder.Services;
            // ��������� ����������� ��� ������� � �� � �������������� EF
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");


            //// ������� ������ ����������� � ���������� ���������� SQL Server, ���������� ��� ������������ � ������
            //// ������� ������������ ��� ���������� ��������� ����������
            //IConfigurationRoot configuration = builder.Configuration.AddUserSecrets<Program>().Build();
            //connectionString = configuration.GetConnectionString("RemoteSQLConnection");
            ////��������� ������ � ��� ������������ �� secrets.json
            //string secretPass = configuration["Database:password"];
            //string secretUser = configuration["Database:login"];
            //SqlConnectionStringBuilder sqlConnectionStringBuilder = new(connectionString)
            //{
            //    Password = secretPass,
            //    UserID = secretUser
            //};
            //connectionString = sqlConnectionStringBuilder.ConnectionString;



            services.AddDbContext<MedicinalProductsContext>(options => options.UseSqlServer(connectionString));

            // ���������� �����������
            services.AddMemoryCache();

            // ���������� ��������� ������
            services.AddDistributedMemoryCache();
            services.AddSession();

            // ��������� ����������� CachedTanksService
            services.AddScoped<ICachedCostMedicineService, CachedCostMedicineService>();
            services.AddScoped<ICachedDiseasesAndSymptomService, CachedDiseasesAndSymptomService>();
            services.AddScoped<ICachedDiseaseService, CachedDiseaseService>();
            services.AddScoped<ICachedFamilyMemberService, CachedFamilyMemberService>();
            services.AddScoped<ICachedMedicineService, CachedMedicineService>();
            services.AddScoped<ICachedPrescriptionService, CachedPrescriptionService>();

            //������������� MVC - ���������
            //services.AddControllersWithViews();
            var app = builder.Build();


            // ��������� ��������� ����������� ������
            app.UseStaticFiles();

            // ��������� ��������� ������
            app.UseSession();

            // ��������� ����������� ��������� middleware �� ������������� ���� ������ � ���������� �� �������������
            app.UseDbInitializer();
            var title = "������� ��������";
            var homeHTML =
            "<head>" +
                "<meta charset='UTF-8'>"+
                "<link rel = 'stylesheet' href = 'https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css'>" +
                "<link rel='stylesheet' href='/css/styles.css'>" + // ����������� CSS �����
                "<link rel = 'stylesheet' href = 'https://use.fontawesome.com/releases/v6.6.0/css/all.css'>" +
            "<title>" + title + "</title>" +
            "</head>" +
            "<body>" +
                "<aside class='menu'>" +
                        "<ul class='navigation'>" +
                            "<li><a href = '/' ><i class='fa-solid fa-house'></i>�������</a></li>" +
                            "<li><a href = '/info'><i class='fa-solid fa-info'></i> ����������</a></li>" +
                            "<li><a href = '/searchform1'><i class='fa-solid fa-F'></i>����� 1</a></li>" +
                            "<li><a href = '/searchform2'><i class='fa-solid fa-F'></i>����� 2</a></li>" +
                            "<li><a href = '/costmedicines'><i class='fa-solid fa-table'></i>CostMedicines</a></li>" +
                            "<li><a href = '/diseasesdndsymptoms'><i class='fa-solid fa-table'></i>DiseasesAndSymptom</a></li>" +
                            "<li><a href = '/diseases'><i class='fa-solid fa-table'></i>Diseases</a></li>" +
                            "<li><a href = '/familymembers'><i class='fa-solid fa-table'></i>FamiliMembers</a></li>" +
                            "<li><a href = '/medicines'><i class='fa-solid fa-table'></i>Medicines</a></li>" +
                            "<li><a href = '/prescriptions'><i class='fa-solid fa-table'></i>Prescriptions</a></li>" +
                        "</ul>" +
                "</aside>";

            //����������� � �ookies ��������, ��������� � �����
            app.Map("/searchform1", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    // ������ �������� �� cookies (���� ��� ����)
                    string nameSearch = context.Request.Cookies["Name"] ?? "";
                    title = "����� ������� �� ���� (1)";

                    // ������������ ������ ��� ������ HTML �����
                    string strResponse = homeHTML +
                    "<H1>����� �� ����� � ������� Medicine</H1>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><FORM action ='/searchform1' method='get'>" +
                    "���:<BR><INPUT type = 'text' name = 'Name' value ='" + nameSearch + "'>" +
                    "<BR><BR><INPUT type ='submit' value='��������� � Cookies'>" +
                    "</FORM>";

                    // ��������, ���� �� ���������� ����� �������� �� �����
                    if (context.Request.Query.ContainsKey("Name"))
                    {
                        // ��������� �������� �� ���������� �������
                        nameSearch = context.Request.Query["Name"];

                        // ����� �������� � �������
                        int N = 3;
                        // ������ �������� � cookies
                        context.Response.Cookies.Append("Name", nameSearch, new CookieOptions
                        {
                            Expires = DateTimeOffset.UtcNow.AddSeconds(2 * N + 240)
                        });

                        // ��������� ������ �� ������ ������
                        var medicinalProductsService = context.RequestServices.GetService<ICachedMedicineService>();
                        var medicine = medicinalProductsService.SearchObject(nameSearch);

                        if (medicine != null)
                        {
                            strResponse += "<TABLE BORDER=1>" +
                            "<TR>" +
                            "<TH>���</TH>" +
                            "<TH>��������</TH>" +
                            "<TH>���������</TH>" +
                            "<TH>����������������</TH>" +
                            "<TH>������������</TH>" +
                            "<TH>������� ��������</TH>" +
                            "</TR>" +
                            "<TR>" +
                            "<TD>" + medicine.Id + "</TD>" +
                            "<TD>" + medicine.Name + "</TD>" +
                            "<TD>" + medicine.Indications + "</TD>" +
                            "<TD>" + medicine.Contraindications + "</TD>" +
                            "<TD>" + medicine.Manufacturer + "</TD>" +
                            "<TD>" + medicine.Packaging + "</TD>" +
                            "</TR>" +
                            "</TABLE>";
                        }
                    }

                    strResponse += "</BODY></HTML>";

                    // ����������� ����� HTML
                    await context.Response.WriteAsync(strResponse);
                });
            });

            //����������� � Session ��������, ��������� � �����
            app.Map("/searchform2", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {

                    // ���������� �� Session ������� User
                    var medicine = context.Session.Get<Medicine>("medicine") ?? new Medicine();
                    string nameSearch = medicine.Name;

                    // ������������ ������ ��� ������ ������������ HTML �����
                    title = "����� ������� �� ���� (2)";
                    string strResponse = homeHTML +
                    "<H1>����� �� ����� � ������� Medicine</H1>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><FORM action ='/searchform2' / >" +
                    "���:<BR><INPUT type = 'text' name = 'Name' value = " + nameSearch + ">" +
                    "<BR><BR><INPUT type ='submit' value='��������� � Session'>";

                    nameSearch = context.Request.Query["Name"];
                    ICachedMedicineService MedicinalProductsService = context.RequestServices.GetService<ICachedMedicineService>();
                    medicine = MedicinalProductsService.SearchObject(nameSearch);
                    if (medicine != null)
                    {
                        context.Session.Set<Medicine>("medicine", medicine);
                        strResponse += "<TABLE BORDER=1>" +
                        "<TR>" +
                        "<TH>���</TH>" +
                        "<TH>��������</TH>" +
                        "<TH>���������</TH>" +
                        "<TH>����������������</TH>" +
                        "<TH>������������</TH>" +
                        "<TH>������� ��������</TH>" +
                        "</TR>" +
                            "<TR>" +
                            "<TD>" + medicine.Id + "</TD>" +
                            "<TD>" + medicine.Name + "</TD>" +
                            "<TD>" + medicine.Indications + "</TD>" +
                            "<TD>" + medicine.Contraindications + "</TD>" +
                            "<TD>" + medicine.Manufacturer + "</TD>" +
                            "<TD>" + medicine.Packaging + "</TD>" +
                            "</TR>" +
                        "</TABLE>" +
                        "</BODY></HTML>";
                    }

                    // ����������� ����� ������������ HTML �����
                    await context.Response.WriteAsync(strResponse);
                });
            });



            // ����� ���������� � �������
            app.Map("/info", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    // ������������ ������ ��� ������ 
                    string strResponse = homeHTML +
                    "<HTML><HEAD><TITLE>����������</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>����������:</H1>";
                    strResponse += "<BR> ������: " + context.Request.Host;
                    strResponse += "<BR> ����: " + context.Request.PathBase;
                    strResponse += "<BR> ��������: " + context.Request.Protocol;
                    // ����� ������
                    await context.Response.WriteAsync(strResponse);
                });
            });

            app.Map("/costmedicines", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    // Retrieve the service
                    ICachedCostMedicineService cachedMedicinalProductsService = context.RequestServices.GetService<ICachedCostMedicineService>();

                    // Retrieve cost medicines with related medicine details
                    IEnumerable<CostMedicine> costMedicines = cachedMedicinalProductsService.GetCostMedicines("CostMedicines20");

                    // Start generating the HTML string
                    string HtmlString = homeHTML +
                    "<HTML><HEAD><TITLE>���������</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>costmedicines</H1>" +
                    "<TABLE BORDER=1>";

                    // Add table headers
                    HtmlString += "<TR><TH>���</TH><TH>��������</TH><TH>������������</TH><TH>����</TH><TH>����</TH></TR>";

                    // Populate the table rows
                    foreach (var costMedicine in costMedicines)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + costMedicine.Id + "</TD>";
                        HtmlString += "<TD>" + costMedicine.Medicines.Name + "</TD>";
                        HtmlString += "<TD>" + costMedicine.Manufacturer + "</TD>";
                        HtmlString += "<TD>" + costMedicine.Price + "</TD>";
                        HtmlString += "<TD>" + costMedicine.Date.ToString() + "</TD>";
                        HtmlString += "</TR>";
                    }

                    HtmlString += "</TABLE>";
                    HtmlString += "</BODY></HTML>";

                    await context.Response.WriteAsync(HtmlString);
                });
            });

            app.Map("/diseasesdndsymptoms", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    //��������� � �������
                    ICachedDiseasesAndSymptomService cachedMedicinalProductsService = context.RequestServices.GetService<ICachedDiseasesAndSymptomService>();
                    IEnumerable<DiseasesAndSymptom> diseasesAndSymptoms = cachedMedicinalProductsService.GetDiseasesAndSymptoms("DiseasesAndSymptoms20");
                    string HtmlString = homeHTML +
                    "<HTML><HEAD><TITLE>���������</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>diseasesdndsymptoms</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>���</TH>";
                    HtmlString += "<TH>����</TH>";
                    HtmlString += "<TH>��� �������</TH>";
                    HtmlString += "<TH>��� ���������</TH>";
                    HtmlString += "</TR>";
                    foreach (var diseasesAndSymptom in diseasesAndSymptoms)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + diseasesAndSymptom.Id + "</TD>";
                        HtmlString += "<TD>" + diseasesAndSymptom.Dosage + "</TD>";
                        HtmlString += "<TD>" + diseasesAndSymptom.Diseases.Name + "</TD>";
                        HtmlString += "<TD>" + diseasesAndSymptom.Medicines.Name + "</TD>";
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "</BODY></HTML>";

                    // ����� ������
                    await context.Response.WriteAsync(HtmlString);
                });
            });

            app.Map("/diseases", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    //��������� � �������
                    ICachedDiseaseService cachedDiseaseService = context.RequestServices.GetService<ICachedDiseaseService>();
                    IEnumerable<Disease> diseases = cachedDiseaseService.GetDiseases("Diseases20");
                    string HtmlString = homeHTML +
                    "<HTML><HEAD><TITLE>���������</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>diseases</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>���</TH>";
                    HtmlString += "<TH>��������</TH>";
                    HtmlString += "<TH>�����������������</TH>";
                    HtmlString += "<TH>��������</TH>";
                    HtmlString += "<TH>Consequences</TH>";
                    HtmlString += "</TR>";
                    foreach (var disease in diseases)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + disease.Id + "</TD>";
                        HtmlString += "<TD>" + disease.Name + "</TD>";
                        HtmlString += "<TD>" + disease.Duration + "</TD>";
                        HtmlString += "<TD>" + disease.Symptoms + "</TD>";
                        HtmlString += "<TD>" + disease.Consequences + "</TD>";
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "</BODY></HTML>";

                    // ����� ������
                    await context.Response.WriteAsync(HtmlString);
                });
            });

            app.Map("/familymembers", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    //��������� � �������
                    ICachedFamilyMemberService cachedFamilyMembersService = context.RequestServices.GetService<ICachedFamilyMemberService>();
                    IEnumerable<FamilyMember> familyMembers = cachedFamilyMembersService.GetFamilyMembers("FamilyMembers20");
                    string HtmlString = homeHTML +
                    "<HTML><HEAD><TITLE>���������</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>familymembers</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>���</TH>";
                    HtmlString += "<TH>���</TH>";
                    HtmlString += "<TH>�������</TH>";
                    HtmlString += "<TH>���</TH>";
                    HtmlString += "</TR>";
                    foreach (var familyMember in familyMembers)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + familyMember.Id + "</TD>";
                        HtmlString += "<TD>" + familyMember.Name + "</TD>";
                        HtmlString += "<TD>" + familyMember.Age + "</TD>";
                        HtmlString += "<TD>" + familyMember.Gender + "</TD>";
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "</BODY></HTML>";

                    // ����� ������
                    await context.Response.WriteAsync(HtmlString);
                });
            });

            app.Map("/prescriptions", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    //��������� � �������
                    ICachedPrescriptionService cachedPrescriptionsService = context.RequestServices.GetService<ICachedPrescriptionService>();
                    IEnumerable<Prescription> prescriptions = cachedPrescriptionsService.GetPrescriptions("Prescriptions20");
                    string HtmlString = homeHTML +
                    "<HTML><HEAD><TITLE>���������</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>prescriptions</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>���</TH>";
                    HtmlString += "<TH>����</TH>";
                    HtmlString += "<TH>Diseases.Name</TH>";
                    HtmlString += "<TH>FamilyMember.Name</TH>";
                    HtmlString += "</TR>";
                    foreach (var prescription in prescriptions)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + prescription.Id + "</TD>";
                        HtmlString += "<TD>" + prescription.Date + "</TD>";
                        HtmlString += "<TD>" + prescription.Diseases.Name + "</TD>";
                        HtmlString += "<TD>" + prescription.FamilyMember.Name + "</TD>";
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "</BODY></HTML>";

                    // ����� ������
                    await context.Response.WriteAsync(HtmlString);
                });
            });

            app.Map("/medicines", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    //��������� � �������
                    ICachedMedicineService cachedMedicinalProductsService = context.RequestServices.GetService<ICachedMedicineService>();
                    IEnumerable<Medicine> medicines = cachedMedicinalProductsService.GetMedicines("Medicines20");
                    string HtmlString = homeHTML +
                    "<HTML><HEAD><TITLE>���������</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>medicines</H1>" +
                    "<TABLE BORDER=1>";
                    HtmlString += "<TR>";
                    HtmlString += "<TH>���</TH>";
                    HtmlString += "<TH>��������</TH>";
                    HtmlString += "<TH>���������</TH>";
                    HtmlString += "<TH>����������������</TH>";
                    HtmlString += "<TH>������������</TH>";
                    HtmlString += "<TH>������� ��������</TH>";
                    HtmlString += "</TR>";
                    foreach (var medicine in medicines)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + medicine.Id + "</TD>";
                        HtmlString += "<TD>" + medicine.Name + "</TD>";
                        HtmlString += "<TD>" + medicine.Indications + "</TD>";
                        HtmlString += "<TD>" + medicine.Contraindications + "</TD>";
                        HtmlString += "<TD>" + medicine.Manufacturer + "</TD>";
                        HtmlString += "<TD>" + medicine.Packaging + "</TD>";
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "</BODY></HTML>";

                    // ����� ������
                    await context.Response.WriteAsync(HtmlString);
                });
            });
            
            app.Run((context) =>
            {
                ICachedCostMedicineService cachedMedicinalProductsService = context.RequestServices.GetService<ICachedCostMedicineService>();
                cachedMedicinalProductsService.AddCostMedicines("CostMedicines20");

                ICachedDiseasesAndSymptomService cachedDiseasesAndSymptomService = context.RequestServices.GetService<ICachedDiseasesAndSymptomService>();
                cachedDiseasesAndSymptomService.AddDiseasesAndSymptoms("DiseasesAndSymptoms20");

                ICachedDiseaseService cachedDiseaseService = context.RequestServices.GetService<ICachedDiseaseService>();
                cachedDiseaseService.AddDiseases("Diseases20");

                ICachedFamilyMemberService cachedFamilyMemberService = context.RequestServices.GetService<ICachedFamilyMemberService>();
                cachedFamilyMemberService.AddFamilyMembers("FamilyMembers20");

                ICachedMedicineService cachedMedicineService = context.RequestServices.GetService<ICachedMedicineService>();
                cachedMedicineService.AddMedicines("Medicines20");

                ICachedPrescriptionService cachedPrescriptionService = context.RequestServices.GetService<ICachedPrescriptionService>();
                cachedPrescriptionService.AddPrescriptions("Prescriptions20");

                string HtmlString = homeHTML +
                "<HTML><HEAD><TITLE>���������</TITLE></HEAD>" +
                "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                "<BODY><H1>�������</H1>";
                HtmlString += "<H2>������ �������� � ��� �������</H2>";;

                return context.Response.WriteAsync(HtmlString);

            });

            //������������� MVC - ���������
            //app.UseRouting();
            //app.UseAuthorization();
            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllerRoute(
            //        name: "default",
            //        pattern: "{controller=Home}/{action=Index}/{id?}");
            //});

            app.Run();
        }
    }
}