using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using Appointment.Models;
using Appointment.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
using System.Data;
namespace Appointment.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult MedicalHistory()
        {

            return View();
        }

        [HttpPost]
        [Route("Home/MedicalHistory/")]
        public IActionResult MedicalHistory(string qone, string qtwo, string qthree, string qfour, string qfive)
        {
            if (string.IsNullOrWhiteSpace(qone) || string.IsNullOrWhiteSpace(qtwo) || string.IsNullOrWhiteSpace(qthree) || string.IsNullOrWhiteSpace(qfour) || string.IsNullOrWhiteSpace(qfive))
            {
                ViewData["Message"] = "All fields are required.";
                return View();
            }

            HttpContext.Session.SetString("qone", qone.ToString() ?? "defaultqone");
            HttpContext.Session.SetString("qtwo", qtwo.ToString() ?? "defaultqtwo");
            HttpContext.Session.SetString("qthree", qthree.ToString() ?? "defaultqthree");
            HttpContext.Session.SetString("qfour", qfour.ToString() ?? "defaultqfour");
            HttpContext.Session.SetString("qfive", qfive.ToString() ?? "defaultqfive");

            return Redirect("/Home/AddAppointment");
        }

        public IActionResult BookedAppointment()
        {
            var data = fetchDataPatientSchedule();
            ViewData["PatientScheduleList"] = data;
            var data2 = fetchDataBookedDentistSchedule();
            ViewData["DentistScheduleList"] = data2;
            var data3 = fetchDataBookedDentistServices();
            ViewData["DentistServicesList"] = data3;
            return View();
        }

        private List<PatientScheduleModel> fetchDataPatientSchedule()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnectionString");
            var dataList = new List<PatientScheduleModel>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var query = "SELECT *,schedule_appointment.id AS schedule_appointment_id FROM schedule_appointment INNER JOIN users ON schedule_appointment.dentist_id=users.id INNER JOIN dentist_schedule ON schedule_appointment.dentist_id=dentist_schedule.users_id";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var data = new PatientScheduleModel
                                {
                                    Id = reader["schedule_appointment_id"]?.ToString() ?? "Unknown",
                                    UsersId = reader["users_id"]?.ToString() ?? "Unknown",
                                    ServicesId = reader["services_id"]?.ToString() ?? "Unknown",
                                    AppointmentTime = reader["appointment_time"]?.ToString() ?? "Unknown",
                                    DateAvailable = reader["date_available"]?.ToString() ?? "Unknown",
                                    FirstName = reader["firstname"]?.ToString() ?? "Unknown",
                                    LastName = reader["lastname"]?.ToString() ?? "Unknown",
                                    DentistId = reader["dentist_id"]?.ToString() ?? "Unknown",

                                };
                                dataList.Add(data);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while fetching data: {ex.Message}");
            }

            return dataList;
        }

        private List<DentistScheduleModel> fetchDataBookedDentistSchedule()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnectionString");
            var dataList = new List<DentistScheduleModel>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var query = "SELECT *,dentist_schedule.id AS dentist_schedule_id FROM dentist_schedule INNER JOIN users ON dentist_schedule.users_id=users.id";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var data = new DentistScheduleModel
                                {
                                    Id = reader["dentist_schedule_id"]?.ToString() ?? "Unknown",
                                    UsersId = reader["users_id"]?.ToString() ?? "Unknown",
                                    DateAvailable = reader["date_available"]?.ToString() ?? "Unknown",
                                    StartTime = reader["start_time"]?.ToString() ?? "Unknown",
                                    EndTime = reader["end_time"]?.ToString() ?? "Unknown",
                                    FirstName = reader["firstname"]?.ToString() ?? "Unknown",
                                    LastName = reader["lastname"]?.ToString() ?? "Unknown",
                                };
                                dataList.Add(data);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while fetching data: {ex.Message}");
            }

            return dataList;
        }

        private List<DentistServicesModel> fetchDataBookedDentistServices()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnectionString");
            var dataList = new List<DentistServicesModel>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var query = "SELECT *, dentist_services.id AS dentist_services_id FROM dentist_services INNER JOIN services ON dentist_services.services_id=services.id";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var data = new DentistServicesModel
                                {
                                    Id = reader["dentist_services_id"]?.ToString() ?? "Unknown",
                                    UsersId = reader["dentist_id"]?.ToString() ?? "Unknown",
                                    Title = reader["title"]?.ToString() ?? "Unknown",
                                    Description = reader["description"]?.ToString() ?? "Unknown",
                                    Hours = reader["hours"]?.ToString() ?? "Unknown",
                                };
                                dataList.Add(data);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while fetching data: {ex.Message}");
            }

            return dataList;
        }



        private List<ServicesModel> fetchDataServices()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnectionString");
            var dataList = new List<ServicesModel>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM services";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var data = new ServicesModel
                                {
                                    Id = reader["dentist_services_id"]?.ToString() ?? "Unknown",
                                    Title = reader["title"]?.ToString() ?? "Unknown",
                                    Description = reader["description"]?.ToString() ?? "Unknown",
                                    Hours = reader["hours"]?.ToString() ?? "Unknown",
                                };
                                dataList.Add(data);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while fetching data: {ex.Message}");
            }

            return dataList;
        }

        public IActionResult AddAppointment()
        {
            var data = fetchDataDentistSchedule();
            ViewData["DentistScheduleList"] = data;
            var data2 = fetchDataDentistServices();
            ViewData["DentistServicesList"] = data2;
            return View();
        }

        [Route("Home/AddAppointment/{id}")]
        public IActionResult AddAppointment(int id)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnectionString");
            DentistServicesModel account = null;

            try
            {
 
                       
                account = new DentistServicesModel
                {
                      UsersId = id.ToString(),
                };
                            

                if (account == null)
                {
                    return NotFound(); // Return a 404 if no account is found
                }

                var data = fetchDataDentistServices();
                ViewData["DentistServicesList"] = data;

                var data2 = fetchDataPatientSchedule();
                ViewData["PatientScheduleList"] = data2;

                
                return View(account);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }




      
                [HttpPost]
        [Route("Home/AddAppointment/{id?}")]
        public IActionResult AddAppointment(string id, string users_id, string services_id, string dentist_id, string appointment_time, string qone, string qtwo, string qthree, string qfour, string qfive)
        {
            if (string.IsNullOrWhiteSpace(users_id) || string.IsNullOrWhiteSpace(services_id) || string.IsNullOrWhiteSpace(dentist_id) || string.IsNullOrWhiteSpace(appointment_time) || string.IsNullOrWhiteSpace(qone) || string.IsNullOrWhiteSpace(qtwo) || string.IsNullOrWhiteSpace(qthree) || string.IsNullOrWhiteSpace(qfour) || string.IsNullOrWhiteSpace(qfive))
            {
                ViewData["Message"] = "All fields are required.";
                return View();
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnectionString");

            try
            {

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for existing appointment
                    var checkQuery = "SELECT COUNT(1) FROM schedule_appointment WHERE appointment_time = @AppointmentTime";
                    using (var checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@AppointmentTime", appointment_time);
                        var existingCount = (int)checkCommand.ExecuteScalar();

                        if (existingCount > 0 && string.IsNullOrWhiteSpace(id))
                        {
                            ViewData["Message"] = "An appointment with this dentist and time already exists.";
                            return View();
                        }
                    }
                        var insertQuery = @"INSERT INTO schedule_appointment (users_id, services_id, dentist_id, appointment_time, status, qone, qtwo, qthree, qfour, qfive) VALUES (@UsersId, @ServicesId, @DentistId, @AppointmentTime, @Status, @Qone, @Qtwo, @Qthree, @Qfour, @Qfive)";
                        using (var insertCommand = new SqlCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@UsersId", users_id);
                            insertCommand.Parameters.AddWithValue("@ServicesId", services_id);
                            insertCommand.Parameters.AddWithValue("@DentistId", dentist_id);
                            insertCommand.Parameters.AddWithValue("@AppointmentTime", appointment_time);
                            insertCommand.Parameters.AddWithValue("@Status", "Pending");
                            insertCommand.Parameters.AddWithValue("@Qone", qone);
                            insertCommand.Parameters.AddWithValue("@Qtwo", qtwo);
                            insertCommand.Parameters.AddWithValue("@Qthree", qthree);
                            insertCommand.Parameters.AddWithValue("@Qfour", qfour);
                            insertCommand.Parameters.AddWithValue("@Qfive", qfive);
                            insertCommand.ExecuteNonQuery();
                        }

                        ViewData["Message"] = "Appointment successfully created!";
                    



                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError($"SQL Error: {sqlEx.Message}");
                ViewData["Message"] = "A database error occurred.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                ViewData["Message"] = "An unexpected error occurred.";
            }

            return Redirect("/");
        }

        private List<DentistScheduleModel> fetchDataDentistSchedule()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnectionString");
            var dataList = new List<DentistScheduleModel>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM dentist_schedule INNER JOIN users ON dentist_schedule.users_id=users.id";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var data = new DentistScheduleModel
                                {
                                    Id = reader["id"]?.ToString() ?? "Unknown",
                                    UsersId = reader["users_id"]?.ToString() ?? "Unknown",
                                    DateAvailable = reader["date_available"]?.ToString() ?? "Unknown",
                                    StartTime = reader["start_time"]?.ToString() ?? "Unknown",
                                    EndTime = reader["end_time"]?.ToString() ?? "Unknown",
                                    FirstName = reader["firstname"]?.ToString() ?? "Unknown",
                                    LastName = reader["lastname"]?.ToString() ?? "Unknown"
                                };
                                dataList.Add(data);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while fetching data: {ex.Message}");
            }

            return dataList;
        }


        private List<DentistModel> fetchDataDentist()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnectionString");
            var dataList = new List<DentistModel>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM users WHERE type = 1";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var data = new DentistModel
                                {
                                    Id = reader["id"]?.ToString() ?? "Unknown",
                                    FirstName = reader["services_id"]?.ToString() ?? "Unknown",
                                    LastName = reader["date_available"]?.ToString() ?? "Unknown",
                                };
                                dataList.Add(data);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while fetching data: {ex.Message}");
            }

            return dataList;
        }


        private List<DentistServicesModel> fetchDataDentistServices()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnectionString");
            var dataList = new List<DentistServicesModel>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var query = "SELECT *,dentist_services.id AS dentist_services_id FROM dentist_services INNER JOIN services ON dentist_services.services_id=services.id";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var data = new DentistServicesModel
                                {
                                    Id = reader["dentist_services_id"]?.ToString() ?? "Unknown",
                                    UsersId = reader["dentist_id"]?.ToString() ?? "Unknown",
                                    Title = reader["title"]?.ToString() ?? "Unknown",
                                    Description = reader["description"]?.ToString() ?? "Unknown",
                                };
                                dataList.Add(data);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while fetching data: {ex.Message}");
            }

            return dataList;
        }

        public IActionResult Privacy()
        {
            return View();
        }


        // Profile view
        public IActionResult Profile()
        {
            var data = fetchDataProfile();
            ViewData["ProfileList"] = data;
            return View();
        }

        // Select Data Display Profile
        private List<AccountModel> fetchDataProfile()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnectionString");
            var dataList = new List<AccountModel>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM users";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var data = new AccountModel
                                {
                                    Id = reader["id"]?.ToString() ?? "Unknown",
                                    FirstName = reader["firstname"]?.ToString() ?? "Unknown",
                                    MiddleName = reader["middlename"]?.ToString() ?? "Unknown",
                                    LastName = reader["lastname"]?.ToString() ?? "Unknown",
                                    Email = reader["email"]?.ToString() ?? "Unknown",

                                };
                                dataList.Add(data);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while fetching data: {ex.Message}");
            }

            return dataList;
        }

        // Update Profile
        [HttpPost]
        [Route("Home/Profile/")]
        public IActionResult Profile(string id, string firstname, string middlename, string lastname, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(firstname) || string.IsNullOrWhiteSpace(middlename) || string.IsNullOrWhiteSpace(lastname) ||
                string.IsNullOrWhiteSpace(email))
            {
                ViewData["Message"] = "All fields are required except for the password.";
                return View();
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnectionString");

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Base query for updating user details
                    var updateQuery = @"UPDATE users SET firstname = @FirstName, middlename = @MiddleName, lastname = @LastName, email = @Email";

                    // If the password is provided, add the password hash to the update query
                    if (!string.IsNullOrWhiteSpace(password))
                    {
                        var passwordHash = PasswordHelper.HashPassword(password);
                        updateQuery += ", password = @PasswordHash";  // Add the password field update
                    }

                    updateQuery += " WHERE id = @Id"; // Always update based on ID

                    using (var updateCommand = new SqlCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@Id", id);
                        updateCommand.Parameters.AddWithValue("@FirstName", firstname);
                        updateCommand.Parameters.AddWithValue("@MiddleName", middlename);
                        updateCommand.Parameters.AddWithValue("@LastName", lastname);
                        updateCommand.Parameters.AddWithValue("@Email", email);

                        // Only add the password parameter if a password is provided
                        if (!string.IsNullOrWhiteSpace(password))
                        {
                            updateCommand.Parameters.AddWithValue("@PasswordHash", PasswordHelper.HashPassword(password));
                        }

                        updateCommand.ExecuteNonQuery();
                    }

                    ViewData["Message"] = "Update successful!";
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError($"SQL Error during registration: {sqlEx.Message} \nStack Trace: {sqlEx.StackTrace}");
                ViewData["Message"] = $"Database error: {sqlEx.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError($"General Error during registration: {ex.Message} \nStack Trace: {ex.StackTrace}");
                ViewData["Message"] = "An unexpected error occurred during registration.";
            }

            return Redirect("/Home/Profile");
        }

    }
}
