using KOAHome.EntityFramework;
using KOAHome.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KOAHome.Controllers
{
  public class HsServicesController : Controller
  {
    private readonly ILogger<HsBookingsController> _logger;
    private readonly QLKCL_NEWContext _db;
    private readonly IHsBookingTableService _book;
    private readonly IHsBookingServiceService _bookser;
    private readonly IReportEditorService _re;
    private readonly IAttachmentService _att;
    private readonly IHsCustomerService _cus;
    private readonly IReportService _report;
    private readonly IFormService _form;
    private readonly IActionService _action;
    private readonly IConnectionService _con;

    public HsServicesController(QLKCL_NEWContext db, ILogger<HsBookingsController> logger, IHsBookingTableService book, IHsBookingServiceService bookser, IReportEditorService re, IAttachmentService att, IHsCustomerService cus, IReportService report, IFormService form, IActionService action, IConnectionService con)
    {
      _db = db;
      _logger = logger;
      _book = book;
      _bookser = bookser;
      _re = re;
      _att = att;
      _cus = cus;
      _report = report;
      _form = form;
      _action = action;
      _con = con;
    }

    // GET: HsServicesController
    public async Task<IActionResult> Index([FromQuery] Dictionary<string, string> parameters, int page = 1, int pageSize = 20)
    {
      try
      {
        // chuyen parameters thanh Idictionary<string, object>
        Dictionary<string, object> objParameters = parameters.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);

        //Phân trang
        // search
        var (resultList, totalRecord, maxPage, totalPage) = await _report.Report_Pagination_search("HS_Service_search", null, objParameters, page, pageSize);
        ViewBag.gridData_Store = resultList;
        // gia tri pham trang tra ve
        ViewBag.Total = totalRecord;
        ViewBag.Page = page;
        ViewBag.TotalPage = totalPage;
        ViewBag.MaxPage = maxPage;
        ViewBag.First = 1;
        ViewBag.Last = totalPage;
        ViewBag.Next = page + 1;
        ViewBag.Prev = page - 1;

        // neu co loi tu action POST tra ve thi bao loi
        if (TempData["ErrorMessage"] != null)
        {
          ViewBag.ErrorMessage = TempData["ErrorMessage"];
        }
        else
        {
          //khai bao success
          ViewBag.success = "Thành công";
        }

        return View();
      }
      catch (Exception ex)
      {
        // Log the exception
        _logger.LogError(ex, "An error occurred while fetching booking service info.");
        // Optionally, return an error view
        return View("Error");
      }
    }

    // POST: HsBookings/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromForm] IFormCollection form)
    {
      // reset tempdata error message
      TempData["ErrorMessage"] = null;
      // Convert the IFormCollection to a dictionary of strings
      var formData = form.ToDictionary(
                      pair => pair.Key,
                      pair => (object)pair.Value.ToString()  // Ensure each value is a string (flatten StringValues)
                  );

      //// gui form data len view de hien thi
      //ViewBag.formData = formData;

      // Tách lại query param gốc từ form input "q_"
      var queryParamerter = form
          .Where(kv => kv.Key.StartsWith("q_"))
          .ToDictionary(
              kv => kv.Key.Substring(2),
              kv => (object)kv.Value.ToString()
          );

      //xu ly report editor
      // Dictionary để nhóm dữ liệu theo số thứ tự [n]
      // Chuyển đổi dữ liệu sang JSON (loc du lieu form tra ve lay du lieu grid va chuyen thanh json)
      string reportJsonData = await _re.ExtractGridDataToJson(form);
      //end xu ly report form
      var reportResultList = await _re.ReportEditor_Json_Update(queryParamerter, null, reportJsonData, "HS_Service_Json_ups", null);
      //kiem tra ton tai error message
      // Kiểm tra và nối giá trị của ErrorMessage
      if (_con.CheckForErrors(reportResultList, out string errorMessage))
      {
        //ViewBag.ErrorMessage = errorMessage;
        TempData["ErrorMessage"] = errorMessage;
        return RedirectToAction("Index");
      }
      // khong tra ve Id, cung khong tra ve error message thi bao loi chua tra ve id
      else
      {
        ViewBag.success = "Xử lý thành công"; // Gán vào ViewBag
        return RedirectToAction("Index");
      }
    }
  }
}
